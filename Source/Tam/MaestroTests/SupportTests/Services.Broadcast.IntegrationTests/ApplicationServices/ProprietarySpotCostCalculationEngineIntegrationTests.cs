using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProprietarySpotCostCalculationEngineIntegrationTests
    {
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietarySpotCostCalculationEngine>();
        private readonly IInventoryDaypartParsingEngine _InventoryDaypartParsingEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryDaypartParsingEngine>();

        [Test]
        public void CanCalculateSpotCost_CustomAudience_TwoBooks()
        {
            const ProposalPlaybackType playbackType = ProposalPlaybackType.Live;
            const int shareBook = 434;
            const int hutBook = 437;
            const int customAudienceId = 34;

            _InventoryDaypartParsingEngine.TryParse("M-F 9-10A", out var dayparts);

            var manifests = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "EESH"
                    },
                    ManifestAudiences = new List<StationInventoryManifestAudience>
                    {
                        new StationInventoryManifestAudience
                        {
                            CPM = 1,
                            Audience = new DisplayAudience
                            {
                                Id = customAudienceId
                            }
                        }
                    },
                    ManifestDayparts = new List<StationInventoryManifestDaypart>
                    {
                        new StationInventoryManifestDaypart
                        {
                            Daypart = dayparts.First()
                        }
                    }
                }
            };

            _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests, playbackType, shareBook, hutBook);

            Assert.AreEqual(3.83, Math.Round(manifests.First().ManifestRates.First().SpotCost, 2));
        }

        [Test]
        public void CanCalculateSpotCost_CustomAudience_SingleBook()
        {
            const ProposalPlaybackType playbackType = ProposalPlaybackType.Live;
            const int shareBook = 434;
            const int customAudienceId = 34;

            _InventoryDaypartParsingEngine.TryParse("M-F 9-10A", out var dayparts);

            var manifests = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "EESH"
                    },
                    ManifestAudiences = new List<StationInventoryManifestAudience>
                    {
                        new StationInventoryManifestAudience
                        {
                            CPM = 1,
                            Audience = new DisplayAudience
                            {
                                Id = customAudienceId
                            }
                        }
                    },
                    ManifestDayparts = new List<StationInventoryManifestDaypart>
                    {
                        new StationInventoryManifestDaypart
                        {
                            Daypart = dayparts.First()
                        }
                    }
                }
            };

            _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests, playbackType, shareBook);

            Assert.AreEqual(3.97, Math.Round(manifests.First().ManifestRates.First().SpotCost, 2));
        }
    }
}
