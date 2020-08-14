using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PricingParametersWithoutPlanDto
    {
        public decimal? MinCpm { get; set; }
        public decimal? MaxCpm { get; set; }
        public double DeliveryImpressions { get; set; }
        public decimal Budget { get; set; }
        public decimal AdjustedBudget { get; set; }
        public double ProprietaryBlend { get; set; }
        public decimal CPM { get; set; }
        public decimal AdjustedCPM { get; set; }
        public double? CompetitionFactor { get; set; }
        public double? InflationFactor { get; set; }
        public int UnitCaps { get; set; }
        public UnitCapEnum UnitCapsType { get; set; }
        public PlanCurrenciesEnum Currency { get; set; }
        public MarketGroupEnum MarketGroup { get; set; }
        public decimal CPP { get; set; }
        public double DeliveryRatingPoints { get; set; }
        public double? Margin { get; set; }
        public int? JobId { get; set; }
        public List<PlanPricingInventorySourceDto> InventorySourcePercentages { get; set; } = new List<PlanPricingInventorySourceDto>();
        public List<PlanPricingInventorySourceTypeDto> InventorySourceTypePercentages { get; set; } = new List<PlanPricingInventorySourceTypeDto>();

        public List<CreativeLength> CreativeLengths { get; set; } = new List<CreativeLength>();
        public int SpotLengthId { get; set; }
        public bool Equivalized { get; set; }
        public List<int> FlightDays { get; set; }
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public List<DateTime> FlightHiatusDays { get; set; } = new List<DateTime>();
        public int AudienceId { get; set; }
        public int ShareBookId { get; set; }
        public int? HUTBookId { get; set; }
        public List<PlanDaypartDto> Dayparts { get; set; } = new List<PlanDaypartDto>();
        public List<PlanAvailableMarketDto> AvailableMarkets { get; set; } = new List<PlanAvailableMarketDto>();
        public double? CoverageGoalPercent { get; set; }
        public PlanGoalBreakdownTypeEnum GoalBreakdownType { get; set; }
        public List<WeeklyBreakdownWeek> WeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();
        public double? TargetRatingPoints { get; set; }
        public double ImpressionsPerUnit { get; set; }
        public PostingTypeEnum PostingType { get; set; }
    }
}
