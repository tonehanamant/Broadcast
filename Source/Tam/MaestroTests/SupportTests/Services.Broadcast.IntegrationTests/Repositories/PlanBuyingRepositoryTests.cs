using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Buying;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class PlanBuyingRepositoryTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveGetBuyingApiResults()
        {
            // this data aligns with the db
            const int planVersionId = 9166;

            // this data is stand-alone
            const string requestId = "RequestA";
            const string buyingVersion = "3";
            const string hangfireJobId = "23";
            var job = new PlanBuyingJob
            {
                PlanVersionId = planVersionId,
                Status = BackgroundJobProcessingStatus.Succeeded,
                Queued = new DateTime(2020, 10, 17, 3, 22, 52),
                Completed = new DateTime(2020, 10, 17, 3, 22, 53),
                HangfireJobId = hangfireJobId
            };

            var allocated = new List<PlanBuyingAllocatedSpot>
            {
                new PlanBuyingAllocatedSpot
                {
                    Id = 4340, ContractMediaWeek = new MediaWeek {Id = 721}, InventoryMediaWeek = new MediaWeek {Id = 821}, Impressions30sec = 12346.69, StandardDaypart = new StandardDaypartDto {Id = 12},
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency {SpotLengthId = 1, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 3, Impressions = 659.63},
                        new SpotFrequency {SpotLengthId = 2, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 1, Impressions = 659.63}
                    }
                },
                // 1/2 of split
                new PlanBuyingAllocatedSpot
                {
                    Id = 4341, ContractMediaWeek = new MediaWeek {Id = 721}, InventoryMediaWeek = new MediaWeek {Id = 821}, Impressions30sec = 12346.69, StandardDaypart = new StandardDaypartDto {Id = 12},
                    SpotFrequencies = new List<SpotFrequency> {new SpotFrequency {SpotLengthId = 1, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 3, Impressions = 659.63}}
                }
            };
            var unallocated = new List<PlanBuyingAllocatedSpot>
            {
                // 2/2 of split
                new PlanBuyingAllocatedSpot
                {
                    Id = 4341, ContractMediaWeek = new MediaWeek {Id = 721}, InventoryMediaWeek = new MediaWeek {Id = 821}, Impressions30sec = 12346.69, StandardDaypart = new StandardDaypartDto {Id = 12},
                    SpotFrequencies = new List<SpotFrequency> {new SpotFrequency {SpotLengthId = 2, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 0, Impressions = 659.63}}
                },
                new PlanBuyingAllocatedSpot
                {
                    Id = 4342, ContractMediaWeek = new MediaWeek {Id = 721}, InventoryMediaWeek = new MediaWeek {Id = 821}, Impressions30sec = 12346.69, StandardDaypart = new StandardDaypartDto {Id = 12},
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency {SpotLengthId = 1, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 0, Impressions = 659.63},
                        new SpotFrequency {SpotLengthId = 2, SpotCost = 65.0m, SpotCostWithMargin = 67.0m, Spots = 0, Impressions = 659.63}
                    }
                }
            };

            // Arrange
            var planBuyingAllocationResult = new PlanBuyingAllocationResult
            {
                RequestId = requestId,
                BuyingVersion = buyingVersion,
                BuyingCpm = 123.4m,
                JobId = null,
                PlanVersionId = planVersionId,
                AllocatedSpots = allocated,
                UnallocatedSpots = unallocated,
                SpotAllocationModelMode = SpotAllocationModelMode.Efficiency,
                PostingType = PostingTypeEnum.NSI
            };
            PlanBuyingAllocationResult savedResults;

            var planBuyingRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanBuyingRepository>();
            var jobId = -1;

            using (new TransactionScopeWrapper())
            {
                jobId = planBuyingRepo.AddPlanBuyingJob(job);
                planBuyingAllocationResult.JobId = jobId;

                // Act
                planBuyingRepo.SaveBuyingApiResults(planBuyingAllocationResult);
                savedResults = planBuyingRepo.GetBuyingApiResultsByJobId(jobId, SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);
            }

            // Assert
            Assert.AreEqual(jobId, savedResults.JobId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanBuyingAllocationResult), "JobId");
            jsonResolver.Ignore(typeof(PlanBuyingAllocatedSpot), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedResults, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveGetBuyingRequest()
        {
            // this data aligns with the db
            const int planVersionId = 9166;
            const int planId = 7454;

            // this data is stand-alone
            const string hangfireJobId = "23";
            var job = new PlanBuyingJob
            {
                PlanVersionId = planVersionId,
                Status = BackgroundJobProcessingStatus.Succeeded,
                Queued = new DateTime(2020, 10, 17, 3, 22, 52),
                Completed = new DateTime(2020, 10, 17, 3, 22, 53),
                HangfireJobId = hangfireJobId
            };

            var planBuyingParameters = new PlanBuyingParametersDto
            {
                PlanId = planId,
                PlanVersionId = planVersionId,
                MaxCpm = 100m,
                MinCpm = 1m,
                Budget = 1000,
                CompetitionFactor = 0.1,
                CPM = 5m,
                DeliveryImpressions = 50000,
                InflationFactor = 0.5,
                ProprietaryBlend = 0.2,
                UnitCaps = 10,
                UnitCapsType = UnitCapEnum.PerDay,
                MarketGroup = MarketGroupEnum.None,
                Margin = 20.0,
                AdjustedCPM = 8m,
                AdjustedBudget = 1200
            };

            var planBuyingRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanBuyingRepository>();
            var jobId = -1;

            List<PlanBuyingApiRequestParametersDto> savedResults;

            using (new TransactionScopeWrapper())
            {
                jobId = planBuyingRepo.AddPlanBuyingJob(job);
                planBuyingParameters.JobId = jobId;
                planBuyingRepo.SavePlanBuyingParameters(planBuyingParameters);

                savedResults = planBuyingRepo.GetPlanBuyingRuns(planId);
            }

            // Assert
            var savedJobIds = savedResults.Select(s => s.JobId).Distinct().ToList();
            var savedJobId = savedJobIds.First().Value;
            Assert.AreEqual(1, savedJobIds.Count);
            Assert.IsTrue(savedJobId > 0);
            Assert.AreEqual(jobId, savedJobId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanBuyingApiRequestParametersDto), "JobId");
            // these are not populated 
            jsonResolver.Ignore(typeof(PlanBuyingMarketDto), "MarketName");
            jsonResolver.Ignore(typeof(PlanBuyingMarketDto), "MarketSegment");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedResults, jsonSettings));
        }
    }
}