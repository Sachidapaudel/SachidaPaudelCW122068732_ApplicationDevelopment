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
        void AddTransaction(Transaction transaction);
        List<Transaction> GetTransactions();
    }
}


