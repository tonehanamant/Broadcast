﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanDto
    {
        public int Id { get; set; }
        public string SpotLengthsString { get; set; }
        public string DemoString { get; set; }
        public string Title { get; set; }
        public string DaypartsString { get; set; }
        public string FlightString { get; set; }
        public List<string> Iscis { get; set; }
        public string UnifiedTacticLineId { get; set; }
        public DateTime? UnifiedCampaignLastSentAt { get; set; }
        public DateTime? UnifiedCampaignLastReceivedAt { get; set; }
    }
}
