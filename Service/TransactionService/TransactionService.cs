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
        private readonly List<Transaction> _transactions = new();
        private readonly CsvHelper _csvHelper;

        public TransactionService(CsvHelper csvHelper)
        {
            _csvHelper = csvHelper;
            _transactions = _csvHelper.LoadTransactions();
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            // Handle transaction types
            switch (transaction.TransactionTransactionType)
            {
                case TransactionType.Credit:
                    // Directly add credit transactions
                    break;

                case TransactionType.Debit:
                    var balance = await GetUserBalanceAsync();
                    if (balance < transaction.TransactionAmount)
                        throw new InvalidOperationException("Insufficient balance for this debit transaction.");
                    break;

                case TransactionType.Debt:
                    // Debt transactions don't immediately affect balance
                    break;

                default:
                    throw new InvalidOperationException("Invalid transaction type.");
            }

            // Add transaction to list and save to CSV
            _transactions.Add(transaction);
            await Task.Run(() => _csvHelper.SaveTransaction(transaction));
        }

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            return await Task.FromResult(_transactions.ToList());
        }

        public async Task<List<Transaction>> SearchTransactionsAsync(string title, string transactionType, List<string> tags, DateTime? startDate, DateTime? endDate, string sortBy, bool ascending)
        {
            var transactions = await Task.FromResult(_transactions);
            var filteredTransactions = transactions.Where(t =>
                (string.IsNullOrEmpty(title) || t.TransactionTitle.Contains(title, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(transactionType) || t.TransactionTransactionType.ToString().Equals(transactionType, StringComparison.OrdinalIgnoreCase)) &&
                (tags == null || tags.Count == 0 || t.Tags.Any(tag => tags.Contains(tag))) &&
                (!startDate.HasValue || t.TransactionDate >= startDate.Value) &&
                (!endDate.HasValue || t.TransactionDate <= endDate.Value)
            );

            filteredTransactions = sortBy switch
            {
                "Title" => ascending ? filteredTransactions.OrderBy(t => t.TransactionTitle) : filteredTransactions.OrderByDescending(t => t.TransactionTitle),
                "Amount" => ascending ? filteredTransactions.OrderBy(t => t.TransactionAmount) : filteredTransactions.OrderByDescending(t => t.TransactionAmount),
                "Date" => ascending ? filteredTransactions.OrderBy(t => t.TransactionDate) : filteredTransactions.OrderByDescending(t => t.TransactionDate),
                _ => filteredTransactions
            };

            return filteredTransactions.ToList();
        }

        public async Task<List<Transaction>> GetTopTransactionsAsync(int count, bool highest = true)
        {
            var transactions = await Task.FromResult(_transactions);
            return highest
                ? transactions.OrderByDescending(t => t.TransactionAmount).Take(count).ToList()
                : transactions.OrderBy(t => t.TransactionAmount).Take(count).ToList();
        }

        public async Task<decimal> GetUserBalanceAsync()
        {
            // Calculate the balance using transactions
            var totalCredits = _transactions
                .Where(t => t.TransactionTransactionType == TransactionType.Credit)
                .Sum(t => t.TransactionAmount);

            var totalDebit = _transactions
                .Where(t => t.TransactionTransactionType == TransactionType.Debit)
                .Sum(t => t.TransactionAmount);

            var balance = totalCredits - totalDebit;

            return await Task.FromResult(balance);
        }

        public async Task<List<Transaction>> GetPendingDebtsAsync()
        {
            return await Task.FromResult(
                _transactions.Where(t => t.TransactionTransactionType == TransactionType.Debt && t.TransactionDate > DateTime.Now).ToList());
        }
    }
}
