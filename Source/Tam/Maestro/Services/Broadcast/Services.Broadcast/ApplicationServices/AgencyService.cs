using Common.Services.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAgencyService : IApplicationService
    {
        List<AgencyDto> GetAgencies();
    }

    public class AgencyService : IAgencyService
    {
        private readonly ITrafficApiCache _TrafficApiCache;

        public AgencyService(ITrafficApiCache trafficApiCache)
        {
            _TrafficApiCache = trafficApiCache;
        }
        
        public List<AgencyDto> GetAgencies()
        {
            return _TrafficApiCache.GetAgencies();
        }
    }
}
