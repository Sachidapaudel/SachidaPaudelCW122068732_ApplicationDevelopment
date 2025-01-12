using SachidaPaudel.Models;
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

    }
}
