using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class SearchPlanIscisDto
    {
        public SearchPlanIscisDto()
        {
            Iscis = new List<SearchPlan>();
        }
        public List<SearchPlan> Iscis { get; set; }
    }
    public class SearchPlan
    {
        public string Isci { get; set; }
        public List<IsciSearchSpotLengthDto> SpotLengths { get; set; }
    }
    public class IsciSearchSpotLengthDto
    {
        public int Id { get; set; }
        public string Length { get; set; }
    }
}
