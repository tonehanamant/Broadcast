using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.PricingGuide;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Microsoft.Practices.Unity;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PricingGuideServiceIntegrationTests
    {
        private readonly IPricingGuideService _PricingGuideService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPricingGuideService>();
        private readonly IMarketService _MarketService = IntegrationTestApplicationServiceFactory.GetApplicationService<IMarketService>();
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private readonly IDaypartCache _DaypartCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IDaypartCache>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingGuideForProposalDetail()
        {
            using (new TransactionScopeWrapper())
            {
                PricingGuideDto proposalInventory = _PricingGuideService.GetPricingGuideForProposalDetail(13402);
                _VerifyPricingGuideModel(proposalInventory);                
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePricingGuide()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new ProposalDetailPricingGuideSaveRequest
                {
                    Inflation = 1,
                    Margin = 1,
                    ImpressionLoss = 1,
                    CpmMax = 10,
                    CpmMin = 10,
                    GoalBudget = 10000,
                    GoalImpression = 10000,
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min,
                    OpenMarketTotals = new OpenMarketTotalsDto
                    {
                        Cost = 1000,
                        Coverage = 80,
                        Cpm = 10,
                        Impressions = 10000
                    },
                    ProprietaryPricing = new List<ProprietaryPricingDto>() { new ProprietaryPricingDto { Cpm = 10, ImpressionsBalance = 10, InventorySource = InventorySourceEnum.CNN } },
                    ProprietaryTotals = new ProprietaryTotalsDto { Cost = 10, Cpm = 10, Impressions = 10000 },
                    UnitCapPerStation = 1,
                    ProposalDetailId = 13402,
                    MarketCoverageFileId = 1,
                    Markets = new List<PricingGuideSaveMarketRequest>() {
                        new PricingGuideSaveMarketRequest
                        {
                            ProgramId = 26589,
                            BlendedCpm = 7.530701184311377M,
                            CostPerSpot = 1,
                            DaypartId = 1,
                            ImpressionsPerSpot = 10,
                            ManifestDaypartId = 213348,
                            MarketId = 101,
                            ProgramName = "CBS Morning News",
                            Spots = 1,
                            StationCode = 5060,
                            StationImpressionsPerSpot = 43156.671875
                        }
                    }
                };

                _PricingGuideService.SaveDistribution(request, "integration test");

                var proposalInventory = _PricingGuideService.GetPricingGuideForProposalDetail(13402);

                _VerifyPricingGuideModel(proposalInventory);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void OpenMarketPricingGuide()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26016,
                    ProposalDetailId = 9978
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void OpenMarketPricingGuide_HasMarketsNotSelected()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 3134,
                    ProposalDetailId = 3253
                };
                var pricingGuideDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = new IgnorableSerializerContractResolver()
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void CanGetOpenMarketPricingGuideWithProgramsGroupedByDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26017,
                    ProposalDetailId = 9979
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void CanGetOpenMarketPricingGuideWithIncludeGenreCriteria()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26018,
                    ProposalDetailId = 9980
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void CanGetOpenMarketPricingGuideWithExcludeGenreCriteria()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26019,
                    ProposalDetailId = 9981
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void CanGetOpenMarketPricingGuideWithMultipleIncludeGenreCriteria()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26020,
                    ProposalDetailId = 9982
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void CanGetOpenMarketPricingGuideWithAllocationGoals()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26020,
                    ProposalDetailId = 9982,
                    BudgetGoal = 10000,
                    OpenMarketShare = 1
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void CanGetOpenMarketPricingGuideWithAllocationGoalsMultiplePrograms()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26021,
                    ProposalDetailId = 9983,
                    BudgetGoal = 10000,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        UnitCapPerStation = 10
                    },
                    OpenMarketShare = 1
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto));
            }
        }

        [Test]
        public void MatchesProgramsUsingProgramNameFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var expectedProgramNames = dto.DisplayFilter.ProgramNames
                                            .Where((name, index) => index == 0)
                                            .ToList();
            dto.Filter.ProgramNames = expectedProgramNames;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultHasProgramsOnlyWithExpectedNames = result.Markets
                                                               .SelectMany(m => m.Stations)
                                                               .SelectMany(s => s.Programs)
                                                               .Select(p => p.ProgramName)
                                                               .All(p => expectedProgramNames.Contains(p));

            Assert.IsTrue(resultHasProgramsOnlyWithExpectedNames);
        }

        [Test]
        public void DoesNotMatchProgramsUsingProgramNameFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var notExpectedProgramNames = new List<string> { "NotExpectedProgramName" };
            dto.Filter.ProgramNames = notExpectedProgramNames;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultDoesNotHavePrograms = !result.Markets
                                                  .SelectMany(m => m.Stations)
                                                  .SelectMany(s => s.Programs)
                                                  .Any();

            Assert.IsTrue(resultDoesNotHavePrograms);
        }

        [Test]
        public void MatchesMarketsUsingMarketsFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var expectedMarketIds = dto.DisplayFilter.Markets
                                            .Where((x, index) => index == 0)
                                            .Select(m => m.Id)
                                            .ToList();
            dto.Filter.Markets = expectedMarketIds;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultHasMarketsOnlyWithExpectedIds = result.Markets.All(m => expectedMarketIds.Contains(m.MarketId));

            Assert.IsTrue(resultHasMarketsOnlyWithExpectedIds);
        }

        [Test]
        public void DoesNotMatchMarketsUsingMarketsFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var notExpectedMarketIds = new List<int> { -1 };
            dto.Filter.Markets = notExpectedMarketIds;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultDoesNotHaveMarkets = !result.Markets.Any();

            Assert.IsTrue(resultDoesNotHaveMarkets);
        }

        [Test]
        public void MatchesStationsUsingAffiliationsFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var expectedAffiliations = dto.DisplayFilter.Affiliations
                                                        .Where((x, index) => index == 0)
                                                        .ToList();
            dto.Filter.Affiliations = expectedAffiliations;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultHasStationsOnlyWithExpectedAffiliations = result.Markets
                                                                      .SelectMany(m => m.Stations)
                                                                      .All(s => expectedAffiliations.Contains(s.Affiliation));

            Assert.IsTrue(resultHasStationsOnlyWithExpectedAffiliations);
        }

        [Test]
        public void DoesNotMatchStationsUsingAffiliationsFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var notExpectedAffiliations = new List<string> { "NotExpectedAffiliation" };
            dto.Filter.Affiliations = notExpectedAffiliations;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultDoesNotHaveStations = !result.Markets
                                                   .SelectMany(m => m.Stations)
                                                   .Any();

            Assert.IsTrue(resultDoesNotHaveStations);
        }

        [Test]
        public void MatchesProgramsUsingGenresFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var program = dto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs).First();
            program.Genres.Add(new LookupDto { Id = 2 });
            var expectedGenres = new List<int> { 1, 2, 3 };
            dto.Filter.Genres = expectedGenres;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultHasProgramsOnlyWhichContainOneOfExpectedGenres = result.Markets
                                                                             .SelectMany(m => m.Stations)
                                                                             .SelectMany(s => s.Programs)
                                                                             .All(p => p.Genres.Any(g => expectedGenres.Contains(g.Id)));

            Assert.IsTrue(resultHasProgramsOnlyWhichContainOneOfExpectedGenres);
        }

        [Test]
        public void DoesNotMatchProgramsUsingGenresFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var notExpectedGenres = new List<int> { -1 };
            dto.Filter.Genres = notExpectedGenres;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultDoesNotHavePrograms = !result.Markets
                                                   .SelectMany(m => m.Stations)
                                                   .SelectMany(s => s.Programs)
                                                   .Any();

            Assert.IsTrue(resultDoesNotHavePrograms);
        }

        [Test]
        [Category("Impressions")]
        public void TotalsAreUpdatedWhenApplyingFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978,
                OpenMarketPricing =
                {
                     OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                }

            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var marketBeforeFiltering = dto.Markets.First();
            var totalCostBeforeFiltering = marketBeforeFiltering.TotalCost;
            var totalImpressionsBeforeFiltering = marketBeforeFiltering.TotalImpressions;

            var notExpectedProgramNames = new List<string> { "NotExpectedProgramName" };
            dto.Filter.ProgramNames = notExpectedProgramNames;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var marketAfterFiltering = result.Markets.First();
            var totalCostAfterFiltering = marketBeforeFiltering.TotalCost;
            var totalImpressionsAfterFiltering = marketBeforeFiltering.TotalImpressions;

            Assert.AreNotEqual(totalCostBeforeFiltering, totalCostAfterFiltering);
            Assert.AreNotEqual(totalImpressionsBeforeFiltering, totalImpressionsAfterFiltering);
        }

        [Test]
        public void MatchesProgramsUsingAirtimesFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };

            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var expectedAirtimes = new List<DaypartDto>
            {
                new DaypartDto
                {
                    tue = true,
                    wed = true,
                    startTime = 30000,
                    endTime = 60000
                }
            };
            dto.Filter.DayParts = expectedAirtimes;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultHasProgramsOnlyWhichHasAirtimeThatIntersectsWithOneOfExpectedOnes = result.Markets
                                                                             .SelectMany(m => m.Stations)
                                                                             .SelectMany(s => s.Programs)
                                                                             .All(p => expectedAirtimes.Any(a =>
                                                                                    DisplayDaypart.Intersects(
                                                                                        DaypartDto.ConvertDaypartDto(a),
                                                                                        _DaypartCache.GetDisplayDaypart(p.Daypart.Id))));

            Assert.IsTrue(resultHasProgramsOnlyWhichHasAirtimeThatIntersectsWithOneOfExpectedOnes);
        }

        [Test]
        public void DoesNotMatchProgramsUsingAirtimesFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };

            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var notExpectedAirtimes = new List<DaypartDto>
            {
                new DaypartDto
                {
                    sun = true,
                    sat = true,
                    startTime = 30000,
                    endTime = 60000
                }
            };
            dto.Filter.DayParts = notExpectedAirtimes;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultDoesNotHavePrograms = !result.Markets
                                                   .SelectMany(m => m.Stations)
                                                   .SelectMany(s => s.Programs)
                                                   .Any();

            Assert.IsTrue(resultDoesNotHavePrograms);
        }

        [Test]
        public void MatchesAllProgramsUsingSpotsFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };

            var dto = _PricingGuideService.GetOpenMarketInventory(request);

            // creating test programs with spots and without spots
            var station = dto.Markets.First().Stations.First();
            station.Programs.Add(new PricingGuideProgramDto { Spots = 0 });
            station.Programs.Add(new PricingGuideProgramDto { Spots = 5 });

            dto.Filter.SpotFilter = OpenMarketPricingGuideGridFilterDto.OpenMarketSpotFilter.AllPrograms;
            var amountOfProgramsBeforeFiltering = dto.Markets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).Count();

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var amountOfProgramsAfterFiltering = result.Markets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).Count();

            Assert.AreEqual(amountOfProgramsBeforeFiltering, amountOfProgramsAfterFiltering);
        }

        [Test]
        public void MatchesProgramsWithSpotsUsingSpotsFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };

            var dto = _PricingGuideService.GetOpenMarketInventory(request);

            // creating test programs with spots and without spots
            var station = dto.Markets.First().Stations.First();
            station.Programs.Add(new PricingGuideProgramDto { Spots = 0 });
            station.Programs.Add(new PricingGuideProgramDto { Spots = 5 });

            dto.Filter.SpotFilter = OpenMarketPricingGuideGridFilterDto.OpenMarketSpotFilter.ProgramWithSpots;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultHasProgramsOnlyWithSpots = result.Markets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).All(p => p.Spots > 0);

            Assert.IsTrue(resultHasProgramsOnlyWithSpots);
        }

        [Test]
        public void MatchesProgramsWithoutSpotsUsingSpotsFilter()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };

            var dto = _PricingGuideService.GetOpenMarketInventory(request);

            // creating test programs with spots and without spots
            var station = dto.Markets.First().Stations.First();
            station.Programs.Add(new PricingGuideProgramDto { Spots = 0 });
            station.Programs.Add(new PricingGuideProgramDto { Spots = 5 });

            dto.Filter.SpotFilter = OpenMarketPricingGuideGridFilterDto.OpenMarketSpotFilter.ProgramWithoutSpots;

            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var resultHasProgramsOnlyWithoutSpots = result.Markets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).All(p => p.Spots == 0);

            Assert.IsTrue(resultHasProgramsOnlyWithoutSpots);
        }

        [Test]
        public void ProposalOpenMarketInventoryService_SortsMarketsByRankAsc_WhenGettingPricingGuideOpenMarketInventory()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 17616,
                ProposalDetailId = 2290
            };

            var dto = _PricingGuideService.GetOpenMarketInventory(request);

            var marketsSortedByRankAsc = true;
            var previousMarketRank = -1;

            foreach (var market in dto.Markets)
            {
                if (previousMarketRank > market.MarketRank)
                {
                    marketsSortedByRankAsc = false;
                    break;
                }

                previousMarketRank = market.MarketRank;
            }

            Assert.IsTrue(marketsSortedByRankAsc);
        }

        [Test]
        public void ProposalOpenMarketInventoryService_SortsMarketsByRankAsc_WhenApplyingPricingGuideOpenMarketInventory()
        {
            var random = new Random();
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 17616,
                ProposalDetailId = 2290
            };

            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            _ShuffleList(dto.Markets, random);
            var result = _PricingGuideService.ApplyFilterOnOpenMarketGrid(dto);

            var marketsSortedByRankAsc = true;
            var previousMarketRank = -1;

            foreach (var market in result.Markets)
            {
                if (previousMarketRank > market.MarketRank)
                {
                    marketsSortedByRankAsc = false;
                    break;
                }

                previousMarketRank = market.MarketRank;
            }

            Assert.IsTrue(marketsSortedByRankAsc);
        }

        [Test]
        public void ProposalOpenMarketInventoryService_ReturnsPricingGuideOpenMarketInventory_WithProgramsFilteredByName()
        {
            using (new TransactionScopeWrapper())
            {
                const int proposalId = 26017;
                const int proposalDetailId = 9979;

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = proposalId,
                    ProposalDetailId = proposalDetailId
                };

                // Setting program criteria empty
                var proposal = _ProposalService.GetProposalById(proposalId);
                var detail = proposal.Details.First(x => x.Id == proposalDetailId);
                detail.ProgramCriteria = new List<ProgramCriteria>();
                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", DateTime.Now);

                var dto = _PricingGuideService.GetOpenMarketInventory(request);
                var programs = dto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs);
                Assert.IsTrue(programs.Any(x => x.ProgramName == "Friends|Friends 2"));
                Assert.IsFalse(programs.Any(x => x.ProgramName == "Friends"));
                Assert.IsFalse(programs.Any(x => x.ProgramName == "Friends 2"));


                // Setting program criteria for excluding 'Friends' program
                proposal = _ProposalService.GetProposalById(proposalId);
                detail = proposal.Details.First(x => x.Id == proposalDetailId);
                detail.ProgramCriteria = new List<ProgramCriteria>
                {
                    new ProgramCriteria
                    {
                        Contain = ContainTypeEnum.Exclude,
                        Program = new LookupDto
                        {
                            Id = 100,
                            Display = "Friends"
                        }
                    }
                };
                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", DateTime.Now);

                dto = _PricingGuideService.GetOpenMarketInventory(request);
                programs = dto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs);
                Assert.IsFalse(programs.Any(x => x.ProgramName == "Friends|Friends 2"));
                Assert.IsFalse(programs.Any(x => x.ProgramName == "Friends"));
                Assert.IsFalse(programs.Any(x => x.ProgramName == "Friends 2"));


                // Setting program criteria for excluding 'Friends 2' program
                proposal = _ProposalService.GetProposalById(proposalId);
                detail = proposal.Details.First(x => x.Id == proposalDetailId);
                detail.ProgramCriteria = new List<ProgramCriteria>
                {
                    new ProgramCriteria
                    {
                        Contain = ContainTypeEnum.Exclude,
                        Program = new LookupDto
                        {
                            Id = 101,
                            Display = "Friends 2"
                        }
                    }
                };
                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", DateTime.Now);

                dto = _PricingGuideService.GetOpenMarketInventory(request);
                programs = dto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs);
                Assert.IsFalse(programs.Any(x => x.ProgramName == "Friends|Friends 2"));
                Assert.IsFalse(programs.Any(x => x.ProgramName == "Friends"));
                Assert.IsFalse(programs.Any(x => x.ProgramName == "Friends 2"));


                // Setting program criteria empty as it was initially
                proposal = _ProposalService.GetProposalById(proposalId);
                detail = proposal.Details.First(x => x.Id == proposalDetailId);
                detail.ProgramCriteria = new List<ProgramCriteria>();
                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", DateTime.Now);
            }
        }

        [Test]
        public void ProposalOpenMarketService_SetsHasImpressionsPropertyTrue_ForPricingModel_ForProgramsThatHaveImpressions()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

            var programs = pricingGuideOpenMarketDto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs);
            var programsWithImpressions = programs.Where(x => x.EffectiveImpressionsPerSpot > 0);
            var programsWithImpressionsHavePropertySetTrue = programsWithImpressions.All(x => x.HasImpressions);

            Assert.True(programsWithImpressionsHavePropertySetTrue);
        }

        [Test]
        public void ProposalOpenMarketService_SetsHasImpressionsPropertyFalse_ForPricingModel_ForProgramsThatDoNotHaveImpressions()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = 9978
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

            var programs = pricingGuideOpenMarketDto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs);
            var programsWithImpressions = programs.Where(x => x.EffectiveImpressionsPerSpot <= 0);
            var programsWithImpressionsHavePropertySetFalse = programsWithImpressions.All(x => !x.HasImpressions);

            Assert.True(programsWithImpressionsHavePropertySetFalse);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void SavePricingGuideAllocations()
        {
            const int proposalDetailId = 9978;
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = proposalDetailId
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var market = dto.Markets.First(m => m.Stations.Any(s => s.Programs.Any()));
            var station = market.Stations.First(s => s.Programs.Any());
            var program = station.Programs.First();

            program.Spots = 5;
            var allocationRequest = new PricingGuideDto
            {
                ProposalDetailId = proposalDetailId,
                Markets = dto.Markets,
                Filter = dto.Filter,
            };

            var result = _PricingGuideService.SaveAllocations(allocationRequest);
            var resultJson = IntegrationTestHelper.ConvertToJson(result, _GetPricingGuideJsonSerializerSettings());

            Approvals.Verify(resultJson);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot allocate spots that have zero impressions",
            MatchType = MessageMatch.Contains)]
        public void SavePricingGuideAllocations_WithoutImpressions()
        {
            const int proposalDetailId = 9978;
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26016,
                ProposalDetailId = proposalDetailId
            };
            var dto = _PricingGuideService.GetOpenMarketInventory(request);
            var market = dto.Markets.First(m => m.Stations.Any(s => s.Programs.Any()));
            var station = market.Stations.First(s => s.Programs.Any());
            var program = station.Programs.First();

            program.Spots = program.Spots + 5;
            program.ImpressionsPerSpot = 0;
            program.StationImpressionsPerSpot = 0;
            var allocationRequest = new PricingGuideDto
            {
                ProposalDetailId = proposalDetailId,
                Markets = dto.Markets,
                Filter = dto.Filter,
            };

            var result = _PricingGuideService.SaveAllocations(allocationRequest);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideWithIncludedMarket()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26022,
                    ProposalDetailId = 9984,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                    }
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void ProposalOpenMarketService_SetsTotalsForPricingGrid()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26020,
                ProposalDetailId = 9982,
                BudgetGoal = 10000,
                OpenMarketPricing = new OpenMarketPricingGuideDto
                {
                    UnitCapPerStation = 100
                },
                OpenMarketShare = 1
            };

            var result = _PricingGuideService.GetOpenMarketInventory(request);
            var resultJson = IntegrationTestHelper.ConvertToJsonMoreRounding(result, _GetPricingGuideJsonSerializerSettings());

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideWithExcludedMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26023,
                    ProposalDetailId = 9985,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                    }
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingGuideExcludeAllMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26023);

                proposal.BlackoutMarketGroupId = ProposalEnums.ProposalMarketGroups.All;

                var result = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", new DateTime(2018, 10, 31));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26023,
                    ProposalDetailId = 9985,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                    }
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideMultipleIncludeMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26023);

                proposal.Markets = new List<ProposalMarketDto>
                {
                    new ProposalMarketDto()
                    {
                        Id = 200
                    },
                    new ProposalMarketDto()
                    {
                        Id = 343
                    },
                    new ProposalMarketDto()
                    {
                        Id = 358,
                        IsBlackout = true
                    }
                };

                var result = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", new DateTime(2018, 10, 31));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26023,
                    ProposalDetailId = 9985,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                    }
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideMultipleIncludeMarketsNoRemaningCoverage()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26023);

                proposal.Markets = new List<ProposalMarketDto>
                {
                    new ProposalMarketDto()
                    {
                        Id = 200
                    },
                    new ProposalMarketDto()
                    {
                        Id = 343
                    },
                    new ProposalMarketDto()
                    {
                        Id = 358,
                        IsBlackout = true
                    }
                };

                var result = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", new DateTime(2018, 10, 31));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26023,
                    ProposalDetailId = 9985,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                    }
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideFilterCpmShouldReachGoal()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26023);

                proposal.MarketCoverage = 0.005;

                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", new DateTime(2018, 10, 31));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26023,
                    ProposalDetailId = 9985,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Min,
                        CpmMax = 5,
                        CpmMin = 3
                    }
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        public void ProposalOpenMarketService_UpdatesOpenMarketPricingGuide_OnlyWithSelectedMarkets()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 17616,
                ProposalDetailId = 2290
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

            pricingGuideOpenMarketDto.AllMarkets.ForEach(x => x.Selected = false);
            var firstMarket = pricingGuideOpenMarketDto.AllMarkets.First();
            var lastMarket = pricingGuideOpenMarketDto.AllMarkets.Last();
            firstMarket.Selected = true;
            lastMarket.Selected = true;

            var result = _PricingGuideService.UpdateOpenMarketMarkets(pricingGuideOpenMarketDto);

            var firstMarketIsInResultList = result.Markets.Any(x => x.MarketId == firstMarket.Id);
            var lastMarketIsInResultList = result.Markets.Any(x => x.MarketId == lastMarket.Id);

            Assert.AreEqual(17, result.Markets.Count);
            Assert.IsTrue(firstMarketIsInResultList);
            Assert.IsTrue(lastMarketIsInResultList);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        [Category("2book")]
        public void ProposalOpenMarketService_UpdatesOpenMarketPricingGuideMarkets()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 17616,
                ProposalDetailId = 2290
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

            pricingGuideOpenMarketDto.AllMarkets.ForEach(x => x.Selected = false);
            pricingGuideOpenMarketDto.AllMarkets.First().Selected = true;
            pricingGuideOpenMarketDto.AllMarkets.Last().Selected = true;

            var result = _PricingGuideService.UpdateOpenMarketMarkets(pricingGuideOpenMarketDto);
            var resultJson = IntegrationTestHelper.ConvertToJson(result);

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        [Category("2book")]
        public void ProposalOpenMarketService_SavesAllocatedSpots_ForPricingMarkets()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 17616,
                ProposalDetailId = 2290
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

            pricingGuideOpenMarketDto.AllMarkets.ForEach(x => x.Selected = false);
            var firstMarket = pricingGuideOpenMarketDto.AllMarkets.First();
            firstMarket.Selected = true;
            var firstMarketProgram = pricingGuideOpenMarketDto.Markets
                .Where(x => x.MarketId == firstMarket.Id)
                .SelectMany(x => x.Stations)
                .SelectMany(x => x.Programs)
                .First();
            firstMarketProgram.Spots = firstMarketProgram.Spots + 5;

            var result = _PricingGuideService.UpdateOpenMarketMarkets(pricingGuideOpenMarketDto);
            var resultJson = IntegrationTestHelper.ConvertToJson(result);

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideAllocateCoverageWithGoals()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26023);

                proposal.MarketCoverage = 0.5;

                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", new DateTime(2018, 10, 31));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26023,
                    ProposalDetailId = 9985,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Max,
                        UnitCapPerStation = 20
                    },
                    BudgetGoal = 5000,
                    OpenMarketShare = 1
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideWithBudgetGoalAndOpenMarketShare()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26016);

                proposal.MarketCoverage = 0.5;

                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", new DateTime(2018, 10, 31));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26016,
                    ProposalDetailId = 9978,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Max
                    },
                    BudgetGoal = 5000,
                    OpenMarketShare = 0.5m
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideWithImpressionGoalAndOpenMarketShare()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26016);

                proposal.MarketCoverage = 0.5;

                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", new DateTime(2018, 10, 31));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26016,
                    ProposalDetailId = 9978,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Max
                    },
                    ImpressionGoal = 8503184,
                    OpenMarketShare = 0.5m
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void GetPricingGuideWithBothGoalAndOpenMarketShare()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26016);

                proposal.MarketCoverage = 0.5;

                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", new DateTime(2018, 10, 31));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26016,
                    ProposalDetailId = 9978,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Max
                    },
                    BudgetGoal = 2000,
                    ImpressionGoal = 8503184,
                    OpenMarketShare = 0.5m
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("Impressions")]
        public void UpdatesTotalsWithPassedProprietaryCpms()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26020,
                ProposalDetailId = 9982,
                BudgetGoal = 10000,
                OpenMarketShare = 0.5m,
                OpenMarketPricing = new OpenMarketPricingGuideDto
                {
                    UnitCapPerStation = 100
                }
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);
            pricingGuideOpenMarketDto.ProprietaryPricing = new List<ProprietaryPricingDto>
            {
                new ProprietaryPricingDto { Cpm = 5, ImpressionsBalance = 0.11 },
                new ProprietaryPricingDto { Cpm = 5, ImpressionsBalance = 0.13 },
                new ProprietaryPricingDto { Cpm = 5, ImpressionsBalance = 0.15 },
                new ProprietaryPricingDto { Cpm = 5, ImpressionsBalance = 0.17 }
            };

            var result = _PricingGuideService.UpdateProprietaryCpms(pricingGuideOpenMarketDto);
            var resultJson = IntegrationTestHelper.ConvertToJson(result);

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingGuideWithEquivalizedImpressions()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26024,
                ProposalDetailId = 9986,
                OpenMarketShare = 1,
                OpenMarketPricing = new OpenMarketPricingGuideDto
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                }
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

            var resultJson = IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto);

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingGuideWithIndexing()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26024,
                ProposalDetailId = 9986,
                OpenMarketShare = 1,
                Margin = 0.2,
                Inflation = 0.1,
                ImpressionLoss = 0.2,
                OpenMarketPricing = new OpenMarketPricingGuideDto
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                }
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

            var resultJson = IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto);

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingGuideWithLatestCoverage()
        {
            using (new TransactionScopeWrapper())
            {
                const string filename = @".\Files\Market_Coverages_Pricing_Guide.xlsx";

                _MarketService.LoadCoverages(filename, "IntegrationTestUser", new DateTime(2018, 12, 18));

                var request = new PricingGuideOpenMarketInventoryRequestDto
                {
                    ProposalId = 26024,
                    ProposalDetailId = 9986,
                    OpenMarketShare = 1,
                    OpenMarketPricing = new OpenMarketPricingGuideDto
                    {
                        OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                    }
                };

                var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

                var jsonResolver = new IgnorableSerializerContractResolver();

                jsonResolver.Ignore(typeof(PricingGuideDto), "MarketCoverageFileId");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var resultJson = IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto, jsonSettings);

                Approvals.Verify(resultJson);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CopyToBuyTest()
        {
            using (new TransactionScopeWrapper())
            {
                _PricingGuideService.CopyPricingGuideAllocationsToOpenMarket(9987);

                var proposalOpenMarketInventoryService =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();

                var inventory = proposalOpenMarketInventoryService.GetInventory(9987);

                var resultJson = IntegrationTestHelper.ConvertToJson(inventory);

                Approvals.Verify(resultJson);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HasSpotsAllocatedTest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PricingGuideService.HasSpotsAllocated(9987);

                Assert.True(result);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPricingGuideWithIndexingOnlyProjectedImpressionsAreAdjusted()
        {
            var request = new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalId = 26024,
                ProposalDetailId = 9986,
                OpenMarketShare = 1,
                ImpressionLoss = 0.5,
                OpenMarketPricing = new OpenMarketPricingGuideDto
                {
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min
                }
            };

            var pricingGuideOpenMarketDto = _PricingGuideService.GetOpenMarketInventory(request);

            var resultJson = IntegrationTestHelper.ConvertToJson(pricingGuideOpenMarketDto);

            Approvals.Verify(resultJson);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CopyToBuyNoProjectedImpressionsTest()
        {
            using (new TransactionScopeWrapper())
            {
                _PricingGuideService.CopyPricingGuideAllocationsToOpenMarket(9988);

                var proposalOpenMarketInventoryService =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();

                var inventory = proposalOpenMarketInventoryService.GetInventory(9988);

                var resultJson = IntegrationTestHelper.ConvertToJson(inventory);

                Approvals.Verify(resultJson);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePricingGuideWithExtensiveProgramName()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new ProposalDetailPricingGuideSaveRequest
                {
                    Inflation = 1,
                    Margin = 1,
                    ImpressionLoss = 1,
                    CpmMax = 10,
                    CpmMin = 10,
                    GoalBudget = 10000,
                    GoalImpression = 10000,
                    OpenMarketCpmTarget = OpenMarketCpmTarget.Min,
                    OpenMarketTotals = new OpenMarketTotalsDto
                    {
                        Cost = 1000,
                        Coverage = 80,
                        Cpm = 10,
                        Impressions = 10000
                    },
                    ProprietaryPricing = new List<ProprietaryPricingDto>() { new ProprietaryPricingDto { Cpm = 10, ImpressionsBalance = 10, InventorySource = InventorySourceEnum.CNN } },
                    ProprietaryTotals = new ProprietaryTotalsDto { Cost = 10, Cpm = 10, Impressions = 10000 },
                    UnitCapPerStation = 1,
                    ProposalDetailId = 13402,
                    Markets = new List<PricingGuideSaveMarketRequest>() {
                        new PricingGuideSaveMarketRequest
                        {
                            ProgramId = 26589,
                            BlendedCpm = 7.530701184311377M,
                            CostPerSpot = 1,
                            DaypartId = 1,
                            ImpressionsPerSpot = 10,
                            ManifestDaypartId = 213348,
                            MarketId = 101,
                            ProgramName = "CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News|CBS Morning News",
                            Spots = 1,
                            StationCode = 5060,
                            StationImpressionsPerSpot = 43156.671875
                        }
                    }
                };

                _PricingGuideService.SaveDistribution(request, "integration test");

                var proposalInventory = _PricingGuideService.GetPricingGuideForProposalDetail(13402);

                _VerifyPricingGuideModel(proposalInventory);
            }
        }

        private JsonSerializerSettings _GetPricingGuideJsonSerializerSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PricingGuideMarketDto), "MarketId");
            jsonResolver.Ignore(typeof(PricingGuideStationDto), "StationCode");
            jsonResolver.Ignore(typeof(PricingGuideProgramDto), "ProgramId");
            jsonResolver.Ignore(typeof(PricingGuideProgramDto), "ManifestDaypartId");
            jsonResolver.Ignore(typeof(PricingGuideProgramDto), "Genres");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        private void _ShuffleList<T>(IList<T> list, Random random)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void _VerifyPricingGuideModel(PricingGuideDto proposalInventory)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(PricingGuideDto), "ProposalId");
            jsonResolver.Ignore(typeof(PricingGuideDto), "ProposalVersionId");
            jsonResolver.Ignore(typeof(PricingGuideDto), "ProposalDetailId");
            jsonResolver.Ignore(typeof(PricingGuideDto), "DistributionId");
            jsonResolver.Ignore(typeof(PricingGuideMarketDto), "MarketId");
            jsonResolver.Ignore(typeof(PricingGuideProgramDto), "ProgramId");
            jsonResolver.Ignore(typeof(PricingGuideProgramDto), "ManifestDaypartId");
            jsonResolver.Ignore(typeof(LookupDto), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(proposalInventory, jsonSettings);
            Approvals.Verify(json);
        }
    }
}
