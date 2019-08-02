using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class ProposalDetailRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<FlightWeekDto> FlightWeeks { get; set; }
        public PostingTypeEnum PostType { get; set; }
    }
}
