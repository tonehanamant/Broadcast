using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;


namespace Services.Broadcast.Entities
{
    public class InventoryFileSaveResult
    {
        public int FileId { get; set; }
        public List<string> ValidationProblems { get; set; }
        public FileStatusEnum Status { get; set; }
    }
}
