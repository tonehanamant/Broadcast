﻿using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalDetailRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<FlightWeekDto> FlightWeeks { get; set; }
    }
}
