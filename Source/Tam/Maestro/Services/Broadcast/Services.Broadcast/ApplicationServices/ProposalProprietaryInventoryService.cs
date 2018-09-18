﻿using System;
using System.Collections.Generic;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using System.Transactions;
using OfficeOpenXml.FormulaParsing.Exceptions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.OpenMarketInventory;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Entities.DTO;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProposalProprietaryInventoryService : IApplicationService
    {
        ProposalDetailProprietaryInventoryDto GetInventory(int proposalDetailId);
        List<ProprietaryInventoryAllocationConflict> SaveInventoryAllocations(ProprietaryInventoryAllocationRequest actualRequest);
        ProposalInventoryTotalsDto GetInventoryTotals(ProposalInventoryTotalsRequestDto request);
        void RecalculateInventoryTotals(int proposalDetailId);

        ProposalDetailPricingGuideGridDto GetProposalDetailPricingGuideGridDto(ProposalDetailPricingGuidGridRequestDto request);

        ProposalDetailPricingGuideGridDto ApplyFilterOnOpenMarketPricingGuideGrid(ProposalDetailPricingGuideGridDto dto);
    }

    public class ProposalProprietaryInventoryService : BaseProposalInventoryService, IProposalProprietaryInventoryService
    {
        internal static readonly string MissingGuaranteedAudienceErorMessage = "Unable to get proprietary inventory information due to null guaranteed audience";
        private readonly IProposalWeeklyTotalCalculationEngine _ProposalWeeklyTotalCalculationEngine;

        public ProposalProprietaryInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory, IDaypartCache daypartCache, IProposalMarketsCalculationEngine proposalMarketsCalculationEngine, IProposalWeeklyTotalCalculationEngine proposalWeeklyTotalCalculationEngine, IImpressionAdjustmentEngine impressionAdjustmentEngine, IProposalTotalsCalculationEngine proposalTotalsCalculationEngine)
            : base(broadcastDataRepositoryFactory, daypartCache, proposalMarketsCalculationEngine, impressionAdjustmentEngine, proposalTotalsCalculationEngine)
        {
            _ProposalWeeklyTotalCalculationEngine = proposalWeeklyTotalCalculationEngine;
        }

        public ProposalDetailProprietaryInventoryDto GetInventory(int proposalDetailId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalInventory = GetProposalDetailDto(proposalDetailId);
                _SetProprietaryInventoryDaypartAndWeekInfo(proposalInventory, proposalDetailId);
                return proposalInventory;
            }
        }

        private ProposalDetailProprietaryInventoryDto GetProposalDetailDto(int proposalDetailId)
        {
            var dto = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>().GetProprietaryProposalDetailInventory(proposalDetailId);
            _SetProposalInventoryDetailSpotLength(dto);
            _SetProposalInventoryDetailDaypart(dto);
            return dto;
        }

        internal void _SetProprietaryInventoryDaypartAndWeekInfo(ProposalDetailProprietaryInventoryDto proposalDetailInventory, int detailId)
        {
            if (!proposalDetailInventory.GuaranteedAudience.HasValue)
                throw new ApplicationException(MissingGuaranteedAudienceErorMessage);

            if (!proposalDetailInventory.HutProjectionBookId.HasValue && !proposalDetailInventory.ShareProjectionBookId.HasValue && !proposalDetailInventory.SingleProjectionBookId.HasValue)
                throw new ApplicationException(MissingBooksErrorMessage);

            var daypartGroupings = new Dictionary<string, ProposalInventoryDaypartDto>();
            var stationDetailImpressions = new Dictionary<int, ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto>();
            var impressionRequests = new List<ManifestDetailDaypart>();

            var relevantMediaWeeks = proposalDetailInventory.Weeks.Select(w => w.MediaWeekId).ToList();
            var spotLengthRepository = BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            var spotLengthMappings = spotLengthRepository.GetSpotLengths();
            var proposalMarketIds = ProposalMarketsCalculationEngine.GetProposalMarketsList(proposalDetailInventory.ProposalId, proposalDetailInventory.ProposalVersion, detailId).Select(m => m.Id).ToList();
            var spotLengths = spotLengthMappings.Where(m => int.Parse(m.Display) >= proposalDetailInventory.DetailSpotLength && int.Parse(m.Display) <= 30).Select(m => m.Id);
            var sortedFilteredInventoryDetails = BroadcastDataRepositoryFactory.GetDataRepository<IProposalInventoryRepository>().GetSortedFilteredInventoryDetails(relevantMediaWeeks, proposalMarketIds, spotLengths);

            //Remove ones with non intersecting dayparts
            var proposalDetailDaypart = DaypartDto.ConvertDaypartDto(proposalDetailInventory.DetailDaypart);
            sortedFilteredInventoryDetails.ForEach(isg => isg.InventoryDetailSlots.RemoveAll(ids => !DisplayDaypart.Intersects(Common.Services.DaypartCache.Instance.GetDisplayDaypart(ids.RolleupDaypartId), proposalDetailDaypart)));

            foreach (var inventoryDetail in sortedFilteredInventoryDetails.Where(g => g.InventoryDetailSlots.Any()))
            {
                var inventoryDetailSlotsByWeek = inventoryDetail.InventoryDetailSlots.GroupBy(ids => ids.MediaWeekId).ToList();

                //Create 1 grouping object
                ProposalInventoryDaypartDto daypartGroup;
                var inventorySource = inventoryDetail.InventorySource.ToString();
                if (!daypartGroupings.TryGetValue(inventorySource, out daypartGroup))
                {
                    daypartGroup = new ProposalInventoryDaypartDto((int)inventoryDetail.InventorySource, inventorySource);
                    daypartGroupings[inventorySource] = daypartGroup;
                }

                //Add inventory detail header to daypart group
                var daypartDetails = new List<ProposalInventoryDaypartDetailDto>();
                for (var index = 1; index <= inventoryDetailSlotsByWeek.Max(wg => wg.Count()); index++)
                {
                    var detailDto = new ProposalInventoryDaypartDetailDto
                    {
                        InventoryDetailId = inventoryDetail.Id,
                        DetailLevel = index,
                        InventorySpot = inventoryDetail.DaypartCode
                    };
                    daypartDetails.Add(detailDto);
                }

                daypartGroup.Details.AddRange(daypartDetails);

                //Populate Weekly Info
                foreach (var mediaWeek in relevantMediaWeeks)
                {
                    var week = proposalDetailInventory.Weeks.Single(w => w.MediaWeekId == mediaWeek);

                    ProposalProprietaryInventoryWeekDto.InventoryDaypartGroupDto daypartGroupDto;
                    if (!week.DaypartGroupsDict.TryGetValue(inventoryDetail.InventorySource.ToString(), out daypartGroupDto))
                    {
                        daypartGroupDto = new ProposalProprietaryInventoryWeekDto.InventoryDaypartGroupDto();
                        week.DaypartGroupsDict[inventoryDetail.InventorySource.ToString()] = daypartGroupDto;
                    }

                    var matchingWeek = inventoryDetailSlotsByWeek.Where(wg => wg.Key == mediaWeek).ToList();
                    if (matchingWeek.Any())
                    {
                        var inventoryDetailSlots = matchingWeek.Single().OrderBy(ids => ids.SpotLengthId).ThenBy(ids => ids.DetailLevel).ToList();
                        for (var index = 1; index <= inventoryDetailSlots.Count; index++)
                        {
                            var detailSlot = inventoryDetailSlots[index - 1];
                            var slotSpotLength = spotLengthRepository.GetSpotLengthById(detailSlot.SpotLengthId);

                            if (week.MaxSlotSpotLength < slotSpotLength)
                                week.MaxSlotSpotLength = slotSpotLength;

                            var slotDto = new ProposalProprietaryInventoryWeekDto.InventoryDaypartSlotDto(slotSpotLength, detailSlot.Id, detailSlot.InventoryDetailId, index);

                            foreach (var slotProposal in detailSlot.InventoryDetailSlotProposals.OrderBy(sp => sp.Order))
                            {
                                var detailSpotLength = spotLengthRepository.GetSpotLengthById(slotProposal.ProposalDetailSpotLengthId);
                                var isCurrentProposal = detailId == slotProposal.ProposalDetailId;
                                slotDto.ProposalsAllocations.Add(new ProposalProprietaryInventoryWeekDto.ProposalAllocationDto(slotProposal.ProposalName, detailSpotLength, slotProposal.Order, isCurrentProposal, slotProposal.ProposalVersionDetailQuarterWeekId, slotProposal.UserName, slotProposal.WeekStartDate));
                            }

                            slotDto.Cost = inventoryDetailSlots[index - 1].SlotCost ?? 0;

                            slotDto.HasWastage = HasMarketWastageInSlot(detailSlot, relevantMediaWeeks, spotLengthMappings);

                            foreach (var component in detailSlot.InventoryDetailSlotComponents)
                            {
                                var displayDaypart = Common.Services.DaypartCache.Instance.GetDisplayDaypart(component.DaypartId);

                                if (!displayDaypart.Intersects(proposalDetailDaypart))
                                {
                                    slotDto.HasWastage = true;
                                    continue;
                                }

                                if (!DisplayDaypart.IsContained(displayDaypart, proposalDetailDaypart))
                                    slotDto.HasWastage = true;
                                throw new Exception("Should not have gotten this far");
                                var detail = new ManifestDetailDaypart
                                {
                                    Id = component.Id,
                                    LegacyCallLetters = "xyz",
                                    DisplayDaypart = DisplayDaypart.Intersect(displayDaypart, proposalDetailDaypart)
                                };

                                impressionRequests.Add(detail);
                                stationDetailImpressions[detail.Id] = slotDto;
                            }

                            daypartGroupDto.DaypartSlots.Add(slotDto);
                        }
                    }

                    //Fill in nulls for detail slots not in this week
                    foreach (var detail in daypartDetails)
                        if (!daypartGroupDto.DaypartSlots.Any(ds => ds != null && ds.InventoryDetailId == detail.InventoryDetailId && ds.DetailLevel == detail.DetailLevel))
                            daypartGroupDto.DaypartSlots.Add(null);

                    week.DaypartGroups = week.DaypartGroupsDict.OrderBy(r => r.Key).ToList();
                }
            }

            //Using share book for now, need to confirm with stephan
            var ratingAudiences = BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>().GetRatingsAudiencesByMaestroAudience(new List<int> { proposalDetailInventory.GuaranteedAudience.Value }).Select(r => r.rating_audience_id).Distinct().ToList();

            foreach (var imp in GetImpressions(proposalDetailInventory, ratingAudiences, impressionRequests))
                stationDetailImpressions[imp.id].Impressions += imp.impressions;

            foreach (var week in proposalDetailInventory.Weeks)
                foreach (var daypartGroup in week.DaypartGroups)
                    foreach (var daypartSlot in daypartGroup.Value.DaypartSlots.Where(dps => dps != null))
                        daypartSlot.CPM = daypartSlot.Impressions > 0 ? daypartSlot.Cost / (decimal)daypartSlot.Impressions : 0;

            proposalDetailInventory.Dayparts = daypartGroupings.OrderBy(r => r.Key).Select(r => r.Value).ToList();

            foreach (var dp in proposalDetailInventory.Dayparts)
            {
                foreach (var week in proposalDetailInventory.Weeks)
                {
                    var matchingGroup = week.DaypartGroups.Single(g => g.Key == dp.InventorySource);
                    if (matchingGroup.Value.DaypartSlots.Count != dp.Details.Count)
                        throw new Exception("Inventory misalignment");
                }
            }
        }

        private bool HasMarketWastageInSlot(InventoryDetailSlot inventoryDetailSlot, List<int> relevantMediaWeeks, List<LookupDto> spotLengthMappings)
        {
            var countOfComponentsForSlot =
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalInventoryRepository>()
                    .GetCountOfComponentsForSlot(inventoryDetailSlot.Id, relevantMediaWeeks, spotLengthMappings);

            return countOfComponentsForSlot != inventoryDetailSlot.InventoryDetailSlotComponents.Count;
        }

        public List<ProprietaryInventoryAllocationConflict> SaveInventoryAllocations(ProprietaryInventoryAllocationRequest actualRequest)
        {
            var spotLengthRepository = BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            var spotLengthMappings = spotLengthRepository.GetSpotLengths();

            using (var transaction = new TransactionScopeWrapper())
            {
                var conflicts =
                    BroadcastDataRepositoryFactory.GetDataRepository<IProposalInventoryRepository>()
                        .SaveInventoryAllocations(actualRequest, spotLengthMappings);

                if (actualRequest.ForceSave || !conflicts.Any())
                {
                    var inventoryDto = GetInventory(actualRequest.ProposalDetailId);
                    var totals = _ProposalWeeklyTotalCalculationEngine.CalculatePartialProprietaryTotals(inventoryDto);
                    BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                        .SaveProposalDetailProprietaryInventoryTotals(actualRequest.ProposalDetailId, totals);
                    BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                        .SaveProposalDetailProprietaryWeekInventoryTotals(actualRequest.ProposalDetailId, totals);
                    _UpdateProposalTotals(inventoryDto.ProposalVersionId);
                }

                conflicts.ForEach(c =>
                {
                    foreach (var allocation in c.SlotProposals)
                    {
                        var detailSpotLength =
                            spotLengthRepository.GetSpotLengthById(allocation.ProposalDetailSpotLengthId);
                        var isCurrentProposal = actualRequest.ProposalDetailId == allocation.ProposalDetailId;
                        c.ProposalAllocations.Add(
                            new ProposalProprietaryInventoryWeekDto.ProposalAllocationDto(allocation.ProposalName,
                                detailSpotLength, allocation.Order, isCurrentProposal,
                                allocation.ProposalVersionDetailQuarterWeekId, allocation.UserName,
                                allocation.WeekStartDate));
                    }
                });

                transaction.Complete();

                return conflicts;
            }
        }

        public ProposalInventoryTotalsDto GetInventoryTotals(ProposalInventoryTotalsRequestDto request)
        {
            var totals = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                .GetProposalDetailOpenMarketInventoryTotals(request.ProposalDetailId);
            var weeksTotals = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                .GetProposalDetailOpenMarketWeekInventoryTotals(request.ProposalDetailId);
            return _ProposalWeeklyTotalCalculationEngine.CalculateProprietaryDetailTotals(request, totals, weeksTotals);
        }

        public void RecalculateInventoryTotals(int proposalDetailId)
        {
            using (var transaction = new TransactionScopeWrapper())
            {
                var inventoryDto = GetInventory(proposalDetailId);
                var totals = _ProposalWeeklyTotalCalculationEngine.CalculatePartialProprietaryTotals(inventoryDto);
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                    .SaveProposalDetailProprietaryInventoryTotals(proposalDetailId, totals);
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                    .SaveProposalDetailProprietaryWeekInventoryTotals(proposalDetailId, totals);
                _UpdateProposalTotals(inventoryDto.ProposalVersionId);
                transaction.Complete();
            }
        }

        public ProposalDetailPricingGuideGridDto GetProposalDetailPricingGuideGridDto(ProposalDetailPricingGuidGridRequestDto request)
        {
            // return mock data for now
            var result = new ProposalDetailPricingGuideGridDto()
            {
                ProposalId = request.ProposalId,
                ProposalDetailId = request.ProposalDetailId,
                OpenMarkets = new List<ProposalDetailPricingGuideGridDto.OpenMarket>()
                {
                    new ProposalDetailPricingGuideGridDto.OpenMarket()
                    {
                        MarketName = "New York (Big Apple)",
                        MarketRank = 1,
                        MarketId = 123,
                        Stations = new List<ProposalDetailPricingGuideGridDto.OpenMarketStation>()
                        {
                            new ProposalDetailPricingGuideGridDto.OpenMarketStation()
                            {
                                CallLetters = "WNBC-TV",
                                LegacyCallLetters = "WNBC",
                                StationCode = 123,
                                Affiliation = "NBC4",
                                Programs = new List<ProposalDetailPricingGuideGridDto.OpenMarketProgram>()
                                {
                                    new ProposalDetailPricingGuideGridDto.OpenMarketProgram()
                                    {
                                        ProgramName = "Tonight Show",
                                        Spots = 2,
                                        Impressions = 1029333,
                                        OvernightImpressions = 131999,
                                        CPM = 10.34M,
                                        Cost = Convert.ToDecimal(2*10.34),
                                        Daypart = new LookupDto()
                                            {
                                                Display = "M-F 10PM - 11AM",
                                                Id = 123
                                            },
                                        ProgramId = 1233,
                                    },
                                    new ProposalDetailPricingGuideGridDto.OpenMarketProgram()
                                    {
                                    ProgramName = "Late Show",
                                    Spots = 1,
                                    Impressions = 1029333,
                                    OvernightImpressions = 131999,
                                    CPM = 8.34M,
                                    Cost = Convert.ToDecimal(1*8.34),

                                    Daypart = new LookupDto()
                                    {
                                        Display = "M-F 11AM - 12AM",
                                        Id = 124
                                    },
                                    ProgramId = 1233,
                                }
                                }
                            },
                            new ProposalDetailPricingGuideGridDto.OpenMarketStation()
                            {
                                CallLetters = "WKEW",
                                LegacyCallLetters = "WKEW-TV",
                                StationCode = 123,
                                Affiliation = "NBC3",
                                Programs = new List<ProposalDetailPricingGuideGridDto.OpenMarketProgram>()
                                {
                                    new ProposalDetailPricingGuideGridDto.OpenMarketProgram()
                                    {
                                        ProgramName = "Mid-day Drama",
                                        Spots = 3,
                                        Impressions = 29333,
                                        OvernightImpressions = 1999,
                                        CPM = 5.34M,
                                        Cost = Convert.ToDecimal(3*5.34),
                                        Daypart = new LookupDto()
                                            {
                                                Display = "M-F 1PM - 2:30AM",
                                                Id = 123
                                            },
                                        ProgramId = 145,
                                    }
                                }
                            }

                        }
                    },
                                        new ProposalDetailPricingGuideGridDto.OpenMarket()
                    {
                        MarketName = "Los Angelas",
                        MarketRank = 2,
                        MarketId = 124,
                        Stations = new List<ProposalDetailPricingGuideGridDto.OpenMarketStation>()
                        {
                            new ProposalDetailPricingGuideGridDto.OpenMarketStation()
                            {
                                CallLetters = "KTLA-TV",
                                LegacyCallLetters = "KTLA",
                                StationCode = 136,
                                Affiliation = "NBC2",
                                Programs = new List<ProposalDetailPricingGuideGridDto.OpenMarketProgram>()
                                {
                                    new ProposalDetailPricingGuideGridDto.OpenMarketProgram()
                                    {
                                        ProgramName = "Conan",
                                        Spots = 4,
                                        Impressions = 3027333,
                                        OvernightImpressions = 13199,
                                        CPM = 15.34M,
                                        Cost = Convert.ToDecimal(4*15.34M),
                                        Daypart = new LookupDto()
                                            {
                                                Display = "M-F 10PM - 11AM",
                                                Id = 1234
                                            },
                                        ProgramId = 1233,
                                    },
                                    new ProposalDetailPricingGuideGridDto.OpenMarketProgram()
                                    {
                                    ProgramName = "After conana",
                                    Spots = 1,
                                    Impressions = 1029333,
                                    OvernightImpressions = 131999,
                                    CPM = 8.34M,
                                    Cost = Convert.ToDecimal(1*8.34),

                                    Daypart = new LookupDto()
                                    {
                                        Display = "M-F 11AM - 12AM",
                                        Id = 1246
                                    },
                                    ProgramId = 1233,
                                }
                                }
                            },
                            new ProposalDetailPricingGuideGridDto.OpenMarketStation()
                            {
                                CallLetters = "KABC-TV",
                                LegacyCallLetters = "KABC",
                                StationCode = 123,
                                Affiliation = "NBC1",
                                Programs = new List<ProposalDetailPricingGuideGridDto.OpenMarketProgram>()
                                {
                                    new ProposalDetailPricingGuideGridDto.OpenMarketProgram()
                                    {
                                        ProgramName = "Mid-day Drama",
                                        Spots = 3,
                                        Impressions = 29333,
                                        OvernightImpressions = 1999,
                                        CPM = 5.34M,
                                        Cost = Convert.ToDecimal(3*5.34),
                                        Daypart = new LookupDto()
                                            {
                                                Display = "M-F 1PM - 2:30AM",
                                                Id = 123
                                            },
                                        ProgramId = 145,
                                    }
                                }
                            }

                        }
                    }
                }
            };

            result.OpenMarkets.ForEach(m => m.Cost = m.Stations.Sum(s => s.Programs.Sum(p => p.Cost)));
            result.OpenMarkets.ForEach(m => m.Spots = m.Stations.Sum(s => s.Programs.Sum(p => p.Spots)));
            result.OpenMarkets.ForEach(m => m.Impressions = m.Stations.Sum(s => s.Programs.Sum(p => p.Impressions)));
            result.OpenMarkets.ForEach(m => m.OvernightImpressions = m.Stations.Sum(s => s.Programs.Sum(p => p.OvernightImpressions)));
            result.OpenMarkets.ForEach(m => m.StationImpressions = m.Stations.Sum(s => s.Programs.Sum(p => p.StationImpressions)));

            // grand total
            result.Spots = result.OpenMarkets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).Sum(p => p.Spots);
            result.Cost = result.OpenMarkets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).Sum(p => p.Cost);
            result.Impressions = result.OpenMarkets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).Sum(p => p.Impressions);
            result.OvernightImpressions = result.OpenMarkets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).Sum(p => p.OvernightImpressions);
            result.StationImpressions = result.OpenMarkets.SelectMany(m => m.Stations).SelectMany(s => s.Programs).Sum(p => p.StationImpressions);

            _SetProposalOpenMarketPricingGuideGridDisplayFilters(result);

            return result;
        }

        public ProposalDetailPricingGuideGridDto ApplyFilterOnOpenMarketPricingGuideGrid(ProposalDetailPricingGuideGridDto dto)
        {
            _SetProposalOpenMarketPricingGuideGridDisplayFilters(dto);
            _ApplyFilterForProposalOpenMarketPricingGuideGrid(dto);

            return dto;
        }

        private static void _SetProposalOpenMarketPricingGuideGridDisplayFilters(ProposalDetailPricingGuideGridDto dto)
        {
            dto.DisplayFilter = new OpenMarketPricingGuideGridDisplayFilterDto();

            var stations = dto.OpenMarkets
                    .Where(m => m.Stations != null)
                    .SelectMany(s => s.Stations)
                    .ToList();

            dto.DisplayFilter.ProgramNames = stations
                .Where(p => p.Programs.Any())
                .SelectMany(s => s.Programs.Where(p => p != null).Select(p => p.ProgramName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList();

            dto.DisplayFilter.Markets = dto.OpenMarkets
                .Select(a => new { a.MarketId, a.MarketName })
                .Distinct()
                .Select(y => new LookupDto { Id = y.MarketId, Display = y.MarketName })
                .OrderBy(n => n.Display)
                .ToList();

            dto.DisplayFilter.Affiliations = stations.Select(s => s.Affiliation)
                .Distinct()
                .OrderBy(a => a)
                .ToList();
        }

        private static void _ApplyFilterForProposalOpenMarketPricingGuideGrid(ProposalDetailPricingGuideGridDto dto)
        {
            if (dto.Filter == null)
            {
                return;
            }

            var filter = dto.Filter;

            _ApplyMarketsFilter(dto, filter);

            foreach (var market in dto.OpenMarkets)
            {
                _ApplyAffiliationsFilter(market, filter);

                foreach (var station in market.Stations)
                {
                    _ApplyProgramNamesFilter(station, filter);
                }
            }
        }

        private static void _ApplyProgramNamesFilter(ProposalDetailPricingGuideGridDto.OpenMarketStation station, OpenMarketPricingGuideGridFilterDto filter)
        {
            var programNames = filter.ProgramNames;
            
            if (programNames != null && programNames.Any())
            {
                station.Programs = station.Programs.Where(p => programNames.Contains(p.ProgramName)).ToList();
            }
        }

        private static void _ApplyMarketsFilter(ProposalDetailPricingGuideGridDto dto, OpenMarketPricingGuideGridFilterDto filter)
        {
            if (filter.Markets != null && filter.Markets.Any())
            {
                dto.OpenMarkets = dto.OpenMarkets.Where(m => filter.Markets.Contains(m.MarketId)).ToList();
            }
        }

        private static void _ApplyAffiliationsFilter(ProposalDetailPricingGuideGridDto.OpenMarket market, OpenMarketPricingGuideGridFilterDto filter)
        {
            if (filter.Affiliations != null && filter.Affiliations.Any())
            {
                market.Stations = market.Stations.Where(s => filter.Affiliations.Contains(s.Affiliation)).ToList();
            }
        }
    }
}
