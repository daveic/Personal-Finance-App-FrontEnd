using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class KnownMovement
    {
        public int ID { get; set; }
        public string KMType { get; set; }
        public string KMTitle { get; set; }
        public int KMValue { get; set; }
        public string KMNote { get; set; }
    }
}
