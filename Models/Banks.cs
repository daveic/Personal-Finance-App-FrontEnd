using Microsoft.EntityFrameworkCore;
using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class Bank
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string BankName { get; set; }
        public string Iban { get; set; }
        public double BankValue { get; set; }
        public string BankNote { get; set; }
    }
}
