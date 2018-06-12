using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class AffidavitDemographics
    {
        public int AudienceId { get; set; }
        public string Demographic { get; set; }
        public double OvernightRating { get; set; }
        public double OvernightImpressions { get; set; }
    }
}
