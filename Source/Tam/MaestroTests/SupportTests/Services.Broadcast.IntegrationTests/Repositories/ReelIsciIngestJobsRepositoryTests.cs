using NUnit.Framework;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    public class ReelIsciIngestJobsRepositoryTests
    {
        [Test]
        public void DeleteReelIscisBetweenRange_OverlapStartDateEndDate()
        {
            var startDate = new DateTime(2018, 12, 01);
            var endDate = new DateTime(2019, 02, 01);
            var expectedDeleteCount = 2;
            var reelIsciIngestJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciIngestJobsRepository>();
            var reelIscis = _GetReelIscis();
            var result = 0;

            // Act
            using (new TransactionScopeWrapper())
            {
                var addedCount = reelIsciIngestJobsRepository.AddReelIscis(reelIscis);
                result = reelIsciIngestJobsRepository.DeleteReelIscisBetweenRange(startDate, endDate);
            }

            // Assert
            Assert.AreEqual(expectedDeleteCount, result);
        }

        [Test]
        public void DeleteReelIscisBetweenRange_OverlapStartDate()
        {
            var startDate = new DateTime(2018, 12, 01);
            var endDate = new DateTime(2019, 01, 05);
            var expectedDeleteCount = 2;
            var reelIsciIngestJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciIngestJobsRepository>();
            var reelIscis = _GetReelIscis();
            var result = 0;

            // Act
            using (new TransactionScopeWrapper())
            {
                var addedCount = reelIsciIngestJobsRepository.AddReelIscis(reelIscis);
                result = reelIsciIngestJobsRepository.DeleteReelIscisBetweenRange(startDate, endDate);
            }

            // Assert
            Assert.AreEqual(expectedDeleteCount, result);
        }

        [Test]
        public void DeleteReelIscisBetweenRange_OverlapEndDate()
        {
            var startDate = new DateTime(2019, 01, 15);
            var endDate = new DateTime(2019, 01, 20);
            var expectedDeleteCount = 2;
            var reelIsciIngestJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciIngestJobsRepository>();
            var reelIscis = _GetReelIscis();
            var result = 0;

            // Act
            using (new TransactionScopeWrapper())
            {
                var addedCount = reelIsciIngestJobsRepository.AddReelIscis(reelIscis);
                result = reelIsciIngestJobsRepository.DeleteReelIscisBetweenRange(startDate, endDate);
            }

            // Assert
            Assert.AreEqual(expectedDeleteCount, result);
        }

        [Test]
        public void AddReelIscis()
        {
            // Arrange
            int expectedAddCount = 3;
            var reelIsciIngestJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciIngestJobsRepository>();
            var reelIscis = _GetReelIscis();
            var result = 0;

            // Act
            using (new TransactionScopeWrapper())
            {
                result = reelIsciIngestJobsRepository.AddReelIscis(reelIscis);
            }

            // Assert
            Assert.AreEqual(expectedAddCount, result);
        }

        private List<ReelIsciDto> _GetReelIscis()
        {
            return new List<ReelIsciDto>()
            {
                new ReelIsciDto
                {
                    Isci = "OKWF1701H",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2019,01,01),
                    ActiveEndDate = new DateTime(2019,01,17),
                    ReelIsciAdvertiserNameReferences = new List<ReelIsciAdvertiserNameReferenceDto>
                    {
                        new ReelIsciAdvertiserNameReferenceDto()
                        {
                            AdvertiserNameReference = "Colgate EM"
                        },
                        new ReelIsciAdvertiserNameReferenceDto()
                        {
                            AdvertiserNameReference = "Nature's Bounty"
                        }
                    }
                },
                new ReelIsciDto
                {
                    Isci = "OKWL1702H",
                    SpotLengthId = 3,
                    ActiveStartDate = new DateTime(2019,01,01),
                    ActiveEndDate = new DateTime(2019,01,17),
                    ReelIsciAdvertiserNameReferences = new List<ReelIsciAdvertiserNameReferenceDto>
                    {
                        new ReelIsciAdvertiserNameReferenceDto()
                        {
                            AdvertiserNameReference = "O'Keeffes"
                        }
                    }
                },
                new ReelIsciDto
                {
                    Isci = "OKWL1703H",
                    SpotLengthId = 3,
                    ActiveStartDate = new DateTime(2020,01,01),
                    ActiveEndDate = new DateTime(2020,01,17),
                    ReelIsciAdvertiserNameReferences = new List<ReelIsciAdvertiserNameReferenceDto>
                    {
                        new ReelIsciAdvertiserNameReferenceDto()
                        {
                            AdvertiserNameReference = "O'Keeffes"
                        }
                    }
                }
            };
        }
    }
}
