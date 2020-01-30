using System;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// Buying data for a plan
    /// </summary>
    public class PlanBuyingListingItem
    {
        public int Id { get; set; }

        public decimal? BookedBudget { get; set; }

        public decimal? BookedImpressions { get; set; }

        public double? BookedCPM { get; set; }

        public decimal? GoalBudget { get; set; }

        public decimal? GoalImpressions { get; set; }

        public double? GoalCPM { get; set; }

        public double? BookedMarginPercent { get; set; }

        public double? GoalMarginPercent { get; set; }
        
        public double? BookedImpressionsPercent { get; set; }

        public double? GoalImpressionsPercent { get; set; }

        public double? BookedCPMMarginPercent { get; set; }

        public double? GoalCPMMarginPercent { get; set; }

        public decimal Status { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }

    }
}