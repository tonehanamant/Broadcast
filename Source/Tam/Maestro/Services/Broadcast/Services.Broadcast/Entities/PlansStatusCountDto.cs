using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// The count of Plans with the specified status.
    /// </summary>
    public class PlansStatusCountDto
    {
        /// <summary>
        /// The status.
        /// </summary>
        public PlanStatusEnum PlanStatus { get; set; }

        /// <summary>
        /// The plans count.
        /// </summary>
        public int Count { get; set; }
    }
}
