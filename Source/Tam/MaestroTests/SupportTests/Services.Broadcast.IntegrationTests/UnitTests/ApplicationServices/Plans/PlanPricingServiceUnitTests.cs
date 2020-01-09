using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Hangfire;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram.ManifestDaypart;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Plans
{
    [TestFixture]
    public class PlanPricingServiceUnitTests
    {
        private readonly Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private readonly Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private readonly Mock<IPricingApiClient> _PricingApiClientMock;
        private readonly Mock<IBackgroundJobClient> _BackgroundJobClientMock;
        private readonly Mock<IPlanPricingInventoryEngine> _PlanPricingInventoryEngineMock;
        private readonly Mock<IBroadcastLockingManagerApplicationService> _BroadcastLockingManagerApplicationServiceMock;

        public PlanPricingServiceUnitTests()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _PricingApiClientMock = new Mock<IPricingApiClient>();
            _BackgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _PlanPricingInventoryEngineMock = new Mock<IPlanPricingInventoryEngine>();
            _BroadcastLockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AggregateResultsTest()
        {
            var inventory = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    ManifestId = 1,
                    StationLegacyCallLetters = "wnbc",
                    MarketCode = 101,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 1,
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "seinfeld",
                                    MaestroGenre = "News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "seinfeld",
                                MaestroGenre = "News"
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 2,
                    StationLegacyCallLetters = "wabc",
                    MarketCode = 101,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 2
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "seinfeld",
                                    MaestroGenre = "News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "seinfeld",
                                MaestroGenre = "News"
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 3,
                    StationLegacyCallLetters = "kpdx",
                    MarketCode = 100,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 3
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "seinfeld",
                                    MaestroGenre = "News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "seinfeld",
                                MaestroGenre = "News"
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 4,
                    StationLegacyCallLetters = "kabc",
                    MarketCode = 302,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 4
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "seinfeld",
                                    MaestroGenre = "News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "seinfeld",
                                MaestroGenre = "News"
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 5,
                    StationLegacyCallLetters = "wnbc",
                    MarketCode = 101,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 5
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "Good morning america",
                                    MaestroGenre = "Early News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "Good morning america",
                                MaestroGenre = "Early News"
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 6,
                    StationLegacyCallLetters = "wabc",
                    MarketCode = 101,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 6
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "Good morning america",
                                    MaestroGenre = "Early News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "Good morning america",
                                MaestroGenre = "Early News"
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    ManifestId = 7,
                    StationLegacyCallLetters = "kpdx",
                    MarketCode = 100,
                    ManifestDayparts = new List<ManifestDaypart>
                    {
                        new ManifestDaypart
                        {
                            Daypart = new DisplayDaypart
                            {
                                Id = 7
                            },
                            Programs = new List<Program>
                            {
                                new Program
                                {
                                    Name = "Good morning america",
                                    MaestroGenre = "Early News"
                                }
                            },
                            PrimaryProgram = new Program
                            {
                                Name = "Good morning america",
                                MaestroGenre = "Early News"
                            }
                        }
                    }
                }
            };

            var apiResponse = new PlanPricingApiResponsetDto
            {
                Results = new PlanPricingApiResultDto
                {
                    OptimalCpm = 5.78m,
                    Spots = new List<PlanPricingApiResultSpotDto>
                    {
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 1,
                            DaypartId = 1,
                            Spots = 2,
                            Cost = 200,
                            Impressions = 10000,
                        },
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 2,
                            DaypartId = 2,
                            Spots = 4,
                            Cost = 300,
                            Impressions = 50000,
                        },
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 3,
                            DaypartId = 3,
                            Spots = 3,
                            Cost = 500,
                            Impressions = 20000,
                        },
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 4,
                            DaypartId = 4,
                            Spots = 1,
                            Cost = 100,
                            Impressions = 30000,
                        },
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 5,
                            DaypartId = 5,
                            Spots = 3,
                            Cost = 300,
                            Impressions = 10000,
                        },
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 6,
                            DaypartId = 6,
                            Spots = 2,
                            Cost = 400,
                            Impressions = 50000,
                        },
                        new PlanPricingApiResultSpotDto
                        {
                            Id = 7,
                            DaypartId = 7,
                            Spots = 1,
                            Cost = 250,
                            Impressions = 20000,
                        }
                    }
                }
            };

            var service = new PlanPricingService(
                _DataRepositoryFactoryMock.Object,
                _SpotLengthEngineMock.Object,
                _PricingApiClientMock.Object,
                _BackgroundJobClientMock.Object,
                _PlanPricingInventoryEngineMock.Object,
                _BroadcastLockingManagerApplicationServiceMock.Object);

            var result = service.AggregateResults(inventory, apiResponse);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Queued)]
        [TestCase(BackgroundJobProcessingStatus.Processing)]
        public void CanNotQueuePricingJobWhenThereIsOneActive(BackgroundJobProcessingStatus status)
        {
            const string expectedMessage = "The pricing model is already running for the plan";

            var planRepositoryMock = new Mock<IPlanRepository>();
            planRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob { Status = status });

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanRepository>())
                .Returns(planRepositoryMock.Object);

            _BroadcastLockingManagerApplicationServiceMock
                .Setup(x => x.GetNotUserBasedLockObjectForKey(It.IsAny<string>()))
                .Returns(new object());

            var service = new PlanPricingService(
                _DataRepositoryFactoryMock.Object,
                _SpotLengthEngineMock.Object,
                _PricingApiClientMock.Object,
                _BackgroundJobClientMock.Object,
                _PlanPricingInventoryEngineMock.Object,
                _BroadcastLockingManagerApplicationServiceMock.Object);

            var exception = Assert.Throws<Exception>(() => service.QueuePricingJob(
                planPricingParametersDto: new PlanPricingParametersDto(), 
                currentDate: new DateTime(2019, 10, 23)));

            Assert.AreEqual(expectedMessage, exception.Message);
        }
    }
}
