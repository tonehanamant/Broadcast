using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlansResultsDto
    {
        public SpotExceptionsRecommendedPlansResultsDto()
        {
            Active = new List<SpotExceptionsRecommandedToDoPlansDto>();
            Completed = new List<SpotExceptionsRecommandedCompletedPlansDto>();
        }
        public List<SpotExceptionsRecommandedToDoPlansDto> Active { get; set; }
        public List<SpotExceptionsRecommandedCompletedPlansDto> Completed { get; set; }
    }

    public class SpotExceptionsRecommandedToDoPlansDto
    {
        public int? PlanId { get; set; }
        public string AdvertiserName { get; set; }
        public string PlanName { get; set; }
        public int AffectedSpotsCount { get; set; }
        public double? Impressions { get; set; }
        public string SpotLengthString { get; set; }
        public string Pacing { get; set; }
        public string AudienceName { get; set; }
        public string FlightString { get; set; }
    }

    public class SpotExceptionsRecommandedCompletedPlansDto
    {
        public int? PlanId { get; set; }
        public string AdvertiserName { get; set; }
        public string PlanName { get; set; }
        public int AffectedSpotsCount { get; set; }
        public double? Impressions { get; set; }
        public string SpotLengthString { get; set; }
        public string Pacing { get; set; }
        public string AudienceName { get; set; }
        public string FlightString { get; set; }
    }
}
