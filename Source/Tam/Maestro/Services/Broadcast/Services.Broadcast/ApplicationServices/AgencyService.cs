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
        private readonly IAgencyCache _AgencyCache;

        public AgencyService(IAgencyCache agencyCache)
        {
            _AgencyCache = agencyCache;
        }
        
        public List<AgencyDto> GetAgencies()
        {
            return _AgencyCache.GetAgencies();
        }
    }
}
