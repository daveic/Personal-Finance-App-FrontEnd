using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class Balance
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public DateTime BalDateTime { get; set; }
        public int ActBalance { get; set; }
    }
}
