using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Cache
{
    public interface IGenreCache
    {
        LookupDto GetMaestroGenreBySourceGenreName(string sourceGenreName, GenreSourceEnum genreSource);

        LookupDto GetMaestroGenreBySourceGenre(LookupDto sourceGenre, GenreSourceEnum genreSource);

        LookupDto GetSourceGenreByName(string genreName, GenreSourceEnum genreSource);
    }

    public class GenreCache : IGenreCache
    {
        private readonly Dictionary<int, Genre> _GenresByIds;

        private readonly Dictionary<GenreSourceEnum, Dictionary<string, Genre>> _GenresByNamesBySource;
        private readonly Dictionary<GenreSourceEnum, Dictionary<int, int>> _MappingsToMaestroGenreBySource;

        public GenreCache(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            var repository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            var genres = repository.GetAllGenres();
            var genreMappings = repository.GetGenreMappings();

            _GenresByNamesBySource = new Dictionary<GenreSourceEnum, Dictionary<string, Genre>>();
            _MappingsToMaestroGenreBySource = new Dictionary<GenreSourceEnum, Dictionary<int, int>>();

            foreach (var genreSource in EnumHelper.GetValues<GenreSourceEnum>())
            {
                _GenresByNamesBySource[genreSource] = genres
                    .Where(x => x.SourceId == (int)genreSource)
                    .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

                _MappingsToMaestroGenreBySource[genreSource] = genreMappings
                    .Where(x => x.SourceId == (int)genreSource)
                    .ToDictionary(x => x.SourceGenreId, x => x.MaestroGenreId);
            }

            _GenresByIds = genres.ToDictionary(x => x.Id, x => x);
        }

        public LookupDto GetMaestroGenreBySourceGenreName(string sourceGenreName, GenreSourceEnum genreSource)
        {
            var sourceGenre = GetSourceGenreByName(sourceGenreName, genreSource);
            var maestroGenre = GetMaestroGenreBySourceGenre(sourceGenre, genreSource);

            return maestroGenre;
        }

        public LookupDto GetMaestroGenreBySourceGenre(LookupDto sourceGenre, GenreSourceEnum genreSource)
        {
            var mappingsToMaestroGenre = _MappingsToMaestroGenreBySource[genreSource];

            if (!mappingsToMaestroGenre.TryGetValue(sourceGenre.Id, out var maestroGenreId))
                throw new UnknownGenreException($"There is no mapping from the {genreSource.ToString()} genre : {sourceGenre.Display} to a Maestro genre");

            return _ToLookupDto(_GenresByIds[maestroGenreId]);
        }

        public LookupDto GetSourceGenreByName(string genreName, GenreSourceEnum genreSource)
        {
            var genresByNames = _GenresByNamesBySource[genreSource];

            if (!genresByNames.TryGetValue(genreName, out var sourceGenre))
                throw new UnknownGenreException($"An unknown {genreSource.ToString()} genre was discovered: {genreName}");

            return _ToLookupDto(sourceGenre);
        }

        private LookupDto _ToLookupDto(Genre genre)
        {
            return new LookupDto
            {
                Id = genre.Id,
                Display = genre.Name
            };
        }
    }
}
