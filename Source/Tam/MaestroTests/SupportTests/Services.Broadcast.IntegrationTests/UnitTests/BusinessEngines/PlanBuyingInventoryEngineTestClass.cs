﻿using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanBuyingInventoryEngineTestClass : PlanBuyingInventoryEngine
    {
        public int UT_ThresholdInSecondsForProgramIntersect { get; set; } = 1800;
        public bool UT_UseTrueIndependentStations { get; set; } = false;
        public string UT_PlanPricingEndpointVersion { get; set; } = "2";
        public int UT_NumberOfFallbackQuarters { get; set; } = 8;

        public PlanBuyingInventoryEngineTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsCalculationEngine impressionsCalculationEngine,
            IPlanBuyingInventoryQuarterCalculatorEngine planBuyingInventoryQuarterCalculatorEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IDaypartCache daypartCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ISpotLengthEngine spotLengthEngine, 
            IFeatureToggleHelper featureToggleHelper)
            : base(
                  broadcastDataRepositoryFactory, 
                  impressionsCalculationEngine,
                  planBuyingInventoryQuarterCalculatorEngine,
                  mediaMonthAndWeekAggregateCache,
                  daypartCache,
                  quarterCalculationEngine,
                  spotLengthEngine, 
                  featureToggleHelper)
        {
            // override our lazy delegates
            _ThresholdInSecondsForProgramIntersect = new Lazy<int>(() => UT_ThresholdInSecondsForProgramIntersect);
            _UseTrueIndependentStations = new Lazy<bool>(() => UT_UseTrueIndependentStations);
            _PlanPricingEndpointVersion = new Lazy<string>(() => UT_PlanPricingEndpointVersion);
            _NumberOfFallbackQuarters = new Lazy<int>(() => UT_NumberOfFallbackQuarters);
        }
    }
}