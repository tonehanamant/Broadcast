using Common.Services.Repositories;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;

namespace Services.Broadcast.IntegrationTests.UnitTests.CampaignAggregation
{
    [TestFixture]
    public class CampaignAggregatorUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Aggregate()
        {
            const int campaignId = 666;
            var plans = new List<PlanDto>
            {
                new PlanDto
                {
                    Status = PlanStatusEnum.Contracted,
                    Budget = 1000.0m,
                    CPM = 2000.0m,
                    CPP = 3000.0m,
                    DeliveryRatingPoints = 4000.0,
                    DeliveryImpressions = 5000.0,
                    FlightStartDate = new DateTime(2019,08, 01),
                    FlightEndDate = new DateTime(2019, 08, 20),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 8, 15),
                        new DateTime(2019, 8, 17)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 23),
                    ModifiedBy = "TestUserOne",
                    HouseholdDeliveryImpressions = 10000,
                    HouseholdUniverse = 1000000,
                    HouseholdCPM = 0.05m,
                    HouseholdRatingPoints = 2,
                    HouseholdCPP = 1,
                    Universe = 2000000
                },
                new PlanDto
                {
                    Status = PlanStatusEnum.ClientApproval,
                    Budget = 500.0m,
                    CPM = 500.0m,
                    CPP = 500.0m,
                    DeliveryRatingPoints = 500.0,
                    DeliveryImpressions = 500.0,
                    FlightStartDate = new DateTime(2019,08, 21),
                    FlightEndDate = new DateTime(2019, 08, 30),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 8, 18),
                        new DateTime(2019, 8, 19)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 32),
                    ModifiedBy = "TestUserTwo",
                    HouseholdDeliveryImpressions = 10000,
                    HouseholdUniverse = 1000000,
                    HouseholdCPM = 0.05m,
                    HouseholdRatingPoints = 50,
                    HouseholdCPP = 1,
                    Universe = 2000000
                }
            };
            var planRepository = new Mock<IPlanRepository>();
            planRepository.Setup(s => s.GetPlansForCampaign(It.IsAny<int>())).Returns(plans);
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(d => d.GetDataRepository<IPlanRepository>()).Returns(planRepository.Object);
            var tc = new CampaignAggregatorUnitTestClass(dataRepositoryFactory.Object);

            var summary = tc.Aggregate(campaignId);

            planRepository.Verify(c => c.GetPlansForCampaign(campaignId), Times.Once());
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(summary));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Aggregate_WithoutPlans()
        {
            const int campaignId = 666;
            var planRepository = new Mock<IPlanRepository>();
            planRepository.Setup(s => s.GetPlansForCampaign(It.IsAny<int>())).Returns(new List<PlanDto>());
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(d => d.GetDataRepository<IPlanRepository>()).Returns(planRepository.Object);
            var tc = new CampaignAggregatorUnitTestClass(dataRepositoryFactory.Object);

            var summary = tc.Aggregate(campaignId);

            planRepository.Verify(c => c.GetPlansForCampaign(campaignId), Times.Once());
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(summary));
        }

        [Test]
        public void Aggregate_WithError()
        {
            const int campaignId = 666;
            var planRepository = new Mock<IPlanRepository>();
            planRepository.Setup(s => s.GetPlansForCampaign(It.IsAny<int>()))
                .Throws(new Exception("Test Exception thrown from GetPlansForCampaign"));
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(d => d.GetDataRepository<IPlanRepository>()).Returns(planRepository.Object);
            var tc = new CampaignAggregatorUnitTestClass(dataRepositoryFactory.Object);

            var caught = Assert.Throws<Exception>(() => tc.Aggregate(campaignId));

            Assert.AreEqual("Test Exception thrown from GetPlansForCampaign", caught.Message);
            planRepository.Verify(c => c.GetPlansForCampaign(campaignId), Times.Once());
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AggregateFlightInfo_OverlapsWithoutHiatus()
        {
            const int campaignId = 666;
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var tc = new CampaignAggregatorUnitTestClass(dataRepositoryFactory.Object);
            var plans = new List<PlanDto>
            {
                new PlanDto
                {
                    FlightStartDate = new DateTime(2019,08, 01),
                    FlightEndDate = new DateTime(2019, 08, 20)
                },
                new PlanDto
                {
                    FlightStartDate = new DateTime(2019,08, 11),
                    FlightEndDate = new DateTime(2019, 08, 30)
                }
            };
            var summary = new CampaignSummaryDto { CampaignId = campaignId };

            tc.UT_AggregateFlightInfo(plans, summary);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(summary));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AggregateFlightInfo_WithGapsWithoutHiatusDays()
        {
            const int campaignId = 666;
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var tc = new CampaignAggregatorUnitTestClass(dataRepositoryFactory.Object);
            var plans = new List<PlanDto>
            {
                new PlanDto
                {
                    FlightStartDate = new DateTime(2019,08, 01),
                    FlightEndDate = new DateTime(2019, 08, 20)
                },
                new PlanDto
                {
                    FlightStartDate = new DateTime(2019,08, 22),
                    FlightEndDate = new DateTime(2019, 08, 30)
                }
            };
            var summary = new CampaignSummaryDto { CampaignId = campaignId };
            
            tc.UT_AggregateFlightInfo(plans, summary);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(summary));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AggregateFlightInfo_WithHiatusDayOverlaps()
        {
            const int campaignId = 666;
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            var tc = new CampaignAggregatorUnitTestClass(dataRepositoryFactory.Object);
            var plans = new List<PlanDto>
            {
                new PlanDto
                {
                    FlightStartDate = new DateTime(2019,08, 01),
                    FlightEndDate = new DateTime(2019, 08, 20),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 8, 15),
                        new DateTime(2019, 8, 18)
                    }
                },
                new PlanDto
                {
                    FlightStartDate = new DateTime(2019,08, 11),
                    FlightEndDate = new DateTime(2019, 08, 30),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 8, 18),
                        new DateTime(2019, 8, 19)
                    }
                }
            };
            var summary = new CampaignSummaryDto { CampaignId = campaignId };

            tc.UT_AggregateFlightInfo(plans, summary);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(summary));
        }
    }
}