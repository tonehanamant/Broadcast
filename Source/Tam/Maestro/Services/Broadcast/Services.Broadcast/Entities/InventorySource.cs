using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class InventorySource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public InventorySourceTypeEnum InventoryType { get; set; }
    }
}
