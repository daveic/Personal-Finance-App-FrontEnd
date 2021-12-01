using Microsoft.EntityFrameworkCore;
using System;

namespace WeatherReportsFrontEnd.Models
{
    public class Ticket
    {
        public int ID { get; set; }
        public string TicketName { get; set; }
        public string NumTicket { get; set; }
        public int TicketValue { get; set; }
        public string TicketNote { get; set; }
    }
}
