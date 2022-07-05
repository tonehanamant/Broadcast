using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
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

        /// <summary>
        /// Sync the schedules table
        /// </summary>
        bool SyncAdvertisersToSchedules();
    }

    public class AdvertiserService : IAdvertiserService
    {
        private readonly IAabEngine _AabEngine;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;

        public AdvertiserService(IAabEngine aabEngine, IDataRepositoryFactory dataRepositoryFactory)
        {
            _AabEngine = aabEngine;
            _BroadcastDataRepositoryFactory = dataRepositoryFactory;
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

        public bool SyncAdvertisersToSchedules()
        {
            var result = _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().SyncAdvertisersToSchedules();
            return result;
        }
    }
}
