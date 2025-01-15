using SachidaPaudel.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SachidaPaudel.Service.DebtsService
{
    public interface IDebtsService
    {
        Task AddDebtAsync(Debts debt);
        Task<List<Debts>> GetDebtsAsync();
        Task ClearDebtAsync(int debtId, decimal cashInflows);
        Task<List<Debts>> SearchDebtsAsync(string source, DateTime? startDate, DateTime? endDate);
        Task RemoveDebtAsync(int debtId);
        Task UpdateDebtAsync(Debts debt);
        Task<decimal> CalculateTotalDebtBalanceAsync();

        event EventHandler<DebtClearedEventArgs> DebtCleared;
        event EventHandler<Debts> DebtUpdated;
        event EventHandler<int> DebtDeleted;
    }

    public class DebtClearedEventArgs : EventArgs
    {
        public int DebtId { get; set; }
        public decimal Amount { get; set; }
    }


}
