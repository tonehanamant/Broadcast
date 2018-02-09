﻿using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class PostScrubbingProposalHeaderDTO
    {        
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Advertiser { get; set; }
                
        public List<ProposalMarketDto> Markets { get; set; } = new List<ProposalMarketDto>();

        public List<string> SecondaryDemos { get; set; } = new List<string>();
        
        public string Notes { get; set; }
        
        public string GuaranteedDemo { get; set; }

        public List<ProposalDetailDto> Details { get; set; }

        public List<LookupDto> SpotLengths { get; set; }

    }
}
