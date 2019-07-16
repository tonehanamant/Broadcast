using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventorySummaryManifestFileDto
    {
        public int FileId { get; set; }
        public DateTime? JobCompletedDate { get; set; }
        public int? HutProjectionBookId { get; set; }
        public int? ShareProjectionBookId { get; set; }
        public BackgroundJobProcessingStatus? JobStatus { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
