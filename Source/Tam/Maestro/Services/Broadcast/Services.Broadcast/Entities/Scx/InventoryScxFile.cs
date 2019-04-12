using System.IO;

namespace Services.Broadcast.Entities.Scx
{
    public class InventoryScxFile
    {
        public MemoryStream ScxStream { get; set; }
        public string InventorySourceName { get; set; }
        public string UnitName { get; set; }
    }
}
