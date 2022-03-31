using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class RatingsForecastResponseDto
    {
        public double ProjectedImpressions { get; set; }

        public List<StationImpressionsWithAudience> NielsenAudienceImpressions { get; set; }
    }
}
