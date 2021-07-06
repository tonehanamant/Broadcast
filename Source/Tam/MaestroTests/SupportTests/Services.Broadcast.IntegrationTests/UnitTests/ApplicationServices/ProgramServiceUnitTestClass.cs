using Common.Services.Repositories;
using ConfigurationService.Client;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
	public class ProgramServiceUnitTestClass : ProgramService
	{
		public ProgramServiceUnitTestClass(IGenreCache genreCache,
			IConfigurationWebApiClient configurationWebApiClient,
			IProgramsSearchApiClient programsSearchApiClient,
			IDataRepositoryFactory broadcastDataRepositoryFactory,
			IFeatureToggleHelper featureToggleHelper)
			: base(genreCache, configurationWebApiClient, programsSearchApiClient, broadcastDataRepositoryFactory, featureToggleHelper)
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