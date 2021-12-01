using System;

namespace WeatherReportsFrontEnd.Models
{
    public class Debit
    {
        public int ID { get; set; }
        public string DebCode { get; set; }
        public string DebTitle { get; set; }
        public string CredName { get; set; }
        public int DebValue { get; set; }        
        public DateTime DebDateTime { get; set; } //Scadenza debito
        public DateTime DebInsDate { get; set; }
        public int RtNum { get; set; } //Numero di rate
        public int RtPaid { get; set; } //Rate pagate
        public int RemainToPay { get; set; } //Importo da pagare
        public string DebNote { get; set; }
    }
}
