using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class InventoryFileBase
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public FileStatusEnum FileStatus { get; set; }
        public string Hash { get; set; }
        public InventorySource InventorySource { get; set; }
        public string UniqueIdentifier { get; set; }
    }
}
