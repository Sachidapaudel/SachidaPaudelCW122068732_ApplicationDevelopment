using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SachidaPaudel.Models;
using SachidaPaudel.Utils;

namespace SachidaPaudel.Service.TransactionService
{
    public class TransactionService : ITransactionService
    {
        private readonly List<Transaction> _transactions;
        private readonly CsvHelper _csvHelper;
        private int _nextTransactionId;

        public TransactionService(CsvHelper csvHelper)
        {
            _csvHelper = csvHelper ?? throw new ArgumentNullException(nameof(csvHelper));
            _transactions = _csvHelper.LoadTransactions() ?? new List<Transaction>();
            _nextTransactionId = _transactions.Any() ? _transactions.Max(t => t.TransactionId) + 1 : 1;
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
                    // Debts can be added without immediate balance impact
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

        public async Task<decimal> GetUserBalanceAsync()
        {
            var credit = _transactions
                .Where(t => t.TransactionTransactionType == TransactionType.Credit)
                .Sum(t => t.TransactionAmount);

            var debit = _transactions
                .Where(t => t.TransactionTransactionType == TransactionType.Debit)
                .Sum(t => t.TransactionAmount);

            return await Task.FromResult(credit - debit);
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
    }
}

