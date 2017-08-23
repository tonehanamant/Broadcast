using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class ProposalLoadDto
    {
        public List<LookupDto> Advertisers { get; set; }
        public List<LookupDto> Audiences { get; set; }
        public int SweepMonthId { get; set; }
        public List<LookupDto> SpotLengths { get; set; }
        public List<LookupDto> Markets { get; set; } 
        public List<MarketGroupDto> MarketGroups { get; set; }
        public List<LookupDto> SchedulePostTypes { get; set; }
        public List<LookupDto> Statuses { get; set; }
        public ForecastRatingsDefaultsDto ForecastDefaults { get; set; }
    }
}
