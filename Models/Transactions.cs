using System;


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
}
    
