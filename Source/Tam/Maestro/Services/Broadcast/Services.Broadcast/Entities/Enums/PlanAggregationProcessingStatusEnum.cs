namespace Services.Broadcast.Entities.Enums
{
    /// <summary>
    /// Indicates the status of processing.
    /// </summary>
    public enum PlanAggregationProcessingStatusEnum
    {
        /// <summary>
        /// The status is unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The process is idle.
        /// </summary>
        Idle = 1,

        /// <summary>
        /// The process is in progress.
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// The process is in error.
        /// </summary>
        Error = 3
    }
}