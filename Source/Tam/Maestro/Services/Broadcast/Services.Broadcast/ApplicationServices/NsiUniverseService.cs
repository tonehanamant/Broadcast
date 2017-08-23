using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface INsiUniverseService : IApplicationService
    {
        Dictionary<short, double> GetUniverseDataByAudience(int audienceId, int sweepMediaMonth);
    }

    public class NsiUniverseService : INsiUniverseService
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        public NsiUniverseService(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
        }

        public Dictionary<short, double> GetUniverseDataByAudience(int audienceId, int sweepMediaMonth)
        {
            var audiencesMappings = _DataRepositoryFactory
                .GetDataRepository<IBroadcastAudienceRepository>()
                .GetRatingsAudiencesByMaestroAudience(new List<int> { audienceId }).Select(am => am.rating_audience_id).Distinct().ToList();
                      
            return _DataRepositoryFactory
                .GetDataRepository<INsiUniverseRepository>()
                .GetUniverseDataByAudience(sweepMediaMonth, audiencesMappings);
        }
    }
}