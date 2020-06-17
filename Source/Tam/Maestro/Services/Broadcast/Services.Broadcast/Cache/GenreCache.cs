﻿using Common.Services.Repositories;
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

        LookupDto GetMaestroGenreBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource);

        LookupDto GetMaestroGenreByName(string genreName);

        LookupDto GetSourceGenreByName(string genreName, ProgramSourceEnum programSource);
        LookupDto GetGenreById(int genreId, ProgramSourceEnum programSource);
    }

    public class GenreCache : IGenreCache
    {
        private readonly Dictionary<int, Genre> _GenresByIds;

        private readonly Dictionary<ProgramSourceEnum, Dictionary<string, Genre>> _GenresByNamesBySource;
        private readonly Dictionary<ProgramSourceEnum, Dictionary<int, int>> _MappingsToMaestroGenreBySource;
        private readonly Dictionary<String, int> _MaestroGenresByName;

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

            _MaestroGenresByName = genres.Where(g => g.ProgramSourceId == (int)ProgramSourceEnum.Mapped)
                .ToDictionary(g => g.Name.ToUpper(), g => g.Id);

            _GenresByIds = genres.ToDictionary(x => x.Id, x => x);
        }

        public LookupDto GetMaestroGenreBySourceGenreName(string sourceGenreName, ProgramSourceEnum programSource)
        {
            var sourceGenre = GetSourceGenreByName(sourceGenreName, programSource);
            var maestroGenre = GetMaestroGenreBySourceGenre(sourceGenre, programSource);

            return maestroGenre;
        }

        public LookupDto GetMaestroGenreByName(string genreName)
        {
            if (!_MaestroGenresByName.TryGetValue(genreName.ToUpper(), out var maestroGenreId))
                throw new UnknownGenreException($"There is no genre {genreName}.");

            return new LookupDto
            {
                Id = maestroGenreId,
                Display = genreName.ToUpper()
            };
        }

        public LookupDto GetMaestroGenreBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource)
        {
            var mappingsToMaestroGenre = _MappingsToMaestroGenreBySource[programSource];

            if (!mappingsToMaestroGenre.TryGetValue(sourceGenre.Id, out var maestroGenreId))
                throw new UnknownGenreException($"There is no mapping from the {programSource.ToString()} genre : {sourceGenre.Display} to a Maestro genre");

            return _ToLookupDto(_GenresByIds[maestroGenreId]);
        }

        public LookupDto GetSourceGenreByName(string genreName, ProgramSourceEnum programSource)
        {
            var genresByNames = _GenresByNamesBySource[programSource];

            if (!genresByNames.TryGetValue(genreName, out var sourceGenre))
                throw new UnknownGenreException($"An unknown {programSource.ToString()} genre was discovered: {genreName}");

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
        public LookupDto GetGenreById(int genreId, ProgramSourceEnum programSource)
        {
	        var genre = _GenresByIds[genreId];

	        if (genre == null)
		        return null;
	        if (genre.ProgramSourceId != (int)programSource)
		        return null;

	        return _ToLookupDto(genre);
        }

    }
}
