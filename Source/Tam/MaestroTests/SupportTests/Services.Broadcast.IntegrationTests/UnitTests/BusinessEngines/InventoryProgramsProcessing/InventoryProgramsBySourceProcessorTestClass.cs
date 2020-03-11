using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class InventoryProgramsBySourceProcessorTestClass : InventoryProgramsBySourceProcessor
    {
        public InventoryProgramsBySourceProcessorTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IGenreCache genreCache,
            IFileService fileService,
            IEmailerService emailerService
        )
            : base(
                broadcastDataRepositoryFactory,
                programGuideApiClient,
                stationMappingService,
                mediaMonthAndWeekAggregateCache,
                genreCache,
                fileService,
                emailerService)
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

        public string UT_GetShortenedInventorySourceName(string sourceName)
        {
            return _GetShortenedInventorySourceName(sourceName);
        }
    }
}