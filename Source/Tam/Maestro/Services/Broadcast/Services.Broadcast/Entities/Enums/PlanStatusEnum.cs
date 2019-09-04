using System.ComponentModel;

namespace Services.Broadcast.Entities.Enums
{
    public enum PlanStatusEnum
    {
        [Description("Working")]
        Working = 1,
        [Description("Reserved")]
        Reserved = 2,
        [Description("Client Approval")]
        ClientApproval = 3,
        [Description("Contracted")]
        Contracted = 4,
        [Description("Live")]
        Live = 5,
        [Description("Complete")]
        Complete = 6,         
    }
}
