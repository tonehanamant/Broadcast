using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// The default values for plan properties in the UI.
    /// </summary>
    public class PlanDefaultsDto
    {
        public string Name { get; set; }
        public int AudienceId { get; set; }
        public List<CreativeLength> CreativeLengths { get; set; }
        public bool Equivalized { get; set; }
        public AudienceTypeEnum AudienceType { get; set; }
        public PostingTypeEnum PostingType { get; set; }
        public PlanStatusEnum Status { get; set; }
        public PlanCurrenciesEnum Currency { get; set; }
        public PlanGoalBreakdownTypeEnum GoalBreakdownType { get; set; }
        public ContainTypeEnum ShowTypeContainType { get; set; }
        public ContainTypeEnum GenreContainType { get; set; }
        public ContainTypeEnum ProgramContainType { get; set; }
        public ContainTypeEnum AffiliateContainType { get; set; }
        public double CoverageGoalPercent { get; set; }
        public double WeekdaysWeighting { get; set; }
        public double WeekendWeighting { get; set; }
        public List<PlanBlackoutMarketDto> BlackoutMarkets { get; set; } = new List<PlanBlackoutMarketDto>();
        public List<DateTime> FlightHiatusDays { get; set; } = new List<DateTime>();
        public List<int> FlightDays { get; set; }
        public List<WeeklyBreakdownWeek> WeeklyBreakdownWeeks { get; set; } = new List<WeeklyBreakdownWeek>();

        /// <summary>
        /// True to indicate the Plan is an ADU Only Plan.
        /// </summary>
        public bool IsAduPlan { get; set; } = false;
    }
}
