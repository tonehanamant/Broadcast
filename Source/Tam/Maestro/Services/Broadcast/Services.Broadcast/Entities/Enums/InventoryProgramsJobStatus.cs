namespace Services.Broadcast.Entities.Enums
{
    /// <summary>
    /// Step \ status for the job.
    /// </summary>
    public enum InventoryProgramsJobStatus
    {
        /// <summary>
        /// The job has been queued for processing.
        /// </summary>
        Queued = 1,

        /// <summary>
        /// Working on the "Gather the Inventory" step.
        /// </summary>
        GatherInventory = 2,

        /// <summary>
        /// Working on the "Call The Api" step.
        /// </summary>
        CallApi = 3,

        /// <summary>
        /// Working on the "Apply The Program Data" step.
        /// </summary>
        ApplyProgramData = 4,

        /// <summary>
        /// Working on the "Apply The Program Data" step.
        /// </summary>
        SavePrograms = 5,

        /// <summary>
        /// The job completed successfully.
        /// </summary>
        Completed = 6,

        /// <summary>
        /// The job failed with an error.
        /// </summary>
        Error = 7,

        /// <summary>
        /// The job completed successfully with a warning.
        /// </summary>
        Warning = 8
    }
}