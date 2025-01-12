using System;
using System.Collections.Generic;

namespace SachidaPaudel.Models
{
    public enum TransactionType
    {
        Credit,
        Debit,
        Debt
    }

    public class Transaction
    {
        public int TransactionId { get; set; }
        public string TransactionTitle { get; set; }
        public decimal TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType TransactionTransactionType { get; set; }
        public string? Note { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }


}
