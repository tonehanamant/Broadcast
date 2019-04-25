using System;

namespace Services.Broadcast.Entities
{
    public class QuarterDetailDto
    {
        public int Quarter { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
