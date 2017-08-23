using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalProgramFlightWeek
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsHiatus { get; set; }
        public int MediaWeekId { get; set; }
        public float Rate { get; set; }
        public List<OpenMarketAllocationDto> Allocations { get; set; }
    }
}
