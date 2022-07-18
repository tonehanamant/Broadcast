using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlanDetailsDto
    {
        public SpotExceptionsRecommendedPlanDetailsDto()
        {
            SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto();
            RecommendedPlanDetail = new RecommendedPlanDetailDto();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the spot exceptions recommended plan identifier.
        /// </summary>
        /// <value>The spot exceptions recommended plan identifier.</value>
        public int SpotExceptionsRecommendedPlanId { get; set; }

        /// <summary>
        /// Gets or sets the recommended plan identifier.
        /// </summary>
        /// <value>The recommended plan identifier.</value>
        public int RecommendedPlanId { get; set; }

        /// <summary>
        /// Gets or sets the execution trace identifier.
        /// </summary>
        /// <value>The execution trace identifier.</value>
        public long ExecutionTraceId { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal? Rate { get; set; }

        /// <summary>
        /// Gets or sets the name of the audience.
        /// </summary>
        /// <value>The name of the audience.</value>
        public string AudienceName { get; set; }

        /// <summary>
        /// Gets or sets the contracted impressions.
        /// </summary>
        /// <value>The contracted impressions.</value>
        public double? ContractedImpressions { get; set; }

        /// <summary>
        /// Gets or sets the delivered impressions.
        /// </summary>
        /// <value>The delivered impressions.</value>
        public double? DeliveredImpressions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is recommended plan.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is recommended plan; otherwise, <c>false</c>.</value>
        public bool IsRecommendedPlan { get; set; }

        /// <summary>
        /// Gets or sets the plan clearance percentage.
        /// </summary>
        /// <value>The plan clearance percentage.</value>
        public double? PlanClearancePercentage { get; set; }

        /// <summary>
        /// Gets or sets the daypart code.
        /// </summary>
        /// <value>The daypart code.</value>
        public string DaypartCode { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        public int? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        public int? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the monday.
        /// </summary>
        /// <value>The monday.</value>
        public int? Monday { get; set; }

        /// <summary>
        /// Gets or sets the tuesday.
        /// </summary>
        /// <value>The tuesday.</value>
        public int? Tuesday { get; set; }

        /// <summary>
        /// Gets or sets the wednesday.
        /// </summary>
        /// <value>The wednesday.</value>
        public int? Wednesday { get; set; }

        /// <summary>
        /// Gets or sets the thursday.
        /// </summary>
        /// <value>The thursday.</value>
        public int? Thursday { get; set; }

        /// <summary>
        /// Gets or sets the friday.
        /// </summary>
        /// <value>The friday.</value>
        public int? Friday { get; set; }

        /// <summary>
        /// Gets or sets the saturday.
        /// </summary>
        /// <value>The saturday.</value>
        public int? Saturday { get; set; }

        /// <summary>
        /// Gets or sets the sunday.
        /// </summary>
        /// <value>The sunday.</value>
        public int? Sunday { get; set; }

        /// <summary>
        /// Gets or sets the spot exceptions recommended plan decision.
        /// </summary>
        /// <value>The spot exceptions recommended plan decision.</value>
        public SpotExceptionsRecommendedPlanDecisionDto SpotExceptionsRecommendedPlanDecision { get; set; }

        /// <summary>
        /// Gets or sets the recommended plan detail.
        /// </summary>
        /// <value>The recommended plan detail.</value>
        public RecommendedPlanDetailDto RecommendedPlanDetail { get; set; }
    }
}