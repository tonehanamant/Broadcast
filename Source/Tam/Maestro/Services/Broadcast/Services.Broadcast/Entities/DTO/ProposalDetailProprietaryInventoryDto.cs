using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class ProposalDetailProprietaryInventoryDto : ProposalDetailInventoryBase
    {
        public ProposalDetailProprietaryInventoryDto()
        {
            Weeks = new List<ProposalProprietaryInventoryWeekDto>();
            Dayparts = new List<ProposalInventoryDaypartDto>();
        }

        public List<ProposalProprietaryInventoryWeekDto> Weeks { get; set; }
        public List<ProposalInventoryDaypartDto> Dayparts { get; set; }
    }
}
