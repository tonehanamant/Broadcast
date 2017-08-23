using System;

namespace Services.Broadcast.Entities
{
    public class ProposalVersion
    {
        public int Id { get; set; }
        public int ProposalId { get; set; }
        public short VersionNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int AdvertiserId { get; set; }
        public int? GuaranteedAudienceId { get; set; }
        public ProposalEnums.ProposalMarketGroups? Markets { get; set; }
        public int? SweepMonthId { get; set; }
        public decimal? Budget { get; set; }
        public int? TargetUnits { get; set; }
        public double? TargetImpressions { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string Notes { get; set; }
        public bool Primary { get; set; }
        public ProposalEnums.ProposalStatusType Status { get; set; }

        public string Owner
        {
            get { return LastModifiedBy; }
        }

    }
}