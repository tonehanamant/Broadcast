namespace Services.Broadcast.Entities
{
    public class ProposalTotalFields
    {
        public int TotalSpots { get; set; }
        public decimal TotalCost { get; set; }
        public double TotalTargetImpressions { get; set; }

        public decimal TotalTargetCPM { get; set; }
        public float TotalTRP { get; set; }
        public float TotalGRP { get; set; }
        public decimal TotalHHCPM { get; set; }
        public double TotalHHImpressions { get; set; }
        public double TotalAdditionalAudienceImpressions { get; set; }

        public decimal TotalAdditionalAudienceCPM { get; set; }
    }
}
