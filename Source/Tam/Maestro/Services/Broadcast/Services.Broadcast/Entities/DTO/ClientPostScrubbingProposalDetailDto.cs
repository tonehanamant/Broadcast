using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class ClientPostScrubbingProposalDetailDto
    {
        public int? Id { get; set; }
        
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public string DayPart { get; set; }
        public string SpotLength { get; set; }
        public List<ProgramCriteria> Programs { get; set; }
        public List<GenreCriteria> Genres { get; set; }
        public int? Sequence { get; set; }
        public List<ProposalFlightWeek> FlightWeeks { get; set; }
    }
}
