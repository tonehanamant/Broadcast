using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.UnitTests.ScxFileGeneration;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.UnitTests.InventoryRatingsProcessing
{
    [Category("short_running")]
    public class InventoryRatingsProcessingServiceUnitTests
    {
        #region Ensure ProcessInventoryRatingsJob Enqueued
        [Test]
        public void ProcessInventoryRatingsJobEnqueued()
        {
            var inventoryFileRatingsJobRepository = new Mock<IInventoryFileRatingsJobsRepository>();
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<IInventoryFileRatingsJobsRepository>())
                .Returns(inventoryFileRatingsJobRepository.Object);
            var impressionService = new Mock<IImpressionsService>();
            var inventoryProprietarySummaryService = new Mock<IInventoryProprietarySummaryService>();
         
           var proprietarySpotCostCalculationEngine = new Mock<IProprietarySpotCostCalculationEngine>();
            var nsiPostingBookService = new Mock<INsiPostingBookService>();
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var backgroundJobClient = new Mock<IBackgroundJobClient>();
            const int jobId = 5;
            inventoryFileRatingsJobRepository.Setup(x => x.AddJob(It.IsAny<InventoryFileRatingsProcessingJob>()))
                .Returns(jobId);
            var tc = new InventoryRatingsProcessingServiceUnitTestClass(dataRepoFactory.Object, inventoryProprietarySummaryService.Object,
                impressionService.Object, proprietarySpotCostCalculationEngine.Object, nsiPostingBookService.Object,
                mediaMonthAndWeekAggregateCache.Object, backgroundJobClient.Object, featureToggleHelper.Object);

            tc.QueueInventoryFileRatingsJob(It.IsAny<int>());

            backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "ProcessInventoryRatingsJob" && 
                                  job.Args[0].ToString().Equals(jobId.ToString(), StringComparison.OrdinalIgnoreCase)),
                It.IsAny<EnqueuedState>()));
        }
        #endregion

    }
}
