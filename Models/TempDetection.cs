
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherReportsFrontEnd.Models
{
    public class TempDetection
    {
        public int ID { get; set; }
        public DateTime DateTime { get; set; } 
        public int TempMin { get; set; }
        public int TempMax { get; set; }
        public Weather Weather { get; set; }     
        public Location WorldLocation { get; set; }

        public int ToFarConvert(int CTemp)
        {
            int FTemp = CTemp * 9 / 5 + 32;
            return FTemp;
        }
    }

    public enum Weather
    {
        Sun = 1,
        Cloud = 2,
        Rain = 3,
        Storm = 4
    }

    
    public class Location
    {
        public string Region { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
    }
}
