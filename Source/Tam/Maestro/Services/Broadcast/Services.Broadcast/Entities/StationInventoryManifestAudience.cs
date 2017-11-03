using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Entities
{
    public class StationInventoryManifestAudience
    {
        public DisplayAudience Audience { get; set; }
        public double? Impressions { get; set; }
        public double? Rating { get; set; }
        public decimal Rate { get; set; }
    }
}
