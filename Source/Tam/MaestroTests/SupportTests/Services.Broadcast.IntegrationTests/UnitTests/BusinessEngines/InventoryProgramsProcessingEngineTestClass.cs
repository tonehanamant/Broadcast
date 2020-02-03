using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class InventoryProgramsProcessingEngineTestClass : InventoryProgramsProcessingEngine
    {
        public InventoryProgramsProcessingEngineTestClass(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IProgramGuideApiClient programGuideApiClient)
            : base(broadcastDataRepositoryFactory, mediaMonthAndWeekAggregateCache, programGuideApiClient)
        {
        }

        public int RequestElementMaxCount { get; set; } = 10;

        protected override int _GetRequestElementMaxCount()
        {
            return RequestElementMaxCount;
        }
    }
}