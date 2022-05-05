using System;

namespace Services.Broadcast.Entities.Isci
{
    public class MappingDetailsDto
    {
        public int MappingId { get; set; }
        public int Length { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? StartTime { get; set; }
        public int? EndTime { get; set; }
    }
}