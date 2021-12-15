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
        public string FU_Logo_B1 { get; set; }
        public int FU_Value_B1 { get; set; }
        public int FU_ID_B2 { get; set; }
        public string FU_Logo_B2 { get; set; }
        public int FU_Value_B2 { get; set; }
        public int FU_ID_B3 { get; set; }
        public string FU_Logo_B3 { get; set; }
        public int FU_Value_B3 { get; set; }
        //Duplicate following 3 to add a new bank, change 3 -> 4....
        public int FU_ID_B4 { get; set; }
        public string FU_Logo_B4 { get; set; }
        public int FU_Value_B4 { get; set; }
        //Add below here new bank properties


        //Contanti
        public int FU_ID_C { get; set; }
        public string FU_Logo_C { get; set; }
        public int FU_Value_C { get; set; }
        //Ticket
        public int FU_ID_T { get; set; }
        public string FU_Logo_T { get; set; }
        public int FU_Value_T { get; set; }
    }
}
