using Services.Broadcast.Entities.DTO.SpotExceptionsApi;

namespace Services.Broadcast.Entities.DTO
{
    public class SpotExceptionsIngestTriggerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public SpotExceptionsIngestTriggerRequest Request { get; set; }
        public IngestApiResponse Response { get; set; }
    }
}
