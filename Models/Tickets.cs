using Microsoft.EntityFrameworkCore;
using System;

namespace PersonalFinanceFrontEnd.Models
{
    public class Ticket
    {
        public string Usr_OID { get; set; }
        public int ID { get; set; }
        public string TicketName { get; set; }
        public string NumTicket { get; set; }
        public float TicketValue { get; set; }
        public string TicketNote { get; set; }
    }
}
