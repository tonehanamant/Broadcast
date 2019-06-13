using System.Collections.Generic;

namespace Services.Broadcast.Entities
{

    public class InventoryFileProblem
    {
        public string StationLetters { get; set; }
        public string ProgramName { get; set; }
        public string ProblemDescription { get; set; }

        public InventoryFileProblem() { }

        public InventoryFileProblem(string description)
        {
            ProblemDescription = description;
            ProgramName = null;
            StationLetters = null;
        }
    }
}
