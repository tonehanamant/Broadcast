using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using System;

namespace Services.Broadcast.Entities
{
    public class ScxOpenMarketsGenerationJob
    {
        public int Id { get; set; }
        public InventoryScxOpenMarketsDownloadRequest InventoryScxOpenMarketsDownloadRequest { get; set; }
        public BackgroundJobProcessingStatus Status { get; set; }
        public DateTime QueuedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string RequestedBy { get; set; }

        public void Complete(DateTime currentDate)
        {
            Status = BackgroundJobProcessingStatus.Succeeded;
            CompletedAt = currentDate;
        }
    }
}
