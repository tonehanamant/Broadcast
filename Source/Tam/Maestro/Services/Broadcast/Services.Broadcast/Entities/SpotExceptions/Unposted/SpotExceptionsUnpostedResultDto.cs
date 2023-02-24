using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions.Unposted
{
    public class SpotExceptionsUnpostedResultDto
    {
        public SpotExceptionsUnpostedResultDto()
        {
            NoPlan = new List<SpotExceptionOutOfSpecNoPlanDto>();
            NoReelRoster = new List<SpotExceptionOutOfSpecNoReelRosterDto>();
        }
        public List<SpotExceptionOutOfSpecNoPlanDto> NoPlan { get; set; }
        public List<SpotExceptionOutOfSpecNoReelRosterDto> NoReelRoster { get; set; }

    }

    public class SpotExceptionOutOfSpecNoPlanDto
    {
        public string HouseIsci { get; set; }
        public string ClientIsci { get; set; }
        public string ClientSpotLength { get; set; }
        public int AffectedSpotsCount { get; set; }
        public string ProgramAirDate { get; set; }
        public long? EstimateId { get; set; }
    }

    public class SpotExceptionOutOfSpecNoReelRosterDto
    {
        public string HouseIsci { get; set; }
        public int AffectedSpotsCount { get; set; }
        public string ProgramAirDate { get; set; }
        public long EstimateId { get; set; }
    }
}
