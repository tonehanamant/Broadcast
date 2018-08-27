using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalScrubbingDetailHeaderDto
    {
        public int Id { get; set; }
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public string DayPart { get; set; }
        public string SpotLength { get; set; }
        public List<ClientPostScrubbingProposalDetailDto> ClientScrubs { get; set; } = new List<ClientPostScrubbingProposalDetailDto>();
    }
}
