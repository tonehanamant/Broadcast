using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InventoryProgramsBySourceJobEnqueueResultDto
    {
        public List<InventoryProgramsBySourceJob> Jobs { get; set; } = new List<InventoryProgramsBySourceJob>();
    }
}