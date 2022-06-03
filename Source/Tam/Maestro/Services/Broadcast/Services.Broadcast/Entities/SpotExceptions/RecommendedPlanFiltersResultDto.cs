using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class RecommendedPlanFiltersResultDto
    {
        public RecommendedPlanFiltersResultDto()
        {
            Markets = new List<string>();
            Stations = new List<string>();
            InventorySources = new List<string>();
        }
        public List<string> Markets { get; set; }
        public List<string> Stations { get; set; }
        public List<string> InventorySources { get; set; }
    }
}
