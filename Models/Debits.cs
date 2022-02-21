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
        public string input_value { get; set; }
        public DateTime DebDateTime { get; set; } //Scadenza debito
        public DateTime DebInsDate { get; set; }
        public double RtNum { get; set; } //Numero di rate
        public double RtPaid { get; set; } //Rate pagate
        public double RemainToPay { get; set; } //Importo da pagare
        public string input_value_remain { get; set; }
        public string DebNote { get; set; }
        public int Exp_ID { get; set; }
    }
}
