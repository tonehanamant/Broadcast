using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.PricingGuide;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PricingGuide : PricingGuideDto
    {
        public bool KeepManuallyEditedSpots { get; set; }

        public Dictionary<int, int> ProgramsWithManuallyEditedSpots { get; set; } = new Dictionary<int, int>();

        public List<PricingGuideMarketDto> MarketsWithManuallyEditedSpots { get; set; } = new List<PricingGuideMarketDto>();
    }
}
