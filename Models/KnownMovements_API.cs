using System.Collections.Generic;

namespace PersonalFinanceFrontEnd.Models
{
    public class KnownMovements_API
    {
        public KnownMovement KnownMovement { get; set; }
        public IEnumerable<KnownMovement> KnownMovements { get; set; }
        public IEnumerable<Expiration> Expirations { get; set; }
        public KnownMovement_Exp KnownMovement_Exp { get; set; }
    }


}
