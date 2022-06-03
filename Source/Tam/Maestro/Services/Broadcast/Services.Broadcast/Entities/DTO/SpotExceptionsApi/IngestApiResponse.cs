namespace Services.Broadcast.Entities.DTO.SpotExceptionsApi
{
    public class IngestApiResponse
    {
        public int JobId { get; set; }
        public bool SkipClearStaged { get; set; }
        public bool SkipIngestAndStaged { get; set; }

        public IngestResultCounts ReceivedCounts { get; set; } = new IngestResultCounts();
        public IngestResultCounts StagedCounts { get; set; } = new IngestResultCounts();
        public IngestResultCounts ProcessedCounts { get; set; } = new IngestResultCounts();
    }
}
