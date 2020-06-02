using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Entities.Vpvh
{
    public class VpvhAudienceMapping
    {
        public VpvhAudienceMapping()
        {
            Audience = new DisplayAudience();
            ComposeAudience = new DisplayAudience();
        }

        public int Id { get; set; }
        public DisplayAudience Audience { get; set; }
        public DisplayAudience ComposeAudience { get; set; }
        public VpvhOperationEnum Operation { get; set; }
    }
}
