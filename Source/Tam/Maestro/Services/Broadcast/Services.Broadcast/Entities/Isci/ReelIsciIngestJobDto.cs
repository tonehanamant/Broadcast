using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
