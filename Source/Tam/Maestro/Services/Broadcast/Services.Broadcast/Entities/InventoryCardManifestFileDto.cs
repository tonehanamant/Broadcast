using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities
{
    public class InventoryCardManifestFileDto
    {
        public DateTime CreatedDate { get; set; }
        public int? HutProjectionBookId { get; set; }
        public int? ShareProjectionBookId { get; set; }
        public FileStatusEnum Status { get; set; }
    }
}
