using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProposalInventoryServiceIntegrationTests
    {
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private readonly IProposalProprietaryInventoryService _ProposalProprietaryInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();
        private readonly DateTime _TestDateTime = new DateTime(2016, 02, 15);

        [Test]
        public void SetProprietaryInventoryDaypartAndWeekInfo_Throws_When_ShareHutSweepsBook_Null()
        {
            var sut = (ProposalProprietaryInventoryService)_ProposalProprietaryInventoryService;
            Assert.That(() => sut._SetProprietaryInventoryDaypartAndWeekInfo(new ProposalDetailProprietaryInventoryDto { SharePostingBookId = null, HutPostingBookId = null, SinglePostingBookId = null, GuaranteedAudience = 1 }, 0), Throws.Exception.With.Message.EqualTo(BaseProposalInventoryService.MissingBooksErrorMessage));
        }

        [Test]
        public void SetProprietaryInventoryDaypartAndWeekInfo_Throws_When_GuaranteedAudience_null()
        {
            var sut = (ProposalProprietaryInventoryService)_ProposalProprietaryInventoryService;
            Assert.That(() => sut._SetProprietaryInventoryDaypartAndWeekInfo(new ProposalDetailProprietaryInventoryDto { GuaranteedAudience = null }, 0), Throws.Exception.With.Message.EqualTo(ProposalProprietaryInventoryService.MissingGuaranteedAudienceErorMessage));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProposalInventoryWithWastageIndicator()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalInventory = _ProposalProprietaryInventoryService.GetInventory(2290);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailDaypartId");
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailSpotLengthId");
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailId");
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "ProposalId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "InventoryDetailSlotId");
                jsonResolver.Ignore(typeof(ProposalInventoryDaypartDetailDto), "InventoryDetailId");
                jsonResolver.Ignore(typeof(ProposalInventoryDaypartDetailDto), "DetailLevel");
                jsonResolver.Ignore(typeof(ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto), "InventoryDetailId");
                jsonResolver.Ignore(typeof(ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto), "DetailLevel");
                jsonResolver.Ignore(typeof(ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto), "InventoryDetailSlotId");
                jsonResolver.Ignore(typeof(ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto), "CPM");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailBudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailCpmPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalBudget");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalCpm");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalImpressions");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetTotal");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsTotal");


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
        public void CanLoadProposalInventoryWithWastageIndicatorOnMarketsChange()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(17616);
                var proposalDetail = proposal.Details.First();

                // No wastage indicator from daypart, since
                // the following daypart includes all other dayparts.
                proposalDetail.DaypartId = 0;
                proposalDetail.Daypart = new DaypartDto
                {
                    mon = true,
                    tue = true,
                    wed = true,
                    thu = true,
                    fri = true,
                    sat = true,
                    sun = true,                    
                    startTime = 0,
                    endTime = 86399
                };

                proposal.MarketGroupId = ProposalEnums.ProposalMarketGroups.Top50;

                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _TestDateTime);

                var proposalInventory = _ProposalProprietaryInventoryService.GetInventory(2290);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailDaypartId");
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailSpotLengthId");
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailId");
                jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "ProposalId");
                jsonResolver.Ignore(typeof(InventoryDetailSlotComponents), "InventoryDetailSlotId");
                jsonResolver.Ignore(typeof(ProposalInventoryDaypartDetailDto), "InventoryDetailId");
                jsonResolver.Ignore(typeof(ProposalInventoryDaypartDetailDto), "DetailLevel");
                jsonResolver.Ignore(typeof(ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto), "InventoryDetailId");
                jsonResolver.Ignore(typeof(ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto), "DetailLevel");
                jsonResolver.Ignore(typeof(ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto), "InventoryDetailSlotId");
                jsonResolver.Ignore(typeof(ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto), "CPM");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailBudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailCpmPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalBudget");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalCpm");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalImpressions");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "ProposalVersionId");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetTotal");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsTotal");


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
        public void SaveInventoryAllocations_NoDelete_OneConflict()
        {
            using (new TransactionScopeWrapper())
            {
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();

                var request = new ProprietaryInventoryAllocationRequest();
                request.UserName = "";
                request.SlotAllocations.Add(new ProprietaryInventorySlotAllocations { InventoryDetailSlotId = 10206, Adds = new List<ProprietaryInventorySlotProposal> { new ProprietaryInventorySlotProposal { QuarterWeekId = 7, Order = 1, SpotLength = 30 } } });
                var ret = sut.SaveInventoryAllocations(request);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(ret));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveInventoryAllocations_Delete_NoConflict()
        {
            using (new TransactionScopeWrapper())
            {
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();

                var request = new ProprietaryInventoryAllocationRequest();
                request.ProposalDetailId = 7;
                request.UserName = "";
                request.SlotAllocations.Add(new ProprietaryInventorySlotAllocations
                {
                    InventoryDetailSlotId = 10206,
                    Deletes = new List<ProprietaryInventorySlotProposal> { new ProprietaryInventorySlotProposal { QuarterWeekId = 7, Order = 1, SpotLength = 30 } },
                    Adds = new List<ProprietaryInventorySlotProposal> { new ProprietaryInventorySlotProposal { QuarterWeekId = 7, Order = 1, SpotLength = 30 } },
                });

                var ret = sut.SaveInventoryAllocations(request);
                Assert.That(ret, Is.Empty);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveInventoryAllocations_SaveSnapshot()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();
                var request = new ProprietaryInventoryAllocationRequest {ProposalDetailId = 7, UserName = "test-user"};

                request.SlotAllocations.Add(new ProprietaryInventorySlotAllocations
                {
                    InventoryDetailSlotId = 10206,
                    Deletes =
                        new List<ProprietaryInventorySlotProposal>
                        {
                            new ProprietaryInventorySlotProposal {QuarterWeekId = 7, Order = 1, SpotLength = 30, Impressions = 30203.123d}
                        },
                    Adds =
                        new List<ProprietaryInventorySlotProposal>
                        {
                            new ProprietaryInventorySlotProposal {QuarterWeekId = 7, Order = 1, SpotLength = 30, Impressions = 2000.12d}
                        },
                });

                inventoryService.SaveInventoryAllocations(request);

                var proposalInventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalInventoryRepository>();

                var proprietaryInventoryAllocationSnapshot = proposalInventoryRepository.GetProprietaryInventoryAllocationSnapshot(10206);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proprietaryInventoryAllocationSnapshot));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveInventoryAllocations_CalculateHeaderTotals()
        {
            using (new TransactionScopeWrapper())
            {
                var inventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();
                var request = new ProprietaryInventoryAllocationRequest { ProposalDetailId = 7, UserName = "test-user" };

                request.SlotAllocations.Add(new ProprietaryInventorySlotAllocations
                {
                    InventoryDetailSlotId = 10206,
                    Deletes =
                        new List<ProprietaryInventorySlotProposal>
                        {
                            new ProprietaryInventorySlotProposal {QuarterWeekId = 7, Order = 1, SpotLength = 30, Impressions = 30203.123d}
                        },
                    Adds =
                        new List<ProprietaryInventorySlotProposal>
                        {
                            new ProprietaryInventorySlotProposal {QuarterWeekId = 7, Order = 1, SpotLength = 30, Impressions = 2000.12d}
                        },
                });

                inventoryService.SaveInventoryAllocations(request);

                var proposal = _ProposalService.GetProposalById(248);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposal));
            }
        }
    }
}
