using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalInventoryWeekDaypartDto
    {
        public int Id { get; set; }
        public string InventorySource { get; set; }
        public List<ProposalInventoryWeekDaypartDetailDto> Details { get; set; }
    }
}
