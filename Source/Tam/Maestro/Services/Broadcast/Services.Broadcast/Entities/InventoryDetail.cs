using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InventoryDetail
    {
        public int Id { get; set; }
        public RatesFile.RateSourceType InventorySource { get; set; }
        public string DaypartCode { get; set; }

        public List<InventoryDetailSlot> InventoryDetailSlots { get; set; }
    }

}
