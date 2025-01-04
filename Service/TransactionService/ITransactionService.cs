using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SachidaPaudel.Models;

namespace SachidaPaudel.Service.TransactionService
{
    internal interface ITransactionService
    {
        Task AddTransactionAsync(Transaction transaction);
        Task<List<Transaction>> GetTransactionsAsync();
        Task<List<Transaction>> SearchTransactionsAsync( string Transactiontitle, string TransactionType, List<string> Tags, DateTime? startDate, DateTime? endDate, string sortBy, bool ascending);
        Task<List<Transaction>> GetTopTransactionsAsync( int count, bool highest = true);
        Task<decimal> GetUserBalanceAsync();
        Task<List<Transaction>> GetPendingDebtsAsync();

    }
}


