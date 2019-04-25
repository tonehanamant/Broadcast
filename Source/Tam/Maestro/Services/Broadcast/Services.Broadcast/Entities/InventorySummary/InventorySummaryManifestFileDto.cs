using Services.Broadcast.ApplicationServices;
using System;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryManifestFileDto
    {
        public DateTime? LastCompletedDate { get; set; }
        public int? HutProjectionBookId { get; set; }
        public int? ShareProjectionBookId { get; set; }
        public InventoryFileRatingsProcessingStatus JobStatus { get; set; }
    }
}
