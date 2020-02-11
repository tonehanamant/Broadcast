using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanPricingInventoryEngineTestClass : PlanPricingInventoryEngine
    {
        public PlanPricingInventoryEngineTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionsCalculationEngine impressionsCalculationEngine,
            IGenreCache genreCache)
            : base(broadcastDataRepositoryFactory, impressionsCalculationEngine, genreCache)
        { }

        public List<PlanPricingInventoryProgram> UT_FilterProgramsByDayparts(
            PlanDto plan,
            List<PlanPricingInventoryProgram> programs,
            List<DateRange> planFlightDateRanges,
            double? inflationFactor = null)
        {
            return _FilterProgramsByDayparts(plan, programs, planFlightDateRanges);
        }

        public List<PlanPricingInventoryProgram> UT_FilterProgramsByMinAndMaxCPM(
            List<PlanPricingInventoryProgram> programs,
            decimal? minCPM,
            decimal? maxCPM)
        {
            return _FilterProgramsByMinAndMaxCPM(programs, minCPM, maxCPM);
        }

        public void UT_ApplyInflationFactorToSpotCost(PlanPricingInventoryProgram program, double? inflationFactor)
        {
            _ApplyInflationFactorToSpotCost(program, inflationFactor);
        }
    }
}
