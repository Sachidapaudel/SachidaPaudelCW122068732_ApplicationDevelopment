using System;
using System.Collections.Generic;

namespace SachidaPaudel.Models
{
    public enum TransactionType //dropdown
    {
        Credit,
        Debit,
        Debt
    }

    public class Transaction
    {
        public int TransactionId { get; set; }
        public string TransactionTitle { get; set; } = string.Empty;
        public decimal TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType TransactionTransactionType { get; set; }
        public string? Note { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        //// Default constructor
        //public Transaction() { }

        //// Parameterized constructor
        //public Transaction(string transactionTitle, decimal transactionAmount, DateTime transactionDate, TransactionType transactionTransactionType, string? note = null, List<string>? tags = null)
        //{
        //    TransactionTitle = transactionTitle ?? throw new ArgumentNullException(nameof(transactionTitle));
        //    TransactionAmount = transactionAmount;
        //    TransactionDate = transactionDate;
        //    TransactionTransactionType = transactionTransactionType;
        //    Note = note;
        //    if (tags != null)
        //    {
        //        Tags = tags ?? new List<string>();
        //    }
        //}
    }
}