using SachidaPaudel.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SachidaPaudel.Service.TransactionService
{
    public interface ITransactionService
    {
        Task AddTransactionAsync(Transaction transaction);
        Task<List<Transaction>> GetTransactionsAsync();
        Task<List<Transaction>> SearchTransactionsAsync(string title, string transactionType, List<string> tags, DateTime? startDate, DateTime? endDate, string sortBy, bool ascending);
        Task<List<Transaction>> GetTopTransactionsAsync(int count, bool highest = true);
        Task<decimal> GetUserBalanceAsync();
        Task<List<Transaction>> GetPendingDebtsAsync();
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(int transactionId);
        Task<List<string>> GetExistingTagsAsync();
        Task DeleteTransactionByDebtAsync(string debtSource, decimal debtAmount); // Add this method
    }

}
