using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class StationProgramConflictRequest
    {
        public int StationCode { get; set; }
        public string RateSource { get; set; }
        public DaypartDto Airtime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ConflictedProgramNewStartDate { get; set; }
        public DateTime ConflictedProgramNewEndDate { get; set; }
    }
}
