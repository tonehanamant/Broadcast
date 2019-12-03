using System.ComponentModel.DataAnnotations;

namespace Services.Broadcast.Entities.Enums
{
    public enum QueueEnum
    {
        [Display(Name = "planpricing")]
        PlanPricing,
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
