using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlanDetailsResultDto
    {
        public SpotExceptionsRecommendedPlanDetailsResultDto()
        {
            Plans = new List<RecommendedPlanDetailResultDto>();
        }
        public int Id { get; set; }
        public int EstimateId { get; set; }
        public string SpotLengthString { get; set; }
        public string DaypartCode { get; set; }
        public string AudienceName { get; set; }
        public string Product { get; set; }
        public string FlightStartDate { get; set; }
        public string FlightEndDate { get; set; }
        public string FlightDateString { get; set; }
        public string ProgramName { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
        public double? Impressions { get; set; }
        public string InventorySourceName { get; set; }
        public List<RecommendedPlanDetailResultDto> Plans { get; set; }
    }

    public class RecommendedPlanDetailResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SpotLengthString { get; set; }
        public string FlightStartDate { get; set; }
        public string FlightEndDate { get; set; }
        public string FlightDateString { get; set; }
        public bool IsRecommendedPlan { get; set; }        
        public bool IsSelected { get; set; }
        public string Pacing { get; set; }
        public bool? AcceptedAsInSpec { get; set; }
        public int RecommendedPlanId { get; set; }
    }
}
