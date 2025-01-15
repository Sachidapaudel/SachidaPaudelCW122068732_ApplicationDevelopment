using System;

namespace SachidaPaudel.Models
{
    public class Debts
    {
        public int DebtId { get; set; }        // Unique ID for the debt
        public string DebtSource { get; set; } = string.Empty; // Source of the debt (e.g., lender, bank, etc.)
        public decimal DebtAmount { get; set; } // Total amount of the debt
        public DateTime DebtDueDate { get; set; } // The due date for repayment
        public bool IsCleared { get; set; } // Whether the debt is cleared (true) or pending (false)

        public bool IsPending => !IsCleared && DebtDueDate > DateTime.Now;

        // Parameterless constructor (required for compatibility)
        public Debts() { }

        // Parameterized constructor for easier initialization
        public Debts(string source, decimal amount, DateTime dueDate, bool isCleared)
        {
            DebtSource = source;
            DebtAmount = amount;
            DebtDueDate = dueDate;
            IsCleared = isCleared;
        }

        // Method to clear debt from cash inflows
        public void ClearDebt(decimal cashInflows)
        {
            if (cashInflows >= DebtAmount)
            {
                IsCleared = true;
            }
            else
            {
                throw new InvalidOperationException("Insufficient cash inflows to clear the debt.");
            }
        }
    }
}

