using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Broadcast.Entities
{
    public class InventoryProgramsBySourceJobEnqueueResultDto
    {
        public Guid? JobGroupId { get; set; }

        public List<InventoryProgramsBySourceJob> Jobs { get; set; } = new List<InventoryProgramsBySourceJob>();

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{Jobs.Count} Jobs :");

            foreach (var job in Jobs)
            {
                sb.AppendLine("****************************************");
                sb.AppendLine($"Job ID : {job.Id}");
                sb.AppendLine($"Job Group ID : {JobGroupId?.ToString() ?? "null"}");
                sb.AppendLine($"InventorySourceId : {job.InventorySourceId}");
                sb.AppendLine($"StartDate : {job.StartDate}");
                sb.AppendLine($"EndDate : {job.EndDate}");
                sb.AppendLine($"Status : {job.Status}");
                sb.AppendLine($"QueuedAt : {job.QueuedAt}");
                sb.AppendLine($"QueuedBy : {job.QueuedBy}");
                sb.AppendLine($"CompletedAt : {job.CompletedAt}");
                sb.AppendLine("****************************************");
            }

            return sb.ToString();
        }
    }
}