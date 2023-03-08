using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// A dto for a Plan.
    /// </summary>
    public class PlanDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the campaign identifier.
        /// </summary>
        /// <value>
        /// The campaign identifier.
        /// </value>
        public int CampaignId { get; set; }

        /// <summary>
        /// Gets or sets the name of the campaign.
        /// </summary>
        /// <value>
        /// The name of the campaign.
        /// </value>
        public string CampaignName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the creative lengths.
        /// </summary>
        /// <value>
        /// The creative lengths.
        /// </value>
        public List<CreativeLength> CreativeLengths { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PlanDto"/> is equivalized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if equivalized; otherwise, <c>false</c>.
        /// </value>
        public bool? Equivalized { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public PlanStatusEnum Status { get; set; }

        /// <summary>
        /// Gets or sets the product identifier.
        /// The integer id used by the Aab Traffic Api.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        public int? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product master identifier.
        /// The guid id used by the Aab Api.
        /// </summary>
        /// <value>
        /// The product master identifier.
        /// </value>
        public Guid? ProductMasterId { get; set; }

        /// <summary>
        /// Gets or sets the flight days.
        /// </summary>
        /// <value>
        /// The flight days.
        /// </value>
        public List<int> FlightDays { get; set; }

        /// <summary>
        /// Gets or sets the flight start date.
        /// </summary>
        /// <value>
        /// The flight start date.
        /// </value>
        public DateTime? FlightStartDate { get; set; }

        /// <summary>
        /// Gets or sets the flight end date.
        /// </summary>
        /// <value>
        /// The flight end date.
        /// </value>
        public DateTime? FlightEndDate { get; set; }

        /// <summary>
        /// Gets or sets the external flight notes.
        /// </summary>
        /// <value>
        /// The external flight notes.
        /// </value>
        public string FlightNotes { get; set; }

        /// <summary>
        /// Gets or sets the internal flight notes.
        /// </summary>
        /// <value>
        /// The internal flight notes.
        /// </value>
        public string FlightNotesInternal { get; set; }

        /// <summary>
        /// Gets or sets the flight hiatus days.
        /// </summary>
        /// <value>
        /// The flight hiatus days.
        /// </value>
        public List<DateTime> FlightHiatusDays { get; set; } = new List<DateTime>();

        /// <summary>
        /// The total number of Hiatus days in the flight.
        /// </summary>
        /// <value>
        /// The total number of Hiatus days in the flight.
        /// </value>
        public int? TotalHiatusDays { get { return FlightHiatusDays.Count; } }

        /// <summary>
        /// The total number of Active days in the flight.
        /// </summary>
        /// <value>
        /// The total number of Active days in the flight.
        /// </value>
        public int? TotalActiveDays { get { return WeeklyBreakdownTotals.TotalActiveDays; } }

        /// <summary>
        /// Gets or sets the audience identifier.
        /// </summary>
        /// <value>
        /// The audience identifier.
        /// </value>
        public int AudienceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the audience.
        /// </summary>
        /// <value>
        /// The type of the audience.
        /// </value>
        public AudienceTypeEnum AudienceType { get; set; }

        /// <summary>
        /// Gets or sets the type of the posting.
        /// </summary>
        /// <value>
        /// The type of the posting.
        /// </value>
        public PostingTypeEnum PostingType { get; set; }

        /// <summary>
        /// Gets or sets the share book identifier.
        /// </summary>
        /// <value>
        /// The share book identifier.
        /// </value>
        public int? ShareBookId { get; set; }

        /// <summary>
        /// Gets or sets the hut book identifier.
        /// </summary>
        /// <value>
        /// The hut book identifier.
        /// </value>
        public int? HUTBookId { get; set; }

        /// <summary>
        /// Gets or sets the dayparts.
        /// </summary>
        /// <value>
        /// The dayparts.
        /// </value>
        public List<PlanDaypartDto> Dayparts { get; set; } = new List<PlanDaypartDto>();
        /// <summary>
        /// Gets or sets the Custom dayparts.
        /// </summary>
        /// <value>
        /// The Custom dayparts.
        /// </value>
        public List<PlanCustomDaypartDto> CustomDayparts { get; set; } = new List<PlanCustomDaypartDto>();

        /// <summary>
        /// Gets or sets the secondary audiences.
        /// </summary>
        /// <value>
        /// The secondary audiences.
        /// </value>
        public List<PlanAudienceDto> SecondaryAudiences { get; set; } = new List<PlanAudienceDto>();

        /// <summary>
        /// Gets or sets the budget.
        /// </summary>
        /// <value>
        /// The budget.
        /// </value>
        public decimal? Budget { get; set; }

        /// <summary>
        /// Gets or sets the target impressions.
        /// </summary>
        /// <value>
        /// The target impressions.
        /// </value>
        public double? TargetImpressions { get; set; }        

        /// <summary>
        /// Gets or sets the target rating points.
        /// </summary>
        /// <value>
        /// The target rating points.
        /// </value>
        public double? TargetRatingPoints { get; set; }

        /// <summary>
        /// Gets or sets the adu impressions.
        /// </summary>
        public double AduImpressions { get; set; }

        /// <summary>
        /// Gets or sets the target CPP.
        /// </summary>
        /// <value>
        /// The target CPP.
        /// </value>
        public decimal? TargetCPP { get; set; }

        /// <summary>
        /// Gets or sets the target CPM.
        /// </summary>
        /// <value>
        /// The target CPM.
        /// </value>
        public decimal? TargetCPM { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>
        /// The currency.
        /// </value>
        public PlanCurrenciesEnum Currency { get; set; }

        /// <summary>
        /// Gets or sets the coverage goal percent.
        /// </summary>
        /// <value>
        /// The coverage goal percent.
        /// </value>
        public double? CoverageGoalPercent { get; set; }

        /// <summary>
        /// The list of markets blacked out for the plan.
        /// </summary>
        /// <value>
        /// The blackout markets.
        /// </value>
        public List<PlanBlackoutMarketDto> BlackoutMarkets { get; set; } = new List<PlanBlackoutMarketDto>();

        /// <summary>
        /// The lst of markets available for a plan.
        /// </summary>
        /// <value>
        /// The lst of markets available for a plan.
        /// </value>
        public List<PlanAvailableMarketDto> AvailableMarkets { get; set; } = new List<PlanAvailableMarketDto>();

        /// <summary>
        /// Gets or sets the available markets sov total.
        /// </summary>
        /// <value>
        /// The available markets sov total.
        /// </value>
        public double AvailableMarketsSovTotal { get; set; }

        /// <summary>
        /// Gets or sets the type of the goal breakdown.
        /// </summary>
        /// <value>
        /// The type of the goal breakdown.
        /// </value>
        public PlanGoalBreakdownTypeEnum GoalBreakdownType { get; set; }

        /// <summary>
        /// Gets or sets the weekly breakdown weeks.
        /// </summary>
        /// <value>
        /// The weekly breakdown weeks.
        /// </value>
        public List<WeeklyBreakdownWeek> WeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();
        /// <summary>
        /// Gets or sets the rawweekly breakdown weeks.
        /// </summary>
        /// <value>
        /// The raw weekly breakdown weeks.
        /// </value>
        public List<WeeklyBreakdownWeek> RawWeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();
        /// <summary>
        /// 
        /// </summary>
        public WeeklyBreakdownTotals WeeklyBreakdownTotals { get; set; } = new WeeklyBreakdownTotals();
        /// <summary>
        /// Gets or sets the total share of voice.
        /// </summary>
        /// <value>
        /// The total share of voice.
        /// </value>
        public double TotalShareOfVoice { get { return WeeklyBreakdownTotals.TotalImpressionsPercentage; } }

        /// <summary>
        /// Gets or sets the modified by.
        /// </summary>
        /// <value>
        /// The modified by.
        /// </value>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the modified date.
        /// </summary>
        /// <value>
        /// The modified date.
        /// </value>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the VPVH.
        /// </summary>
        /// <value>
        /// The VPVH.
        /// </value>
        public double? Vpvh { get; set; }

        /// <summary>
        /// Gets or sets the household CPM.
        /// </summary>
        /// <value>
        /// The household CPM.
        /// </value>
        public decimal? HHCPM { get; set; }

        /// <summary>
        /// Gets or sets the household universe.
        /// </summary>
        /// <value>
        /// The household universe.
        /// </value>
        public double? HHUniverse { get; set; }

        /// <summary>
        /// Gets or sets the household CPP.
        /// </summary>
        /// <value>
        /// The household CPP.
        /// </value>
        public decimal? HHCPP { get; set; }

        /// <summary>
        /// Gets or sets the household rating points.
        /// </summary>
        /// <value>
        /// The household rating points.
        /// </value>
        public double? HHRatingPoints { get; set; }

        /// <summary>
        /// Gets or sets the target universe.
        /// </summary>
        /// <value>
        /// The target universe.
        /// </value>
        public double? TargetUniverse { get; set; }

        /// <summary>
        /// Gets or sets the household impressions.
        /// </summary>
        /// <value>
        /// The household impressions.
        /// </value>
        public double? HHImpressions { get; set; }

        /// <summary>
        /// Gets or sets the blackout market count.
        /// </summary>
        /// <value>
        /// The blackout market count.
        /// </value>
        public int? BlackoutMarketCount { get; set; }

        /// <summary>
        /// Gets or sets the blackout market total us coverage percent.
        /// </summary>
        /// <value>
        /// The blackout market total us coverage percent.
        /// </value>
        public double? BlackoutMarketTotalUsCoveragePercent { get; set; }

        /// <summary>
        /// Gets or sets the count of available markets with share of voice.
        /// </summary>
        /// <value>
        /// The count of available markets with sov.
        /// </value>
        public int? AvailableMarketsWithSovCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is draft.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is draft; otherwise, <c>false</c>.
        /// </value>
        public bool? IsDraft { get; set; }

        /// <summary>
        /// Gets or sets the plans version number.
        /// </summary>
        /// <value>
        /// The version number.
        /// </value>
        public int? VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the version identifier.
        /// </summary>
        /// <value>
        /// The version identifier.
        /// </value>
        public int VersionId { get; set; }

        /// <summary>
        /// The plan pricing parameters.
        /// </summary>
        public PlanPricingParametersDto PricingParameters { get; set; }

        /// <summary>
        /// The plan buying parameters.
        /// </summary>
        public PlanBuyingParametersDto BuyingParameters { get; set; }

        /// <summary>
        /// True - when there is a pricing model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsPricingModelRunning { get; set; }

        /// <summary>
        /// True - when there is a buying model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsBuyingModelRunning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has enable ADUs.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enable; otherwise, <c>false</c>.
        /// </value>
        public bool? IsAduEnabled { get; set; }

        /// <summary>
        /// Gets or sets the impressions per unit.
        /// </summary>
        /// <value>
        /// The impressions per unit value.
        /// </value>
        public double? ImpressionsPerUnit { get; set; }

        /// <summary>
        /// Gets or sets the pricing job id.
        /// </summary>
        /// <value>
        /// The pricing job id.
        /// </value>
        public int? JobId { get; set; }

        /// <summary>
        /// Gets or sets the out of sync status.
        /// </summary>
        /// <value>
        /// The out of sync status.
        /// </value>
        public bool IsOutOfSync { get; set; }

        /// <summary>
        /// Gets or sets the committed spot allocation model mode.
        /// </summary>
        /// <value>
        /// The committed spot allocation model mode.
        /// </value>
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; } = SpotAllocationModelMode.Quality;

        /// <summary>
        /// Gets the pricing budget CPM lever.
        /// </summary>
        /// <value>
        /// The pricing budget CPM lever.
        /// </value>
        public BudgetCpmLeverEnum PricingBudgetCpmLever => _GetPricingPlanBudgetCpmLever();

        /// <summary>
        /// Gets the buying budget CPM lever.
        /// </summary>
        /// <value>
        /// The buying budget CPM lever.
        /// </value>
        public BudgetCpmLeverEnum BuyingBudgetCpmLever => _GetBuyingPlanBudgetCpmLever();

        protected virtual BudgetCpmLeverEnum _GetPricingPlanBudgetCpmLever()
        {
            var result = PricingParameters?.BudgetCpmLever ?? BudgetCpmLeverEnum.Cpm;
            return result;
        }

        protected virtual BudgetCpmLeverEnum _GetBuyingPlanBudgetCpmLever()
        {
            var result = BuyingParameters?.BudgetCpmLever ?? BudgetCpmLeverEnum.Cpm;
            return result;
        }

        public double? FluidityPercentage { get; set; }
        public int? FluidityCategory { get; set; }
        public int? FluidityChildCategory { get; set; }
        public string UnifiedTacticLineId { get; set; }
        public DateTime? UnifiedCampaignLastSentAt { get; set; }
        public DateTime? UnifiedCampaignLastReceivedAt { get; set; }
        public string NielsenTransmittalCode { get; set; }

        public bool IsAduPlan { get; set; } = false;
    }

    public class PlanDto_v2 : PlanDto
    {
        /// <summary>
        /// The plan pricing parameters NSI and NTI.
        /// </summary>
        public new IList<PlanPricingParametersDto> PricingParameters { get; set; }

        protected override BudgetCpmLeverEnum _GetPricingPlanBudgetCpmLever()
        {
            BudgetCpmLeverEnum result;
            if (PricingParameters?.Any() ?? false)
            {
                result = PricingParameters?[0].BudgetCpmLever ?? BudgetCpmLeverEnum.Cpm;
            }
            else
            {
                result = BudgetCpmLeverEnum.Cpm;
            }
            return result;
        }
      
    }

    public class PlanDto_v3 : PlanDto_v2
    {
        /// <summary>
        /// The plan pricing parameters NSI and NTI.
        /// </summary>
        public new IList<PlanPricingParametersDto> PricingParameters { get; set; }
        /// <summary>
        /// Gets or sets the rawweekly breakdown weeks.
        /// </summary>
        /// <value>
        /// The raw weekly breakdown weeks.
        /// </value>
        public Dictionary<int, List<WeeklyBreakdownWeek>> SecondaryRawWeeklyBreakdownWeeks = new Dictionary<int, List<WeeklyBreakdownWeek>>();
        /// <summary>
        /// Gets or sets the NetBudget.
        /// </summary>
        /// <value>
        /// The NetBudget
        /// </value>
        public decimal? NetBudget { get; set; }
    }
}
