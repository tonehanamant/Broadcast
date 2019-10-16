using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    /// <summary>
    /// Operations related to the Genre domain.
    /// </summary>
    /// <seealso cref="Common.Services.ApplicationServices.IApplicationService" />
    public interface IGenreService : IApplicationService
    {
        /// <summary>
        /// Gets all genres filter by source.
        /// </summary>
        /// <param name="sourceId">The source id.</param>
        /// <returns></returns>
        List<LookupDto> GetGenres(int sourceId);
    }

    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _GenreRepository;

        public GenreService(IDataRepositoryFactory dataRepositoryFactory)
        {
            _GenreRepository = dataRepositoryFactory.GetDataRepository<IGenreRepository>();
        }


        public List<LookupDto> GetGenres(int sourceId) =>
            _GenreRepository.GetGenresBySourceId(sourceId);
    }
}
