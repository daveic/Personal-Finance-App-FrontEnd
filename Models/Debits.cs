using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class Debit
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string DebCode { get; set; }
        public string DebTitle { get; set; }
        public string CredName { get; set; }
        public double DebValue { get; set; }        
        public DateTime DebDateTime { get; set; } //Scadenza debito
        public DateTime DebInsDate { get; set; }
        public double RtNum { get; set; } //Numero di rate
        public double RtPaid { get; set; } //Rate pagate
        public double RemainToPay { get; set; } //Importo da pagare
        public string DebNote { get; set; }
    }
}
