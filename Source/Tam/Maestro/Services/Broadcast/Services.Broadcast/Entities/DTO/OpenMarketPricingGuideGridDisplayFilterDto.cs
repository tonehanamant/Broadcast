using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.DTO
{
    public class OpenMarketPricingGuideGridDisplayFilterDto
    {
        public OpenMarketPricingGuideGridDisplayFilterDto()
        {
            ProgramNames = new List<string>();
            Affiliations = new List<string>();
            Markets = new List<LookupDto>();
        }

        public List<string> ProgramNames { get; set; }

        public List<string> Affiliations { get; set; }

        public List<LookupDto> Markets { get; set; }
    }
}
