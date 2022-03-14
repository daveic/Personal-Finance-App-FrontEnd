using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

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
    public class Expirations
    {
        public Expiration Expiration { get; set; }
        public IEnumerable<Expiration> ExpirationList { get; set; }
        public List<SelectListItem> ItemlistYear { get; set; }
        public List<int> UniqueMonth { get; set; }
        public List<string> UniqueMonthNames { get; set; }
        public List<ExpMonth> ExpMonth { get; set; }
    }
}
