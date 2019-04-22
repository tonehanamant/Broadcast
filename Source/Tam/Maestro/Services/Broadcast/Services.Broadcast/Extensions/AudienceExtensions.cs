using Services.Broadcast.Entities;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Extensions
{
    public static class AudienceExtensions
    {
        public static DisplayAudience ToDisplayAudience(this BroadcastAudience audience)
        {
            return new DisplayAudience(audience.Id, audience.Name);
        }
    }
}
