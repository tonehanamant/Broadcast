using Common.Services.ApplicationServices;
using ConfigurationService.Client;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Exceptions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tam.Maestro.Common.Utilities.Logging;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProgramService : IApplicationService
    {
        List<ProgramDto> GetPrograms(SearchRequestProgramDto searchRequest, string userName);
    }

    public class ProgramService : IProgramService
    {
        private IGenreCache _GenreCache;
        private IConfigurationWebApiClient _ConfigurationWebApiClient;
        private IProgramsSearchApiClient _ProgramsSearchApiClient;

        public ProgramService(
            IGenreCache genreCache,
            IConfigurationWebApiClient configurationWebApiClient,
            IProgramsSearchApiClient programsSearchApiClient)
        {
            _GenreCache = genreCache;
            _ConfigurationWebApiClient = configurationWebApiClient;
            _ProgramsSearchApiClient = programsSearchApiClient;
        }

        public List<ProgramDto> GetPrograms(SearchRequestProgramDto searchRequest, string userName)
        {
            var durationSw = new Stopwatch();
            durationSw.Start();
            const int upperLimit = 10000;
            searchRequest.Start = searchRequest.Start ?? 1;
            searchRequest.Limit = searchRequest.Limit ?? upperLimit;

            var externalApiPrograms = _ProgramsSearchApiClient.GetPrograms(searchRequest);
            var result = new List<ProgramDto>();

            foreach (var externalApiProgram in externalApiPrograms)
            {
                if (searchRequest.IgnorePrograms.Contains(externalApiProgram.ProgramName))
                    continue;

                try
                {
                    var programGenre = _GenreCache.GetMaestroGenreBySourceGenreName(externalApiProgram.Genre, GenreSourceEnum.Dativa);

                    result.Add(new ProgramDto
                    {
                        Name = externalApiProgram.ProgramName,
                        Genre = programGenre,
                        ContentRating = externalApiProgram.MpaaRating
                    });
                }
                catch (UnknownGenreException ex)
                {
                    LogHelper.Log.ServiceError(
                        "Broadcast ProgramGuideService",
                        ex.Message,
                        ex.ToString(),
                        userName,
                        _ConfigurationWebApiClient.TAMEnvironment.ToString());
                }
            }

            var sortedResults = result.OrderBy(x => x.Name).ToList();

            durationSw.Stop();

            LogHelper.Logger.Info($"Method {nameof(GetPrograms)} completed having received {sortedResults.Count} programs in {durationSw.ElapsedMilliseconds}ms from search parameters : " +
                                  $"ProgramName = '{searchRequest.ProgramName}'; Start = '{searchRequest.Start}'; Limit = '{searchRequest.Limit}';");

            return sortedResults;
        }
    }
}
