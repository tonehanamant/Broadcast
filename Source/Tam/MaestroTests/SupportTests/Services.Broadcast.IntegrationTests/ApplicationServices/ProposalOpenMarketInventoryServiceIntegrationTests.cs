using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
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
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private readonly IProposalRepository _ProposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
        private readonly IStationProgramRepository _StationProgramRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalMarketsCalculationEngine>();

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
        [Ignore]
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
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "ProgramId");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(ProposalOpenMarketInventoryWeekDto.InventoryWeekProgram), "ProgramId");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "ManifestId");
                jsonResolver.Ignore(typeof(LookupDto), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Ignore]
        public void CanLoadOpenMarketProposalInventory_With_MinCpmRefinements()
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
                            new CpmCriteria { MinMax = MinMaxEnum.Min, Value = 1 }
                        }
                    }
                };
                var proposalInventory = _ProposalOpenMarketInventoryService.RefinePrograms(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(CpmCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketProposalInventory_With_MaxCpmRefinements()
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
                            new CpmCriteria { MinMax = MinMaxEnum.Max, Value = 1 }
                        }
                    }
                };
                var proposalInventory = _ProposalOpenMarketInventoryService.RefinePrograms(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(CpmCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory, jsonSettings));
            }
        }

        [Test]
        [Ignore]
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
                            new ProgramCriteria{Contain = ContainTypeEnum.Include, Program = new LookupDto{Display = "Open Market Program" } }
                        }
                    }
                };
                var proposalInventory = _ProposalOpenMarketInventoryService.RefinePrograms(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");

                var jsonSettings = new JsonSerializerSettings
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
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                var dtoMarkets = dto.Markets;
                dto.Filter.Markets.Add(139);

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);

                Assert.IsTrue(filteredDto.Markets.Count == 1);
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalInventoryByAffiliation()
        {
            // affiliation COZ
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                var dtoPrograms = dto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).ToList();
                dto.Filter.Affiliations.Add("FOX");

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filteredProgram =
                    filteredDto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).ToList();

                Assert.IsTrue(14 == filteredProgram.Count());
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalInventoryByProgramName_SingleProgramName()
        {
            // open market program
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                var dtoPrograms = dto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs));
                dto.Filter.ProgramNames.Add("Fox40 Morning News");

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filteredProgram =
                    filteredDto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs));

                Assert.IsTrue(1 == filteredProgram.Count());
                Assert.IsTrue(filteredProgram.First().ProgramNames.Count() == 1);
            }
        }


        [Test]
        public void CanFilterOpenMarketProposalInventoryByProgramName_MultiProgramNames()
        {
            // open market program
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                var dtoPrograms = dto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs));
                dto.Filter.ProgramNames.Add("America next Morning");

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filteredProgram =
                    filteredDto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs));

                Assert.IsTrue(1 == filteredProgram.Count());
                Assert.IsTrue(filteredProgram.First().ProgramNames.Count() == 2);
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalInventoryByDaypart()
        {
            // open market program
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                var dtoPrograms = dto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs));

                DisplayDaypart daypart;
                DisplayDaypart.TryParse("M-SU 11AM-12:30AM", out daypart);
                dto.Filter.DayParts.Add(DaypartDto.ConvertDisplayDaypart(daypart));

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filtereddaypartCount =
                    filteredDto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).Count();

                Assert.IsTrue(filtereddaypartCount == 2);
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalProgramsWithOutSpotsAllocated()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

                dto.Filter.SpotFilter = ProposalOpenMarketFilter.OpenMarketSpotFilter.ProgramWithoutSpots;

                var filteredDto = _ProposalOpenMarketInventoryService.ApplyFilterOnOpenMarketInventory(dto);
                var filteredProgramsWithSpot =
                    filteredDto.Markets.SelectMany(a => a.Stations.SelectMany(b => b.Programs)).Count();
                // the first tow programs have spots agasint it
                Assert.IsTrue(filteredProgramsWithSpot > 0);
            }
        }

        [Test]
        public void CanFilterOpenMarketProposalProgramsWithSpotsAllocated()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = new ProposalDto();
                var proposalDetailId = ProposalTestHelper.GetPickleProposalDetailId(ref proposal);

                var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                proposalRepository.UpdateProposalDetailSweepsBooks(proposalDetailId, 416, 413);

                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                var programId = dto.Weeks.SelectMany(w => w.Markets).SelectMany(m => m.Stations).SelectMany(s => s.Programs).First(p => p.UnitImpression > 0).ProgramId;

                AllocationProgram(proposalDetailId, programId, proposal.FlightWeeks.First().MediaWeekId);

                dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

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
        public void GetInventory_Refined()
        {
            using (new TransactionScopeWrapper())
            {
                int proposalDetailId = 7;
                var dto = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);
                                
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "ProgramId");
                jsonResolver.Ignore(typeof(LookupDto), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(dto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventory_RefineOnlyIncludeGenre()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new OpenMarketRefineProgramsRequest
                {
                    Criteria = new OpenMarketCriterion
                    {
                        GenreSearchCriteria = new List<GenreCriteria> { new GenreCriteria { Contain = ContainTypeEnum.Include, Genre = new LookupDto { Id = 15 } } }
                    },
                    ProposalDetailId = 7
                };
                var dto = _ProposalOpenMarketInventoryService.RefinePrograms(request);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "ProgramId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(dto, jsonSettings));
            }
        }

        private void AllocationProgram(int proposalDetailId, int programId, int mediaWeekId)
        {
            var request = new OpenMarketAllocationSaveRequest
            {
                ProposalVersionDetailId = proposalDetailId,
                Username = "test-user",
                Weeks = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek>
                {
                    new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek
                    {
                        MediaWeekId = mediaWeekId,
                        Programs = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram>
                        {
                            new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram
                            {
                                UnitImpressions = 1000,
                                TotalImpressions = 10000,
                                ProgramId = programId,
                                Spots = 10
                            }
                        }
                    }
                }
            };

            _ProposalOpenMarketInventoryService.SaveInventoryAllocations(request);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadOpenMarketProposalInventoryWithHutAndShareBooks()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(7);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "ProgramId");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(ProposalOpenMarketInventoryWeekDto.InventoryWeekProgram), "ProgramId");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "ManifestId");
                jsonResolver.Ignore(typeof(LookupDto), "Id");

                var jsonSettings = new JsonSerializerSettings
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
                //var proposalRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                //proposalRepository.UpdateProposalDetailSweepsBook(14, 413);
                var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(14);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "Genres");
                jsonResolver.Ignore(typeof(ProposalInventoryMarketDto.InventoryMarketStationProgram), "ProgramId");
                jsonResolver.Ignore(typeof(ProposalOpenMarketFilter), "SpotFilter");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "RefineFilterPrograms");
                jsonResolver.Ignore(typeof(ProposalOpenMarketInventoryWeekDto.InventoryWeekProgram), "ProgramId");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "ManifestId");
                jsonResolver.Ignore(typeof(LookupDto), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory.Markets, jsonSettings));
            }
        }

        [Ignore]
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

        [Ignore]
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

        [Ignore]
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
        [Ignore]
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
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "Markets");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "Criteria");
                jsonResolver.Ignore(typeof(ProposalDetailOpenMarketInventoryDto), "DisplayFilter");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(filteredAndCalculatedInventory, jsonSettings));

            }
        }

        [Ignore]
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
                        ProgramNameSearchCriteria = new List<ProgramCriteria>
                        {
                            new ProgramCriteria
                            {
                                Contain = ContainTypeEnum.Exclude,
                                Program = new LookupDto{Display = "Open Market Program Too" }
                            }
                        }
                    }
                };

                Assert.IsTrue(
                    _ProposalOpenMarketInventoryService.RefinePrograms(request).NewCriteriaAffectsExistingAllocations);
            }
        }

        [Test]
        [Ignore]
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
                        ProgramNameSearchCriteria = new List<ProgramCriteria>
                        {
                            new ProgramCriteria
                            {
                                Contain = ContainTypeEnum.Exclude,
                                Program = new LookupDto{Display = "Open Market Program Too" }
                            }
                        }
                    }
                };

                Assert.IsFalse(
                    _ProposalOpenMarketInventoryService.RefinePrograms(request).NewCriteriaAffectsExistingAllocations);
            }
        }

        [Test]
        [Ignore]
        public void OpenMarketInventoryOnFilterCPMSetFlagToShowWarning()
        {
            using (new TransactionScopeWrapper())
            {
                var prop = ProposalTestHelper.CreateProposal();
                var request = new OpenMarketRefineProgramsRequest
                {
                    ProposalDetailId = prop.Details.First().Id.Value,
                    Criteria = new OpenMarketCriterion
                    {
                        CpmCriteria = new List<CpmCriteria>
                        {
                            new CpmCriteria()
                            {
                                MinMax = MinMaxEnum.Max,
                                Value = 100.00M
                            }
                        }
                    }
                };
                Assert.IsTrue(_ProposalOpenMarketInventoryService.RefinePrograms(request).NewCriteriaAffectsExistingAllocations);
            }
        }

        [Ignore]
        [Test]
        public void OpenMarketInventoryFilterCPM()
        {
            using (new TransactionScopeWrapper())
            {
                var prop = ProposalTestHelper.CreateProposal();
                var request = new OpenMarketRefineProgramsRequest
                {
                    ProposalDetailId = prop.Details.First().Id.Value,
                    IgnoreExistingAllocation = true,
                    Criteria = new OpenMarketCriterion
                    {
                        CpmCriteria = new List<CpmCriteria>
                        {
                            new CpmCriteria()
                            {
                                MinMax = MinMaxEnum.Max,
                                Value = 100.00M
                            }
                        }
                    }
                };

                var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory;
                // get allocations before filter
                var rawAllocations = repo.GetDataRepository<IProposalOpenMarketInventoryRepository>().GetProposalDetailAllocations(request.ProposalDetailId);
                Assert.IsNotNull(rawAllocations.FirstOrDefault(a => a.ManifestId == 519159));

                var refinedPrograms = _ProposalOpenMarketInventoryService.RefinePrograms(request);
                // that should have filtered out exactly one allocation with station_program_id = 519159

                var refinedAllocations = repo.GetDataRepository<IProposalOpenMarketInventoryRepository>().GetProposalDetailAllocations(request.ProposalDetailId);
                Assert.IsNull(refinedAllocations.FirstOrDefault(a => a.ManifestId == 519159));
            }
        }

        [Ignore]
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
                        ProgramNameSearchCriteria = new List<ProgramCriteria>
                        {
                            new ProgramCriteria {Contain = ContainTypeEnum.Exclude, Program = new LookupDto{Display = proposalInventory.RefineFilterPrograms.First() } }
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
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

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
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

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
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

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
                new GenreCriteria {Contain = ContainTypeEnum.Include,Genre = new LookupDto(1,"")},
                new GenreCriteria {Contain = ContainTypeEnum.Exclude,Genre = new LookupDto(2,"")}
            };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(), It.IsAny<List<int>>(),
                newCriteria,
                It.IsAny<List<int>>(), It.IsAny<List<ProgramCriteria>>(), It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

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
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    GenreSearchCriteria = new List<GenreCriteria>
                    {
                        new GenreCriteria {Contain = ContainTypeEnum.Include, Id = deleteCriteriaIds.First(),Genre = new LookupDto(1,"")},
                        new GenreCriteria {Contain = ContainTypeEnum.Exclude, Id = deleteCriteriaIds.Last(),Genre = new LookupDto(2,"")}
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
            var newCriteria = new List<GenreCriteria>(123) { new GenreCriteria { Contain = ContainTypeEnum.Include } };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(),
                It.IsAny<List<int>>(),
                newCriteria,
                deleteCriteriaIds, It.IsAny<List<ProgramCriteria>>(), It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    GenreSearchCriteria = new List<GenreCriteria>
                    {
                        new GenreCriteria {Contain = ContainTypeEnum.Exclude, Id = 322,Genre = new LookupDto(2,"")},
                        new GenreCriteria {Contain = ContainTypeEnum.Exclude, Id = deleteCriteriaIds.First(),Genre = new LookupDto(2,"")}
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
                new ProgramCriteria {Contain = ContainTypeEnum.Include, Program = new LookupDto{ Display = "cde" } },
                new ProgramCriteria {Contain = ContainTypeEnum.Exclude, Program = new LookupDto { Display = "abc"} }
            };

            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(), It.IsAny<List<CpmCriteria>>(), It.IsAny<List<int>>(), It.IsAny<List<GenreCriteria>>(), It.IsAny<List<int>>(),
                newCriteria,
                It.IsAny<List<int>>()));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

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
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    ProgramNameSearchCriteria = new List<ProgramCriteria>
                    {
                        new ProgramCriteria {Contain = ContainTypeEnum.Include, Program = new LookupDto{ Display = "cde" }, Id = deleteCriteriaIds.First()},
                        new ProgramCriteria {Contain = ContainTypeEnum.Exclude, Program = new LookupDto{ Display = "abc" }, Id = deleteCriteriaIds.Last()}
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
            var newCriteria = new List<ProgramCriteria> { new ProgramCriteria { Contain = ContainTypeEnum.Include, Program = new LookupDto { Display = "cde" } } };


            var mock = new Mock<IProposalProgramsCriteriaRepository>();
            mock.Setup(m => m.UpdateCriteria(It.IsAny<int>(),
                It.IsAny<List<CpmCriteria>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<GenreCriteria>>(), It.IsAny<List<int>>(), newCriteria, deleteCriteriaIds));

            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IProposalProgramsCriteriaRepository>()).Returns(mock.Object);
            var sut = new ProposalOpenMarketInventoryService(factory.Object, null, null, null, null, null, null, null);

            var dto = new ProposalDetailOpenMarketInventoryDto
            {
                Criteria = new OpenMarketCriterion
                {
                    ProgramNameSearchCriteria = new List<ProgramCriteria>
                    {
                        new ProgramCriteria {Contain = ContainTypeEnum.Exclude, Program = new LookupDto{Display = "cde" }, Id = 322},
                        new ProgramCriteria {Contain = ContainTypeEnum.Exclude, Program = new LookupDto{Display = "abc" }, Id = deleteCriteriaIds.First()}
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
            var cpmCriteria = new List<CpmCriteria>
            {
                new CpmCriteria
                {
                    MinMax = MinMaxEnum.Min,
                    Value = 5
                }
            };
            var program = new ProposalProgramDto { TargetCpm = 3 };
            Assert.IsTrue(ProposalOpenMarketInventoryService.FilterByCpmCriteria(program, cpmCriteria));
        }

        [Test]
        public void ShouldRemoveProgram_Cpm_Max()
        {
            var cpmCriteria = new List<CpmCriteria>
            {
                new CpmCriteria
                {
                    MinMax = MinMaxEnum.Max,
                    Value = 5
                }
            };

            var program = new ProposalProgramDto { TargetCpm = 6 };
            Assert.IsTrue(ProposalOpenMarketInventoryService.FilterByCpmCriteria(program, cpmCriteria));
        }

        [Test]
        public void ShouldRemoveProgram_Genre_Include()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.GenreSearchCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain =ContainTypeEnum.Include,Genre = new LookupDto {Id=5}}
            };
            var program = new ProposalProgramDto { Genres = new List<LookupDto> { new LookupDto { Id = 6 } } };
            Assert.True(ProposalOpenMarketInventoryService.FilterByGenreAndProgramNameCriteria(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_Genre_Include2()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.GenreSearchCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain =ContainTypeEnum.Include,Genre = new LookupDto {Id=5}}
            };
            var program = new ProposalProgramDto { Genres = new List<LookupDto> { new LookupDto { Id = 5 } } };
            Assert.False(ProposalOpenMarketInventoryService.FilterByGenreAndProgramNameCriteria(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_Genre_Exclude()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.GenreSearchCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain =ContainTypeEnum.Exclude,Genre = new LookupDto {Id=5}}
            };
            var program = new ProposalProgramDto { Genres = new List<LookupDto> { new LookupDto { Id = 5 } } };
            Assert.True(ProposalOpenMarketInventoryService.FilterByGenreAndProgramNameCriteria(program, marketCriterion));
        }

        [Test]
        public void ShouldRemoveProgram_Genre_Exclude2()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.GenreSearchCriteria = new List<GenreCriteria>
            {
                new GenreCriteria {Contain =ContainTypeEnum.Exclude,Genre = new LookupDto {Id=5}}
            };
            var program = new ProposalProgramDto { Genres = new List<LookupDto> { new LookupDto { Id = 6 } } };
            Assert.False(ProposalOpenMarketInventoryService.FilterByGenreAndProgramNameCriteria(program, marketCriterion));
        }

        [Ignore]
        [Test]
        public void ShouldRemoveProgram_ProgramName_Include()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.ProgramNameSearchCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Include, Program = new LookupDto{ Display = "ABC" } }
            };
            //var program = new ProposalProgramDto { ProgramName = "ABC" };
            //Assert.False(ProposalOpenMarketInventoryService.FilterByGenreAndProgramNameCriteria(program, marketCriterion));
        }

        [Ignore]
        [Test]
        public void ShouldRemoveProgram_ProgramName_Include2()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.ProgramNameSearchCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Include, Program = new LookupDto{Display = "ABC" } }
            };
            //var program = new ProposalProgramDto { ProgramName = "AB123C123" };
            //Assert.True(ProposalOpenMarketInventoryService.FilterByGenreAndProgramNameCriteria(program, marketCriterion));
        }

        [Ignore]
        [Test]
        public void ShouldRemoveProgram_ProgramName_Exclude()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.ProgramNameSearchCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Exclude, Program = new LookupDto{ Display = "ABC" } }
            };
            //var program = new ProposalProgramDto { ProgramName = "ABC" };
            //Assert.True(ProposalOpenMarketInventoryService.FilterByGenreAndProgramNameCriteria(program, marketCriterion));
        }

        [Ignore]
        [Test]
        public void ShouldRemoveProgram_ProgramName_Exclude2()
        {
            var marketCriterion = new OpenMarketCriterion();
            marketCriterion.ProgramNameSearchCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria {Contain = ContainTypeEnum.Exclude, Program = new LookupDto{Display = "ABC" } }
            };
            //var program = new ProposalProgramDto { ProgramName = "AB123C123" };
            //Assert.False(ProposalOpenMarketInventoryService.FilterByGenreAndProgramNameCriteria(program, marketCriterion));
        }

        [Test]
        public void AddAllocationsTest()
        {
            using (new TransactionScopeWrapper())
            {
                const int programId = 46003;

                var request = new OpenMarketAllocationSaveRequest
                {
                    ProposalVersionDetailId = 7,
                    Username = "test-user",
                    Weeks = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek>
                    {
                        new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek
                        {
                            MediaWeekId = 649,
                            Programs = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram>
                            {
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram
                                {
                                    UnitImpressions = 1000,
                                    TotalImpressions = 10000,
                                    ProgramId = programId,
                                    Spots = 10
                                }
                            }
                        }
                    }
                };

                _ProposalOpenMarketInventoryService.SaveInventoryAllocations(request);

                var allocations =
                    _ProposalOpenMarketInventoryService.GetProposalInventoryAllocations(request.ProposalVersionDetailId);

                var firstAllocation = allocations.First();

                Assert.AreEqual(11, allocations.Count);
                Assert.AreEqual(programId, firstAllocation.ManifestId);
            }
        }

        [Test]
        public void RemoveAllocationsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new OpenMarketAllocationSaveRequest
                {
                    // There are five spots allocated for this 
                    // proposal detail saved in the integration database.
                    ProposalVersionDetailId = 8123,
                    Username = "test-user",
                    Weeks = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek>
                    {
                        new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek
                        {
                            MediaWeekId = 705,
                            Programs = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram>
                            {
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram
                                {
                                    UnitImpressions = 1000,
                                    ProgramId = 26672,
                                    Spots = 3
                                }
                            }
                        }
                    }
                };

                _ProposalOpenMarketInventoryService.SaveInventoryAllocations(request);

                var allocations =
                    _ProposalOpenMarketInventoryService.GetProposalInventoryAllocations(request.ProposalVersionDetailId);

                Assert.AreEqual(3, allocations.Count);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateProposalHeaderTotalsAfterInventoryIsSaved()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new OpenMarketAllocationSaveRequest
                {
                    ProposalVersionDetailId = 7,
                    Username = "test-user",
                    Weeks = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek>
                    {
                        new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek
                        {
                            MediaWeekId = 649,
                            Programs = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram>
                            {
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram
                                {
                                    UnitImpressions = 1000,
                                    ProgramId = 2000,
                                    Spots = 1,
                                    UnitCost = 1234
                                },
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram
                                {
                                    UnitImpressions = 2000,
                                    ProgramId = 2001,
                                    Spots = 50,
                                    UnitCost = 0
                                },
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram
                                {
                                    UnitImpressions = 1234,
                                    ProgramId = 2004,
                                    Spots = 10,
                                    UnitCost = 488
                                }
                            }
                        }
                    }
                };

                _ProposalOpenMarketInventoryService.SaveInventoryAllocations(request);

                var proposal = _ProposalService.GetProposalById(248);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposal));
            }
        }

        [Ignore]
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot allocate spots that have zero impressions", MatchType = MessageMatch.Contains)]
        public void CannotSaveInventoryAllocationWhenImpressionsIsZero()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new OpenMarketAllocationSaveRequest
                {
                    ProposalVersionDetailId = 7,
                    Username = "test-user",
                    Weeks = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek>
                    {
                        new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeek
                        {
                            MediaWeekId = 649,
                            Programs = new List<OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram>
                            {
                                new OpenMarketAllocationSaveRequest.OpenMarketAllocationWeekProgram
                                {
                                    UnitImpressions = 0,
                                    ProgramId = 2000,
                                    Spots = 1,
                                    UnitCost = 2.5m
                                },
                            }
                        }
                    }
                };

                _ProposalOpenMarketInventoryService.SaveInventoryAllocations(request);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void Proposal_Update_Delete_Allocation_Outside_Daypart_BCOP1932()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = ProposalTestHelper.CreateProposal();

                // this has 3 allocation with dayparts: M 8PM-9PM;  TU 8PM - 10PM; SA 5:30PM - 9PM
                // the proposal detail has daypart  M-SU 8AM-11PM.
                // if we update the proposal to M-SU 9PM-11PM, that should remove allocation for SA 5:30PM - 9PM and M 8PM-9PM; 

                var newProposalDetailDaypart = DaypartCache.Instance.GetDisplayDaypart(proposal.Details.First().DaypartId);
                newProposalDetailDaypart.StartTime = Convert.ToInt32(Convert.ToDateTime("1/1/2017 9PM").TimeOfDay.TotalSeconds);
                var detail = proposal.Details.First();
                proposal.Details.First().Daypart = DaypartDto.ConvertDisplayDaypart(newProposalDetailDaypart);
                detail.DaypartId = DaypartCache.Instance.GetIdByDaypart(newProposalDetailDaypart);
                _ProposalService.SaveProposal(proposal, "Test user", DateTime.Now);

                var allocations = _ProposalOpenMarketInventoryService.GetProposalInventoryAllocations(detail.Id.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(OpenMarketInventoryAllocation), "ProposalVersionDetailQuarterWeekId");
                jsonResolver.Ignore(typeof(OpenMarketInventoryAllocation), "ProposalVersionDetailId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(allocations, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Proposal_Remove_Allocations_On_Status_Changed()
        {
            //Confirms that all spot allocations are removed when proposal status is changed
            //from Agency On Hold to Proposed
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(24425);

                proposal.Status = ProposalEnums.ProposalStatusType.Proposed;
                proposal.ForceSave = true;
                _ProposalService.SaveProposal(proposal, "Test user", DateTime.Now);

                var allocations = _ProposalOpenMarketInventoryService.GetProposalInventoryAllocations(8123);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(OpenMarketInventoryAllocation), "ProposalVersionDetailQuarterWeekId");
                jsonResolver.Ignore(typeof(OpenMarketInventoryAllocation), "ProposalVersionDetailId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(allocations, jsonSettings));
            }
        }

        [Test]
        public void ProposalOpenMarketService_CanGetInventory()
        {
            var inventory = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId: 10799);
            var inventoryJson = IntegrationTestHelper.ConvertToJson(inventory);

            Approvals.Verify(inventoryJson);
        }

        [Test]
        public void ProposalOpenMarketService_UseStationImpressions()
        {
            var proposalDetailId = 10799;
            var inventory = _ProposalOpenMarketInventoryService.GetInventory(proposalDetailId);

            var dto = _ProposalRepository.GetOpenMarketProposalDetailInventory(proposalDetailId);
            var proposalMarketIds = _ProposalMarketsCalculationEngine.GetProposalMarketsList(dto.ProposalId, dto.ProposalVersion, dto.DetailId).Select(m => (short)m.Id).ToList();
            var programs = _StationProgramRepository.GetStationProgramsForProposalDetail(
                dto.DetailFlightStartDate, 
                dto.DetailFlightEndDate, 
                dto.DetailSpotLengthId, 
                BroadcastConstants.OpenMarketSourceId, 
                proposalMarketIds, 
                dto.DetailId);

            var programWithStationImpressionsExcpected = programs.First(x => x.ManifestAudiences.Any(ma => ma.Impressions != null));
            var manifestAudiencesExpected = programWithStationImpressionsExcpected.ManifestAudiences.First(ma => ma.Impressions != null);
            var allProgramsResult = inventory.Markets
                .SelectMany(x => x.Stations)
                .SelectMany(x => x.Programs);
            var programWithStationImpressionsExists = allProgramsResult.Any(x => 
                x.ProgramId == programWithStationImpressionsExcpected.ManifestId && 
                x.UnitImpressions == manifestAudiencesExpected.Impressions);

            Assert.IsTrue(programWithStationImpressionsExists);
        }
    }
}