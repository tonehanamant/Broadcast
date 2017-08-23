using System.Collections.Generic;

namespace Services.Broadcast.Entities
{

    public class RatesFileProblem
    {
        public string StationLetters { get; set; }
        public string ProgramName { get; set; }
        public string ProblemDescription { get; set; }
        public List<string> AffectedProposals { get; set; } 
    }
}
