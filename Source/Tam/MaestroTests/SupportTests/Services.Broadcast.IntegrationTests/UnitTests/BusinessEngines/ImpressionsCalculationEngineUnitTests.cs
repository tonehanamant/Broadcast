using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class ImpressionsCalculationEngineUnitTests
    {
        private ImpressionsCalculationEngine _ImpressionsCalculationEngine;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<IImpressionAdjustmentEngine> _ImpressionAdjustmentEngineMock;
        private Mock<ISpotLengthEngine> _SpotLengthEngineMock;
        private Mock<IBroadcastAudienceRepository> _BroadcastAudienceRepositoryMock;
        private Mock<IRatingForecastRepository> _RatingForecastRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _ImpressionAdjustmentEngineMock = new Mock<IImpressionAdjustmentEngine>();
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
            _BroadcastAudienceRepositoryMock = new Mock<IBroadcastAudienceRepository>();
            _RatingForecastRepositoryMock = new Mock<IRatingForecastRepository>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IBroadcastAudienceRepository>())
                .Returns(_BroadcastAudienceRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IRatingForecastRepository>())
                .Returns(_RatingForecastRepositoryMock.Object);

            _ImpressionsCalculationEngine = new ImpressionsCalculationEngine(
                _DataRepositoryFactoryMock.Object,
                _ImpressionAdjustmentEngineMock.Object,
                _SpotLengthEngineMock.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AppliesProjectedImpressions_SingleBook()
        {
            // Arrange
            const int audienceId = 5;
            const int spotLength = 90;
            const int spotLengthId = 8;
            const int shareBookId = 215;

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KOB"
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            Daypart = new DisplayDaypart
                            {
                                Id = 2,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        },
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 3,
                            Daypart = new DisplayDaypart
                            {
                                Id = 4,
                                Thursday = true,
                                Friday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KSTP"
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 5,
                            Daypart = new DisplayDaypart
                            {
                                Id = 6,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 72000, // 8pm
                                EndTime = 75599 // 9pm
                            }
                        }
                    }
                }
            };
            
            var request = new ImpressionsRequestDto
            {
                SpotLengthId = spotLengthId,
                HutProjectionBookId = null,
                ShareProjectionBookId = shareBookId,
                PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3,
                Equivalized = true,
                PostType = PostingTypeEnum.NTI
            };

            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 7 },
                    new audience_audiences { rating_audience_id = 9 }
                });

            _SpotLengthEngineMock
                .Setup(x => x.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns(spotLength);

            object getImpressionsDaypartParameters = null;
            _RatingForecastRepositoryMock
                .Setup(x => x.GetImpressionsDaypart(
                    It.IsAny<int>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<ManifestDetailDaypart>>(),
                    It.IsAny<ProposalEnums.ProposalPlaybackType?>()))
                .Callback<int, List<int>, List<ManifestDetailDaypart>, ProposalEnums.ProposalPlaybackType?>(
                    (postingBookId, uniqueRatingsAudiences, stationDetails, playbackType) =>
                    {
                        // deep copy
                        getImpressionsDaypartParameters = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
                        {
                            postingBookId,
                            uniqueRatingsAudiences,
                            stationDetails,
                            playbackType
                        })));
                    })
                .Returns(new ImpressionsDaypartResultForSingleBook
                {
                    Impressions = new List<StationImpressionsWithAudience>
                    {
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 7,
                            Id = 1,
                            Impressions = 1000
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 9,
                            Id = 1,
                            Impressions = 1100
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 7,
                            Id = 3,
                            Impressions = 1200
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 9,
                            Id = 3,
                            Impressions = 1300
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 7,
                            Id = 5,
                            Impressions = 1400
                        }
                    }
                });

            _ImpressionAdjustmentEngineMock
                .Setup(x => x.AdjustImpression(
                    It.IsAny<double>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<PostingTypeEnum>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Returns<double, bool, int, PostingTypeEnum, int, bool>(
                    (impression, isEquivilized, spotLengthValue, postType, schedulePostingBook, applyAnnualAdjustment) =>
                    {
                        return impression * 1.5;
                    });

            // Act
            _ImpressionsCalculationEngine.ApplyProjectedImpressions(programs, request, audienceId);

            // Assert
            _BroadcastAudienceRepositoryMock.Verify(x => x.GetRatingsAudiencesByMaestroAudience(
                It.Is<List<int>>(list => list.SequenceEqual(new List<int> { audienceId }))), Times.Once);

            _SpotLengthEngineMock.Verify(x => x.GetSpotLengthValueById(spotLengthId), Times.Exactly(1));

            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1000, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1100, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1200, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1300, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1400, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);

            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                getImpressionsDaypartParameters,
                programs
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AppliesHouseholdProjectedImpressions_SingleBook()
        {
            // Arrange           
            int householdAudienceId = BroadcastConstants.HouseholdAudienceId;
            const int spotLength = 90;
            const int spotLengthId = 8;
            const int shareBookId = 215;

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KOB"
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            Daypart = new DisplayDaypart
                            {
                                Id = 2,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        },
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 3,
                            Daypart = new DisplayDaypart
                            {
                                Id = 4,
                                Thursday = true,
                                Friday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KSTP"
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 5,
                            Daypart = new DisplayDaypart
                            {
                                Id = 6,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 72000, // 8pm
                                EndTime = 75599 // 9pm
                            }
                        }
                    }
                }
            };

            var request = new ImpressionsRequestDto
            {
                SpotLengthId = spotLengthId,
                HutProjectionBookId = null,
                ShareProjectionBookId = shareBookId,
                PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3,
                Equivalized = true,
                PostType = PostingTypeEnum.NTI
            };

            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 31 }                   
                });

            _SpotLengthEngineMock
                .Setup(x => x.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns(spotLength);

            object getImpressionsDaypartParameters = null;
            _RatingForecastRepositoryMock
                .Setup(x => x.GetImpressionsDaypart(
                    It.IsAny<int>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<ManifestDetailDaypart>>(),
                    It.IsAny<ProposalEnums.ProposalPlaybackType?>()))
                .Callback<int, List<int>, List<ManifestDetailDaypart>, ProposalEnums.ProposalPlaybackType?>(
                    (postingBookId, uniqueRatingsAudiences, stationDetails, playbackType) =>
                    {
                        // deep copy
                        getImpressionsDaypartParameters = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
                        {
                            postingBookId,
                            uniqueRatingsAudiences,
                            stationDetails,
                            playbackType
                        })));
                    })
                .Returns(new ImpressionsDaypartResultForSingleBook
                {
                    Impressions = new List<StationImpressionsWithAudience>
                    {
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 1,
                            Impressions = 1000
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 1,
                            Impressions = 1100
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 3,
                            Impressions = 1200
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 3,
                            Impressions = 1300
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 5,
                            Impressions = 1400
                        }
                    }
                });

            _ImpressionAdjustmentEngineMock
                .Setup(x => x.AdjustImpression(
                    It.IsAny<double>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<PostingTypeEnum>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Returns<double, bool, int, PostingTypeEnum, int, bool>(
                    (impression, isEquivilized, spotLengthValue, postType, schedulePostingBook, applyAnnualAdjustment) =>
                    {
                        return impression * 1.5;
                    });

            // Act
            _ImpressionsCalculationEngine.ApplyHouseholdProjectedImpressions(programs, request);

            // Assert
            _BroadcastAudienceRepositoryMock.Verify(x => x.GetRatingsAudiencesByMaestroAudience(
                It.Is<List<int>>(list => list.SequenceEqual(new List<int> { householdAudienceId }))), Times.Once);

            _SpotLengthEngineMock.Verify(x => x.GetSpotLengthValueById(spotLengthId), Times.Exactly(1));

            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1000, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1100, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1200, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1300, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1400, true, spotLength, PostingTypeEnum.NTI, shareBookId, false), Times.Once);

            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                getImpressionsDaypartParameters,
                programs
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AppliesProjectedImpressions_TwoBooks()
        {
            // Arrange
            const int audienceId = 6;
            const int spotLength = 60;
            const int spotLengthId = 7;
            const int shareBookId = 215;
            const int hutBookId = 216;

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KOB"
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            Daypart = new DisplayDaypart
                            {
                                Id = 2,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        },
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 3,
                            Daypart = new DisplayDaypart
                            {
                                Id = 4,
                                Thursday = true,
                                Friday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KSTP"
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 5,
                            Daypart = new DisplayDaypart
                            {
                                Id = 6,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 72000, // 8pm
                                EndTime = 75599 // 9pm
                            }
                        }
                    }
                }
            };

            var request = new ImpressionsRequestDto
            {
                SpotLengthId = spotLengthId,
                HutProjectionBookId = hutBookId,
                ShareProjectionBookId = shareBookId,
                PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus1,
                Equivalized = false,
                PostType = PostingTypeEnum.NSI
            };

            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 7 },
                    new audience_audiences { rating_audience_id = 9 }
                });

            _SpotLengthEngineMock
                .Setup(x => x.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns(spotLength);

            object getImpressionsDaypartParameters = null;
            _RatingForecastRepositoryMock
                .Setup(x => x.GetImpressionsDaypart(
                    It.IsAny<short>(),
                    It.IsAny<short>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<List<ManifestDetailDaypart>>(),
                    It.IsAny<ProposalEnums.ProposalPlaybackType?>()))
                .Callback<short, short, IEnumerable<int>, List<ManifestDetailDaypart>, ProposalEnums.ProposalPlaybackType?>(
                    (hutBook, shareBook, uniqueRatingsAudiences, stationDetails, playbackType) =>
                    {
                        // deep copy
                        getImpressionsDaypartParameters = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
                        {
                            hutBook,
                            shareBook,
                            uniqueRatingsAudiences,
                            stationDetails,
                            playbackType
                        })));
                    })
                .Returns(new ImpressionsDaypartResultForTwoBooks
                {
                    Impressions = new List<StationImpressionsWithAudience>
                    {
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 7,
                            Id = 1,
                            Impressions = 1000
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 9,
                            Id = 1,
                            Impressions = 1100
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 7,
                            Id = 3,
                            Impressions = 1200
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 9,
                            Id = 3,
                            Impressions = 1300
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 7,
                            Id = 5,
                            Impressions = 1400
                        }
                    }
                });

            _ImpressionAdjustmentEngineMock
                .Setup(x => x.AdjustImpression(
                    It.IsAny<double>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<PostingTypeEnum>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Returns<double, bool, int, PostingTypeEnum, int, bool>(
                    (impression, isEquivilized, spotLengthValue, postType, schedulePostingBook, applyAnnualAdjustment) =>
                    {
                        return impression * 1.5;
                    });

            // Act
            _ImpressionsCalculationEngine.ApplyProjectedImpressions(programs, request, audienceId);

            // Assert
            _BroadcastAudienceRepositoryMock.Verify(x => x.GetRatingsAudiencesByMaestroAudience(
                It.Is<List<int>>(list => list.SequenceEqual(new List<int> { audienceId }))), Times.Once);

            _SpotLengthEngineMock.Verify(x => x.GetSpotLengthValueById(spotLengthId), Times.Exactly(1));

            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1000, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1100, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1200, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1300, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1400, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);

            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                getImpressionsDaypartParameters,
                programs
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AppliesHouseholdProjectedImpressions_TwoBooks()
        {
            // Arrange
            int householdAudienceId = BroadcastConstants.HouseholdAudienceId;
            const int spotLength = 60;
            const int spotLengthId = 7;
            const int shareBookId = 215;
            const int hutBookId = 216;

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KOB"
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 1,
                            Daypart = new DisplayDaypart
                            {
                                Id = 2,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        },
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 3,
                            Daypart = new DisplayDaypart
                            {
                                Id = 4,
                                Thursday = true,
                                Friday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        }
                    }
                },
                new PlanPricingInventoryProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KSTP"
                    },
                    ManifestDayparts = new List<PlanPricingInventoryProgram.ManifestDaypart>
                    {
                        new PlanPricingInventoryProgram.ManifestDaypart
                        {
                            Id = 5,
                            Daypart = new DisplayDaypart
                            {
                                Id = 6,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 72000, // 8pm
                                EndTime = 75599 // 9pm
                            }
                        }
                    }
                }
            };

            var request = new ImpressionsRequestDto
            {
                SpotLengthId = spotLengthId,
                HutProjectionBookId = hutBookId,
                ShareProjectionBookId = shareBookId,
                PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus1,
                Equivalized = false,
                PostType = PostingTypeEnum.NSI
            };

            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 31 }                   
                });

            _SpotLengthEngineMock
                .Setup(x => x.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns(spotLength);

            object getImpressionsDaypartParameters = null;
            _RatingForecastRepositoryMock
                .Setup(x => x.GetImpressionsDaypart(
                    It.IsAny<short>(),
                    It.IsAny<short>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<List<ManifestDetailDaypart>>(),
                    It.IsAny<ProposalEnums.ProposalPlaybackType?>()))
                .Callback<short, short, IEnumerable<int>, List<ManifestDetailDaypart>, ProposalEnums.ProposalPlaybackType?>(
                    (hutBook, shareBook, uniqueRatingsAudiences, stationDetails, playbackType) =>
                    {
                        // deep copy
                        getImpressionsDaypartParameters = JsonConvert.DeserializeObject((JsonConvert.SerializeObject(new
                        {
                            hutBook,
                            shareBook,
                            uniqueRatingsAudiences,
                            stationDetails,
                            playbackType
                        })));
                    })
                .Returns(new ImpressionsDaypartResultForTwoBooks
                {
                    Impressions = new List<StationImpressionsWithAudience>
                    {
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 1,
                            Impressions = 1000
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 1,
                            Impressions = 1100
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 3,
                            Impressions = 1200
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 3,
                            Impressions = 1300
                        },
                        new StationImpressionsWithAudience
                        {
                            AudienceId = 31,
                            Id = 5,
                            Impressions = 1400
                        }
                    }
                });

            _ImpressionAdjustmentEngineMock
                .Setup(x => x.AdjustImpression(
                    It.IsAny<double>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>(),
                    It.IsAny<PostingTypeEnum>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Returns<double, bool, int, PostingTypeEnum, int, bool>(
                    (impression, isEquivilized, spotLengthValue, postType, schedulePostingBook, applyAnnualAdjustment) =>
                    {
                        return impression * 1.5;
                    });

            // Act
            _ImpressionsCalculationEngine.ApplyHouseholdProjectedImpressions(programs, request);

            // Assert
            _BroadcastAudienceRepositoryMock.Verify(x => x.GetRatingsAudiencesByMaestroAudience(
                It.Is<List<int>>(list => list.SequenceEqual(new List<int> { householdAudienceId }))), Times.Once);

            _SpotLengthEngineMock.Verify(x => x.GetSpotLengthValueById(spotLengthId), Times.Exactly(1));

            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1000, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1100, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1200, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1300, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1400, false, spotLength, PostingTypeEnum.NSI, hutBookId, false), Times.Once);

            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                getImpressionsDaypartParameters,
                programs
            });

            Approvals.Verify(resultJson);
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AppliesProvidedImpressions()
        {
            // Arrange
            const int audienceId = 6;
            const int spotLengthId = 7;

            var programs = new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                    SpotLengthId = 1,
                    ManifestAudiences = new List<PlanPricingInventoryProgram.ManifestAudience>
                    {
                        new PlanPricingInventoryProgram.ManifestAudience
                        {
                            AudienceId = audienceId,
                            Impressions = 1000
                        }
                    }
                }
            };

            _SpotLengthEngineMock
                .Setup(x => x.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns<int>(id => id == 1 ? 30 : 60);

            _ImpressionAdjustmentEngineMock
                .Setup(x => x.AdjustImpression(
                    It.IsAny<double>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>()))
                .Returns<double, bool?, int>((impression, isEquivilized, spotLengthValue) => impression * 1.5);

            // Act
            _ImpressionsCalculationEngine.ApplyProvidedImpressions(programs, audienceId, spotLengthId, equivalized: true);

            // Assert
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1000, true, 60), Times.Once);

            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                programs
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AppliesProvidedImpressions_QuoteReport()
        {
            // Arrange
            var programs = new List<QuoteProgram>
            {
                new QuoteProgram
                {
                    DeliveryPerAudience = new List<QuoteProgram.ImpressionsPerAudience>
                    {
                        new QuoteProgram.ImpressionsPerAudience
                        {
                            AudienceId = 1,
                            ProjectedImpressions = 2000,
                        },
                        new QuoteProgram.ImpressionsPerAudience
                        {
                            AudienceId = 2,
                            ProjectedImpressions = 3000,
                        }
                    },
                    ManifestAudiences = new List<QuoteProgram.ManifestAudience>
                    {
                        new QuoteProgram.ManifestAudience
                        {
                            AudienceId = 1,
                            Impressions = 1000
                        }
                    }
                }
            };

            _SpotLengthEngineMock
                .Setup(x => x.GetSpotLengthValueById(It.IsAny<int>()))
                .Returns(60);

            _ImpressionAdjustmentEngineMock
                .Setup(x => x.AdjustImpression(
                    It.IsAny<double>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>()))
                .Returns<double, bool?, int>((impression, isEquivilized, spotLengthValue) => impression * 1.5);

            // Act
            _ImpressionsCalculationEngine.ApplyProvidedImpressions(programs, spotLengthId: 7, equivalized: true);

            // Assert
            _ImpressionAdjustmentEngineMock.Verify(x => x.AdjustImpression(1000, true, 60), Times.Once);

            var resultJson = IntegrationTestHelper.ConvertToJson(new
            {
                programs
            });

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AppliesProjectedImpressions_QuoteReport_WithHutBook()
        {
            var request = new ImpressionsRequestDto
            {
                SpotLengthId = 2,
                HutProjectionBookId = 101,
                ShareProjectionBookId = 102,
                PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus1,
                Equivalized = true,
                PostType = PostingTypeEnum.NTI
            };

            Func<List<object>> getImpressionsFunc = () =>
            {
                var parametersGetImpressionsDaypart = new List<object>();
                _RatingForecastRepositoryMock
                    .Setup(x => x.GetImpressionsDaypart(
                        It.IsAny<short>(),
                        It.IsAny<short>(),
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<List<ManifestDetailDaypart>>(),
                        It.IsAny<ProposalPlaybackType?>()))
                    .Callback((
                        short hutMediaMonth,
                        short shareMediaMonth,
                        IEnumerable<int> uniqueRatingsAudiences,
                        List<ManifestDetailDaypart> stationDetails,
                        ProposalPlaybackType? playbackType) =>
                    {
                        // deep copy
                        parametersGetImpressionsDaypart.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                        {
                            hutMediaMonth,
                            shareMediaMonth,
                            uniqueRatingsAudiences,
                            stationDetails,
                            playbackType
                        })));
                    })
                    .Returns(new ImpressionsDaypartResultForTwoBooks
                    {
                        Impressions = _GetImpressions()
                    });

                return parametersGetImpressionsDaypart;
            };

            var resultJson = _AppliesProjectedImpressions_QuoteReport(request, getImpressionsFunc);

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AppliesProjectedImpressions_QuoteReport_WithoutHutBook()
        {
            var request = new ImpressionsRequestDto
            {
                SpotLengthId = 2,
                HutProjectionBookId = null,
                ShareProjectionBookId = 102,
                PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus1,
                Equivalized = true,
                PostType = PostingTypeEnum.NTI
            };

            Func<List<object>> getImpressionsFunc = () =>
            {
                var parametersGetImpressionsDaypart = new List<object>();
                _RatingForecastRepositoryMock
                    .Setup(x => x.GetImpressionsDaypart(
                        It.IsAny<int>(),
                        It.IsAny<List<int>>(),
                        It.IsAny<List<ManifestDetailDaypart>>(),
                        It.IsAny<ProposalPlaybackType?>()))
                    .Callback((
                        int shareMediaMonth,
                        List<int> uniqueRatingsAudiences,
                        List<ManifestDetailDaypart> stationDetails,
                        ProposalPlaybackType? playbackType) =>
                    {
                        // deep copy
                        parametersGetImpressionsDaypart.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new
                        {
                            shareMediaMonth,
                            uniqueRatingsAudiences,
                            stationDetails,
                            playbackType
                        })));
                    })
                    .Returns(new ImpressionsDaypartResultForSingleBook
                    {
                        Impressions = _GetImpressions()
                    });

                return parametersGetImpressionsDaypart;
            };

            var resultJson = _AppliesProjectedImpressions_QuoteReport(request, getImpressionsFunc);

            Approvals.Verify(resultJson);
        }

        private string _AppliesProjectedImpressions_QuoteReport(ImpressionsRequestDto request, Func<List<object>> setupGetImpressions)
        {
            // Arrange
            var programs = _GetQuotePrograms();

            var audienceIds = new List<int> { 2, 3 };

            var parametersGetRatingsAudiencesByMaestroAudience = new List<object>();
            _BroadcastAudienceRepositoryMock
                .Setup(x => x.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Callback((List<int> audiences) => parametersGetRatingsAudiencesByMaestroAudience.Add(audiences))
                .Returns(new List<audience_audiences>
                {
                    new audience_audiences { rating_audience_id = 7, custom_audience_id = 2 },
                    new audience_audiences { rating_audience_id = 9, custom_audience_id = 2 },
                    new audience_audiences { rating_audience_id = 9, custom_audience_id = 3 }
                });

            var parametersGetImpressionsDaypart = setupGetImpressions();

            var parametersGetSpotLengthValueById = new List<object>();
            _SpotLengthEngineMock
                .Setup(x => x.GetSpotLengthValueById(It.IsAny<int>()))
                .Callback((int spotLengthId) => parametersGetSpotLengthValueById.Add(new { spotLengthId }))
                .Returns(60);

            var parametersAdjustImpression = new List<object>();
            _ImpressionAdjustmentEngineMock
                .Setup(x => x.AdjustImpression(
                    It.IsAny<double>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<PostingTypeEnum?>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Callback<double, bool?, int, PostingTypeEnum?, int, bool>(
                    (impressions, isEquivilized, spotLengthValue, postType, schedulePostingBook, applyAnnualAdjustment) =>
                    {
                        parametersAdjustImpression.Add(new
                        {
                            impressions,
                            isEquivilized,
                            spotLengthValue,
                            postType,
                            schedulePostingBook,
                            applyAnnualAdjustment
                        });
                    })
                .Returns<double, bool?, int, PostingTypeEnum?, int, bool>(
                    (impressions, isEquivilized, spotLengthValue, postType, schedulePostingBook, applyAnnualAdjustment) =>
                    {
                        return impressions * 1.5;
                    });

            // Act
            _ImpressionsCalculationEngine.ApplyProjectedImpressions(programs, request, audienceIds);

            // Assert
            return IntegrationTestHelper.ConvertToJson(new
            {
                programs,
                parametersGetRatingsAudiencesByMaestroAudience,
                parametersGetImpressionsDaypart,
                parametersGetSpotLengthValueById,
                parametersAdjustImpression,
            });
        }

        private List<QuoteProgram> _GetQuotePrograms()
        {
            return new List<QuoteProgram>
            {
                new QuoteProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KOB"
                    },
                    ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                    {
                        new QuoteProgram.ManifestDaypart
                        {
                            Id = 1,
                            Daypart = new DisplayDaypart
                            {
                                Id = 2,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        },
                        new QuoteProgram.ManifestDaypart
                        {
                            Id = 3,
                            Daypart = new DisplayDaypart
                            {
                                Id = 4,
                                Thursday = true,
                                Friday = true,
                                StartTime = 18000, // 5am
                                EndTime = 21599 // 6am
                            }
                        }
                    }
                },
                new QuoteProgram
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "KSTP"
                    },
                    ManifestDayparts = new List<QuoteProgram.ManifestDaypart>
                    {
                        new QuoteProgram.ManifestDaypart
                        {
                            Id = 5,
                            Daypart = new DisplayDaypart
                            {
                                Id = 6,
                                Monday = true,
                                Wednesday = true,
                                StartTime = 72000, // 8pm
                                EndTime = 75599 // 9pm
                            }
                        }
                    }
                }
            };
        }

        private List<StationImpressionsWithAudience> _GetImpressions()
        {
            return new List<StationImpressionsWithAudience>
            {
                new StationImpressionsWithAudience
                {
                    Id = 1,
                    AudienceId = 7,
                    Impressions = 1000
                },
                new StationImpressionsWithAudience
                {
                    Id = 1,
                    AudienceId = 9,
                    Impressions = 2000
                },
                new StationImpressionsWithAudience
                {
                    Id = 3,
                    AudienceId = 7,
                    Impressions = 3000
                },
                new StationImpressionsWithAudience
                {
                    Id = 3,
                    AudienceId = 9,
                    Impressions = 4000
                },
                new StationImpressionsWithAudience
                {
                    Id = 5,
                    AudienceId = 7,
                    Impressions = 5000
                }
            };
        }
    }
}
