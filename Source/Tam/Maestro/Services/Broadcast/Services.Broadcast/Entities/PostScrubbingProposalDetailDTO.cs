using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PostScrubbingProposalDetailDTO
    {
        public int? Id { get; set; }
        
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public string DayPart { get; set; }
        public string SpotLength { get; set; }
        public List<ProgramCriteria> Programs { get; set; }
        public List<GenreCriteria> Genres { get; set; }

        public List<ProposalDetailPostScrubbingDto> Details { get; set; } = new List<ProposalDetailPostScrubbingDto>();
    }
}
