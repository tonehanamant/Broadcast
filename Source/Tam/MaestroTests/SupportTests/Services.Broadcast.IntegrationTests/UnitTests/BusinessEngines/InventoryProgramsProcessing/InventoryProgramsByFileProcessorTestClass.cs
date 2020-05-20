using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsByFileProcessorTestClass : InventoryProgramsByFileProcessor
    {
        public InventoryProgramsByFileProcessorTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService,
            IGenreCache genreCache,
            IFileService fileService,
            IEmailerService emailerService,
            IEnvironmentService environmentService
            )
            : base(
                  broadcastDataRepositoryFactory, 
                  programGuideApiClient, 
                  stationMappingService, 
                  genreCache,
                  fileService,
                  emailerService,
                  environmentService)
        {
        }

        public int UT_RequestElementMaxCount { get; set; } = 10;

        protected override int _GetRequestElementMaxCount()
        {
            return UT_RequestElementMaxCount;
        }

        public bool UT_ParallelApiCallsEnabled { get; set; } = false;

        protected override bool _GetParallelApiCallsEnabled()
        {
            return UT_ParallelApiCallsEnabled;
        }

        public int UT_MaxDegreesOfParallelism { get; set; } = -1;

        protected override int _GetMaxDegreesOfParallelism()
        {
            return UT_MaxDegreesOfParallelism;
        }

        public int UT_ParallelApiCallsBatchSize { get; set; } = 10;

        protected override int _GetParallelApiCallsBatchSize()
        {
            return UT_ParallelApiCallsBatchSize;
        }

        protected override string _GetBroadcastSharedDirectoryPath()
        {
            return "testSettingBroadcastSharedDirectoryPath";
        }

        protected override string[] _GetProcessingBySourceResultReportToEmails()
        {
            return new[] { "ToEmail1", "ToEmail2" };
        }

        public DateTime? UT_CurrentDateTime { get; set; }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_CurrentDateTime.HasValue
                ? UT_CurrentDateTime.Value
                : base._GetCurrentDateTime();
        }

        protected override int _GetSaveBatchSize()
        {
            return 1000;
        }

        protected override int _GetDeleteBatchSize()
        {
            return 1000;
        }

        public string UT_GetProgramGuideInterfaceProcessedDirectoryPath(int dayOffset)
        {
            return _GetProgramGuideInterfaceProcessedDirectoryPath(dayOffset);
        }

        public List<Tuple<Stream, string>> UT_PerformImportInventoryProgramResultsCalls { get; set; } = new List<Tuple<Stream, string>>();
        public bool UT_ShouldPerformImportInventoryProgramResults { get; set; } = true;

        protected override string _PerformImportInventoryProgramResults(Stream fileStream, string fileName)
        {
            UT_PerformImportInventoryProgramResultsCalls.Add(new Tuple<Stream, string>(fileStream, fileName));
            if (UT_ShouldPerformImportInventoryProgramResults)
            {
                return base._PerformImportInventoryProgramResults(fileStream, fileName);
            }

            return "called";
        }
    }
}