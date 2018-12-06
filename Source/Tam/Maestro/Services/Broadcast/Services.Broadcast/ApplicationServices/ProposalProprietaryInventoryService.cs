using System;
using System.Collections.Generic;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using System.Transactions;
using Services.Broadcast.BusinessEngines;
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
    }

    public class ProposalProprietaryInventoryService : BaseProposalInventoryService, IProposalProprietaryInventoryService
    {
        internal static readonly string MissingGuaranteedAudienceErorMessage = "Unable to get proprietary inventory information due to null guaranteed audience";
        private readonly IProposalWeeklyTotalCalculationEngine _ProposalWeeklyTotalCalculationEngine;

        public ProposalProprietaryInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IDaypartCache daypartCache
            , IProposalMarketsCalculationEngine proposalMarketsCalculationEngine
            , IProposalWeeklyTotalCalculationEngine proposalWeeklyTotalCalculationEngine
            , IProposalTotalsCalculationEngine proposalTotalsCalculationEngine
            , MediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IImpressionAdjustmentEngine impressionAdjustmentEngine)
            : base(broadcastDataRepositoryFactory, daypartCache, proposalMarketsCalculationEngine
                  , proposalTotalsCalculationEngine, mediaMonthAndWeekAggregateCache, impressionAdjustmentEngine)
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
            var dto = BroadcastDataRepositoryFactory.GetDataRepository<IPricingGuideRepository>().GetProprietaryProposalDetailInventory(proposalDetailId);
            _SetProposalInventoryDetailSpotLength(dto);
            _SetProposalInventoryDetailDaypart(dto);
            return dto;
        }

        private void _SetProposalInventoryDetailSpotLength(ProposalDetailInventoryBase proposalInventory)
        {
            if (proposalInventory == null) return;
            proposalInventory.DetailSpotLength =
                BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthById(proposalInventory.DetailSpotLengthId);
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
            var proposalMarketIds = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposalDetailInventory.ProposalId, proposalDetailInventory.ProposalVersion).Select(m => m.Id).ToList();
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
                                //var detail = new ManifestDetailDaypart
                                //{
                                //    Id = component.Id,
                                //    LegacyCallLetters = "xyz",
                                //    DisplayDaypart = DisplayDaypart.Intersect(displayDaypart, proposalDetailDaypart)
                                //};

                                //impressionRequests.Add(detail);
                                //stationDetailImpressions[detail.Id] = slotDto;
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
    }
}
