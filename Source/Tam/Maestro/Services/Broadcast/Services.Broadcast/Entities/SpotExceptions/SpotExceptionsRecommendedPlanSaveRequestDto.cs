using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlanSaveRequestDto
    {
        public SpotExceptionsRecommendedPlanSaveRequestDto()
        {
            this.SpotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSaveDto>();
        }
        /// <summary>
        /// True - when the SpotExceptionsRecommendedPlanDecision is saved as AcceptAsInSpec in the DB
        /// </summary>
        public bool AcceptAsInSpec { get; set; }
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
        ///  Gets or sets the SpotExceptionsRecommendedPlans identifier.
        /// </summary>
        public int Id { get; set; }
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
