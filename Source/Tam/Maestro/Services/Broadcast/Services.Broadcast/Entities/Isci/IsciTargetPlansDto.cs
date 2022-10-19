using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciTargetPlansDto
    {
        public IsciTargetPlansDto()
        {
            Plans = new List<TargetPlans>();
        }
        public List<TargetPlans> Plans { get; set; }
    }

    public class TargetPlans
    {
        public int Id { get; set; }
        public string SpotLengthString { get; set; }
        public string DemoString { get; set; }
        public string Title { get; set; }
        public string DaypartsString { get; set; }
        public string FlightString { get; set; }
    }
}
