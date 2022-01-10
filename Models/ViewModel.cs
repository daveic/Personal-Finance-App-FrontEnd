using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;


namespace PersonalFinanceFrontEnd.Models
{
    public class ViewModel
    {
        public IEnumerable<Bank> Banks { get; set; }
        public Bank Bank { get; set; }
        public IEnumerable<Deposit> Deposits { get; set; }
        public Deposit Deposit { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
        public Ticket Ticket { get; set; }
        public IEnumerable<KnownMovement> KnownMovements { get; set; }
        public KnownMovement KnownMovement { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
        public TransactionExt TransactionExt { get; set; }
        public IEnumerable<Credit> Credits { get; set; }
        public Credit Credit { get; set; }
        public IEnumerable<Debit> Debits { get; set; }
        public Debit Debit { get; set; }
        public int state { get; internal set; }
        public int TransactionSum { get; set; }
        public int CreditSum { get; set; }
        public int DebitSum { get; set; }
        public int TotWithDebits { get; set; }
        public int TotNoDebits { get; set; }

    }
}
