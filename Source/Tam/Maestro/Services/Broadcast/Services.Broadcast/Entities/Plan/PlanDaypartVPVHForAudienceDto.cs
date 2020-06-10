using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanDaypartVpvhForAudienceDto
    {
        public int AudienceId { get; set; }
        public double Vpvh { get; set; }
        public VpvhTypeEnum VpvhType { get; set; }
        public DateTime StartingPoint { get; set; }
    }
}
