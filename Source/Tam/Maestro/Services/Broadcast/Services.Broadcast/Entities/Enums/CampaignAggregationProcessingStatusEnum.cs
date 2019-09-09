namespace Services.Broadcast.Entities.Enums
{
    /// <summary>
    /// Indicates the status of processing.
    /// </summary>
    public enum CampaignAggregationProcessingStatusEnum
    {
        /// <summary>
        /// The process has completed.
        /// </summary>
        Completed = 1,

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