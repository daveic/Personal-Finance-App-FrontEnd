using System.Collections.Generic;

namespace PersonalFinanceFrontEnd.Models
{
    public class KnownMovement
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string KMType { get; set; }
        public string KMTitle { get; set; }
        public double KMValue { get; set; }
        public string KMNote { get; set; }
        public string Input_value { get; set; }
        public int Exp_ID { get; set; }
        public bool On_Exp { get; set; }
    }

    public class KnownMovement_Exp
    {
        public string Usr_OID { get; set; }
        public int Month_Num { get; set; }
    }

    public class KnownMovements
    {
        public KnownMovement KnownMovement { get; set; }
        public IEnumerable<KnownMovement> KnownMovementList { get; set; }
        public KnownMovement_Exp KnownMovement_Exp { get; set; }
    }



}
