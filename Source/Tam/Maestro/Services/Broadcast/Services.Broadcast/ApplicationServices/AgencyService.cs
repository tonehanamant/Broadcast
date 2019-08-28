using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
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
        private readonly ITrafficApiClient _TrafficApiClient;

        public AgencyService(ITrafficApiClient trafficApiClient)
        {
            _TrafficApiClient = trafficApiClient;
        }
        
        public List<AgencyDto> GetAgencies()
        {
            return _TrafficApiClient.GetAgencies();
        }
    }
}
