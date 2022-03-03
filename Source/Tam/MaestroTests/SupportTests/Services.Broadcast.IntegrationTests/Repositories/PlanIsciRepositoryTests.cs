using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanIsciRepositoryTests
    {
        [Test]
        public void GetAvailableIscis()
        {
            //This test will cover all 3 scenario: 1. Mapped with plan only 2.Unmapped with plan only and 3. A mix of mapped and unmapped
            // Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            DateTime endDate = new DateTime(2024, 08, 29);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            //Act
            var result = repo.GetAvailableIscis(startDate, endDate);
            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        [Test]
        public void GetUnAvailableIscis()
        {
            // Arrange
            DateTime startDate = new DateTime(2015, 01, 01);
            DateTime endDate = new DateTime(2016, 08, 29);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            //Act
            var result = repo.GetAvailableIscis(startDate, endDate);
            //Assert
            Assert.IsTrue(result.Count == 0);
        }
        [Test]
        public void GetAvailableIscis_Overlap()
        {
            // Arrange
            DateTime startDate = new DateTime(2021, 07, 26);
            DateTime endDate = new DateTime(2021, 08, 29);

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            // Act
            var result = repo.GetAvailableIscis(startDate, endDate);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        [Test]
        public void GetAvailableIscis_OverlapOneDay()
        {
            // Arrange
            DateTime startDate = new DateTime(2020, 08, 08);
            DateTime endDate = new DateTime(2021, 01, 01);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            // Act
            var result = repo.GetAvailableIscis(startDate, endDate);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIsciPlans_Plan_DoesNotExist()
        {
            // Arrange
            DateTime mediaMonthStartDate = new DateTime(2015, 02, 23);
            DateTime mediaMonthEndDate = new DateTime(2015, 03, 29);
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            // Act
            var result = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetAvailableIsciPlans_PlanWithIsci_Exists_OverlapStartDateEndDate()
        {
            // Arrange
            DateTime mediaMonthStartDate = new DateTime(2019, 08, 26);
            DateTime mediaMonthEndDate = new DateTime(2019, 09, 29);
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            // Act
            var result = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIsciPlans_PlanWithIsci_Exists_OverlapStartDate()
        {
            // Arrange
            DateTime mediaMonthStartDate = new DateTime(2019, 07, 29);
            DateTime mediaMonthEndDate = new DateTime(2019, 08, 25);
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            // Act
            var result = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIsciPlans_PlansWithoutIsci_Exist_OverlapEndDate()
        {
            // Arrange
            DateTime mediaMonthStartDate = new DateTime(2020, 03, 30);
            DateTime mediaMonthEndDate = new DateTime(2020, 04, 26);
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            // Act
            var result = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIsciPlans_IsciDeleted()
        {
            // Arrange
            var createdBy = "Test User";
            var createdAt = DateTime.Now;
            DateTime mediaMonthStartDate = new DateTime(2020, 02, 17);
            DateTime mediaMonthEndDate = new DateTime(2020, 03, 02);
            var reelIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            var planRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();


            var Iscis = new List<ReelIsciDto>
            {
                new ReelIsciDto()
                {
                    Isci = "UniqueIsci1",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2019, 02, 17),
                    ActiveEndDate = new DateTime(2019, 03, 02),
                    IngestedAt = new DateTime(2010, 10, 12),
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
                }
            };

            var planDto = new PlanDto
            {
                CampaignId = 1,
                Id = 7009,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                AudienceType = AudienceTypeEnum.Nielsen,
                AudienceId = 31,
                ShareBookId = 437,
                HUTBookId = 437,
                PostingType = PostingTypeEnum.NSI,
                Status = PlanStatusEnum.Contracted,
                ModifiedBy = createdBy,
                ModifiedDate = createdAt,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2020, 02, 17),
                FlightEndDate = new DateTime(2019, 03, 02),
                Budget = 500000m,
                TargetImpressions = 50000000,
                TargetCPM = 10m,
                CoverageGoalPercent = 60,
                TargetRatingPoints = 0.00248816152650979,
                TargetCPP = 200951583.9999m,
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 }
            };

            var isciMappings = new List<PlanIsciDto>
            {
                new PlanIsciDto()
                {
                    PlanId = 7009,
                    Isci = "UniqueIsci1",
                    FlightStartDate = new DateTime(2020, 02, 17),
                    FlightEndDate = new DateTime(2020, 03, 02),
                },
                new PlanIsciDto()
                {
                    PlanId = 7009,
                    Isci = "UniqueIsci2",
                    FlightStartDate = new DateTime(2020, 02, 17),
                    FlightEndDate = new DateTime(2020, 03, 02),
                }
            };

            // Act
            List<IsciPlanDetailDto> results;
            using (new TransactionScopeWrapper())
            {
                reelIsciRepository.AddReelIscis(Iscis);
                planRepository.SavePlan(planDto, createdBy, createdAt);
                planIsciRepository.SaveIsciPlanMappings(isciMappings, createdBy, createdAt);
                results = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);
            }

            // Assert
            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, jsonSettings));
        }

        [Test]
        public void GetPlanIscis_IsciDeleted()
        {
            // Arrange
            var createdBy = "Test User";
            var createdAt = DateTime.Now;
            var reelIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            var planId = 7009;

            var Iscis = new List<ReelIsciDto>
            {
                new ReelIsciDto()
                {
                    Isci = "UniqueIsci1",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2020, 02, 17),
                    ActiveEndDate = new DateTime(2019, 03, 02),
                    IngestedAt = new DateTime(2010, 10, 12),
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
                }
            };

            var isciMappings = new List<PlanIsciDto>
            {
                new PlanIsciDto()
                {
                    PlanId = 7009,
                    Isci = "UniqueIsci1",
                    FlightStartDate = new DateTime(2020, 02, 17),
                    FlightEndDate = new DateTime(2020, 03, 02),
                },
                new PlanIsciDto()
                {
                    PlanId = 7009,
                    Isci = "UniqueIsci2",
                    FlightStartDate = new DateTime(2020, 02, 17),
                    FlightEndDate = new DateTime(2020, 03, 02),
                }
            };

            // Act
            List<PlanIsciDto> results;
            using (new TransactionScopeWrapper())
            {
                reelIsciRepository.AddReelIscis(Iscis);
                planIsciRepository.SaveIsciPlanMappings(isciMappings, createdBy, createdAt);
                results = planIsciRepository.GetPlanIscis(planId);
            }

            // Assert
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanIsciDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, jsonSettings));
        }

        [Test]
        public void SaveIsciPlanMappings()
        {
            // Arrange
            string createdBy = "Test User";
            DateTime createdAt = DateTime.Now;
            var isciMappings = _GetIsciMappings();
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            int result = 0;
            List<PlanIsciDto> saveResult;

            var itemsToSave = isciMappings.IsciPlanMappings.Select(s => new PlanIsciDto
            {
                PlanId = s.PlanId,
                Isci = s.Isci,
                FlightStartDate = new DateTime(2020, 2, 17),
                FlightEndDate = new DateTime(2020, 3, 2),
            }).ToList();

            // Act
            using (new TransactionScopeWrapper())
            {
                result = planIsciRepository.SaveIsciPlanMappings(itemsToSave, createdBy, createdAt);
                saveResult = planIsciRepository.GetPlanIscis();
            }
            Assert.AreEqual(1, result);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanIsciDto), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saveResult, jsonSettings));
        }

        [Test]
        public void SaveIsciPlanMappings_DuplicateMapping()
        {
            // Arrange
            string createdBy = "Test User";
            DateTime createdAt = DateTime.Now;
            var isciMappings = _GetIsciMappings_DuplicatePlan();
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            Exception caught = null;

            var itemsToSave = isciMappings.IsciPlanMappings.Select(s => new PlanIsciDto
            {
                PlanId = s.PlanId,
                Isci = s.Isci,
                FlightStartDate = new DateTime(2020, 2, 17),
                FlightEndDate = new DateTime(2020, 3, 2),
            }).ToList();

            // Act
            using (new TransactionScopeWrapper())
            {
                caught = Assert.Throws<Exception>(() =>
                    planIsciRepository.SaveIsciPlanMappings(itemsToSave, createdBy, createdAt));
            }

            // Assert
            Assert.IsTrue(caught.InnerException.InnerException.InnerException.Message.Contains("Cannot insert duplicate key"));
        }

        private IsciPlanMappingsSaveRequestDto _GetIsciMappings()
        {
            return new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappings = new List<IsciPlanMappingDto>
                    {
                        new IsciPlanMappingDto()
                        {
                            PlanId = 7009,
                            Isci= "AE67VR14"
                        }
                    },
                IsciProductMappings = new List<IsciProductMappingDto>
                {
                    new IsciProductMappingDto()
                        {
                            ProductName = "Femoston",
                            Isci= "AE67VR14"
                        },
                        new IsciProductMappingDto()
                        {
                            ProductName = "Abbot Labs",
                            Isci= "AE67VR14"
                        }
                }
            };
        }

        private IsciPlanMappingsSaveRequestDto _GetIsciMappings_DuplicatePlan()
        {
            return new IsciPlanMappingsSaveRequestDto()
            {
                IsciPlanMappings = new List<IsciPlanMappingDto>
                    {
                        new IsciPlanMappingDto()
                        {
                            PlanId = 7009,
                            Isci= "AE67VR14"
                        },
                        new IsciPlanMappingDto()
                        {
                            PlanId = 7009,
                            Isci= "AE67VR14"
                        }
                    },
                IsciProductMappings = new List<IsciProductMappingDto>
                {
                    new IsciProductMappingDto()
                        {
                            ProductName = "Femoston",
                            Isci= "AE67VR14"
                        },
                        new IsciProductMappingDto()
                        {
                            ProductName = "Abbot Labs",
                            Isci= "AE67VR14"
                        }
                }
            };
        }

        [Test]
        public void DeleteIsciPlanMappings()
        {
            // Arrange
            string deletedBy = "Test User";
            DateTime deletedAt = DateTime.Now;

            var Iscis = new List<ReelIsciDto>
            {
                new ReelIsciDto()
                {
                    Isci = "UniqueIsci1",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2019,01,01),
                    ActiveEndDate = new DateTime(2019,01,17),
                    IngestedAt = new DateTime(2010,10,12),
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
                new ReelIsciDto()
                {
                    Isci = "UniqueIsci2",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2019,01,01),
                    ActiveEndDate = new DateTime(2019,01,17),
                    IngestedAt = new DateTime(2010,10,12),
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
                }
            };

            var isciMappings = new List<PlanIsciDto>
            {
                new PlanIsciDto()
                {
                    PlanId = 7009,
                    Isci = "UniqueIsci1",
                    FlightStartDate = new DateTime(2020, 2, 17),
                    FlightEndDate = new DateTime(2020, 3, 2),
                },
                new PlanIsciDto()
                {
                    PlanId = 7009,
                    Isci = "UniqueIsci2",
                    FlightStartDate = new DateTime(2020, 2, 17),
                    FlightEndDate = new DateTime(2020, 3, 2),
                }
            };
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            var reelIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            var before = new List<PlanIsciDto>();
            var after = new List<PlanIsciDto>();
            var deletedCount = 0;

            // Act
            using (new TransactionScopeWrapper())
            {
                reelIsciRepository.AddReelIscis(Iscis);
                var addedCount = planIsciRepository.SaveIsciPlanMappings(isciMappings, deletedBy, deletedAt);
                before = planIsciRepository.GetPlanIscis(7009);
                var idToDelete = new List<int> { before.First().Id };
                deletedCount = planIsciRepository.DeleteIsciPlanMappings(idToDelete, deletedBy, deletedAt);
                after = planIsciRepository.GetPlanIscis(7009);
            }

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanIsciDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var resultToVerify = new
            {
                deletedCount,
                before,
                after
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(resultToVerify, jsonSettings));
        }

        [Test]
        public void DeletePlanIscisNotExistInReelIsci()
        {
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            var reelIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            var createdBy = "Test User";
            var createdAt = DateTime.Now;
            var deletedAt = DateTime.Now;
            var deletedBy = "TestUser";

            var Iscis = new List<ReelIsciDto>
            {
                new ReelIsciDto()
                {
                    Isci = "UniqueIsci1",
                    SpotLengthId = 1,
                    ActiveStartDate = new DateTime(2019,01,01),
                    ActiveEndDate = new DateTime(2019,01,17),
                    IngestedAt = new DateTime(2010,10,12),
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
                }
            };

            var isciMappings = new List<PlanIsciDto>
            {
                new PlanIsciDto()
                {
                    PlanId = 7009,
                    Isci = "UniqueIsci1",
                    FlightStartDate = new DateTime(2020, 2, 17),
                    FlightEndDate = new DateTime(2020, 3, 2),
                },
                new PlanIsciDto()
                {
                    PlanId = 7009,
                    Isci = "UniqueIsci2",
                    FlightStartDate = new DateTime(2020, 2, 17),
                    FlightEndDate = new DateTime(2020, 3, 2),
                }
            };
            var deletedCount = 0;
            var before = new List<PlanIsciDto>();
            var after = new List<PlanIsciDto>();

            // Act
            using (new TransactionScopeWrapper())
            {
                reelIsciRepository.AddReelIscis(Iscis);
                planIsciRepository.SaveIsciPlanMappings(isciMappings, createdBy, createdAt);
                before = planIsciRepository.GetPlanIscis(7009);
                deletedCount = planIsciRepository.DeletePlanIscisNotExistInReelIsci(deletedAt, deletedBy);
                after = planIsciRepository.GetPlanIscis(7009);
            }

            // Assert
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PlanIsciDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var resultToVerify = new
            {
                deletedCount,
                before,
                after
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(resultToVerify, jsonSettings));
        }
    }
}
