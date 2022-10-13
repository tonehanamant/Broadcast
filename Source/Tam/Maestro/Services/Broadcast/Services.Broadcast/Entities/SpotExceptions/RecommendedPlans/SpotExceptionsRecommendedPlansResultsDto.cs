using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    public class SpotExceptionsRecommendedPlansResultsDto
    {
        public SpotExceptionsRecommendedPlansResultsDto()
        {
            Active = new List<SpotExceptionsRecommendedToDoPlansDto>();
            Completed = new List<SpotExceptionsRecommendedDonePlansDto>();
        }
        public List<SpotExceptionsRecommendedToDoPlansDto> Active { get; set; }
        public List<SpotExceptionsRecommendedDonePlansDto> Completed { get; set; }
    }

    public class SpotExceptionsRecommendedToDoPlansDto
    {
        public int? PlanId { get; set; }
        public string AdvertiserName { get; set; }
        public string PlanName { get; set; }
        public int AffectedSpotsCount { get; set; }
        public double? Impressions { get; set; }
        public string SpotLengthString { get; set; }
        public string AudienceName { get; set; }
        public string FlightString { get; set; }
    }

    public class SpotExceptionsRecommendedDonePlansDto
    {
        public int? PlanId { get; set; }
        public string AdvertiserName { get; set; }
        public string PlanName { get; set; }
        public int AffectedSpotsCount { get; set; }
        public double? Impressions { get; set; }
        public string SpotLengthString { get; set; }
        public string AudienceName { get; set; }
        public string FlightString { get; set; }
    }
}
