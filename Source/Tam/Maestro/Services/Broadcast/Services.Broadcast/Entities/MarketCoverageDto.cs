using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class MarketCoverageDto
    {
        public int MarketCoverageFileId { get; set; }
        public Dictionary<int, double> MarketCoveragesByMarketCode { get; set; } = new Dictionary<int, double>();
    }
}
