using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingProgram
    {
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public decimal AvgCpm { get; set; }
        public double AvgImpressions { get; set; }
        public double PercentageOfBuy { get; set; }
        public double TotalImpressions { get; set; }        
        public decimal TotalCost { get; set; }
        public int TotalSpots { get; set; }
        public List<string> Stations { get; set; }
        public List<int> MarketCodes { get; set; }
    }
}
