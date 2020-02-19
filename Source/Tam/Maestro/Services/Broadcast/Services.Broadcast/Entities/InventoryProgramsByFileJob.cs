using System;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class InventoryProgramsByFileJob
    {
        public int Id { get; set; }

        public int InventoryFileId { get; set; }

        public InventoryProgramsJobStatus Status { get; set; }

        public string StatusMessage { get; set; }

        public DateTime QueuedAt { get; set; }

        public string QueuedBy { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}