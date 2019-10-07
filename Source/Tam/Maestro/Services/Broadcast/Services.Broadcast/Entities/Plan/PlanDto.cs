using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

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
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the spot length identifier.
        /// </summary>
        /// <value>
        /// The spot length identifier.
        /// </value>
        public int SpotLengthId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PlanDto"/> is equivalized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if equivalized; otherwise, <c>false</c>.
        /// </value>
        public bool Equivalized { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public PlanStatusEnum Status { get; set; }

        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        public int ProductId { get; set; }

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
        /// Gets or sets the flight notes.
        /// </summary>
        /// <value>
        /// The flight notes.
        /// </value>
        public string FlightNotes { get; set; }

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
        public int? TotalHiatusDays { get; set; }

        /// <summary>
        /// The total number of Active days in the flight.
        /// </summary>
        /// <value>
        /// The total number of Active days in the flight.
        /// </value>
        public int? TotalActiveDays { get; set; }

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
        public int ShareBookId { get; set; }

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
        /// Gets or sets the delivery.
        /// </summary>
        /// <value>
        /// The delivery.
        /// </value>
        public double? DeliveryImpressions { get; set; }

        /// <summary>
        /// Gets or sets the delivery rating points.
        /// </summary>
        /// <value>
        /// The delivery rating points.
        /// </value>
        public double? DeliveryRatingPoints { get; set; }

        /// <summary>
        /// Gets or sets the CPP.
        /// </summary>
        /// <value>
        /// The CPP.
        /// </value>
        public decimal? CPP { get; set; }

        /// <summary>
        /// Gets or sets the CPM.
        /// </summary>
        /// <value>
        /// The CPM.
        /// </value>
        public decimal? CPM { get; set; }

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
        /// Gets or sets the total share of voice.
        /// </summary>
        /// <value>
        /// The total share of voice.
        /// </value>
        public double TotalShareOfVoice { get; set; }

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
        public double Vpvh { get; set; }

        /// <summary>
        /// Gets or sets the household CPM.
        /// </summary>
        /// <value>
        /// The household CPM.
        /// </value>
        public decimal HouseholdCPM { get; set; }

        /// <summary>
        /// Gets or sets the household universe.
        /// </summary>
        /// <value>
        /// The household universe.
        /// </value>
        public double HouseholdUniverse { get; set; }

        /// <summary>
        /// Gets or sets the household CPP.
        /// </summary>
        /// <value>
        /// The household CPP.
        /// </value>
        public decimal HouseholdCPP { get; set; }

        /// <summary>
        /// Gets or sets the household rating points.
        /// </summary>
        /// <value>
        /// The household rating points.
        /// </value>
        public double HouseholdRatingPoints { get; set; }

        /// <summary>
        /// Gets or sets the universe.
        /// </summary>
        /// <value>
        /// The universe.
        /// </value>
        public double Universe { get; set; }

        /// <summary>
        /// Gets or sets the household delivery impressions.
        /// </summary>
        /// <value>
        /// The household delivery impressions.
        /// </value>
        public double HouseholdDeliveryImpressions { get; set; }
    }
}
