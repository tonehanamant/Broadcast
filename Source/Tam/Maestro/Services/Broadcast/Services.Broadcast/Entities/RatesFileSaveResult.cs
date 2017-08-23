using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class RatesFileSaveResult
    {
        public int FileId { get; set; }
        public List<RatesFileProblem> Problems { get; set; } 
    }
}
