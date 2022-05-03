using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Helpers;
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
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public GenreService(IDataRepositoryFactory dataRepositoryFactory, IFeatureToggleHelper featureToggleHelper)
        {
            _GenreRepository = dataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _FeatureToggleHelper = featureToggleHelper;
        }

        /// <inheritdoc />
        public List<LookupDto> GetGenres(int sourceId)
        {
            var genres = _GenreRepository.GetGenresBySourceId(sourceId);
            _RemoveUnmatched(genres);

            if (!_FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_VARIOUS_GENRE_RESTRICTION))
            {
                _RemoveVarious(genres);
            }

            return genres;
        }
            
        private void _RemoveUnmatched(List<LookupDto> genres)
        {
            genres.RemoveAll(x => x.Display.Equals("Unmatched", StringComparison.OrdinalIgnoreCase));
        }

        private void _RemoveVarious(List<LookupDto> genres)
        {
            genres.RemoveAll(x => x.Display.Equals("Various", StringComparison.OrdinalIgnoreCase));
        }

    }
}
