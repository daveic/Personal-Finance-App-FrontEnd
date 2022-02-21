﻿using Microsoft.EntityFrameworkCore;
using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class Credit
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string CredCode { get; set; }
        public string CredTitle { get; set; }
        public string DebName { get; set; }
        public double CredValue { get; set; }
        public DateTime CredDateTime { get; set; }
        public DateTime PrevDateTime { get; set; } //Data prevista di rientro credito
        public string CredNote { get; set; }
        public string input_value { get; set; }
        public int Exp_ID { get; set; }
    }
}
