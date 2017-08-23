using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalInventoryDaypartDto
    {
        public int Id { get; set; }
        public string InventorySource { get; set; }
        public List<ProposalInventoryDaypartDetailDto> Details { get; set; }

        public ProposalInventoryDaypartDto(int id, string inventorySource)
        {
            Id = id;
            InventorySource = inventorySource;
            Details = new List<ProposalInventoryDaypartDetailDto>();
        }
    }
}
