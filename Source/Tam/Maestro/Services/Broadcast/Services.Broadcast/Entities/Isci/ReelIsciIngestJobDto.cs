using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.Isci
{
    public class ReelIsciIngestJobDto
    {
        public int Id { get; set; }
        public BackgroundJobProcessingStatus Status { get; set; }
        public DateTime QueuedAt { get; set; }
        public string QueuedBy { get; set; }
        public Nullable<DateTime> CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
    }
}
