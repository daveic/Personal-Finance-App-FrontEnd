using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class Expiration
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string ExpTitle { get; set; }
        public double ExpValue { get; set; } //Diffenziato In - Out
        public string input_value { get; set; }
        public DateTime ExpDateTime { get; set; } //Data scadenza
        public string ExpDescription { get; set; }
        public string ColorLabel { get; set; }
    }

    public class ExpMonth
    {
        public string Month { get; set; }
        public Expiration ExpItem { get; set; }
    }
}
