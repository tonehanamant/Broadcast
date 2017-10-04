using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InventoryFileSaveResult
    {
        public int FileId { get; set; }
        public List<InventoryFileProblem> Problems { get; set; } 
    }
}
