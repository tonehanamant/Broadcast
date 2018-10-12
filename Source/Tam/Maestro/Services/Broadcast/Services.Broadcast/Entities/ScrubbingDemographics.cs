namespace Services.Broadcast.Entities
{
    public class ScrubbingDemographics
    {
        public int AudienceId { get; set; }
        public string Demographic { get; set; }
        public double OvernightRating { get; set; }
        public double OvernightImpressions { get; set; }
    }
}
