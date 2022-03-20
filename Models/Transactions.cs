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
    }
    public class TransactionDetailsEdit
    {
        public int ID { get; set; }
        public string User_OID { get; set; }
        public List<SelectListItem> Codes { get; set; }
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
    
