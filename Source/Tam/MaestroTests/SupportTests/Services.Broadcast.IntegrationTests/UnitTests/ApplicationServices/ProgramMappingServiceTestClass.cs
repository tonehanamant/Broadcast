﻿using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class ProgramMappingServiceTestClass : ProgramMappingService
    {
        public ProgramMappingServiceTestClass(
            IBackgroundJobClient backgroundJobClient,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISharedFolderService sharedFolderService,
            IProgramNameMappingsExportEngine programNameMappingsExportEngine) 
            : base(backgroundJobClient, broadcastDataRepositoryFactory, sharedFolderService, programNameMappingsExportEngine)
        {
        }

        public void UT_ProcessProgramMappings(
            List<ProgramMappingsFileRequestDto> programMappings,
            DateTime createdDate, string username,
            ref int updatedInventoryCount, ref int ingestedRecordsCount)
        {
            _ProcessProgramMappings(programMappings, createdDate, username, ref updatedInventoryCount, ref ingestedRecordsCount);
        }
    }
}
