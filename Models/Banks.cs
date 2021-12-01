using Microsoft.EntityFrameworkCore;
using System;

namespace WeatherReportsFrontEnd.Models
{
    public class Bank
    {
        public int ID { get; set; }
        public string BankName { get; set; }
        public string Iban { get; set; }
        public int BankValue { get; set; }
        public string BankNote { get; set; }
    }
}
