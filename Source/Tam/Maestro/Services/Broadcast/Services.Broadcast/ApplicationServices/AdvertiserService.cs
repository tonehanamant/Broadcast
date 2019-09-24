using Common.Services.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAdvertiserService : IApplicationService
    {
        /// <summary>
        /// Returns advertisers for a specific agency
        /// </summary>
        List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId);
    }

    public class AdvertiserService : IAdvertiserService
    {
        private readonly ITrafficApiCache _TrafficApiCache;

        public AdvertiserService(ITrafficApiCache trafficApiCache)
        {
            _TrafficApiCache = trafficApiCache;
        }

        /// <inheritdoc />
        public List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId)
        {
            return _TrafficApiCache.GetAdvertisersByAgencyId(agencyId);
        }
    }
}
