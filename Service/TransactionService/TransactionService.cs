using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SachidaPaudel.Models;
using SachidaPaudel.Utils;
using SachidaPaudel.Service.DebtsService;

namespace SachidaPaudel.Service.TransactionService
{
    public class TransactionService : ITransactionService
    {
        private readonly List<Transaction> _transactions;
        private readonly CsvHelper _csvHelper;
        private readonly IDebtsService _debtsService;
        private int _nextTransactionId;

        public TransactionService(CsvHelper csvHelper, IDebtsService debtsService)
        {
            _csvHelper = csvHelper ?? throw new ArgumentNullException(nameof(csvHelper));
            _debtsService = debtsService ?? throw new ArgumentNullException(nameof(debtsService));
            _transactions = _csvHelper.LoadTransactions() ?? new List<Transaction>();
            _nextTransactionId = _transactions.Any() ? _transactions.Max(t => t.TransactionId) + 1 : 1;

            // Subscribe to the DebtCleared event
            _debtsService.DebtCleared += OnDebtCleared;
            _debtsService.DebtUpdated += OnDebtUpdated;
            _debtsService.DebtDeleted += OnDebtDeleted;
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            // Assign a unique ID to the transaction
            transaction.TransactionId = _nextTransactionId++;

            // Adjust balance based on transaction type
            switch (transaction.TransactionTransactionType)
            {
                case TransactionType.Credit:
                    // No balance check needed for credit transactions
                    break;

                case TransactionType.Debit:
                    var balance = await GetUserBalanceAsync();
                    if (balance < transaction.TransactionAmount)
                        throw new InvalidOperationException("Insufficient balance for this debit transaction.");
                    break;

                case TransactionType.Debt:
                    // Add the debt amount to the user balance
                    await AdjustUserBalanceAsync(transaction.TransactionAmount);
                    break;

                default:
                    throw new InvalidOperationException("Invalid transaction type.");
            }

            // Add the transaction to the in-memory list
            _transactions.Add(transaction);

            // Save the transaction to the storage
            await Task.Run(() => _csvHelper.SaveTransaction(transaction));
        }

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            return await Task.FromResult(_transactions.ToList());
        }

        public async Task<List<Transaction>> SearchTransactionsAsync(string title, string transactionType, List<string> tags, DateTime? startDate, DateTime? endDate, string sortBy, bool ascending)
        {
            var filteredTransactions = _transactions.AsEnumerable();

            if (!string.IsNullOrEmpty(title))
                filteredTransactions = filteredTransactions.Where(t => t.TransactionTitle.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(transactionType) && Enum.TryParse<TransactionType>(transactionType, true, out var type))
                filteredTransactions = filteredTransactions.Where(t => t.TransactionTransactionType == type);

            if (tags != null && tags.Any())
                filteredTransactions = filteredTransactions.Where(t => t.Tags != null && t.Tags.Intersect(tags).Any());

            if (startDate.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.TransactionDate <= endDate.Value);

            filteredTransactions = sortBy switch
            {
                "Title" => ascending ? filteredTransactions.OrderBy(t => t.TransactionTitle) : filteredTransactions.OrderByDescending(t => t.TransactionTitle),
                "Amount" => ascending ? filteredTransactions.OrderBy(t => t.TransactionAmount) : filteredTransactions.OrderByDescending(t => t.TransactionAmount),
                "Date" => ascending ? filteredTransactions.OrderBy(t => t.TransactionDate) : filteredTransactions.OrderByDescending(t => t.TransactionDate),
                _ => filteredTransactions
            };

            return await Task.FromResult(filteredTransactions.ToList());
        }

        public async Task<List<Transaction>> GetTopTransactionsAsync(int count, bool highest = true)
        {
            var sortedTransactions = highest
                ? _transactions.OrderByDescending(t => t.TransactionAmount)
                : _transactions.OrderBy(t => t.TransactionAmount);

            return await Task.FromResult(sortedTransactions.Take(count).ToList());
        }

        //It allows at add debt also
        public async Task<decimal> GetUserBalanceAsync()
        {
            var transactions = await GetTransactionsAsync();
            decimal balance = transactions
                .Where(t => t.TransactionTransactionType == TransactionType.Credit)
                .Sum(t => t.TransactionAmount) - transactions
                .Where(t => t.TransactionTransactionType == TransactionType.Debit)
                .Sum(t => t.TransactionAmount);

            // Include total debt amount in the balance calculation
            var totalDebtAmount = await _debtsService.CalculateTotalDebtBalanceAsync();
            balance += totalDebtAmount;

            return balance;
        }

        public async Task<List<Transaction>> GetPendingDebtsAsync()
        {
            var pendingDebts = _transactions
                .Where(t => t.TransactionTransactionType == TransactionType.Debt && t.TransactionDate > DateTime.Now)
                .ToList();

            return await Task.FromResult(pendingDebts);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            var existingTransaction = _transactions.FirstOrDefault(t => t.TransactionId == transaction.TransactionId);
            if (existingTransaction != null)
            {
                // If the transaction type is Debt and the status is Cleared, adjust the user balance
                if (existingTransaction.TransactionTransactionType == TransactionType.Debt && transaction.TransactionTransactionType == TransactionType.Credit)
                {
                    await AdjustUserBalanceAsync(-transaction.TransactionAmount);
                }

                _transactions.Remove(existingTransaction);
                _transactions.Add(transaction);
                await Task.Run(() => _csvHelper.UpdateTransaction(transaction));
            }
        }

        public async Task DeleteTransactionAsync(int transactionId)
        {
            var transaction = _transactions.FirstOrDefault(t => t.TransactionId == transactionId);
            if (transaction != null)
            {
                _transactions.Remove(transaction);
                await Task.Run(() => _csvHelper.DeleteTransaction(transactionId));

                // If the transaction is a debt, remove the corresponding debt
                if (transaction.TransactionTransactionType == TransactionType.Debt)
                {
                    var debts = await _debtsService.GetDebtsAsync();
                    var debt = debts.FirstOrDefault(d => d.DebtSource == transaction.TransactionTitle && d.DebtAmount == transaction.TransactionAmount);
                    if (debt != null)
                    {
                        await _debtsService.RemoveDebtAsync(debt.DebtId);
                    }
                }
            }
        }

        public async Task<List<string>> GetExistingTagsAsync()
        {
            var transactions = await GetTransactionsAsync();
            return transactions.SelectMany(t => t.Tags).Distinct().ToList();
        }

        public Task<List<Transaction>> GetTopTransactionsAsync(int count, bool highest = true, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }

        private async Task AdjustUserBalanceAsync(decimal amount)
        {
            // Adjust the user balance by the specified amount
            var balance = await GetUserBalanceAsync();
            balance += amount;
            // Save the updated balance to the storage (if needed)
        }

        public async Task<bool> ClearDebtAsync(int debtId, decimal userBalance)
        {
            var debts = await _debtsService.GetDebtsAsync();
            var debt = debts.FirstOrDefault(d => d.DebtId == debtId);
            if (debt == null)
            {
                throw new ArgumentException("Debt not found.");
            }

            if (userBalance >= debt.DebtAmount)
            {
                debt.IsCleared = true;
                await _debtsService.UpdateDebtAsync(debt);

                // Recalculate the user balance after clearing the debt
                await AdjustUserBalanceAsync(-debt.DebtAmount);

                // Raise the DebtCleared event
                OnDebtCleared(this, new DebtClearedEventArgs { DebtId = debtId, Amount = debt.DebtAmount });
                return true;
            }
            else
            {
                throw new InvalidOperationException("Insufficient balance to clear the debt.");
            }
        }


        private async void OnDebtCleared(object sender, DebtClearedEventArgs e)
        {
            // Update the user balance when a debt is cleared
            await AdjustUserBalanceAsync(-e.Amount);
        }

        private async void OnDebtUpdated(object sender, Debts debt)
        {
            // Update the corresponding transaction if it exists
            var transaction = _transactions.FirstOrDefault(t => t.TransactionId == debt.DebtId);
            if (transaction != null)
            {
                transaction.TransactionTitle = debt.DebtSource;
                transaction.TransactionAmount = debt.DebtAmount;
                transaction.TransactionDate = debt.DebtDueDate;
                transaction.Note = debt.IsCleared ? "Cleared" : "Pending";
                await UpdateTransactionAsync(transaction);
            }
        }

        private async void OnDebtDeleted(object sender, int debtId)
        {
            // Delete the corresponding transaction if it exists
            var transaction = _transactions.FirstOrDefault(t => t.TransactionId == debtId);
            if (transaction != null)
            {
                await DeleteTransactionAsync(transaction.TransactionId);
            }
        }

        public Task DeleteTransactionByDebtAsync(string debtSource, decimal debtAmount)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUserBalanceAsync()
        {
            var transactions = _csvHelper.LoadTransactions();
            var debts = _csvHelper.LoadDebts().Where(d => !d.IsCleared).ToList();

            decimal balance = transactions.Sum(t => t.TransactionTransactionType == TransactionType.Credit ? t.TransactionAmount : -t.TransactionAmount);
            balance -= debts.Sum(d => d.DebtAmount);

            // Save the updated balance to the storage (if needed)
            // Assuming there's a method to save the balance in the CsvHelper class
            // _csvHelper.SaveBalance(balance);
        }
    }
}
