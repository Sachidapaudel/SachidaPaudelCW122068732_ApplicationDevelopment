using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SachidaPaudel.Models
{

    public class User //public can be access by all
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Currency { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<Debts> Debts { get; set; } = new List<Debts>();
    }
}
