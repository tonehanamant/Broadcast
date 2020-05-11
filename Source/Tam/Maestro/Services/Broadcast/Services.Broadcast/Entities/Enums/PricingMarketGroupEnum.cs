using System.ComponentModel;

namespace Services.Broadcast.Entities.Enums
{
    public enum PricingMarketGroupEnum
    {
        [Description("Top 100 Markets")]
        Top100 = 1,
        [Description("Top 50 Markets")]
        Top50 = 2,
        [Description("Top 25 Markets")]
        Top25 = 3,
        [Description("All")]
        All = 4,
        [Description("None")]
        None = 5
    }
}
