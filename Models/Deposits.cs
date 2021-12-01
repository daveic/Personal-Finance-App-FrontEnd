using Microsoft.EntityFrameworkCore;
using System;

namespace WeatherReportsFrontEnd.Models
{
    public class Deposit
    {
        public int ID { get; set; }
        public string BankName { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DepValue { get; set; }
        public int PercGrow { get; set; }
        public string BankNote { get; set; }
    }
}
