using Microsoft.EntityFrameworkCore;
using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class Credit
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string CredCode { get; set; }
        public string CredTitle { get; set; }
        public string DebName { get; set; }
        public float CredValue { get; set; }
        public DateTime CredDateTime { get; set; }    
        public string CredNote { get; set; }
    }
}
