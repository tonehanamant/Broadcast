using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class OpenMarketPricingGuideGridDisplayFilterDto
    {
        public OpenMarketPricingGuideGridDisplayFilterDto()
        {
            ProgramNames = new List<string>();
        }

        public List<string> ProgramNames { get; set; }
    }
}
