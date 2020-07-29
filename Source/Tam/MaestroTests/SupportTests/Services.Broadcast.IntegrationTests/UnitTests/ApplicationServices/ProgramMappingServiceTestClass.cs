﻿using System;
using System.Collections.Generic;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
	public class ProgramMappingServiceTestClass : ProgramMappingService
	{
		public ProgramMappingServiceTestClass(
			IBackgroundJobClient backgroundJobClient,
			IDataRepositoryFactory broadcastDataRepositoryFactory,
			ISharedFolderService sharedFolderService,
			IProgramNameMappingsExportEngine programNameMappingsExportEngine,
			IGenreCache genreCache,
			IShowTypeCache showTypeCache,
			IProgramsSearchApiClient programsSearchApiClient,
			IProgramMappingCleanupEngine programMappingCleanupEngine)
			: base(backgroundJobClient,
				broadcastDataRepositoryFactory,
				sharedFolderService,
				programNameMappingsExportEngine,
				genreCache,
				showTypeCache,
				programsSearchApiClient,
				programMappingCleanupEngine)
		{
		}

		public void UT_ProcessProgramMappings(List<ProgramMappingsFileRequestDto> programMappings, DateTime createdDate, string username)
		{
			_ProcessProgramMappings(programMappings, createdDate, username);
		}
		public bool UT_EnableInternalProgramSearch { get; set; } = false;


		protected override bool _GetEnableInternalProgramSearch()
		{
			return UT_EnableInternalProgramSearch;
		}
		public void UT_LoadShowTypes(List<ProgramMappingsFileRequestDto> programMappings)
		{
			_LoadShowTypes(programMappings);

		}
	}
}