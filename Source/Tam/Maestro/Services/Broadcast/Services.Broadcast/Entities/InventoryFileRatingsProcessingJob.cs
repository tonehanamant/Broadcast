using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InventoryFileRatingsProcessingJob
    {
        public int Id { get; set; }
        public int InventoryFileId { get; set; }
        public BackgroundJobProcessingStatus Status { get; set; }
        public DateTime QueuedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<Note> Notes { get; set; } = new List<Note>();

        public class Note
        {
            public string Text { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
