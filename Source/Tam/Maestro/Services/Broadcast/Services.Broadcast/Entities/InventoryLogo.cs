using System;

namespace Services.Broadcast.Entities
{
    public class InventoryLogo
    {
        public int Id { get; set; }

        public InventorySource InventorySource { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string FileName { get; set; }

        public byte[] FileContent { get; set; }
    }
}
