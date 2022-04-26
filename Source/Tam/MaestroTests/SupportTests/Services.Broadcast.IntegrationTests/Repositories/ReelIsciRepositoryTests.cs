using NUnit.Framework;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class ReelIsciRepositoryTests
    {
        [Test]
        public void DeleteReelIscisBetweenRange_OverlapStartDateEndDate()
        {
            var lastIngestedDateTime = new DateTime(2010, 10, 12); // put this way out there for our control
            var startDate = new DateTime(2021, 8, 9);
            var endDate = new DateTime(2021, 8, 15);
            var reelIsciRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            var reelIscis = _GetReelIscisForDelete(lastIngestedDateTime);

            List<ReelIsciDto> result;

            // Act
            using (new TransactionScopeWrapper())
            {
                reelIsciRepo.AddReelIscis(reelIscis);
                reelIsciRepo.DeleteReelIscisBetweenRange(startDate, endDate);
                result = reelIsciRepo.GetReelIscis();
            }

            // Assert
            var resultForValidation = result.Where(s => s.IngestedAt.Equals(lastIngestedDateTime));
            
            var settings = IntegrationTestHelper._GetJsonSettings();
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciAdvertiserNameReferenceDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciAdvertiserNameReferenceDto), "ReelIsciId");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(resultForValidation, settings));
        }

        [Test]
        public void DeleteReelIscisBetweenRange_OverlapStartDateAndEndDate()
        {
            var lastIngestedDateTime = new DateTime(2010, 10, 12); // put this way out there for our control
            var startDate = new DateTime(2021, 7, 28);
            var endDate = new DateTime(2021, 8, 10);
            var reelIsciRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            var reelIscis = _GetReelIscisForDelete(lastIngestedDateTime);

            List<ReelIsciDto> result;

            // Act
            using (new TransactionScopeWrapper())
            {
                reelIsciRepo.AddReelIscis(reelIscis);
                reelIsciRepo.DeleteReelIscisBetweenRange(startDate, endDate);
                result = reelIsciRepo.GetReelIscis();
            }

            // Assert
            var resultForValidation = result.Where(s => s.IngestedAt.Equals(lastIngestedDateTime));

            var settings = IntegrationTestHelper._GetJsonSettings();
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciAdvertiserNameReferenceDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciAdvertiserNameReferenceDto), "ReelIsciId");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(resultForValidation, settings));
        }

        [Test]
        public void DeleteReelIscisBetweenRange_CompareDateWithoutTime()
        {
            var lastIngestedDateTime = new DateTime(2010, 10, 12); // put this way out there for our control
            var startDate = new DateTime(2021, 8, 22, 1, 0, 0);
            var endDate = new DateTime(2021, 8, 26);
            var reelIsciRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            var reelIscis = _GetReelIscisForDelete(lastIngestedDateTime);

            List<ReelIsciDto> result;

            // Act
            using (new TransactionScopeWrapper())
            {
                reelIsciRepo.AddReelIscis(reelIscis);
                reelIsciRepo.DeleteReelIscisBetweenRange(startDate, endDate);
                result = reelIsciRepo.GetReelIscis();
            }

            // Assert
            var resultForValidation = result.Where(s => s.IngestedAt.Equals(lastIngestedDateTime));

            var settings = IntegrationTestHelper._GetJsonSettings();
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciAdvertiserNameReferenceDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciAdvertiserNameReferenceDto), "ReelIsciId");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(resultForValidation, settings));
        }

        [Test]
        public void AddReelIscis()
        {
            // Arrange
            int expectedAddCount = 3;
            var reelIsciRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            var ingestedDateTime = new DateTime(2010, 10, 12);
            var reelIscis = _GetReelIscisToAdd(ingestedDateTime);
            var addedCount = 0;
            List<ReelIsciDto> result;

            // Act
            using (new TransactionScopeWrapper())
            {
                addedCount = reelIsciRepo.AddReelIscis(reelIscis);
                result = reelIsciRepo.GetReelIscis();
            }

            // Assert
            var resultForValidation = result.Where(s => s.IngestedAt.Equals(ingestedDateTime));

            var settings = IntegrationTestHelper._GetJsonSettings();
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciAdvertiserNameReferenceDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(ReelIsciAdvertiserNameReferenceDto), "ReelIsciId");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(resultForValidation, settings));
            Assert.AreEqual(expectedAddCount, addedCount);
        }

        private List<ReelIsciDto> _GetReelIscisToAdd(DateTime ingestedAt)
        {
            return new List<ReelIsciDto>()
            {
                new ReelIsciDto
                {
                    Isci = "OKWF1701H",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2019,01,01),
                    ActiveEndDate = new DateTime(2019,01,17),
                    IngestedAt = ingestedAt,
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
                    IngestedAt = ingestedAt,
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
                    IngestedAt = ingestedAt,
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

        private List<ReelIsciDto> _GetReelIscisForDelete(DateTime ingestedAt)
        {
            var reelIscis = new List<ReelIsciDto>()
            {
                new ReelIsciDto
                {
                    Isci = "DELETE_TEST_1",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2021,8,2),
                    ActiveEndDate = new DateTime(2021,8,8),
                    IngestedAt = ingestedAt,
                    ReelIsciAdvertiserNameReferences = new List<ReelIsciAdvertiserNameReferenceDto>
                    {
                        new ReelIsciAdvertiserNameReferenceDto()
                        {
                            AdvertiserNameReference = "Colgate EM"
                        }
                    }
                },
                new ReelIsciDto
                {
                    Isci = "DELETE_TEST_2",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2021,8,9),
                    ActiveEndDate = new DateTime(2021,8,15),
                    IngestedAt = ingestedAt,
                    ReelIsciAdvertiserNameReferences = new List<ReelIsciAdvertiserNameReferenceDto>
                    {
                        new ReelIsciAdvertiserNameReferenceDto()
                        {
                            AdvertiserNameReference = "Colgate EM"
                        }
                    }
                },
                new ReelIsciDto
                {
                    Isci = "DELETE_TEST_3",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2021,8,16),
                    ActiveEndDate = new DateTime(2021,8,22),
                    IngestedAt = ingestedAt,
                    ReelIsciAdvertiserNameReferences = new List<ReelIsciAdvertiserNameReferenceDto>
                    {
                        new ReelIsciAdvertiserNameReferenceDto()
                        {
                            AdvertiserNameReference = "Colgate EM"
                        }
                    }
                },
            };
            return reelIscis;
        }
    }
}
