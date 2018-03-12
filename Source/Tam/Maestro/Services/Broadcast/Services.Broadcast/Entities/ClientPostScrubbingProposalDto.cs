using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ClientPostScrubbingProposalDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Advertiser { get; set; }

        public List<ProposalMarketDto> Markets { get; set; } = new List<ProposalMarketDto>();

        public MarketGroupDto BlackoutMarketGroup { get; set; }

        public ProposalEnums.ProposalMarketGroups MarketGroupId { get; set; }

        public ProposalEnums.ProposalMarketGroups? BlackoutMarketGroupId { get; set; }

        public List<string> SecondaryDemos { get; set; } = new List<string>();

        public string Notes { get; set; }

        public string GuaranteedDemo { get; set; }

        public List<ClientPostScrubbingProposalDetailDto> Details { get; set; }        
    }
}
