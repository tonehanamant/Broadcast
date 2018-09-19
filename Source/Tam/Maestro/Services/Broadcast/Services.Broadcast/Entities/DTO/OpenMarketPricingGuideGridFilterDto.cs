using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class OpenMarketPricingGuideGridFilterDto
    {
        public OpenMarketPricingGuideGridFilterDto()
        {
            ProgramNames = new List<string>();
            Affiliations = new List<string>();
            Markets = new List<int>();
            Genres = new List<int>();
        }

        public List<string> ProgramNames { get; set; }

        public List<string> Affiliations { get; set; }

        public List<int> Markets { get; set; }

        public List<int> Genres { get; set; }
    }
}
