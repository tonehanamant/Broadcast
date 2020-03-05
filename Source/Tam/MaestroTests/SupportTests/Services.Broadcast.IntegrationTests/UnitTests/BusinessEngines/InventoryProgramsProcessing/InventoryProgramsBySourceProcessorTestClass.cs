using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class InventoryProgramsBySourceProcessorTestClass : InventoryProgramsBySourceProcessor
    {
        public InventoryProgramsBySourceProcessorTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IGenreCache genreCache)
            : base(
                  broadcastDataRepositoryFactory, 
                  programGuideApiClient, 
                  stationMappingService, 
                  mediaMonthAndWeekAggregateCache,
                  genreCache)
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
    }
}