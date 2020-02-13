using System.ComponentModel;

namespace Services.Broadcast.Entities.Enums
{
    public enum PlanBuyingTimeFramesEnum
    {
        [Description("All")]
        All = 1,
        [Description("Within next 4 weeks")]
        NextFourWeeks = 2,
    }
}
