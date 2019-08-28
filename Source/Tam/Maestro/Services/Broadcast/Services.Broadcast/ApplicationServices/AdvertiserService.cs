using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
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
        private readonly ITrafficApiClient _TrafficApiClient;

        public AdvertiserService(ITrafficApiClient trafficApiClient)
        {
            _TrafficApiClient = trafficApiClient;
        }

        /// <inheritdoc />
        public List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId)
        {
            return _TrafficApiClient.GetAdvertisersByAgencyId(agencyId);
        }
    }
}
