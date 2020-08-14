using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Buying
{
    /// <summary>
    /// Buying data for a plan
    /// </summary>
    public class PlanBuying
    {
        public int Id { get; set; }

        public PlanBuyingCampaign Campaign { get; set; }

        public PlanBuyingDetails Plan { get; set; }

        public PlanBuyingInventoryPlanner InventoryPlanner { get; set; }

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

        public List<PlanBuyingInventoryTypeDetail> InventoryTypeMakeupDetails { get; set; } = new List<PlanBuyingInventoryTypeDetail>();

        public List<PlanBuyingDaypart> DaypartMakeupDetails { get; set; } = new List<PlanBuyingDaypart>();

        public decimal Status { get; set; }

        public string Notes { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }

        public class PlanBuyingInventoryTypeDetail
        {
            public int InventoryTypeId { get; set; }
            public decimal? Budget { get; set; }
            public double? CPM { get; set; }
            public decimal? Impressions { get; set; }
            public double? SharePercent { get; set; }
        }

        public class PlanBuyingCampaign
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class PlanBuyingDaypart
        {
            public int DaypartId { get; set; }
            public double? GoalPercent { get; set; }
            public double? SharePercent { get; set; }
            public decimal? Budget { get; set; }
            public double? CPM { get; set; }
            public decimal? Impressions { get; set; }
        }

        public class PlanBuyingInventoryPlanner
        {
            public int Id { get; set; }
            public List<PlanBuyingInventoryPlannerDetail> Details { get; set; }

            public class PlanBuyingInventoryPlannerDetail
            {
                public int InventorySourceId { get; set; }
                public decimal? Budget { get; set; }
                public double? CPM { get; set; }
                public decimal? Impressions { get; set; }
                public double? SharePercent { get; set; }
                public string EstimateId { get; set; }
                public string Notes { get; set; }
                public bool IsBooked { get; set; }
            }
        }

        public class PlanBuyingDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime FlightStartDate { get; set; }
            public DateTime FlightEndDate { get; set; }
            public int FlightActiveDays { get; set; }
            public int FlightHiatusDays { get; set; }
            public decimal? Impressions { get; set; }
            public decimal? Budget { get; set; }
            public double? CPM { get; set; }
            public List<PlanBuyingDetailsDaypart> Dayparts { get; set; }
            public double? CoverageGoalPercent { get; set; }
            public int AvailableMarketsCount { get; set; }
            public int BlackoutMarketsCount { get; set; }
            public int AudienceId { get; set; }
            public int PostingType { get; set; }
            public int SpotLengthId { get; set; }

            public class PlanBuyingDetailsDaypart
            {
                public int DaypartCodeId { get; set; }
                public int DaypartTypeId { get; set; }
                public int StartTimeSeconds { get; set; }
                public int EndTimeSeconds { get; set; }
                public bool IsStartTimeModified { get; set; }
                public bool IsEndTimeModified { get; set; }
                public bool HasRestrictions { get; set; }
            }
        }
    }

    public class PlanBuyingListingItem
    {
        public int Id { get; set; }

        public PlanBuyingListingItem.PlanBuyingListingItemDetails Plan { get; set; }

        public PlanBuyingListingItem.PlanBuyingListingItemCampaign Campaign { get; set; }

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

        public int Status { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }

        public class PlanBuyingListingItemCampaign
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAdvertiser Advertiser { get; set; }

            public PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAgency Agency { get; set; }

            public class PlanBuyingListingItemCampaignAdvertiser
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class PlanBuyingListingItemCampaignAgency
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

        }

        public class PlanBuyingListingItemDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime FlightStartDate { get; set; }
            public DateTime FlightEndDate { get; set; }
            public int FlightActiveDays { get; set; }
            public int FlightHiatusDays { get; set; }
            public decimal? Impressions { get; set; }
            public decimal? Budget { get; set; }
            public double? CPM { get; set; }
        }
    }

    public class PlanBuyingListRequest
    {
        public PlanBuyingStatusEnum StatusFilter { get; set; }
        public PlanBuyingTimeFramesEnum FlightFilter { get; set; }
    }

    public class PlanBuyingRequest
    {
        public int Id { get; set; }
        public List<PlanBuyingRequestDetails> Details { get; set; }

        public class PlanBuyingRequestDetails
        {
            public int InventorySourceId { get; set; }
            public decimal? Budget { get; set; }
            public double? CPM { get; set; }
            public decimal? Impressions { get; set; }
            public double? SharePercent { get; set; }
            public string EstimateId { get; set; }
            public string Notes { get; set; }
        }
    }
}