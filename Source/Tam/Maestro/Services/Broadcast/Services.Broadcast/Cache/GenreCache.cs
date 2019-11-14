using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Cache
{
    public interface IGenreCache
    {
        LookupDto GetMaestroGenreFromDativaGenre(string dativaGenre);
    }

    public class GenreCache : IGenreCache
    {
        private readonly Dictionary<int, Genre> _GenresByIds;
        private readonly Dictionary<string, Genre> _DativaGenresByNames; 
        private readonly Dictionary<int, int> _DativaMappings;

        public GenreCache(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            var repository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            var genres = repository.GetAllGenres();

            _GenresByIds = genres.ToDictionary(x => x.Id, x => x);
            _DativaGenresByNames = genres.Where(x => x.SourceId == (int)GenreSourceEnum.Dativa).ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);
            _DativaMappings = repository.GetGenreMappingsBySourceId((int)GenreSourceEnum.Dativa).ToDictionary(x => x.GenreIdToMap, x => x.MaestroGenreId);
        }

        public LookupDto GetMaestroGenreFromDativaGenre(string dativaGenreName)
        {
            if (!_DativaGenresByNames.TryGetValue(dativaGenreName, out var dativaGenre))
                throw new UnknownGenreException($"An unknown Dativa genre was discovered: {dativaGenreName}");

            if (!_DativaMappings.TryGetValue(dativaGenre.Id, out var maestroGenreId))
                throw new UnknownGenreException($"There is no mapping from the Dativa genre: {dativaGenreName} to a Maestro genre");
            
            return _ToLookupDto(_GenresByIds[maestroGenreId]);
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
