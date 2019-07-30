using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface ISpotLengthService : IApplicationService
    {
        /// <summary>
        /// Gets all spot lengths.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetAllSpotLengths();
    }

    public class SpotLengthService : ISpotLengthService
    {
        private readonly ISpotLengthRepository _SpotLengthRepository;

        public SpotLengthService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _SpotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
        }

        /// <inheritdoc/>
        public List<LookupDto> GetAllSpotLengths()
        {
            return _SpotLengthRepository.GetSpotLengths();
        }
    }
}
