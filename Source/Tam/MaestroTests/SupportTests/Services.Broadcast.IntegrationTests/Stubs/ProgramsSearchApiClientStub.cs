using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Extensions;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class ProgramsSearchApiClientStub : IProgramsSearchApiClient
    {
        public List<SearchProgramDativaResponseDto> GetPrograms(SearchRequestProgramDto searchRequest)
        {
            return _Programs
                .Where(x => x.ProgramName.Contains(searchRequest.ProgramName, StringComparison.InvariantCultureIgnoreCase))
                .Skip(searchRequest.Start.Value - 1) // start is 1 based
                .Take(searchRequest.Limit.Value)
                .ToList();
        }

        private readonly List<SearchProgramDativaResponseDto> _Programs = new List<SearchProgramDativaResponseDto>
        {
            new SearchProgramDativaResponseDto
            {
                ProgramId = "1",
                ProgramName = "Parasite",
                GenreId = "1",
                Genre = "Comedy",
                ShowType = "Movie",
                MpaaRating = "R",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "2",
                ProgramName = "Jojo Rabbit",
                GenreId = "1",
                Genre = "Comedy",
                ShowType = "Movie",
                MpaaRating = "PG-13",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "3",
                ProgramName = "Harley Quinn: Birds of Prey",
                GenreId = "2",
                Genre = "Crime",
                ShowType = "Movie",
                MpaaRating = "R",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "4",
                ProgramName = "Joker",
                GenreId = "2",
                Genre = "Crime",
                ShowType = "Movie",
                MpaaRating = "R",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "5",
                ProgramName = "1917",
                GenreId = "3",
                Genre = "Drama",
                ShowType = "Movie",
                MpaaRating = "R",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "6",
                ProgramName = "Mr. Jones",
                GenreId = "3",
                Genre = "Drama",
                ShowType = "Movie",
                MpaaRating = "R",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "7",
                ProgramName = "black-ish",
                GenreId = "8",
                Genre = "comedy",
                ShowType = "Series",
                MpaaRating = "",
                SyndicationType = "Public broadcasting syndication"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "8",
                ProgramName = "black-ish",
                GenreId = "8",
                Genre = "comedy",
                ShowType = "Series",
                MpaaRating = "",
                SyndicationType = "Non-Public broadcasting syndication"
            }
        };
    }
}
