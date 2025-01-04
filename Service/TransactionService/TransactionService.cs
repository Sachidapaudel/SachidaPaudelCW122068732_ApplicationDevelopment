using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SachidaPaudel.Models;
using SachidaPaudel.Utils;

namespace SachidaPaudel.Service.TransactionService
{
    public class TransactionService : ITransactionService
    {
        private readonly List<Transaction> _transactions = new();
        private readonly CsvHelper _csvHelper;

        public TransactionService(CsvHelper csvHelper)
        {
            _csvHelper = csvHelper;
            _transactions = _csvHelper.LoadTransactions();
        }

        public void AddTransaction(Transaction transaction)
        {
            // Add transaction logic (e.g., save to a database or in-memory list)
            _transactions.Add(transaction);
            _csvHelper.SaveTransaction(transaction);
        }

        public List<Transaction> GetTransactions()
        {
            // Retrieve transactions logic (e.g., fetch from a database or in-memory list)
            return _transactions;
        }
    }
}



