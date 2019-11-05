using System;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class InventoryFileProgramEnrichmentJob
    {
        public int Id { get; set; }

        public int InventoryFileId { get; set; }

        public InventoryFileProgramEnrichmentJobStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime QueuedAt { get; set; }

        public string QueuedBy { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}