﻿using Common.Services.ApplicationServices;
using ConfigurationService.Client;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Exceptions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using System;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProgramService : IApplicationService
    {
        List<ProgramDto> GetPrograms(SearchRequestProgramDto searchRequest, string userName);
       
    }

    public class ProgramService : BroadcastBaseClass, IProgramService
    {
        private IGenreCache _GenreCache;
        private IConfigurationWebApiClient _ConfigurationWebApiClient;
        private IProgramsSearchApiClient _ProgramsSearchApiClient;
        private readonly IProgramNameRepository _ProgramNameRepository;

        public ProgramService(
            IGenreCache genreCache,
            IConfigurationWebApiClient configurationWebApiClient,
            IProgramsSearchApiClient programsSearchApiClient,
            IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _GenreCache = genreCache;
            _ConfigurationWebApiClient = configurationWebApiClient;
            _ProgramsSearchApiClient = programsSearchApiClient;
            _ProgramNameRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramNameRepository>();
        }

        public List<ProgramDto> GetPrograms(SearchRequestProgramDto searchRequest, string userName)
        {
	        var durationSw = new Stopwatch();
	        durationSw.Start();
	        const int upperLimit = 10000;
	        searchRequest.Start = searchRequest.Start ?? 1;
	        searchRequest.Limit = searchRequest.Limit ?? upperLimit;
	        var result = new List<ProgramDto>();
	        var exceptionResults = _LoadProgramFromExceptions(searchRequest);
	        foreach (var exceptionResult in exceptionResults)
	        {
		        result.Add(exceptionResult);
	        }

	        if (_GetEnableInternalProgramSearch())
	        {
		        var internalResults = _LoadProgramFromMapping(searchRequest);
		        foreach (var internalResult in internalResults)
		        {
			        result.Add(internalResult);
		        }
	        }
	        else
	        {
		        var externalResults = _LoadProgramFromExternalApi(searchRequest);
		        foreach (var externalResult in externalResults)
		        {
			        result.Add(externalResult);
		        }
	        }
			
			_RemoveVariousAndUnmatched(result);
	        var sortedResults = result
		        .Distinct(new ProgramEqualityComparer())
		        .OrderBy(x => x.Name)
		        .ToList();

	        durationSw.Stop();

	        _LogInfo(
		        $"Method {nameof(GetPrograms)} completed having received {sortedResults.Count} programs in {durationSw.ElapsedMilliseconds}ms from search parameters : " +
		        $"ProgramName = '{searchRequest.ProgramName}'; Start = '{searchRequest.Start}'; Limit = '{searchRequest.Limit}';");

	        return sortedResults;
        }

		private void _RemoveVariousAndUnmatched(List<ProgramDto> result)
		{
			result.RemoveAll(x => x.Genre.Display.Equals("Various", StringComparison.OrdinalIgnoreCase) 
					|| x.Name.Equals("Unmatched", StringComparison.OrdinalIgnoreCase));
		}

		protected virtual bool _GetEnableInternalProgramSearch()
		{
			return BroadcastServiceSystemParameter.EnableInternalProgramSearch;
		}
		private List<ProgramDto> _LoadProgramFromExternalApi(SearchRequestProgramDto searchRequest)
		{
			var result = new List<ProgramDto>();
			var externalApiPrograms = _ProgramsSearchApiClient.GetPrograms(searchRequest);

			foreach (var externalApiProgram in externalApiPrograms)
			{
				if (searchRequest.IgnorePrograms.Contains(externalApiProgram.ProgramName))
					continue;

				try
				{
					var programGenre = _GenreCache.GetMaestroGenreBySourceGenreName(externalApiProgram.Genre,
						ProgramSourceEnum.Master);

					result.Add(new ProgramDto
					{
						Name = externalApiProgram.ProgramName,
						Genre = programGenre,
						ContentRating = externalApiProgram.MpaaRating
					});
				}
				catch (UnknownGenreException ex)
				{
					_LogError("Exception caught resolving program genre.", ex);
				}
			}

			return result;
		}

        private List<ProgramDto> _LoadProgramFromMapping(SearchRequestProgramDto searchRequest)
        {
            var result = new List<ProgramDto>();
            var internalPrograms = _ProgramNameRepository.FindProgramFromMapping(searchRequest.ProgramName);
            foreach (var internalProgram in internalPrograms)
            {
                if (searchRequest.IgnorePrograms.Contains(internalProgram.OfficialProgramName))
                    continue;
                result.Add(new ProgramDto
                {
                    Name = internalProgram.OfficialProgramName,
                    Genre = _GenreCache.GetGenreLookupDtoById(internalProgram.GenreId)
                });
            }
            return result;
        }		

		private List<ProgramDto> _LoadProgramFromExceptions(SearchRequestProgramDto searchRequest)
		{
			var result = new List<ProgramDto>();
			var exceptionPrograms = _ProgramNameRepository.FindProgramFromExceptions(searchRequest.ProgramName);
			foreach (var exceptionProgram in exceptionPrograms)
			{
				if (searchRequest.IgnorePrograms.Contains(exceptionProgram.CustomProgramName))
					continue;
				result.Add(new ProgramDto
				{
					Name = exceptionProgram.CustomProgramName,
					Genre = _GenreCache.GetGenreLookupDtoById(exceptionProgram.GenreId)

				});
			}
			return result;
		}
	}
}
