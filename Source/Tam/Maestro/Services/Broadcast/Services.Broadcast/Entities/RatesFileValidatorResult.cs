using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class RatesFileValidatorResult
    {
        public RatesFileValidatorResult()
        {
            RatesFileProblems = new List<RatesFileProblem>();
//            InvalidRates = new List<StationProgram>();
        }

        public List<RatesFileProblem> RatesFileProblems { get; set; }
    }
}
