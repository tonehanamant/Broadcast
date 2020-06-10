using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.Vpvh
{
    public class VpvhDefaultResponse
    {
        public int StandardDaypartId { get; set; }

        public int AudienceId { get; set; }

        public double Vpvh { get; set; }

        public VpvhTypeEnum VpvhType { get; set; }

        public DateTime StartingPoint { get; set; }
    }
}
