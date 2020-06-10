using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface INsiUniverseService : IApplicationService
    {
        /// <summary>
        /// Gets the universe data by audience.
        /// </summary>
        /// <param name="audienceId">The audience identifier.</param>
        /// <param name="sweepMediaMonth">The sweep media month.</param>
        /// <returns>Dictionary of market codes and subscribers </returns>
        Dictionary<short, double> GetUniverseDataByAudience(int audienceId, int sweepMediaMonth);
    }

    public class NsiUniverseService : INsiUniverseService
    {
        private readonly INsiUniverseRepository _NsiUniverseRepository;
        private readonly IBroadcastAudienceRepository _AudienceRepository;
        private ConcurrentDictionary<Tuple<int, int>, double> UniversesValues;

        public NsiUniverseService(IDataRepositoryFactory dataRepositoryFactory)
        {
            _NsiUniverseRepository = dataRepositoryFactory.GetDataRepository<INsiUniverseRepository>();
            _AudienceRepository = dataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            UniversesValues = new ConcurrentDictionary<Tuple<int, int>, double>();
        }

        /// <inheritdoc/>
        public Dictionary<short, double> GetUniverseDataByAudience(int audienceId, int sweepMediaMonth)
        {
            var audiencesMappings = _AudienceRepository.GetRatingsAudiencesByMaestroAudience(new List<int> { audienceId }).Select(am => am.rating_audience_id).Distinct().ToList();
                      
            return _NsiUniverseRepository.GetUniverseDataByAudience(sweepMediaMonth, audiencesMappings);
        }
    }
}