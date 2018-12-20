using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class MarketCoverageDto
    {
        public int MarketCoverageFileId { get; set; }
        public Dictionary<int, double> MarketCoveragesByMarketCode { get; set; } = new Dictionary<int, double>();
    }
}
