using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class StationProgram
    {
        public StationProgram()
        {
            FlightWeeks = new List<FlightWeekDto>();
        }

        public short StationCode { get; set; }
        public int SpotLength { get; set; }
        public int SpotsPerWeek { get; set; }
        public string ProgramName { get; set; }
        public string Airtime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double Rate15 { get; set; }
        public double Rate30 { get; set; }
        public double HouseHoldImpressions { get; set; }
        public double Rating { get; set; }
        public List<FlightWeekDto> FlightWeeks { get; set; }
    }
}
