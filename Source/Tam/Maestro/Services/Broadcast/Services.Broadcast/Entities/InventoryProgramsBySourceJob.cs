using System;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class InventoryProgramsBySourceJob
    {
        public int Id { get; set; }

        public Guid? JobGroupId { get; set; }

        public int InventorySourceId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public InventoryProgramsJobStatus Status { get; set; }

        public string StatusMessage { get; set; }

        public DateTime QueuedAt { get; set; }

        public string QueuedBy { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}