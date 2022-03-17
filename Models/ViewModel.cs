﻿using Microsoft.AspNetCore.Mvc.Rendering;
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
        public Transaction Transaction { get; set; }
        public IEnumerable<Credit> Credits { get; set; }
        public Credit Credit { get; set; }
        public IEnumerable<Debit> Debits { get; set; }
        public Debit Debit { get; set; }


        public double TransactionSum { get; set; }
        public double CreditSum { get; set; }
        public double DebitSum { get; set; }
        public double TotWithDebits { get; set; }
        public double TotNoDebits { get; set; }
        public List<Bank> BankList { get; set; }
        public List<Ticket> TicketList { get; set; }
        public Bank Contanti { get; set; }
        public Budget_Calc Budget_Calc { get; set; }
    }
}
