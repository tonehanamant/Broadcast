using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanResultDto
    {
        public string AdvertiserName { get; set; }
        public List<IsciPlanDto> IsciPlans { get; set; }
    }
}
