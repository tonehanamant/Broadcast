using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class ProprietaryPricingDto
    {
        public InventorySourceEnum InventorySource { get; set; }
        public double ImpressionsBalance { get; set; }
        public decimal Cpm { get; set; }
    }
}
