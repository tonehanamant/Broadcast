using Common.Services.Repositories;
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
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.CampaignAggregation
{
    [TestFixture]
    [Category("short_running")]
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
                    TargetCPM = 2000.0m,
                    TargetCPP = 3000.0m,
                    TargetRatingPoints = 4000.0,
                    TargetImpressions = 5000.0,
                    FlightStartDate = new DateTime(2019,08, 01),
                    FlightEndDate = new DateTime(2019, 08, 20),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 8, 15),
                        new DateTime(2019, 8, 17)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 23),
                    ModifiedBy = "TestUserOne",
                    HHImpressions = 10000,
                    HHUniverse = 1000000,
                    HHCPM = 0.05m,
                    HHRatingPoints = 2,
                    HHCPP = 1,
                    TargetUniverse = 2000000,
                    HhAduImpressions = 5000,
                    AduImpressions = 1200
                },
                new PlanDto
                {
                    Status = PlanStatusEnum.ClientApproval,
                    Budget = 500.0m,
                    TargetCPM = 500.0m,
                    TargetCPP = 500.0m,
                    TargetRatingPoints = 500.0,
                    TargetImpressions = 500.0,
                    FlightStartDate = new DateTime(2019,08, 21),
                    FlightEndDate = new DateTime(2019, 08, 30),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 8, 18),
                        new DateTime(2019, 8, 19)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 32),
                    ModifiedBy = "TestUserTwo",
                    HHImpressions = 10000,
                    HHUniverse = 1000000,
                    HHCPM = 0.05m,
                    HHRatingPoints = 50,
                    HHCPP = 1,
                    TargetUniverse = 2000000,
                    HhAduImpressions = 5000,
                    AduImpressions = 1200
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
        public void Aggregate_IgnoreCanceledRejectedAndScenarioStatusPlans()
        {
            const int campaignId = 420;
            var plans = new List<PlanDto>
            {
                new PlanDto
                {
                    Status = PlanStatusEnum.Contracted,
                    Budget = 1000.0m,
                    TargetCPM = 2000.0m,
                    TargetCPP = 3000.0m,
                    TargetRatingPoints = 4000.0,
                    TargetImpressions = 5000.0,
                    FlightStartDate = new DateTime(2019, 08, 01),
                    FlightEndDate = new DateTime(2019, 08, 20),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 8, 15),
                        new DateTime(2019, 8, 17)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 23),
                    ModifiedBy = "TestUserOne",
                    HHImpressions = 10000,
                    HHUniverse = 1000000,
                    HHCPM = 0.05m,
                    HHRatingPoints = 2,
                    HHCPP = 1,
                    TargetUniverse = 2000000,
                    HhAduImpressions = 5000,
                    AduImpressions = 1200
                },
                new PlanDto
                {
                    Status = PlanStatusEnum.ClientApproval,
                    Budget = 500.0m,
                    TargetCPM = 500.0m,
                    TargetCPP = 500.0m,
                    TargetRatingPoints = 500.0,
                    TargetImpressions = 500.0,
                    FlightStartDate = new DateTime(2019, 09, 21),
                    FlightEndDate = new DateTime(2019, 09, 30),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 9, 25),
                        new DateTime(2019, 9, 26)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 32),
                    ModifiedBy = "TestUserTwo",
                    HHImpressions = 10000,
                    HHUniverse = 1000000,
                    HHCPM = 0.05m,
                    HHRatingPoints = 50,
                    HHCPP = 1,
                    TargetUniverse = 2000000,
                    HhAduImpressions = 5000,
                    AduImpressions = 1200
                },
                new PlanDto
                {
                    Status = PlanStatusEnum.Rejected,
                    Budget = 500.0m,
                    TargetCPM = 500.0m,
                    TargetCPP = 500.0m,
                    TargetRatingPoints = 500.0,
                    TargetImpressions = 500.0,
                    FlightStartDate = new DateTime(2019, 09, 10),
                    FlightEndDate = new DateTime(2019, 09, 20),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 9, 12),
                        new DateTime(2019, 9, 19)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 32),
                    ModifiedBy = "TestUserTwo",
                    HHImpressions = 10000,
                    HHUniverse = 1000000,
                    HHCPM = 0.05m,
                    HHRatingPoints = 50,
                    HHCPP = 1,
                    TargetUniverse = 2000000,
                    HhAduImpressions = 5000,
                    AduImpressions = 1200
                },
                new PlanDto
                {
                    Status = PlanStatusEnum.Canceled,
                    Budget = 500.0m,
                    TargetCPM = 500.0m,
                    TargetCPP = 500.0m,
                    TargetRatingPoints = 500.0,
                    TargetImpressions = 500.0,
                    FlightStartDate = new DateTime(2019, 09, 01),
                    FlightEndDate = new DateTime(2019, 09, 09),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 09, 07)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 32),
                    ModifiedBy = "TestUserTwo",
                    HHImpressions = 10000,
                    HHUniverse = 1000000,
                    HHCPM = 0.05m,
                    HHRatingPoints = 50,
                    HHCPP = 1,
                    TargetUniverse = 2000000,
                    HhAduImpressions = 5000,
                    AduImpressions = 1200
                },
                new PlanDto
                {
                    Status = PlanStatusEnum.Scenario,
                    Budget = 500.0m,
                    TargetCPM = 500.0m,
                    TargetCPP = 500.0m,
                    TargetRatingPoints = 500.0,
                    TargetImpressions = 500.0,
                    FlightStartDate = new DateTime(2019, 10, 01),
                    FlightEndDate = new DateTime(2019, 10, 31),
                    FlightHiatusDays = new List<DateTime>
                    {
                        new DateTime(2019, 10, 18),
                        new DateTime(2019, 10, 19)
                    },
                    ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 32),
                    ModifiedBy = "TestUserTwo",
                    HHImpressions = 10000,
                    HHUniverse = 1000000,
                    HHCPM = 0.05m,
                    HHRatingPoints = 50,
                    HHCPP = 1,
                    TargetUniverse = 2000000,
                    HhAduImpressions = 5000,
                    AduImpressions = 1200
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

            tc.UT_AggregateFlightInfo(plans, plans, summary);

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

            tc.UT_AggregateFlightInfo(plans, plans.Where(p => p.Status != PlanStatusEnum.Scenario).ToList(), summary);

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

            tc.UT_AggregateFlightInfo(plans, plans, summary);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(summary));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Aggregate_ExcludesDrafts()
        {
            const int campaignId = 420;
            var plans = new List<PlanDto>
            {
              new PlanDto
              {
                Status = PlanStatusEnum.Contracted,
                Budget = 1000.0m,
                TargetCPM = 2000.0m,
                TargetCPP = 3000.0m,
                TargetRatingPoints = 4000.0,
                TargetImpressions = 5000.0,
                FlightStartDate = new DateTime(2019, 08, 01),
                FlightEndDate = new DateTime(2019, 08, 20),
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 8, 15),
                    new DateTime(2019, 8, 17)
                },
                ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 23),
                ModifiedBy = "TestUserOne",
                HHImpressions = 10000,
                HHUniverse = 1000000,
                HHCPM = 0.05m,
                HHRatingPoints = 2,
                HHCPP = 1,
                TargetUniverse = 2000000,
                HhAduImpressions = 5000,
                AduImpressions = 1200,
                IsDraft = false
              },
              new PlanDto
              {
                Status = PlanStatusEnum.Contracted,
                Budget = 500.0m,
                TargetCPM = 500.0m,
                TargetCPP = 500.0m,
                TargetRatingPoints = 500.0,
                TargetImpressions = 500.0,
                FlightStartDate = new DateTime(2019, 09, 21),
                FlightEndDate = new DateTime(2019, 09, 30),
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 9, 25),
                    new DateTime(2019, 9, 26)
                },
                ModifiedDate = new DateTime(2019, 08, 28, 12, 30, 32),
                ModifiedBy = "TestUserTwo",
                HHImpressions = 10000,
                HHUniverse = 1000000,
                HHCPM = 0.05m,
                HHRatingPoints = 50,
                HHCPP = 1,
                TargetUniverse = 2000000,
                HhAduImpressions = 5000,
                AduImpressions = 1200,
                IsDraft = true
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
    }
}