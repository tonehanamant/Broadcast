using System;

namespace Services.Broadcast.Entities
{
    public class FlightWeekDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsHiatus { get; set; }
    }
}
