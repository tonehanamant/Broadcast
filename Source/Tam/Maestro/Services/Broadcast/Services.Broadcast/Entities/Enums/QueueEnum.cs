using System.ComponentModel.DataAnnotations;

namespace Services.Broadcast.Entities.Enums
{
    // Order is important because jobs are prioritized by the order of the queues
    // queue names must be lowercase when handed to hangfire
    public enum QueueEnum
    {
        [Display(Name = "campaignaggregation")]
        CampaignAggregation,
        [Display(Name = "planstatustransition")]
        PlanStatusTransition,
        [Display(Name = "inventoryrating")]
        InventoryRating,
        [Display(Name = "inventorysummaryaggregation")]
        InventorySummaryAggregation,
        [Display(Name = "inventoryprogramenrichment")]
        InventoryProgramEnrichment,
        [Display(Name = "scxfilegeneration")]
        ScxFileGeneration,
        [Display(Name = "default")]
        Default
    }
}
