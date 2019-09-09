using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// A plan count for the status.
    /// </summary>
    public class PlanStatusesDto
    {
        /// <summary>
        /// The status.
        /// </summary>
        public PlanStatusEnum Status { get; set; }

        /// <summary>
        /// The count.
        /// </summary>
        public int Count { get; set; }
    }
}