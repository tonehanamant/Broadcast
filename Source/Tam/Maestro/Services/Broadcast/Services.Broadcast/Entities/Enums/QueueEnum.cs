using System.ComponentModel.DataAnnotations;

namespace Services.Broadcast.Entities.Enums
{
    //Order is important because jobs are prioritized by the order of the queues
    public enum QueueEnum
    {
        [Display(Name = "campaign_aggregation")]
        CampaignAggregation,
        [Display(Name = "default")]
        Default
    }
}
