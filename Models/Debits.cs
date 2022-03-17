using System;
using System.Collections.Generic;

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
        public string Input_value { get; set; }
        public DateTime DebDateTime { get; set; } //Scadenza debito
        public DateTime Input_DateTime { get; set; } //Input Scadenza debito
        public DateTime DebInsDate { get; set; }
        public double RtNum { get; set; } //Numero di rate
        public double RtPaid { get; set; } //Rate pagate
        public double RemainToPay { get; set; } //Importo da pagare
        public int Multiplier { get; set; } //Frequenza rate - Ogni *n*
        public string RtFreq { get; set; } //Frequenza rate - Possibile: Settimana, Mese, Anno
        public string Input_value_remain { get; set; }
        public string DebNote { get; set; }
        public int Exp_ID { get; set; }
    }
    public class Debits
    {
        public Debit Debit { get; set; }
        public IEnumerable<Debit> DebitList { get; set; }
    }
    public class Debit_Exp
    {
        public Debit Debit { get; set; }
        public bool FromTransaction { get; set; }
    }
}
