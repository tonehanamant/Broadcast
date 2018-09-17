using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class OpenMarketPricingGuideGridFilterDto
    {
        public OpenMarketPricingGuideGridFilterDto()
        {
            ProgramNames = new List<string>();
        }

        public List<string> ProgramNames { get; set; }
    }
}
