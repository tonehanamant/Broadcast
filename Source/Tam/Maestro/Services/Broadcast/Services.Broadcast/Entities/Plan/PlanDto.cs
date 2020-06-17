﻿using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;
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
        /// 
        /// </summary>
        public WeeklyBreakdownTotals WeeklyBreakdownTotals { get; set; } = new WeeklyBreakdownTotals();
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
        public decimal HHCPM { get; set; }

        /// <summary>
        /// Gets or sets the household universe.
        /// </summary>
        /// <value>
        /// The household universe.
        /// </value>
        public double HHUniverse { get; set; }

        /// <summary>
        /// Gets or sets the household CPP.
        /// </summary>
        /// <value>
        /// The household CPP.
        /// </value>
        public decimal HHCPP { get; set; }

        /// <summary>
        /// Gets or sets the household rating points.
        /// </summary>
        /// <value>
        /// The household rating points.
        /// </value>
        public double HHRatingPoints { get; set; }

        /// <summary>
        /// Gets or sets the target universe.
        /// </summary>
        /// <value>
        /// The target universe.
        /// </value>
        public double TargetUniverse { get; set; }

        /// <summary>
        /// Gets or sets the household impressions.
        /// </summary>
        /// <value>
        /// The household impressions.
        /// </value>
        public double HHImpressions { get; set; }

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
        public bool IsDraft { get; set; }

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
        /// True - when there is a pricing model execution with status 'Queued' or 'Processing' in the DB
        /// </summary>
        public bool IsPricingModelRunning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has enable ADUs.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enable; otherwise, <c>false</c>.
        /// </value>
        public bool IsAduEnabled { get; set; }

        /// <summary>
        /// Gets or sets the impressions per unit.
        /// </summary>
        /// <value>
        /// The impressions per unit value.
        /// </value>
        public double ImpressionsPerUnit { get; set; }
    }
}
