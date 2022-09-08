using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class GenreCacheStub : IGenreCache
    {
        public Genre GetGenreById(int genreId)
        {
            throw new NotImplementedException();
        }

        public LookupDto GetGenreLookupDtoById(int genreId)
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

        public Genre GetMaestroGenreBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }

        public LookupDto GetMaestroGenreBySourceGenreName(string sourceGenreName, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }

        public LookupDto GetMaestroGenreLookupDtoBySourceGenre(LookupDto sourceGenre, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }

        public Genre GetSourceGenreByName(string genreName, ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }

        public List<Genre> GetGenresBySource(ProgramSourceEnum programSource)
        {
            throw new NotImplementedException();
        }       

        public LookupDto GetSourceGenreLookupDtoByName(string genreName, ProgramSourceEnum programSource)
        {
            return new LookupDto
            {
                Id = (int)programSource,
                Display = genreName
            };
        }
    }
}
