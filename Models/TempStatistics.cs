using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;



namespace WeatherReportsFrontEnd.Models
{
    public class TempStatistics
    {
        public TempStatistics()
        {
            List<AreaListItem> Region = new List<AreaListItem>();
            List<AreaListItem> Province = new List<AreaListItem>();
            List<AreaListItem> City = new List<AreaListItem>();
            this.Region = Region;
            this.Province = Province;
            this.City = City;
        }
        public int GlobalMaxTemp { get; set; }
        public int GlobalMinTemp { get; set; }
        public AreaListItem SelCity { get; set; }
        public List<AreaListItem> Region { get; set; }
        public List<AreaListItem> Province { get; set; }
        public List<AreaListItem> City { get; set; }
        public int ToFarConvert(int CTemp)
        {
            int FTemp = CTemp * 9 / 5 + 32;
            return FTemp;
        }

    }
    public class AreaListItem
    {
        public AreaListItem()
        {
            TempData TempData = new TempData();
            this.TempData = TempData;
        }
        public string AreaName { get; set; }
        public TempData TempData { get; set; }

    }
    public class TempData
    {
        public int MaxTemp { get; set; }
        public int MinTemp { get; set; }
    }
    public class UniqueData
    {
        public List<SelectListItem> UniqueDate { get; set; }
        public List<SelectListItem> UniqueRegion { get; set; }
        public List<SelectListItem> UniqueProvince { get; set; }
        public List<SelectListItem> UniqueCity { get; set; }
    }
}
