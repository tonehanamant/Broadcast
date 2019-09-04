using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class PlanStatusDto
    {
        public PlanStatusEnum PlanStatus { get; set; }
        public int Count { get; set; }
    }
}
