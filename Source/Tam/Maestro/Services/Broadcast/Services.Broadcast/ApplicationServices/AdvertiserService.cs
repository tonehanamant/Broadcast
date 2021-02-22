using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAdvertiserService : IApplicationService
    {
        /// <summary>
        /// Returns all advertisers
        /// </summary>
        List<AdvertiserDto> GetAdvertisers();

        /// <summary>
        /// Clears the advertisers cache.
        /// </summary>
        void ClearAdvertisersCache();
    }

    public class AdvertiserService : IAdvertiserService
    {
        private readonly IAabEngine _AabEngine;

        public AdvertiserService(IAabEngine aabEngine)
        {
            _AabEngine = aabEngine;
        }

        /// <inheritdoc />
        public List<AdvertiserDto> GetAdvertisers()
        {
            return _AabEngine.GetAdvertisers();
        }

        /// <inheritdoc />
        public void ClearAdvertisersCache()
        {
            _AabEngine.ClearAdvertisersCache();
        }
    }
}
