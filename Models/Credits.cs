using Microsoft.EntityFrameworkCore;
using System;

namespace WeatherReportsFrontEnd.Models
{
    public class Credit
    {
        public int ID { get; set; }
        public string CredCode { get; set; }
    public string CredTitle { get; set; }
    public string DebName { get; set; }
    public int CredValue { get; set; }
    public DateTime CredDateTime { get; set; }    
    public string CredNote { get; set; }

    }
}
