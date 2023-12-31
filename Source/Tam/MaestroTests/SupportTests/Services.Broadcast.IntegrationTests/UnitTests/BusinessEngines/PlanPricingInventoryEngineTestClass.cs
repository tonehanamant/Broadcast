﻿using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanPricingInventoryEngineTestClass : PlanPricingInventoryEngine
    {
        public PlanPricingInventoryEngineTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsCalculationEngine impressionsCalculationEngine,
            IPlanPricingInventoryQuarterCalculatorEngine planPricingInventoryQuarterCalculatorEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IDaypartCache daypartCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ISpotLengthEngine spotLengthEngine, 
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(
                  broadcastDataRepositoryFactory, 
                  impressionsCalculationEngine,
                  planPricingInventoryQuarterCalculatorEngine,
                  mediaMonthAndWeekAggregateCache,
                  daypartCache,
                  quarterCalculationEngine,
                  spotLengthEngine, 
                  featureToggleHelper,
                  configurationSettingsHelper)
        {
            // override our lazy delegates
            _ThresholdInSecondsForProgramIntersect = new Lazy<int>(() => UT_ThresholdInSecondsForProgramIntersect);
            _NumberOfFallbackQuartersForPricing = new Lazy<int>(() => UT_NumberOfFallbackQuartersForPricing);
        }

        public int UT_ThresholdInSecondsForProgramIntersect { get; set; } = 1800;
        public int UT_NumberOfFallbackQuartersForPricing { get; set; } = 8;

        public void UT_ApplyNTIConversionToNSI(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs)
        {
            ConvertPostingType(plan.PostingType, programs);
        }
    }
}

