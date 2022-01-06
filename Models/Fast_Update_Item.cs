using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace PersonalFinanceFrontEnd.Models
{
    public class Fast_Update_Item
    {
        //Identifier of the class
        public int FU_ID { get; set; }
        public int FU_ID_B1 { get; set; }
        public int FU_Value_B1 { get; set; }
        public int FU_ID_B2 { get; set; }
        public int FU_Value_B2 { get; set; }
        public int FU_ID_B3 { get; set; }
        public int FU_Value_B3 { get; set; }
        public int FU_ID_B4 { get; set; }
        public int FU_Value_B4 { get; set; }
        public int FU_ID_B5 { get; set; }
        public int FU_Value_B5 { get; set; }
        public int FU_ID_B6 { get; set; }
        public int FU_Value_B6 { get; set; }
        public int FU_ID_B7 { get; set; }
        public int FU_Value_B7 { get; set; }
        public int FU_ID_B8 { get; set; }
        public int FU_Value_B8 { get; set; }
        public int FU_ID_B9 { get; set; }
        public int FU_Value_B9 { get; set; }
        //Duplicate following 2 to add a new bank, change 10 -> 11....
        public int FU_ID_B10 { get; set; }
        public int FU_Value_B10 { get; set; }
        //Add below here new bank properties

        //Contanti
        public int FU_ID_C { get; set; }
        public int FU_Value_C { get; set; }
        //Ticket
        public int FU_ID_T { get; set; }
        public int FU_Count_T { get; set; }
    }
}
