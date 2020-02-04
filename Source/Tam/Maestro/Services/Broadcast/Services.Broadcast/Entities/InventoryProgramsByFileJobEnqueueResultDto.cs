using System.Text;

namespace Services.Broadcast.Entities
{
    public class InventoryProgramsByFileJobEnqueueResultDto
    {
        public InventoryProgramsByFileJob Job { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Job ID : {Job.Id}");
            sb.AppendLine($"InventoryFileId : {Job.InventoryFileId}");
            sb.AppendLine($"Status : {Job.Status}");
            sb.AppendLine($"QueuedAt : {Job.QueuedAt}");
            sb.AppendLine($"QueuedBy : {Job.QueuedBy}");
            sb.AppendLine($"CompletedAt : {Job.CompletedAt}");

            return sb.ToString();
        }
    }
}