using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideStationDto
    {
        public int StationCode { get; set; }
        public string CallLetters { get; set; }
        public string LegacyCallLetters { get; set; }
        public string Affiliation { get; set; }
        public List<PricingGuideProgramDto> Programs { get; set; } = new List<PricingGuideProgramDto>();

        public decimal MinProgramsBlendedCpm => Programs.Any() ? Programs.Min(p => p.BlendedCpm) : 0;
    }
}
