using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanAudienceDto
    {
        public AudienceTypeEnum Type { get; set; }
        public int AudienceId { get; set; }
        public double Vpvh { get; set; }
        public double? RatingPoints { get; set; }
        public double? Impressions { get; set; }
        public decimal? CPP { get; set; }
        public decimal? CPM { get; set; }
        public double Universe { get; set; }
    }
}
