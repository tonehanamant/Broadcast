using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class UnlinkedIscis
    {
        public string ISCI { get; set; }
        public int SpotLengthId { get; set; }
        public int Count { get; set; }
        public FileDetailProblemTypeEnum ProblemType { get; set; }
        public string UnlinkedReason { get; set; }
        public int SpotLength { get; set; }
    }
}
