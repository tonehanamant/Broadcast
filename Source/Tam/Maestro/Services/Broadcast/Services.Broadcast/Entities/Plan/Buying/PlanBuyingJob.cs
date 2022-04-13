using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingJob
    {
        public int Id { get; set; }
        public string HangfireJobId { get; set; }
        public int? PlanVersionId { get; set; }
        public BackgroundJobProcessingStatus Status { get; set; }
        public DateTime Queued { get; set; }
        public DateTime? Completed { get; set; }
        public string ErrorMessage { get; set; }
        public string DiagnosticResult { get; set; }
        public string InventoryRawFile { get; set; }
    }
}
