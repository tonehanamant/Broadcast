namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsRecommendedPlansResultDto
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }
        public string RecommendedPlan { get; set; }
        public string Affiliate { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public decimal Cost { get; set; }
        public double Impressions { get; set; }
        public string SpotLengthString { get; set; }
        public string AudienceName { get; set; }
        public string ProductName { get; set; }
        public string ProgramName { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
    }
}
