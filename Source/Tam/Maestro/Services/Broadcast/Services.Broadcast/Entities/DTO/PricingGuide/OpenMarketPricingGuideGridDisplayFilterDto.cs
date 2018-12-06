using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class OpenMarketPricingGuideGridDisplayFilterDto
    {
        public List<string> ProgramNames { get; set; } = new List<string>();

        public List<string> Affiliations { get; set; } = new List<string>();

        public List<LookupDto> Markets { get; set; } = new List<LookupDto>();

        public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
    }
}
