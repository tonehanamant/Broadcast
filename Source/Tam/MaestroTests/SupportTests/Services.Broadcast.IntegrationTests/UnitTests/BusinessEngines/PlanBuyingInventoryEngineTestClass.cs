using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanBuyingInventoryEngineTestClass : PlanBuyingInventoryEngine
    {
        public int UT_ThresholdInSecondsForProgramIntersect { get; set; } = 1800;
        public int UT_NumberOfFallbackQuarters { get; set; } = 8;

        public PlanBuyingInventoryEngineTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsCalculationEngine impressionsCalculationEngine,
            IPlanBuyingInventoryQuarterCalculatorEngine planBuyingInventoryQuarterCalculatorEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IDaypartCache daypartCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ISpotLengthEngine spotLengthEngine, 
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(
                  broadcastDataRepositoryFactory, 
                  impressionsCalculationEngine,
                  planBuyingInventoryQuarterCalculatorEngine,
                  mediaMonthAndWeekAggregateCache,
                  daypartCache,
                  quarterCalculationEngine,
                  spotLengthEngine, 
                  featureToggleHelper,
                  configurationSettingsHelper)
        {
            // override our lazy delegates
            _ThresholdInSecondsForProgramIntersect = new Lazy<int>(() => UT_ThresholdInSecondsForProgramIntersect);
            _NumberOfFallbackQuarters = new Lazy<int>(() => UT_NumberOfFallbackQuarters);
        }
    }
}
