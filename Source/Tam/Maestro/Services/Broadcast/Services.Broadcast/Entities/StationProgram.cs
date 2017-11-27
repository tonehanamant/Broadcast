using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class StationProgram
    {
        public int StationCode { get; set; }
        public int SpotLength { get; set; }
        public int? SpotsPerWeek { get; set; }
        public string ProgramName { get; set; }
        public string Airtime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Rate15 { get; set; }
        public decimal Rate30 { get; set; }
        public double? HouseHoldImpressions { get; set; }
        public double? Rating { get; set; }
    }
}
