using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class MarketCoverageFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public List<MarketCoverage> MarketCoverages { get; set; } = new List<MarketCoverage>();
    }
}
