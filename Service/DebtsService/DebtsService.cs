using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SachidaPaudel.Models;
using SachidaPaudel.Utils;

namespace SachidaPaudel.Service.DebtsService
{
    public class DebtsService : IDebtsService
    {
        private readonly List<Debts> _debts;
        private readonly CsvHelper _csvHelper;

        public DebtsService(CsvHelper csvHelper)
        {
            _csvHelper = csvHelper;
            _debts = _csvHelper.LoadDebts();
        }

        public async Task AddDebtAsync(Debts debt)
        {
            if (debt == null) throw new ArgumentNullException(nameof(debt));

            // Simulate async addition of debt
            await Task.Run(() => _debts.Add(debt));
            _csvHelper.SaveDebts(_debts); // Save all debts to CSV
        }

        public async Task<List<Debts>> GetDebtsAsync()
        {
            // Simulate async retrieval of all debts
            return await Task.FromResult(_debts.ToList());
        }

        public async Task ClearDebtAsync(int debtId, decimal cashInflows)
        {
            // Simulate clearing a debt
            var debt = await Task.Run(() => _debts.FirstOrDefault(d => d.DebtId == debtId));
            if (debt != null)
            {
                debt.ClearDebt(cashInflows);
                _csvHelper.SaveDebts(_debts); // Save all debts to CSV
            }
            else
            {
                throw new InvalidOperationException("Debt not found.");
            }
        }

        public async Task<List<Debts>> SearchDebtsAsync(string source, DateTime? startDate, DateTime? endDate)
        {
            // Simulate searching for debts
            return await Task.Run(() => _debts.Where(d =>
                (string.IsNullOrEmpty(source) || d.DebtSource.Contains(source, StringComparison.OrdinalIgnoreCase)) &&
                (!startDate.HasValue || d.DebtDueDate >= startDate.Value) &&
                (!endDate.HasValue || d.DebtDueDate <= endDate.Value)).ToList());
        }
    }
}

