namespace Services.Broadcast.Entities
{
    public enum AffidavitFileDetailProblemTypeEnum
    {
        UnlinkedIsci = 1,
        UnmarriedOnMultipleContracts = 2,
        ArchivedIsci = 3,
        MarriedAndUnmarried =4,
        UnmatchedSpotLength = 5
    }

    public class AffidavitFileDetailProblem
    {
        public AffidavitFileDetailProblemTypeEnum Type { get; set; }
        public string Description { get; set; }
        public long DetailId { get; set; }
    }
}
