using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class GenreCacheStub : IGenreCache
    {
        public LookupDto GetGenreById(int genreId, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }

        public LookupDto GetMaestroGenreLookupDtoBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }

        public LookupDto GetMaestroGenreBySourceGenreName(string sourceGenreName, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }

        public Genre GetMaestroGenreByName(string name)
        {
            return new Genre
            {
                Id = 1,
                Name = name,
                ProgramSourceId = 1
            };
        }

        public LookupDto GetSourceGenreLookupDtoByName(string genreName, ProgramSourceEnum programSource)
        {
            return new LookupDto
            {
                Id = (int)programSource,
                Display = genreName
            };
        }

        public Genre GetSourceGenreByName(string genreName, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }

        public Genre GetMaestroGenreBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }
    }
}
