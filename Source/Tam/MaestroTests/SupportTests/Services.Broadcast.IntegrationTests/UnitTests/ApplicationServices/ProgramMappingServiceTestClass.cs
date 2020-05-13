using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class ProgramMappingServiceTestClass : ProgramMappingService
    {
        public ProgramMappingServiceTestClass(
            IBackgroundJobClient backgroundJobClient,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISharedFolderService sharedFolderService) 
            : base(backgroundJobClient, broadcastDataRepositoryFactory, sharedFolderService)
        {
        }

        public void UT_ProcessProgramMappings(
            List<ProgramMappingsFileRequestDto> programMappings,
            DateTime createdDate,
            ref int updatedInventoryCount, ref int ingestedRecordsCount)
        {
            _ProcessProgramMappings(programMappings, createdDate, ref updatedInventoryCount, ref ingestedRecordsCount);
        }
    }
}
