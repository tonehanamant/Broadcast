using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class CampaignQuartersDto
    {
        public QuarterDetailDto DefaultQuarter { get; set; }
        public List<QuarterDetailDto> Quarters { get; set; } = new List<QuarterDetailDto>();
    }
}
