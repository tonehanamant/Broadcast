using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class MatchingDetail
    {
        public ScrubbingFileDetail FileDetail { get; set; }
        public bool Archived { get; set; }
        public List<MatchingProposalWeek> ProposalDetailWeeks { get; set; } = new List<MatchingProposalWeek>();
        public List<FileDetailProblem> FileDetailProblems { get; set; } = new List<FileDetailProblem>();
        public string EffectiveIsci { get; set; }
    }
}
