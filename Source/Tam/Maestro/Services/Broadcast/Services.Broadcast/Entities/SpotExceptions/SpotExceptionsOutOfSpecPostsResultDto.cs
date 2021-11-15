using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecPostsResultDto
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }
        public string RecommendedPlan { get; set; }
        public string Reason { get; set; }
        public string Station { get; set; }
        public string Market { get; set; }
        public string SpotLengthString { get; set; }
        public string AudienceName { get; set; }
        public string ProductName { get; set; }
        public string AdvertiserName { get; set; }
        public string DaypartCode { get; set; }
        public string FlightStartDate { get; set; }
        public string FlightEndDate { get; set; }
        public string FlightDateString { get; set; }
        public string ProgramName { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
    }
}
