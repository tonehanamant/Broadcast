using System.Collections.Generic;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalDetailOpenMarketInventoryDto : ProposalDetailInventoryBase
    {
        public List<ProposalInventoryMarketDto> Markets { get; set; } = new List<ProposalInventoryMarketDto>();
        public OpenMarketCriterion Criteria { get; set; }
        public List<ProposalOpenMarketInventoryWeekDto> Weeks { get; set; } = new List<ProposalOpenMarketInventoryWeekDto>();
        public ProposalOpenMarketDisplayFilter DisplayFilter { get; set; } = new ProposalOpenMarketDisplayFilter();
        public ProposalOpenMarketFilter Filter { get; set; } = new ProposalOpenMarketFilter();
        public List<string> RefineFilterPrograms { get; set; } = new List<string>();
        public bool NewCriteriaAffectsExistingAllocations { get; set; }
    }
}
