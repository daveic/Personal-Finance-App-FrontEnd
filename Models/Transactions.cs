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
        public string input_value { get; set; }

    }
    public class TransactionDetailsEdit
    {
        public int ID { get; set; }
        public string User_OID { get; set; }
        public List<SelectListItem> Codes { get; set; }
    }
    public class Transactions
    {
        public KnownMovement KnownMovement { get; set; }
        public IEnumerable<KnownMovement> KnownMovementList { get; set; }
        public KnownMovement_Exp KnownMovement_Exp { get; set; }
    }
}
    
