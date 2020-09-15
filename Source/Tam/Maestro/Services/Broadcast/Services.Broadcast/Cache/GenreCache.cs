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
        LookupDto GetMaestroGenreBySourceGenreName(string sourceGenreName, ProgramSourceEnum programSource);
        LookupDto GetMaestroGenreLookupDtoBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource);
        Genre GetMaestroGenreBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource);
        Genre GetMaestroGenreByName(string genreName);
        LookupDto GetSourceGenreLookupDtoByName(string genreName, ProgramSourceEnum programSource);
        LookupDto GetGenreLookupDtoById(int genreId);
        Genre GetGenreById(int genreId);
        Genre GetSourceGenreByName(string genreName, ProgramSourceEnum programSource);
    }

    public class GenreCache : IGenreCache
    {
        private readonly Dictionary<int, Genre> _GenresByIds;

        private readonly Dictionary<ProgramSourceEnum, Dictionary<string, Genre>> _GenresByNamesBySource;
        private readonly Dictionary<ProgramSourceEnum, Dictionary<int, int>> _MappingsToMaestroGenreBySource;
        private readonly Dictionary<string, Genre> _MaestroGenresByName;

        public GenreCache(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            var repository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            var genres = repository.GetAllGenres();
            var genreMappings = repository.GetGenreMappings();

            _GenresByNamesBySource = new Dictionary<ProgramSourceEnum, Dictionary<string, Genre>>();
            _MappingsToMaestroGenreBySource = new Dictionary<ProgramSourceEnum, Dictionary<int, int>>();

            foreach (var programSource in EnumHelper.GetValues<ProgramSourceEnum>())
            {
                _GenresByNamesBySource[programSource] = genres
                    .Where(x => x.ProgramSourceId == (int)programSource)
                    .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

                _MappingsToMaestroGenreBySource[programSource] = genreMappings
                    .Where(x => x.ProgramSourceId == (int)programSource)
                    .ToDictionary(x => x.SourceGenreId, x => x.MaestroGenreId);
            }

            _MaestroGenresByName = _GenresByNamesBySource[ProgramSourceEnum.Maestro];

            _GenresByIds = genres.ToDictionary(x => x.Id, x => x);
        }

        public LookupDto GetMaestroGenreBySourceGenreName(string sourceGenreName, ProgramSourceEnum programSource)
        {
            var sourceGenre = GetSourceGenreLookupDtoByName(sourceGenreName, programSource);
            var maestroGenre = GetMaestroGenreLookupDtoBySourceGenre(sourceGenre, programSource);

            return maestroGenre;
        }

        public Genre GetMaestroGenreByName(string genreName)
        {
            if (!_MaestroGenresByName.TryGetValue(genreName, out var maestroGenre))
                throw new UnknownGenreException($"There is no genre {genreName}.");

            return maestroGenre;
        }

        public LookupDto GetMaestroGenreLookupDtoBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource)
        {
            var mappingsToMaestroGenre = _MappingsToMaestroGenreBySource[programSource];

            if (!mappingsToMaestroGenre.TryGetValue(sourceGenre.Id, out var maestroGenreId))
                throw new UnknownGenreException($"There is no mapping from the {programSource.ToString()} genre : {sourceGenre.Display} to a Maestro genre");

            return _ToLookupDto(_GenresByIds[maestroGenreId]);
        }

        public Genre GetMaestroGenreBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource)
        {
            var mappingsToMaestroGenre = _MappingsToMaestroGenreBySource[programSource];

            if (!mappingsToMaestroGenre.TryGetValue(sourceGenre.Id, out var maestroGenreId))
                throw new UnknownGenreException($"There is no mapping from the {programSource.ToString()} genre : {sourceGenre.Display} to a Maestro genre");

            return _GenresByIds[maestroGenreId];
        }

        public LookupDto GetSourceGenreLookupDtoByName(string genreName, ProgramSourceEnum programSource)
        {
            var genresByNames = _GenresByNamesBySource[programSource];

            if (!genresByNames.TryGetValue(genreName, out var sourceGenre))
                throw new UnknownGenreException($"An unknown {programSource.ToString()} genre was discovered: {genreName}");

            return _ToLookupDto(sourceGenre);
        }

        public Genre GetSourceGenreByName(string genreName, ProgramSourceEnum programSource)
        {
            var genresByNames = _GenresByNamesBySource[programSource];

            if (!genresByNames.TryGetValue(genreName, out var sourceGenre))
                throw new UnknownGenreException($"An unknown {programSource.ToString()} genre was discovered: {genreName}");

            return sourceGenre;
        }

        private LookupDto _ToLookupDto(Genre genre)
        {
            return new LookupDto
            {
                Id = genre.Id,
                Display = genre.Name
            };
        }

        public LookupDto GetGenreLookupDtoById(int genreId)
        {
	        var genre = _GenresByIds[genreId];

	        if (genre == null)
		        return null;

	        return _ToLookupDto(genre);
        }

        public Genre GetGenreById(int genreId)
        {
            var genre = _GenresByIds[genreId];

            if (genre == null)
                return null;

            return genre;
        }
    }
}
