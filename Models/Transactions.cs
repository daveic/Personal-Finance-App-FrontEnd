using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceFrontEnd.Models
{
    public class Transaction
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string TrsCode { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Titolo")]
        public string TrsTitle { get; set; }
        [Display(Name = "Data inserimento")]
        public DateTime TrsDateTime { get; set; }
        [Required]
        [Display(Name = "Importo")]
        public double TrsValue { get; set; }
        public string TrsNote { get; set; }
        public bool Type { get; set; }
        public string NewTrsCode { get; set; }
        public string TrsBank { get; set; }
        public string Input_value { get; set; }
        public DateTime? TrsDateTimeExp { get; set; }
        public string DCName { get; set; }
        public string ExpColorLabel { get; set; }
        public string DebCredChoice { get; set; }
        public double DebCredInValue { get; set; } 
    }
    public class TransactionDetailsEdit
    {
        public List<Debit> DebitsRat { get; set; }
        public List<Debit> DebitsMono { get; set; }
        public List<Credit> CreditsMono { get; set; }
        public List<Expiration> MonthExpirations { get; set; }
        public List<Expiration> MonthExpirationsOnExp { get; set; }
        public List<string> Codes { get; set; }
    }
    public class Transactions
    {
        public IEnumerable<Transaction> Trs { get; set; }
        public List<SelectListItem> ItemListYear { get; set; }
        public List<SelectListItem> ItemListMonth { get; set; }
        public List<SelectListItem> Codes { get; set; }
        public List<SelectListItem> BankList { get; set; }
    }
    public class TrsToView
    {
        public IEnumerable<Transaction> Transactions { get; set; }
        public Transaction Transaction { get; set; }
    }
}
    
