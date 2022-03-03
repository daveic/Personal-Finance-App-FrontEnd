using Microsoft.EntityFrameworkCore;
using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class Budget_Calc
    {
        public DateTime Future_Date { get; set; }
        public int Corrective_Item_0 { get; set; }
        public int Corrective_Item_1 { get; set; }
        public int Corrective_Item_2 { get; set; }
        public int Corrective_Item_3 { get; set; }
    }
}