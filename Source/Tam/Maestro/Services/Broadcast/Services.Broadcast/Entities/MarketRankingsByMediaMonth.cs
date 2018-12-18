using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class MarketRankingsByMediaMonth
    {
        public int MediaMonthId { get; set; }

        public Dictionary<int, int> MarketCodeRankMappings { get; set; }
    }
}
