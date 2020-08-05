using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.UnitTests.InventoryRatingsProcessing
{
    public class InventoryRatingsProcessingServiceUnitTestClass : InventoryRatingsProcessingService
    {
        public InventoryRatingsProcessingServiceUnitTestClass(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IInventoryProprietarySummaryService svc,
            IImpressionsService impressionsService,
            IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine,
            INsiPostingBookService nsiPostingBookService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBackgroundJobClient backgroundJobClient, IFeatureToggleHelper featureToggleHelper
            )
            : base(broadcastDataRepositoryFactory,svc,
                impressionsService, proprietarySpotCostCalculationEngine, nsiPostingBookService,
                mediaMonthAndWeekAggregateCache, backgroundJobClient, featureToggleHelper)
        {
        }
    }
}
