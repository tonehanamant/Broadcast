using System.ComponentModel;

namespace Services.Broadcast.Entities
{
    public class ProposalEnums
    {
        public enum ProposalMarketGroups
        {
            [Description("All")]
            OldAll = 0,
            [Description("All")]
            All = 1,
            [Description("Top 50")]
            Top50 = 50,
            [Description("Top 100")]
            Top100 = 100,
            [Description("All")]
            Custom = 255
        }

        public enum ProposalStatusType
        {
            [Description("Proposed")]
            Proposed = 1,

            [Description("Agency on Hold")]
            AgencyOnHold = 2,

            [Description("Contracted")]
            Contracted = 3,

            [Description("Previously Contracted")]
            PreviouslyContracted = 4
        }

        public enum ProposalPlaybackType
        {
            [Description("Live")]
            Live = 1,

            [Description("Live+Same Day")]
            LiveSameDay = 2,

            [Description("Live+1")]
            LivePlus1 = 3,

            [Description("Live+3")]
            LivePlus3 = 4,

            [Description("Live+7")]
            LivePlus7 = 5
        }
    }
}
