using SachidaPaudel.Service.TransactionService;
using System;
using System.Threading.Tasks;

namespace SachidaPaudel.Service
{
    public class BalanceService
    {
        private decimal _userBalance;

        public event Action OnBalanceChanged;

        public decimal UserBalance
        {
            get => _userBalance;
            private set
            {
                _userBalance = value;
                NotifyBalanceChanged();
            }
        }

        public async Task InitializeBalanceAsync(ITransactionService transactionService)
        {
            _userBalance = await transactionService.GetUserBalanceAsync();
            NotifyBalanceChanged();
        }

        public void UpdateBalance(decimal amount)
        {
            UserBalance += amount;
        }

        public void DeductBalance(decimal amount)
        {
            UserBalance -= amount;
        }

        private void NotifyBalanceChanged() => OnBalanceChanged?.Invoke();
    }
}
