using Services.Broadcast.ApplicationServices;
using System;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryManifestFileDto
    {
        public DateTime? JobCompletedDate { get; set; }
        public int? HutProjectionBookId { get; set; }
        public int? ShareProjectionBookId { get; set; }
        public InventoryFileRatingsProcessingStatus? JobStatus { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
