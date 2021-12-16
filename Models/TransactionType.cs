﻿
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalFinanceFrontEnd.Models
{

    public class TransactionType
    {
        public int ID { get; set; }
        public string TrsCode { get; set; }
        public string TrsTitle { get; set; }
        public DateTime TrsDateTime { get; set; }
        public int TrsValue { get; set; }
        public string TrsNote { get; set; }
        public bool Type { get; set; }
        public List<SelectListItem> Codes { get; set; }

    }
}
    
