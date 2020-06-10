using Common.Services.Repositories;
using ConfigurationService.Client;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
	public class ProgramServiceUnitTestClass : ProgramService
	{
		public ProgramServiceUnitTestClass(IGenreCache genreCache,
			IConfigurationWebApiClient configurationWebApiClient,
			IProgramsSearchApiClient programsSearchApiClient,
			IDataRepositoryFactory broadcastDataRepositoryFactory)
			: base(genreCache, configurationWebApiClient, programsSearchApiClient, broadcastDataRepositoryFactory)
		{
		}

		public bool UT_EnableInternalProgramSearch
		{
			get;
			set;
		} = true;
		protected override bool _GetEnableInternalProgramSearch()
		{
			return UT_EnableInternalProgramSearch;
		}
	}
}