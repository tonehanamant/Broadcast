namespace Services.Broadcast.Entities
{
    public class ProposalTargets
    {
        public decimal TargetBudget { get; set; }
        public double? TargetImpressions { get; set; }
        public int TargetUnits { get; set; }
        public decimal? TargetCPM { get; set; }
    }
}
