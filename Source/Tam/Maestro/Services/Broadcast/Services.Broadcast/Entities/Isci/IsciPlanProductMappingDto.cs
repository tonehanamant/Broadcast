using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanProductMappingDto
    {
        public IsciPlanProductMappingDto()
        {
            IsciProductMappings = new List<IsciProductMappingDto>();
            IsciPlanMappings = new List<IsciPlanMappingDto>();
        }
        public List<IsciProductMappingDto> IsciProductMappings { get; set; }
        public List<IsciPlanMappingDto> IsciPlanMappings { get; set; }
        
    }
}
