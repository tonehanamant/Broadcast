using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class SearchIsciRequestDto
    {
        public int SourcePlanId { get; set; }
        public string SearchText { get; set; }
    }
}
