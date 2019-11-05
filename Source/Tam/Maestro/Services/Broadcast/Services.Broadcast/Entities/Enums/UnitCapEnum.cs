using System.ComponentModel;

namespace Services.Broadcast.Entities.Enums
{
    public enum UnitCapEnum
    {
        [Description("Per 30 Min")]
        Per30Min = 1,
        [Description("Per Hour")]
        PerHour = 2,
        [Description("Per Day")]
        PerDay = 3,
        [Description("Per Week")]
        PerWeek = 4,
        [Description("Per Month")]
        PerMonth = 5
    }
}
