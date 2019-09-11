using System.ComponentModel.DataAnnotations;

namespace Services.Broadcast.Entities.Enums
{
    //Order is important because jobs are prioritized by the order of the queues
    public enum QueueEnum
    {
        [Display(Name = "campaignaggregation")]
        CampaignAggregation,
        [Display(Name = "inventoryrating")]
        InventoryRatings,
        [Display(Name = "scxfilegeneration")]
        ScxGeneration,
        [Display(Name = "default")]
        Default
    }
}
