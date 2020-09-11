using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
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

        /// <inheritdoc />
        public List<LookupDto> GetGenres(int sourceId)
        {
            var genres = _GenreRepository.GetGenresBySourceId(sourceId);
            _RemoveVariousAndUnmatched(genres);
            return genres;
        }
            
        private void _RemoveVariousAndUnmatched(List<LookupDto> genres)
        {
            genres.RemoveAll(x => x.Display.Equals("Various", StringComparison.OrdinalIgnoreCase)
                    || x.Display.Equals("Unmatched", StringComparison.OrdinalIgnoreCase));
        }

    }
}
