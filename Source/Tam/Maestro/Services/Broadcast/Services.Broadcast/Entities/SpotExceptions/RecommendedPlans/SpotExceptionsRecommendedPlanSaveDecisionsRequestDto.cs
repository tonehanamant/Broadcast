using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    public class SpotExceptionsRecommendedPlanSaveDecisionsRequestDto
    {
        public SpotExceptionsRecommendedPlanSaveDecisionsRequestDto()
        {
            this.SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>();
        }

        /// <summary>
        /// Gets or sets the SpotExceptionsRecommendedPlans
        /// </summary>
        public List<SpotExceptionsRecommendedPlanSaveDto> SpotExceptionsRecommendedPlans { get; set; }
    }

    /// <summary>
    /// This class passes as argument while saving SpotExceptionsRecommendedPlanDecision
    /// </summary>
    public class SpotExceptionsRecommendedPlanSaveDto
    {
        /// <summary>
        /// Gets or sets the todo identifier.
        /// </summary>
        /// <value>The todo identifier.</value>
        public int? TodoId { get; set; }

        /// <summary>
        /// Gets or sets the done identifier.
        /// </summary>
        /// <value>The done identifier.</value>
        public int? DoneId { get; set; }

        /// <summary>
        /// Gets or sets the recommended_plan_id for the SpotExceptionsRecommendedPlans
        /// </summary>
        public int SelectedPlanId { get; set; }
    }

    /// <summary>
    /// Returns the response after saving SpotExceptionsRecommendedPlanDecision
    /// </summary>
    public class SpotExceptionsRecommendedPlanDecisionResponseDto
    {
        /// <summary>
        /// Specify that SpotExceptionsRecommendedPlanDecision is saved or not
        /// </summary>
        public bool IsSpotExceptionsRecommendedPlanDecisionSaved { get; set; }
        /// <summary>
        /// Gets or sets the SpotExceptionsRecommendedPlanDetails Id 
        /// </summary>
        public int SpotExceptionsRecommendedPlanDetailsId { get; set; }

    }
}
