using System.Collections.Generic;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalDetailOpenMarketInventoryDto : ProposalDetailInventoryBase
    {
        public ProposalDetailOpenMarketInventoryDto()
        {
            Markets = new List<ProposalInventoryMarketDto>();
            Weeks = new List<ProposalOpenMarketInventoryWeekDto>();
            DisplayFilter = new ProposalOpenMarketDisplayFilter();
            Filter = new ProposalOpenMarketFilter();
            RefineFilterPrograms = new List<string>();
        }

        public List<ProposalInventoryMarketDto> Markets { get; set; }
        public OpenMarketCriterion Criteria { get; set; }
        public List<ProposalOpenMarketInventoryWeekDto> Weeks { get; set; }
        public ProposalOpenMarketDisplayFilter DisplayFilter { get; set; }
        public ProposalOpenMarketFilter Filter { get; set; }
        public List<string> RefineFilterPrograms { get; set; }
        public bool NewCriteriaAffectsExistingAllocations { get; set; }
    }
}
