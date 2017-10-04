using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class InventoryFileValidatorResult
    {
        public InventoryFileValidatorResult()
        {
            InventoryFileProblems = new List<InventoryFileProblem>();
        }

        public List<InventoryFileProblem> InventoryFileProblems { get; set; }
    }
}
