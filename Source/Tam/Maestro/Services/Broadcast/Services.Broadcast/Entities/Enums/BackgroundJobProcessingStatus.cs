namespace Services.Broadcast.Entities.Enums
{
    public enum BackgroundJobProcessingStatus
    {
        Queued = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4,
        Canceled = 5
    }
}
