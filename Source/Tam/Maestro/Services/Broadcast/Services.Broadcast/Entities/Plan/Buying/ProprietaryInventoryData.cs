using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class ProprietaryInventoryData
    {
        public double TotalImpressions { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalCostWithMargin { get; set; }

        public Dictionary<short, double> ImpressionsPerMarket { get; set; } = new Dictionary<short, double>();
    }
}
