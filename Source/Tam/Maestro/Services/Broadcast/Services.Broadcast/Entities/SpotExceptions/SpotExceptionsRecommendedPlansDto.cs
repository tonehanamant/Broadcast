using System;
namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlansDto
    {
		public int Id { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }
        public int? RecommendedPlanId { get; set; }
        public string ProgramName { get; set; }
        public DateTime ProgramAirTime { get; set; }
        public string StationLegacyCallLetters { get; set; }
        public decimal? Cost { get; set; }
        public double? Impressions { get; set; }
        public int? SpotLenthId { get; set; }
        public int? AudienceId { get; set; }
        public string Product { get; set; }
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public int? DaypartId { get; set; }
        public string IngestedBy { get; set; }
        public DateTime IngestedAt { get; set; }
    }
}
		
