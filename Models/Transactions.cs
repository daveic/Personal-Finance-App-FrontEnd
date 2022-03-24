using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace PersonalFinanceFrontEnd.Models
{
    public class Transaction
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string TrsCode { get; set; }
        public string TrsTitle { get; set; }
        public DateTime TrsDateTime { get; set; }
        public double TrsValue { get; set; }
        public string TrsNote { get; set; }
        public bool Type { get; set; }
        public string NewTrsCode { get; set; }
        public string Input_value { get; set; }
        public DateTime? TrsDateTimeExp { get; set; }
        public string DebCredChoice { get; set; }
       // [Range(typeof(int), "0", "100", ErrorMessage = "{0} can only be between {1} and {2}")]
        public double DebCredInValue { get; set; } 
    }
    public class TransactionDetailsEdit
    {
        public List<Debit> DebitsRat { get; set; }
        public List<Debit> DebitsMono { get; set; }
        public List<Credit> CreditsMono { get; set; }
        public List<string> Codes { get; set; }
    }
    public class Transactions
    {
        public IEnumerable<Transaction> Trs { get; set; }
        public List<SelectListItem> ItemListYear { get; set; }
        public List<SelectListItem> ItemListMonth { get; set; }
        public List<SelectListItem> Codes { get; set; }
    }
    public class TrsToView
    {
        public IEnumerable<Transaction> Transactions { get; set; }
        public Transaction Transaction { get; set; }
    }
}
    
