using System.Collections.Generic;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class AllocationsChangeRequest
    {
        public List<OpenMarketInventoryAllocation> AllocationsToRemove { get; set; } = new List<OpenMarketInventoryAllocation>();
        public List<OpenMarketInventoryAllocation> AllocationsToAdd { get; set; } = new List<OpenMarketInventoryAllocation>();
    }
}