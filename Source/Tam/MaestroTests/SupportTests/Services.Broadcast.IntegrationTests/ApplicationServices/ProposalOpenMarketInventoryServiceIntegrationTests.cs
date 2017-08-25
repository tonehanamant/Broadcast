using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProposalOpenMarketInventoryServiceIntegrationTests
    {
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();

        [TestCase(MinMaxEnum.Min)]
        [TestCase(MinMaxEnum.Max)]
        public void MultipleMinCpmCriteria(MinMaxEnum minMax)
        {
            var request = new OpenMarketRefineProgramsRequest
            {
                Criteria = new OpenMarketCriterion
                {
                    CpmCriteria = new List<CpmCriteria>
                            {
                                new CpmCriteria {MinMax = minMax},
                                new CpmCriteria {MinMax = minMax}
                            }
                }
            };
            Assert.That(() => _ProposalOpenMarketInventoryService.RefinePrograms(request), Throws.Exception.With.Message.EqualTo("Only 1 Min CPM and 1 Max CPM criteria allowed."));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketProposalInventory()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(7, 413, 416);
                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(7);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketProposalInventory_WithCpmRefinements()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(7, 413, 416);
                var request = new OpenMarketRefineProgramsRequest
                {
                    IgnoreExistingAllocation = true,
                    ProposalDetailId = 7,
                    Criteria = new OpenMarketCriterion
                    {
                        CpmCriteria = new List<CpmCriteria>
                {
                    new CpmCriteria { MinMax = MinMaxEnum.Min, Value = 999 }
                }
                    }
                };
                var proposalInventory = _ProposalOpenMarketInventoryService.RefinePrograms(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(CpmCriteria), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketProposalInventory_WithProgramNameRefinements()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(7, 413, 416);
                var request = new OpenMarketRefineProgramsRequest
                {
                    IgnoreExistingAllocation = true,
                    ProposalDetailId = 7,
                    Criteria = new OpenMarketCriterion
                    {
                        ProgramNameSearchCriteria = new List<ProgramCriteria>
                        {
                            new ProgramCriteria{Contain = ContainTypeEnum.Include, ProgramName = "Open Market Program"}
                        }
                    }
                };
                var proposalInventory = _ProposalOpenMarketInventoryService.RefinePrograms(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory, jsonSettings));
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalInventoryByMarketId()
        {
            using (new TransactionScopeWrapper())
            {
                var dto = _ProposalOpenMarketInventoryService.GetInventory(7);
                var dtoMarketCount = dto.Markets.Count;
                dto.Filter.Markets.Add(111);

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);

                Assert.IsTrue(dtoMarketCount > filteredDto.Markets.Count);
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalInventoryByAffiliation()
        {
            // affiliation COZ
            using (new TransactionScopeWrapper())
            {
                var dto = _ProposalOpenMarketInventoryService.GetInventory(7);
                var dtoStationsCount = dto.Markets.SelectMany(a => a.Stations).Count();
                dto.Filter.Affiliations.Add("COZ");

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filteredAffiliationCount = filteredDto.Markets.SelectMany(a => a.Stations).Count();
                Assert.IsTrue(dtoStationsCount > filteredAffiliationCount);
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalInventoryByProgramName()
        {
            // open market program
            using (new TransactionScopeWrapper())
            {
                var dto = _ProposalOpenMarketInventoryService.GetInventory(7);
                var dtoProgramCount = dto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).Count();
                dto.Filter.ProgramNames.Add("open market program");

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filteredProgramCount =
                    filteredDto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).Count();

                Assert.IsTrue(dtoProgramCount > filteredProgramCount);
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalInventoryByDaypart()
        {
            // open market program
            using (new TransactionScopeWrapper())
            {
                var dto = _ProposalOpenMarketInventoryService.GetInventory(7);
                var dtoProgramCount = dto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).Count();

                DisplayDaypart daypart;
                DisplayDaypart.TryParse("M-SU 6AM-12AM", out daypart);
                dto.Filter.DayParts.Add(DaypartDto.ConvertDisplayDaypart(daypart));

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filtereddaypartCount =
                    filteredDto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).Count();

                Assert.IsTrue(dtoProgramCount == filtereddaypartCount);
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalProgramsWithSpot()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(7, 413, 416);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(7);

                dto.Filter.SpotFilter = ProposalOpenMarketFilter.OpenMarketSpotFilter.ProgramWithSpots;

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filteredProgramsWithSpot =
                    filteredDto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).Count();
                // the first tow programs have spots agasint it
                Assert.IsTrue(filteredProgramsWithSpot == 1);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketProposalInventoryWithHutAndShareBooks()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(2102);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory.Markets, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketProposalInventoryWithSingleBookOnly()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBook(14, 413);
                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(14);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory.Markets, jsonSettings));
            }
        }

        [Test]
        public void CanCalculateTotalsForProposalOpenMarketDto()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(7, 413, 416);

                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(7);
                Assert.IsTrue(proposalInventory.DetailImpressionsPercent > 0);
                Assert.IsTrue(proposalInventory.DetailBudgetPercent > 0);
                Assert.IsTrue(proposalInventory.DetailCpmPercent > 0);
                Assert.IsTrue(proposalInventory.Weeks.All(a => a.ImpressionsPercent > 0));
                Assert.IsTrue(proposalInventory.Weeks.All(a => a.BudgetPercent > 0));
            }
        }

        [Test]
        public void CanEditPorposalInventorySlot()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(7, 413, 416);

                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(7);

                var program =
                    proposalInventory.Weeks.SelectMany(
                        a => a.Markets.SelectMany(b => b.Stations.SelectMany(c => c.Programs.Select(d => d)))).FirstOrDefault();
                program.Spots = 99;
                var detailTotalBudget = proposalInventory.DetailTotalBudget;
                var detailTotalImpressions = proposalInventory.DetailTotalImpressions;

                _ProposalOpenMarketInventoryService.UpdateOpenMarketInventoryTotals(proposalInventory);

                Assert.IsTrue(proposalInventory.DetailTotalBudget != detailTotalBudget);
                Assert.IsTrue(proposalInventory.DetailTotalImpressions != detailTotalImpressions);
            }
        }

        [Test]
        public void CanVerifyMarginHasBeenAchieved()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(7, 413, 416);
                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(7);
                var program =
                    proposalInventory.Weeks.SelectMany(
                        a => a.Markets.SelectMany(b => b.Stations.SelectMany(c => c.Programs.Select(d => d)))).FirstOrDefault();
                program.Spots = 99999;

                var editedInventory = _ProposalOpenMarketInventoryService.UpdateOpenMarketInventoryTotals(proposalInventory);

                Assert.IsTrue(editedInventory.DetailImpressionsMarginAchieved);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanEditPorposalOpenMarketInventorySpotWithFilterOn()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(7, 413, 416);

                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(7);
                proposalInventory.Filter.SpotFilter = ProposalOpenMarketFilter.OpenMarketSpotFilter.ProgramWithoutSpots;

                var filteredProposal =
                    _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(proposalInventory);
                var program =
                    filteredProposal.Weeks.SelectMany(
                        a => a.Markets.SelectMany(b => b.Stations.SelectMany(c => c.Programs.Select(d => d))))
                        .FirstOrDefault();

                program.Spots = 500;

                var filteredAndCalculatedInventory =
                    _ProposalOpenMarketInventoryService.UpdateOpenMarketInventoryTotals(filteredProposal);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalFlightWeeks");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailFlightWeeks");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "Markets");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "Criteria");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "DisplayFilter");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(filteredAndCalculatedInventory, jsonSettings));

            }
        }

        [Test]
        public void OpenMarketInventorySetFlagToShowWarningOnHideProgramWithSpots()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new OpenMarketRefineProgramsRequest
                {
                    ProposalDetailId = 7,
                    Criteria = new OpenMarketCriterion
                    {
                        ProgramNameSearchCriteria = new List<ProgramCriteria>()
                        {
                            new ProgramCriteria()
                            {
                                Contain = ContainTypeEnum.Exclude,
                                ProgramName = "Open Market Program Too"
                            }
                        }
                    }
                };

                Assert.IsTrue(
                    _ProposalOpenMarketInventoryService.RefinePrograms(request).NewCriteriaAffectsExistingAllocations);
            }
        }

        [Test]
        public void OpenMarketInventoryOverrideFlagToShowWarningOnHideProgramWithSpots()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new OpenMarketRefineProgramsRequest
                {
                    ProposalDetailId = 7,
                    IgnoreExistingAllocation = true,
                    Criteria = new OpenMarketCriterion
                    {
                        ProgramNameSearchCriteria = new List<ProgramCriteria>()
                        {
                            new ProgramCriteria()
                            {
                                Contain = ContainTypeEnum.Exclude,
                                ProgramName = "Open Market Program Too"
                            }
                        }
                    }
                };

                Assert.IsFalse(
                    _ProposalOpenMarketInventoryService.RefinePrograms(request).NewCriteriaAffectsExistingAllocations);
            }
        }

        [Test]
        public void RefineByProgramNameDontExcludePreviouslyAddedProgramNames()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(7);

                var request = new OpenMarketRefineProgramsRequest
                {
                    ProposalDetailId = proposalInventory.DetailId,
                    IgnoreExistingAllocation = true,
                    Criteria = new OpenMarketCriterion
                    {
                        ProgramNameSearchCriteria = new List<ProgramCriteria>()
                            {
                                new ProgramCriteria(){Contain = ContainTypeEnum.Exclude, ProgramName = proposalInventory.RefineFilterPrograms.First()}
                            }
                    }
                };

                var refinedPrograms = _ProposalOpenMarketInventoryService.RefinePrograms(request);

                Assert.IsTrue(Enumerable.SequenceEqual(proposalInventory.RefineFilterPrograms, refinedPrograms.RefineFilterPrograms));
            }
        }

        [Test]
        public void ClearExistingCriteria_Saves_NewCpmCriteria()
        {
            var newCriteria = new List<CpmCriteria>
            {
                new CpmCriteria {MinMax = MinMaxEnum.Min, Value = 3.22m},
                new CpmCriteria{MinMax = MinMaxEnum.Max, Value = 4.20m}
            };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(),
                newCriteria,
                It.IsAny<List<int>>(), It.IsAny<List<GenreCriteria>>(), It.IsAny<List<int>>(), It.IsAny<List<ProgramCriteria>>(), It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion()
            };

            var criteria = new OpenMarketCriterion { CpmCriteria = newCriteria };

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ClearExistingCriteria_Deletes_OldCpmCriteria()
        {
            var deleteCriteriaIds = new List<int> { 420, 322 };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(),
                deleteCriteriaIds,
                It.IsAny<List<GenreCriteria>>(), It.IsAny<List<int>>(), It.IsAny<List<ProgramCriteria>>(), It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    CpmCriteria = new List<CpmCriteria>
                    {
                        new CpmCriteria {MinMax = MinMaxEnum.Min, Value = 2.65m, Id = deleteCriteriaIds.First()},
                        new CpmCriteria {MinMax = MinMaxEnum.Max, Value = 7.22m, Id = deleteCriteriaIds.Last()}
                    }
                }
            };

            var criteria = new OpenMarketCriterion();

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ClearExistingCriteria_Deletes_OldCpmCriteria_And_AddNewCpmCriteria()
        {
            var deleteCriteriaIds = new List<int> { 420 };
            var newCriteria = new List<CpmCriteria> { new CpmCriteria { MinMax = MinMaxEnum.Max, Value = 4.20m } };


            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(),
                newCriteria,
                deleteCriteriaIds,
                It.IsAny<List<GenreCriteria>>(), It.IsAny<List<int>>(), It.IsAny<List<ProgramCriteria>>(), It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    CpmCriteria = new List<CpmCriteria>
                    {
                        new CpmCriteria {MinMax = MinMaxEnum.Max, Value = 7.22m, Id = 322},
                        new CpmCriteria {MinMax = MinMaxEnum.Min, Value = 2.65m, Id = deleteCriteriaIds.First()}
                    }
                }
            };

            var criteria = new OpenMarketCriterion
            {
                CpmCriteria = new List<CpmCriteria>
                {
                    dto.Criteria.CpmCriteria.First(),
                    newCriteria.First()
                }
            };

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ClearExistingCriteria_Saves_NewGenreCriteria()
        {
            var newCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain = ContainTypeEnum.Include, GenreId = 1},
                new GenreCriteria {Contain = ContainTypeEnum.Exclude, GenreId = 2}
            };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(), It.IsAny<List<int>>(),
                newCriteria,
                It.IsAny<List<int>>(), It.IsAny<List<ProgramCriteria>>(), It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion()
            };

            var criteria = new OpenMarketCriterion { GenreSearchCriteria = newCriteria };

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ClearExistingCriteria_Deletes_OldGenreCriteria()
        {
            var deleteCriteriaIds = new List<int> { 420, 322 };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(), It.IsAny<List<int>>(), It.IsAny<List<GenreCriteria>>(),
                deleteCriteriaIds,
                It.IsAny<List<ProgramCriteria>>(), It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    GenreSearchCriteria = new List<GenreCriteria>
                    {
                        new GenreCriteria {Contain = ContainTypeEnum.Include, GenreId = 1, Id = deleteCriteriaIds.First()},
                        new GenreCriteria {Contain = ContainTypeEnum.Exclude, GenreId = 2, Id = deleteCriteriaIds.Last()}
                    }
                }
            };

            var criteria = new OpenMarketCriterion();

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ClearExistingCriteria_Deletes_OldGenreCriteria_And_AddNewGenreCriteria()
        {
            var deleteCriteriaIds = new List<int> { 420 };
            var newCriteria = new List<GenreCriteria> { new GenreCriteria { Contain = ContainTypeEnum.Include, GenreId = 123 } };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(),
                It.IsAny<List<int>>(),
                newCriteria,
                deleteCriteriaIds, It.IsAny<List<ProgramCriteria>>(), It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    GenreSearchCriteria = new List<GenreCriteria>
                    {
                        new GenreCriteria {Contain = ContainTypeEnum.Exclude, GenreId = 2, Id = 322},
                        new GenreCriteria {Contain = ContainTypeEnum.Exclude, GenreId = 2, Id = deleteCriteriaIds.First()}
                    }
                }
            };

            var criteria = new OpenMarketCriterion
            {
                GenreSearchCriteria = new List<GenreCriteria>
                {
                    dto.Criteria.GenreSearchCriteria.First(),
                    newCriteria.First()
                }
            };

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ClearExistingCriteria_Saves_NewProgramCriteria()
        {
            var newCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Include, ProgramName = "cde"},
                new ProgramCriteria {Contain = ContainTypeEnum.Exclude, ProgramName = "abc"}
            };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(), It.IsAny<List<int>>(), It.IsAny<List<GenreCriteria>>(), It.IsAny<List<int>>(),
                newCriteria,
                It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion()
            };

            var criteria = new OpenMarketCriterion { ProgramNameSearchCriteria = newCriteria };

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ClearExistingCriteria_Deletes_OldProgramCriteria()
        {
            var deleteCriteriaIds = new List<int> { 420, 322 };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(), It.IsAny<List<int>>(), It.IsAny<List<GenreCriteria>>(), It.IsAny<List<int>>(), It.IsAny<List<ProgramCriteria>>(),
                deleteCriteriaIds));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    ProgramNameSearchCriteria = new List<ProgramCriteria>
                    {
                        new ProgramCriteria {Contain = ContainTypeEnum.Include, ProgramName = "cde", Id = deleteCriteriaIds.First()},
                        new ProgramCriteria {Contain = ContainTypeEnum.Exclude, ProgramName = "abc", Id = deleteCriteriaIds.Last()}
                    }
                }
            };

            var criteria = new OpenMarketCriterion();

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ClearExistingCriteria_Deletes_OldProgramCriteria_And_AddNewProgramCriteria()
        {
            var deleteCriteriaIds = new List<int> { 420 };
            var newCriteria = new List<ProgramCriteria> { new ProgramCriteria { Contain = ContainTypeEnum.Include, ProgramName = "cde" } };


            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(),
                It.IsAny<List<CpmCriteria>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<GenreCriteria>>(), It.IsAny<List<int>>(), newCriteria, deleteCriteriaIds));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    ProgramNameSearchCriteria = new List<ProgramCriteria>
                    {
                        new ProgramCriteria {Contain = ContainTypeEnum.Exclude, ProgramName = "cde", Id = 322},
                        new ProgramCriteria {Contain = ContainTypeEnum.Exclude, ProgramName = "abc", Id = deleteCriteriaIds.First()}
                    }
                }
            };

            var criteria = new OpenMarketCriterion
            {
                ProgramNameSearchCriteria = new List<ProgramCriteria>
                {
                    dto.Criteria.ProgramNameSearchCriteria.First(),
                    newCriteria.First()
                }
            };

            sut.UpdateCriteria(dto, criteria);

            mock.VerifyAll();
        }

        [Test]
        public void ShouldRemoveProgram_Cpm_Min()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.CpmCriteria = new List<CpmCriteria>
            {
                new CpmCriteria
                {
                    MinMax = MinMaxEnum.Min,
                    Value = 5
                }
            };
            var program = new ProposalProgramDto { TargetCpm = 3 };
            Assert.IsTrue(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_Cpm_Max()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.CpmCriteria = new List<CpmCriteria>
            {
                new CpmCriteria
                {
                    MinMax = MinMaxEnum.Max,
                    Value = 5
                }
            };
            var program = new ProposalProgramDto { TargetCpm = 6 };
            Assert.IsTrue(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_Genre_Include()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.GenreSearchCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain =ContainTypeEnum.Include, GenreId = 5}
            };
            var program = new ProposalProgramDto { Genres = new List<LookupDto> { new LookupDto { Id = 6 } } };
            Assert.True(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_Genre_Include2()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.GenreSearchCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain =ContainTypeEnum.Include, GenreId = 5}
            };
            var program = new ProposalProgramDto { Genres = new List<LookupDto> { new LookupDto { Id = 5 } } };
            Assert.False(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_Genre_Exclude()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.GenreSearchCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain =ContainTypeEnum.Exclude, GenreId = 5}
            };
            var program = new ProposalProgramDto { Genres = new List<LookupDto> { new LookupDto { Id = 5 } } };
            Assert.True(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_Genre_Exclude2()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.GenreSearchCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain =ContainTypeEnum.Exclude, GenreId = 5}
            };
            var program = new ProposalProgramDto { Genres = new List<LookupDto> { new LookupDto { Id = 6 } } };
            Assert.False(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_ProgramName_Include()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.ProgramNameSearchCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Include, ProgramName = "ABC"}
            };
            var program = new ProposalProgramDto { ProgramName = "ABC" };
            Assert.False(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_ProgramName_Include2()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.ProgramNameSearchCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Include, ProgramName = "ABC"}
            };
            var program = new ProposalProgramDto { ProgramName = "AB123C123" };
            Assert.True(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_ProgramName_Exclude()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.ProgramNameSearchCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Exclude, ProgramName = "ABC"}
            };
            var program = new ProposalProgramDto { ProgramName = "ABC" };
            Assert.True(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_ProgramName_Exclude2()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.ProgramNameSearchCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Exclude, ProgramName = "ABC"}
            };
            var program = new ProposalProgramDto { ProgramName = "AB123C123" };
            Assert.False(ProposalOpenMarketInventoryService.ShouldRemoveProgram(program, marketCriterion));
        }

        [Ignore("Cannot execute because it waits for an exclusive lock forever")]
        [Test]
        public void AddNewOpenMarketAllocations()
        {
            /*
                insert into station_program_flight_proposal
                (station_program_flight_id, proprosal_version_detail_quarter_week_id, spots, impressions, created_by)
                values
                (483041, 7, 1, 1000, 'test-user')
                insert into station_program_flight_proposal
                (station_program_flight_id, proprosal_version_detail_quarter_week_id, spots, impressions, created_by)
                values
                (483042, 7, 2, 2000, 'test-user')
                GO
             */
            using (var transaction = new TransactionScopeWrapper()) //do not preserve test changes
            {
                var request = new OpenMarketAllocationSaveRequest()
                {
                    ProposalVersionDetailId = 7,
                    Username = "test-user",
                    Weeks = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek>()
                    {
                        new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek()
                        {
                            MediaWeekId = 649,
                            Programs = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram>()
                            {
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram()
                                {
                                    Impressions = 1000,
                                    ProgramId = 2000,
                                    Spots = 1
                                },
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram()
                                {
                                    Impressions = 2000,
                                    ProgramId = 2001,
                                    Spots = 0
                                },
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram()
                                {
                                    Impressions = 1234,
                                    ProgramId = 2004,
                                    Spots = 3
                                }
                            }
                        }
                    }
                };

                var result = _ProposalOpenMarketInventoryService.SaveInventoryAllocations(request);
                Assert.IsNotNull(result);
            }
        }
    }
}