using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlansDto
    {
        public SpotExceptionsRecommendedPlansDto()
        {
            SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>();
        }
        public int Id { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }
        public int? RecommendedPlanId { get; set; }
        public string RecommendedPlanName { get; set; }
        public string ProgramName { get; set; }
        public DateTime ProgramAirTime { get; set; }
        public string StationLegacyCallLetters { get; set; }
        public string Affiliate { get; set; }
        public string Market { get; set; }
        public decimal? Cost { get; set; }
        public double? Impressions { get; set; }
        public int? SpotLengthId { get; set; }
        public string SpotLengthString { get; set; }
        public int? AudienceId { get; set; }
        public string AudienceName { get; set; }
        public string Product { get; set; }
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public int? DaypartId { get; set; }
        public string IngestedBy { get; set; }
        public DateTime IngestedAt { get; set; }
        public List<SpotExceptionsRecommendedPlanDetailsDto> SpotExceptionsRecommendedPlanDetails { get; set; }
        public int? SpotExceptionsRecommendedPlanDecisionId { get; set; }
    }
}
		
