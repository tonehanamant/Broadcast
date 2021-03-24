using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    public class ImpressionsServiceUnitTests
    {
        private Mock<INsiComponentAudienceRepository> _NsiComponentAudienceRepository;
        private Mock<IRatingForecastRepository> _RatingsRepository;
        private Mock<IBroadcastAudienceRepository> _BroadcastAudienceRepository;
        private Mock<IStationRepository> _StationRepository;
        private Mock<IAffidavitRepository> _AffidavitRepository;

        private Mock<IStationProcessingEngine> _StationProcessingEngine;

        [SetUp]
        public void SetUp()
        {
            _NsiComponentAudienceRepository = new Mock<INsiComponentAudienceRepository>();
            _RatingsRepository = new Mock<IRatingForecastRepository>();
            _BroadcastAudienceRepository = new Mock<IBroadcastAudienceRepository>();
            _StationRepository = new Mock<IStationRepository>();
            _AffidavitRepository = new Mock<IAffidavitRepository>();
            _StationProcessingEngine = new Mock<IStationProcessingEngine>();
        }

        private IImpressionsService _GetService()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            dataRepoFactory.Setup(s => s.GetDataRepository<INsiComponentAudienceRepository>())
                .Returns(_NsiComponentAudienceRepository.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IRatingForecastRepository>())
                .Returns(_RatingsRepository.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IBroadcastAudienceRepository>())
                .Returns(_BroadcastAudienceRepository.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IStationRepository>())
                .Returns(_StationRepository.Object);
            dataRepoFactory.Setup(s => s.GetDataRepository<IAffidavitRepository>())
                .Returns(_AffidavitRepository.Object);

            var service = new ImpressionsService(
                dataRepoFactory.Object,
                _StationProcessingEngine.Object
                );

            return service;
        }

        [Test]
        public void AddProjectedImpressionsToManifestsTwoBooksWithCustomAudience()
        {
            // Arrange
            const ProposalEnums.ProposalPlaybackType playbackType = ProposalEnums.ProposalPlaybackType.Live;
            const int shareBook = 434;
            const int hutBook = 437;
            const int customAudienceId = 34;

            var testAudienceMappings = AudienceTestData.GetRatingsAudiencesByMaestroAudience(new List<int> { customAudienceId });
            var testImpressionsValue = 1000;
            double expectedProjectedStationImpressionsValue = testAudienceMappings.Count * testImpressionsValue;

            var testDayparts = new List<StationInventoryManifestDaypart>
            {
                new StationInventoryManifestDaypart
                {
                    // M-F 9-10A
                    Daypart = new DisplayDaypart(1, 32400, 35999, true, true, true, true, true, false, false)
                }
            };

            var manifests = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = "EESH" },
                    ManifestAudiencesReferences = new List<StationInventoryManifestAudience>
                    {
                        new StationInventoryManifestAudience
                        {
                            CPM = 1,
                            Audience = new DisplayAudience {Id = customAudienceId}
                        }
                    },
                    ManifestDayparts = testDayparts
                }
            };

            _BroadcastAudienceRepository.Setup(s => s.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(testAudienceMappings);

            _RatingsRepository.Setup(s => s.GetImpressionsDaypart(It.IsAny<short>(), It.IsAny<short>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<ManifestDetailDaypart>>(), It.IsAny<ProposalEnums.ProposalPlaybackType?>()))
                .Returns<short, short, List<int> , List<ManifestDetailDaypart>, ProposalEnums.ProposalPlaybackType?>(
                    (a,b,audiences,dayparts,pbt) =>
                    {
                        var result = new ImpressionsDaypartResultForTwoBooks {Impressions = _GetResultImpressions(audiences, dayparts, pbt.Value, testImpressionsValue)};
                        return result;
                    });

            var service = _GetService();

            // Act
            service.AddProjectedImpressionsToManifests(manifests, playbackType, shareBook, hutBook);

            // Assert
            Assert.AreEqual(1, manifests.Count);
            Assert.AreEqual(1, manifests[0].ProjectedStationImpressions.Count);
            Assert.AreEqual(expectedProjectedStationImpressionsValue, manifests[0].ProjectedStationImpressions[0].Impressions);

            _RatingsRepository.Verify(s => s.GetImpressionsDaypart(It.IsAny<short>(), It.IsAny<short>(), It.IsAny<List<int>>(),
                It.IsAny<List<ManifestDetailDaypart>>(), It.IsAny<ProposalEnums.ProposalPlaybackType?>()), Times.Exactly(testDayparts.Count));
        }

        [Test]
        public void AddProjectedImpressionsToManifestsSingleBookWithCustomAudience()
        {
            // Arrange
            const ProposalEnums.ProposalPlaybackType playbackType = ProposalEnums.ProposalPlaybackType.Live;
            const int shareBook = 434;
            const int customAudienceId = 34;

            var testAudienceMappings = AudienceTestData.GetRatingsAudiencesByMaestroAudience(new List<int> { customAudienceId });
            var testImpressionsValue = 1000;
            double expectedProjectedStationImpressionsValue = testAudienceMappings.Count * testImpressionsValue;

            var testDayparts = new List<StationInventoryManifestDaypart>
            {
                new StationInventoryManifestDaypart
                {
                    // M-F 9-10A
                    Daypart = new DisplayDaypart(1, 32400, 35999, true, true, true, true, true, false, false)
                }
            };

            var manifests = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = "EESH" },
                    ManifestAudiencesReferences = new List<StationInventoryManifestAudience>
                    {
                        new StationInventoryManifestAudience
                        {
                            CPM = 1,
                            Audience = new DisplayAudience {Id = customAudienceId}
                        }
                    },
                    ManifestDayparts = testDayparts
                }
            };

            _BroadcastAudienceRepository.Setup(s => s.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(testAudienceMappings);

            _RatingsRepository.Setup(s => s.GetImpressionsDaypart(It.IsAny<int>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<ManifestDetailDaypart>>(), It.IsAny<ProposalEnums.ProposalPlaybackType?>()))
                .Returns<int, List<int>, List<ManifestDetailDaypart>, ProposalEnums.ProposalPlaybackType?>(
                    (a, audiences, dayparts, pbt) =>
                    {
                        var result = new ImpressionsDaypartResultForSingleBook { Impressions = _GetResultImpressions(audiences, dayparts, pbt.Value, testImpressionsValue) };
                        return result;
                    });

            var service = _GetService();

            // Act
            service.AddProjectedImpressionsToManifests(manifests, playbackType, shareBook);

            // Assert
            Assert.AreEqual(1, manifests.Count);
            Assert.AreEqual(1, manifests[0].ProjectedStationImpressions.Count);
            Assert.AreEqual(expectedProjectedStationImpressionsValue, manifests[0].ProjectedStationImpressions[0].Impressions);
            
            _RatingsRepository.Verify(s => s.GetImpressionsDaypart(It.IsAny<int>(), It.IsAny<List<int>>(),
                It.IsAny<List<ManifestDetailDaypart>>(), It.IsAny<ProposalEnums.ProposalPlaybackType?>()), Times.Exactly(testDayparts.Count));
        }

        [Test]
        public void AddProjectedImpressionsToManifestsTwoBooksCrossMidnight()
        {
            // Arrange
            const ProposalEnums.ProposalPlaybackType playbackType = ProposalEnums.ProposalPlaybackType.Live;
            const int shareBook = 434;
            const int hutBook = 437;
            const int customAudienceId = 34;

            var testAudienceMappings = AudienceTestData.GetRatingsAudiencesByMaestroAudience(new List<int> { customAudienceId });
            var testImpressionsValue = 1000;
            double expectedProjectedStationImpressionsValue = testAudienceMappings.Count * testImpressionsValue;

            var testDayparts = new List<StationInventoryManifestDaypart>
            {
                new StationInventoryManifestDaypart
                {
                    // M-Su 6a-2aA
                    Daypart = new DisplayDaypart(1, 21600, 7199, true, true, true, true, true, true, true)
                }
            };

            var manifests = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = "EESH" },
                    ManifestAudiencesReferences = new List<StationInventoryManifestAudience>
                    {
                        new StationInventoryManifestAudience
                        {
                            CPM = 1,
                            Audience = new DisplayAudience {Id = customAudienceId}
                        }
                    },
                    ManifestDayparts = testDayparts
                }
            };

            _BroadcastAudienceRepository.Setup(s => s.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(testAudienceMappings);

            _RatingsRepository.Setup(s => s.GetImpressionsDaypart(It.IsAny<short>(), It.IsAny<short>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<ManifestDetailDaypart>>(), It.IsAny<ProposalEnums.ProposalPlaybackType?>()))
                .Returns<short, short, List<int>, List<ManifestDetailDaypart>, ProposalEnums.ProposalPlaybackType?>(
                    (a, b, audiences, dayparts, pbt) =>
                    {
                        var result = new ImpressionsDaypartResultForTwoBooks { Impressions = _GetResultImpressions(audiences, dayparts, pbt.Value, testImpressionsValue) };
                        return result;
                    });

            var service = _GetService();

            // Act
            service.AddProjectedImpressionsToManifests(manifests, playbackType, shareBook, hutBook);

            // Assert
            Assert.AreEqual(1, manifests.Count);
            Assert.AreEqual(1, manifests[0].ProjectedStationImpressions.Count);
            Assert.AreEqual(expectedProjectedStationImpressionsValue, manifests[0].ProjectedStationImpressions[0].Impressions);

            // twice as many calls because the daypart was split.
            _RatingsRepository.Verify(s => s.GetImpressionsDaypart(It.IsAny<short>(), It.IsAny<short>(), It.IsAny<List<int>>(),
                It.IsAny<List<ManifestDetailDaypart>>(), It.IsAny<ProposalEnums.ProposalPlaybackType?>()), Times.Exactly(testDayparts.Count * 2));
        }

        [Test]
        public void AddProjectedImpressionsToManifestsSingleBookCrossMidnight()
        {
            // Arrange
            const ProposalEnums.ProposalPlaybackType playbackType = ProposalEnums.ProposalPlaybackType.Live;
            const int shareBook = 434;
            const int customAudienceId = 31;

            var testAudienceMappings = AudienceTestData.GetRatingsAudiencesByMaestroAudience(new List<int> { customAudienceId });
            var testImpressionsValue = 1000;
            double expectedProjectedStationImpressionsValue = testAudienceMappings.Count * testImpressionsValue;

            var testDayparts = new List<StationInventoryManifestDaypart>
            {
                new StationInventoryManifestDaypart
                {
                    // M-Su 6a-2aA
                    Daypart = new DisplayDaypart(1, 21600, 7199, true, true, true, true, true, true, true)
                }
            };

            var manifests = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    Station = new DisplayBroadcastStation { LegacyCallLetters = "EESH" },
                    ManifestAudiencesReferences = new List<StationInventoryManifestAudience>
                    {
                        new StationInventoryManifestAudience
                        {
                            CPM = 1,
                            Audience = new DisplayAudience {Id = customAudienceId}
                        }
                    },
                    ManifestDayparts = testDayparts
                }
            };

            _BroadcastAudienceRepository.Setup(s => s.GetRatingsAudiencesByMaestroAudience(It.IsAny<List<int>>()))
                .Returns(testAudienceMappings);

            _RatingsRepository.Setup(s => s.GetImpressionsDaypart(It.IsAny<int>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<ManifestDetailDaypart>>(), It.IsAny<ProposalEnums.ProposalPlaybackType?>()))
                .Returns<int, List<int>, List<ManifestDetailDaypart>, ProposalEnums.ProposalPlaybackType?>(
                    (a, audiences, dayparts, pbt) =>
                    {
                        var result = new ImpressionsDaypartResultForSingleBook { Impressions = _GetResultImpressions(audiences, dayparts, pbt.Value, testImpressionsValue) };
                        return result;
                    });

            var service = _GetService();

            // Act
            service.AddProjectedImpressionsToManifests(manifests, playbackType, shareBook);

            // Assert
            Assert.AreEqual(1, manifests.Count);
            Assert.AreEqual(1, manifests[0].ProjectedStationImpressions.Count);
            Assert.AreEqual(expectedProjectedStationImpressionsValue, manifests[0].ProjectedStationImpressions[0].Impressions);

            // twice as many calls because the daypart was split.
            _RatingsRepository.Verify(s => s.GetImpressionsDaypart(It.IsAny<int>(), It.IsAny<List<int>>(),
                It.IsAny<List<ManifestDetailDaypart>>(), It.IsAny<ProposalEnums.ProposalPlaybackType?>()), Times.Exactly(testDayparts.Count * 2));
        }

        private List<StationImpressionsWithAudience> _GetResultImpressions(List<int> audiences, List<ManifestDetailDaypart> dayparts, 
            ProposalEnums.ProposalPlaybackType playbackType, int testImpressionsValue)
        {
            var result = new List<StationImpressionsWithAudience>();

            foreach (var daypart in dayparts)
            {
                foreach (var audience in audiences)
                {
                    var item = new StationImpressionsWithAudience
                    {
                        Id = daypart.Id,
                        LegacyCallLetters = daypart.LegacyCallLetters,
                        Impressions = testImpressionsValue,
                        AudienceId = audience
                    };
                    result.Add(item);
                }
            }
            
            return result;
        }
    }
}