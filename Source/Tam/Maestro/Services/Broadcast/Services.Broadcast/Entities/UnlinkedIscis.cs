namespace Services.Broadcast.Entities
{
    public class UnlinkedIscis
    {
        public string ISCI { get; set; }
        public int SpotLengthId { get; set; }
        public int Count { get; set; }
        public AffidavitFileDetailProblemTypeEnum ProblemType { get; set; }
        public string UnlinkedReason { get; set; }
        public int SpotLength { get; set; }
    }
}
