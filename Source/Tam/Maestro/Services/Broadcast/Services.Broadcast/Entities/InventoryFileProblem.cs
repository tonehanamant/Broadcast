using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Services.Broadcast.Entities
{

    public class InventoryFileProblem
    {
        [IgnoreDataMember]
        public bool IsWarning { get; set; }
        public string StationLetters { get; set; }
        public string ProgramName { get; set; }
        public string ProblemDescription { get; set; }
        public List<string> AffectedProposals { get; set; }

        public InventoryFileProblem() { }

        public InventoryFileProblem(string description)
        {
            AffectedProposals = null;
            ProblemDescription = description;
            ProgramName = null;
            StationLetters = null;
        }

        public void AddWarning(string description)
        {
            ProblemDescription = description;
            IsWarning = true;
        }
    }
}
