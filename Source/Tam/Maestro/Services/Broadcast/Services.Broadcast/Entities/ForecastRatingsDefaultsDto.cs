using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class ForecastRatingsDefaultsDto
    {
        public List<LookupDto> PlaybackTypes { get; set; }
        public List<LookupDto> CrunchedMonths { get; set; }
    }
}
