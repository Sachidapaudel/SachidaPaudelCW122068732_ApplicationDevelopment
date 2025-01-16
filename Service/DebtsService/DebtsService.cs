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
        private int _nextDebtId;

        public DebtsService(CsvHelper csvHelper)
        {
            _csvHelper = csvHelper ?? throw new ArgumentNullException(nameof(csvHelper));
            _debts = _csvHelper.LoadDebts() ?? new List<Debts>();
            _nextDebtId = _debts.Any() ? _debts.Max(d => d.DebtId) + 1 : 1;
        }

        public event EventHandler<DebtClearedEventArgs> DebtCleared;
        public event EventHandler<Debts> DebtUpdated;
        public event EventHandler<int> DebtDeleted;

        public async Task AddDebtAsync(Debts debt)
        {
            if (debt == null) throw new ArgumentNullException(nameof(debt));

            // Assign a unique ID to the debt
            debt.DebtId = _nextDebtId++;

            // Simulate async addition of debt
            await Task.Run(() => _debts.Add(debt));
            _csvHelper.SaveDebts(_debts); // Save all debts to CSV
        }

        public async Task<List<Debts>> GetDebtsAsync()
        {
            // Simulate async retrieval of all debts
            return await Task.FromResult(_debts.ToList());
        }

        public async Task ClearDebtAsync(int debtId, decimal userBalance)
        {
            var debt = _debts.FirstOrDefault(d => d.DebtId == debtId);
            if (debt == null)
            {
                throw new ArgumentException("Debt not found.");
            }

            if (userBalance >= debt.DebtAmount)
            {
                debt.IsCleared = true;
                await _csvHelper.UpdateDebtAsync(debt);

                // Recalculate the user balance after clearing the debt
                await _csvHelper.UpdateUserBalanceAsync();
            }
            else
            {
                throw new InvalidOperationException("Insufficient balance to clear the debt.");
            }
        }

        public async Task<List<Debts>> SearchDebtsAsync(string source, DateTime? startDate, DateTime? endDate, string sortBy, bool ascending)
        {
            // Simulate async operation
            await Task.Delay(100);

            // Filter by source
            var filteredDebts = _debts.AsQueryable();
            if (!string.IsNullOrEmpty(source))
            {
                filteredDebts = filteredDebts.Where(d => d.DebtSource.Contains(source, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                filteredDebts = filteredDebts.Where(d => d.DebtDueDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                filteredDebts = filteredDebts.Where(d => d.DebtDueDate <= endDate.Value);
            }

            // Sort by specified field
            filteredDebts = sortBy switch
            {
                "Source" => ascending ? filteredDebts.OrderBy(d => d.DebtSource) : filteredDebts.OrderByDescending(d => d.DebtSource),
                "Amount" => ascending ? filteredDebts.OrderBy(d => d.DebtAmount) : filteredDebts.OrderByDescending(d => d.DebtAmount),
                _ => ascending ? filteredDebts.OrderBy(d => d.DebtDueDate) : filteredDebts.OrderByDescending(d => d.DebtDueDate),
            };

            return await Task.FromResult(filteredDebts.ToList());
        }

        public async Task RemoveDebtAsync(int debtId)
        {
            var debt = await Task.Run(() => _debts.FirstOrDefault(d => d.DebtId == debtId));
            if (debt != null)
            {
                await Task.Run(() => _debts.Remove(debt));
                _csvHelper.SaveDebts(_debts); // Save all debts to CSV

                // Raise the DebtDeleted event
                DebtDeleted?.Invoke(this, debtId);
            }
            else
            {
                throw new InvalidOperationException("Debt not found.");
            }
        }

        public async Task UpdateDebtAsync(Debts debt)
        {
            var existingDebt = await Task.Run(() => _debts.FirstOrDefault(d => d.DebtId == debt.DebtId));
            if (existingDebt != null)
            {
                existingDebt.DebtSource = debt.DebtSource;
                existingDebt.DebtAmount = debt.DebtAmount;
                existingDebt.DebtDueDate = debt.DebtDueDate;
                existingDebt.IsCleared = debt.IsCleared;
                _csvHelper.SaveDebts(_debts); // Save all debts to CSV

                // Raise the DebtUpdated event
                DebtUpdated?.Invoke(this, debt);
            }
            else
            {
                throw new InvalidOperationException("Debt not found.");
            }
        }

        public async Task<decimal> CalculateTotalDebtBalanceAsync()
        {
            var debts = await GetDebtsAsync();
            return debts.Where(d => !d.IsCleared).Sum(d => d.DebtAmount);
        }
    }
}


