using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SachidaPaudel.Models;

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
        Task UpdateTransactionAsync(Transaction transaction); // Add this method
        Task DeleteTransactionAsync(int transactionId); // Add this method
        Task<List<string>> GetExistingTagsAsync(); // Add this method
    }
}


