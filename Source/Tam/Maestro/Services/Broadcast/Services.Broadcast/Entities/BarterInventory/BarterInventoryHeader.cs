using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.BarterInventory
{
    public class BarterInventoryHeader
    {
        public string DaypartCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Cpm { get; set; }
        public BroadcastAudience Audience { get; set; }
        public int ContractedDaypartId { get; set; }
        public int ShareBookId { get; set; }
        public int? HutBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType PlaybackType { get; set; }
        public int? SpotLengthId { get; set; }
        public decimal? NtiToNsiIncrease { get; set; }
    }
}
