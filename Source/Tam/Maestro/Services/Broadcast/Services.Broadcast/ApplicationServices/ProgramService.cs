using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProgramService : IApplicationService
    {
        List<ProgramDto> GetPrograms(SearchRequestProgramDto searchRequest, string userName);
    }

    public class ProgramService : BroadcastBaseClass, IProgramService
    {
        private IGenreCache _GenreCache;
        private readonly IProgramNameRepository _ProgramNameRepository;

		public ProgramService(
            IGenreCache genreCache,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
			IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
		{
            _GenreCache = genreCache;
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

			var internalResults = _LoadProgramFromMapping(searchRequest);
            foreach (var internalResult in internalResults)
            {
                result.Add(internalResult);
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
