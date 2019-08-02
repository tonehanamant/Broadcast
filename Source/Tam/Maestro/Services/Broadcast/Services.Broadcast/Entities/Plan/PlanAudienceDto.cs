using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanAudienceDto
    {
        public AudienceTypeEnum Type { get; set; }
        public int AudienceId { get; set; }
        public PostingTypeEnum PostingType { get; set; }
        public int ShareBookId { get; set; }
        public int? HutBookId { get; set; }
    }
}
