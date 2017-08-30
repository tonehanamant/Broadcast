using System;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using Services.Broadcast.Entities.spotcableXML;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Services.Broadcast.Repositories
{
    public interface IProposalInventoryRepository : IDataRepository
    {
        List<InventoryDetail> GetSortedFilteredInventoryDetails(ICollection<int> relevantMediaWeeks, List<int> proposalMarketIds, IEnumerable<int> spotLengths);
        Dictionary<int, string> GetAffectedProposalsByUpdatedInventorySlotAndMediaWeekIds(List<int> inventoryDetailSloIds);
        List<int> GetProposalsInventoryDetailSlotIdsByQuarterWeeksIds(List<int> quarterWeeksIds);
        List<InventoryDetailSlot> GetProposalInventorySlotsByQuarterWeekIds(List<int> quarterWeeksIds);
        bool DeleteInventoryAllocationsForDetailQuarterWeek(List<int> detailQuarterWeekIds);
        void DeleteInventoryAllocations(List<int> slotIds, List<int> detailQuarterWeekIds);
        List<ProprietaryInventoryAllocationConflict> SaveInventoryAllocations(ProprietaryInventoryAllocationRequest request, List<LookupDto> spotLengthMappings);
        List<int> DeleteInventoryAllocationsByInventory(InventoryDetail inventory);
        void DeleteInventoryAllocations(int proposalId);
        List<InventoryDetailSlotProposalDto> GetProprietaryInventoryAllocations(int proposalId);
        List<StationProgramFlightDto> GetOpenMarketInventoryAllocations(int proposalId);
        int GetCountOfComponentsForSlot(int slotId, List<int> relevantMediaWeeks, List<LookupDto> spotLengthMappings);
        int GetNumberOfProprietaryInventoryAllocationsForProposal(int proposalId);
        ProprietaryInventoryAllocationSnapshotDto GetProprietaryInventoryAllocationSnapshot(int inventoryDetailSlotId);
    }

    public class ProposalInventoryRepository : BroadcastRepositoryBase, IProposalInventoryRepository
    {
        public ProposalInventoryRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<InventoryDetail> GetSortedFilteredInventoryDetails(ICollection<int> relevantMediaWeeks, List<int> proposalMarketIds, IEnumerable<int> spotLengths)
        {
            return _InReadUncommitedTransaction(c =>
            {
                var inventoryDetailSlotGroup = c.inventory_detail_slots.Include(ids => ids.inventory_details)
                                                      .Where(ids => relevantMediaWeeks.Contains(ids.media_week_id) && spotLengths.Contains(ids.spot_length_id) && ids.inventory_detail_slot_components.Any())
                                                      .Include(ids => ids.inventory_detail_slot_components.Select(idsc => idsc.station_program_flights.station_programs.station))
                                                      .Where(ids => ids.inventory_detail_slot_components.Any(idsc => proposalMarketIds.Contains(idsc.station_program_flights.station_programs.station.market_code)))
                                                      .Include(ids => ids.inventory_detail_slot_proposal.Select(idsp => idsp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.proposal_versions.proposal))
                                                      .GroupBy(ids => ids.inventory_details);

                var relevantInventorySlots = inventoryDetailSlotGroup.Select(idsGroup =>
                new InventoryDetail
                {
                    Id = idsGroup.Key.id,
                    InventorySource = (RatesFile.RateSourceType)idsGroup.Key.inventory_source,
                    DaypartCode = idsGroup.Key.daypart_code,
                    InventoryDetailSlots = idsGroup.Select(ids =>
                    new InventoryDetailSlot
                    {
                        Id = ids.id,
                        RolleupDaypartId = ids.rolled_up_daypart_id,
                        DetailLevel = ids.detail_level,
                        SpotLengthId = ids.spot_length_id,
                        MediaWeekId = ids.media_week_id,
                        SlotCost = ids.slot_cost,
                        InventoryDetailId = ids.inventory_detail_id,
                        InventoryDetailSlotProposals = ids.inventory_detail_slot_proposal.Select(idsp =>
                                                                                                 new InventoryDetailSlotProposal
                                                                                                 {
                                                                                                     Order = idsp.order,
                                                                                                     ProposalName = idsp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.proposal_versions.proposal.name,
                                                                                                     ProposalDetailSpotLengthId = idsp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.spot_length_id,
                                                                                                     ProposalDetailId = idsp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.id,
                                                                                                     ProposalVersionDetailQuarterWeekId = idsp.proprosal_version_detail_quarter_week_id,
                                                                                                     UserName = idsp.created_by,
                                                                                                     WeekStartDate = idsp.proposal_version_detail_quarter_weeks.start_date
                                                                                                 }).ToList(),
                        InventoryDetailSlotComponents = ids.inventory_detail_slot_components
                        .Where(idsc => proposalMarketIds.Contains(idsc.station_program_flights.station_programs.station.market_code))
                        .Select(idsc =>
                                                                                                    new InventoryDetailSlotComponents
                                                                                                    {
                                                                                                        StationCode = idsc.station_code,
                                                                                                        DaypartId = idsc.daypart_id,
                                                                                                        Id = idsc.id,
                                                                                                        SpotCost30 = idsc.station_program_flights.C30s_rate,
                                                                                                        SpotCost15 = idsc.station_program_flights.C15s_rate,
                                                                                                        MarketCode = idsc.station_program_flights.station_programs.station.market_code
                                                                                                    }).ToList()
                    }).ToList()
                });

                return relevantInventorySlots.OrderBy(g => g.DaypartCode).ToList();
            });
        }

        public Dictionary<int, string> GetAffectedProposalsByUpdatedInventorySlotAndMediaWeekIds(
            List<int> inventoryDetailSloIds)

        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from p in context.inventory_detail_slot_proposal
                            join l in inventoryDetailSloIds on p.inventory_detail_slot_id equals l
                            // which slot match
                            join q in context.proposal_version_detail_quarter_weeks on
                                p.proprosal_version_detail_quarter_week_id equals q.id
                            join m in context.proposal_version_detail_quarters on q.proposal_version_quarter_id equals m.id
                            join d in context.proposal_version_details on m.proposal_version_detail_id equals d.id
                            join v in context.proposal_versions on d.proposal_version_id equals v.id
                            join prop in context.proposals on v.proposal_id equals prop.id
                            where prop.primary_version_id == v.id
                            select new
                            {
                                Id = prop.id,
                                Name = prop.name
                            }).Distinct().ToDictionary(n => n.Id, m => m.Name);
                });
        }

        public List<int> GetProposalsInventoryDetailSlotIdsByQuarterWeeksIds(List<int> quarterWeeksIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from p in context.inventory_detail_slot_proposal
                            join l in quarterWeeksIds on p.proprosal_version_detail_quarter_week_id equals l
                            select p.inventory_detail_slot_id).ToList();
                });
        }

        public int GetNumberOfProprietaryInventoryAllocationsForProposal(int proposalId)
        {
            return _InReadUncommitedTransaction(
                context => (from allocation in context.inventory_detail_slot_proposal
                    join week in context.proposal_version_detail_quarter_weeks on allocation.proprosal_version_detail_quarter_week_id equals week.id
                    join quarter in context.proposal_version_detail_quarters on week.proposal_version_quarter_id equals quarter.id
                    join detail in context.proposal_version_details on quarter.proposal_version_detail_id equals detail.id
                    join version in context.proposal_versions on detail.proposal_version_id equals version.id
                    where version.proposal_id == proposalId
                    select allocation).Count());
        }

        public ProprietaryInventoryAllocationSnapshotDto GetProprietaryInventoryAllocationSnapshot(int inventoryDetailSlotId)
        {
            return _InReadUncommitedTransaction(context => (from p in context.inventory_detail_slot_proposal
                where p.inventory_detail_slot_id == inventoryDetailSlotId
                select new ProprietaryInventoryAllocationSnapshotDto
                {
                    CreatedBy = p.created_by,
                    Impressions = p.impressions,
                    InventoryDetailSlotId = p.inventory_detail_slot_id,
                    Isci = p.isci,
                    Order = p.order,
                    ProprosalVersionDetailQuarterWeekId = p.proprosal_version_detail_quarter_week_id,
                    RolledUpDaypartId = p.rolled_up_daypart_id,
                    SlotCost = p.slot_cost,
                    SpotLengthId = p.spot_length_id,
                    Components = (from c in context.inventory_detail_slot_component_proposal
                        where c.inventory_detail_slot_id == inventoryDetailSlotId
                        select new ProprietaryInventoryAllocationSnapshotComponentDto
                        {
                            DaypartId = c.daypart_id,
                            InventoryDetailSlotComponentId = c.inventory_detail_slot_component_id,
                            InventoryDetailSlotId = c.inventory_detail_slot_id,
                            ProprosalVersionDetailQuarterWeekId = c.proprosal_version_detail_quarter_week_id,
                            StationCode = c.station_code,
                            StationProgramFlightId = c.station_program_flight_id,
                            StationProgramId = c.station_program_id
                        }).ToList()
                }).Single());
        }

        public List<InventoryDetailSlot> GetProposalInventorySlotsByQuarterWeekIds(List<int> quarterWeeksIds)
        {
            return _InReadUncommitedTransaction(
                context => (from p in context.inventory_detail_slot_proposal
                            join quarterWeekId in quarterWeeksIds on p.proprosal_version_detail_quarter_week_id equals quarterWeekId
                            join q in context.inventory_detail_slots on p.inventory_detail_slot_id equals q.id
                            select new InventoryDetailSlot
                            {
                                Id = q.id,
                                RolleupDaypartId = q.rolled_up_daypart_id,
                                DetailLevel = q.detail_level,
                                SpotLengthId = q.spot_length_id,
                                MediaWeekId = q.media_week_id,
                                SlotCost = q.slot_cost,
                                InventoryDetailSlotComponents = (from c in context.inventory_detail_slot_components
                                                                 where c.inventory_detail_slot_id == q.id
                                                                 select new InventoryDetailSlotComponents
                                                                 {
                                                                     StationCode = c.station_code,
                                                                     DaypartId = c.daypart_id,
                                                                     Id = c.id,
                                                                 }).ToList()
                            }).ToList());
        }


        public bool DeleteInventoryAllocationsForDetailQuarterWeek(List<int> detailQuarterWeekIds)
        {
            _InReadUncommitedTransaction(c => _DeleteInventoryDetailSlot(c, detailQuarterWeekIds));
            return true;
        }

        public List<int> DeleteInventoryAllocationsByInventory(InventoryDetail inventory)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var originalInventoryDetailSlots =
                    context.inventory_detail_slots.Where(a => a.inventory_detail_id == inventory.Id);

                // deal with excluded slots
                var excludedSlotsIds = originalInventoryDetailSlots.Select(b => b.id)
                    .Except(inventory.InventoryDetailSlots.Where(a => a.Id > 0).Select(b => b.Id).ToList())
                    .ToList();

                // remove inventory slots
                var slotProposals = (from p in context.inventory_detail_slot_proposal
                                     where excludedSlotsIds.Contains(p.inventory_detail_slot_id)
                                     select p).ToList();

                foreach (var allocation in slotProposals)
                {
                    var slotAllocation = allocation;
                    context.inventory_detail_slot_component_proposal.RemoveRange(
                        context.inventory_detail_slot_component_proposal.Where(
                            p => p.inventory_detail_slot_id == slotAllocation.inventory_detail_slot_id &&
                                 p.proprosal_version_detail_quarter_week_id ==
                                 slotAllocation.proprosal_version_detail_quarter_week_id));
                }

                context.inventory_detail_slot_proposal.RemoveRange(slotProposals);

                context.SaveChanges();

                return slotProposals.Select(a => a.inventory_detail_slot_id).ToList();
            });
        }

        public List<InventoryDetailSlotProposalDto> GetProprietaryInventoryAllocations(int proposalId)
        {
            return _InReadUncommitedTransaction(c =>
            {
                var proprietary = from pv in c.proposals.Find(proposalId).proposal_versions
                                  from pvd in pv.proposal_version_details
                                  from pdq in pvd.proposal_version_detail_quarters
                                  from pdqw in pdq.proposal_version_detail_quarter_weeks
                                  from idsp in pdqw.inventory_detail_slot_proposal
                                  select idsp;

                return proprietary.Select(idsp => new InventoryDetailSlotProposalDto
                {
                    InventoryDetailSlotId = idsp.inventory_detail_slot_id,
                    ProposalVersionDetailQuarterWeekId = idsp.proprosal_version_detail_quarter_week_id,
                    Order = idsp.order,
                    CreatedBy = idsp.created_by
                }).ToList();
            });
        }

        public List<StationProgramFlightDto> GetOpenMarketInventoryAllocations(int proposalId)
        {
            return _InReadUncommitedTransaction(c =>
            {
                var openMarket = from pv in c.proposals.Find(proposalId).proposal_versions
                                 from pvd in pv.proposal_version_details
                                 from pdq in pvd.proposal_version_detail_quarters
                                 from pdqw in pdq.proposal_version_detail_quarter_weeks
                                 from spfp in pdqw.station_program_flight_proposal
                                 select spfp;

                return openMarket.Select(spfp => new StationProgramFlightDto
                {
                    StationProgramFlightId = spfp.station_program_flight_id,
                    ProposalVersionDetailQuarterWeekId = spfp.proprosal_version_detail_quarter_week_id,
                    Spots = spfp.spots,
                    CreatedBy = spfp.created_by
                }).ToList();
            });
        }

        public void DeleteInventoryAllocations(int proposalId)
        {
            _InReadUncommitedTransaction(c =>
            {
                var proprietary = from pv in c.proposals.Find(proposalId).proposal_versions
                                  from pvd in pv.proposal_version_details
                                  from pdq in pvd.proposal_version_detail_quarters
                                  from pdqw in pdq.proposal_version_detail_quarter_weeks
                                  from idsp in pdqw.inventory_detail_slot_proposal
                                  select idsp;

                var inventoryDetailSlotProposals = proprietary.ToList();

                foreach (var inventoryDetailSlotProposal in inventoryDetailSlotProposals)
                {
                    var proposal = inventoryDetailSlotProposal;
                    c.inventory_detail_slot_component_proposal.RemoveRange(
                        c.inventory_detail_slot_component_proposal.Where(
                            p =>
                                p.inventory_detail_slot_id == proposal.inventory_detail_slot_id &&
                                p.proprosal_version_detail_quarter_week_id ==
                                proposal.proprosal_version_detail_quarter_week_id));
                }

                c.inventory_detail_slot_proposal.RemoveRange(inventoryDetailSlotProposals);

                var openMarket = from pv in c.proposals.Find(proposalId).proposal_versions
                                 from pvd in pv.proposal_version_details
                                 from pdq in pvd.proposal_version_detail_quarters
                                 from pdqw in pdq.proposal_version_detail_quarter_weeks
                                 from spfp in pdqw.station_program_flight_proposal
                                 select spfp;

                c.station_program_flight_proposal.RemoveRange(openMarket);

                c.SaveChanges();
            });
        }

        private void _DeleteInventoryDetailSlot(QueryHintBroadcastContext context, List<int> quarterWeeksIds)
        {
            context.inventory_detail_slot_proposal.RemoveRange(
                context.inventory_detail_slot_proposal.Where(
                    p => quarterWeeksIds.Contains(p.proprosal_version_detail_quarter_week_id)));

            context.inventory_detail_slot_component_proposal.RemoveRange(
                context.inventory_detail_slot_component_proposal.Where(
                    p => quarterWeeksIds.Contains(p.proprosal_version_detail_quarter_week_id)));

            context.SaveChanges();
        }

        public void DeleteInventoryAllocations(List<int> slotIds, List<int> detailQuarterWeekIds)
        {
            _InReadUncommitedTransaction(context =>
            {
                var slotAllocations =
                    context.inventory_detail_slot_proposal.Where(
                        p =>
                            slotIds.Contains(p.inventory_detail_slot_id) &&
                            detailQuarterWeekIds.Contains(p.proprosal_version_detail_quarter_week_id));

                var snapshotComponents = context.inventory_detail_slot_component_proposal.Where(p =>
                    slotIds.Contains(p.inventory_detail_slot_id) &&
                    detailQuarterWeekIds.Contains(p.proprosal_version_detail_quarter_week_id));

                context.inventory_detail_slot_proposal.RemoveRange(slotAllocations);
                context.inventory_detail_slot_component_proposal.RemoveRange(snapshotComponents);

                context.SaveChanges();
            });
        }

        public List<ProprietaryInventoryAllocationConflict> SaveInventoryAllocations(ProprietaryInventoryAllocationRequest request, List<LookupDto> spotLengthMappings)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var allocationsBySlotId = GetRelevantAllocationsGroupedBySlotId(c, request);
                    var allConflicts = new List<ProprietaryInventoryAllocationConflict>();

                    foreach (var allocation in request.SlotAllocations)
                    {
                        var allocationsForSlot = allocationsBySlotId[allocation.InventoryDetailSlotId];
                        //Remove the delete from in-memory collection and database context
                        var deletes = new List<InventoryDetailSlotProposal>();

                        foreach (var delete in allocation.Deletes)
                        {
                            foreach (var slotProposal in allocationsForSlot.SlotProposals)
                            {
                                if (slotProposal.ProposalVersionDetailQuarterWeekId == delete.QuarterWeekId &&
                                    slotProposal.Order == delete.Order)
                                {
                                    deletes.Add(slotProposal);
                                    c.inventory_detail_slot_proposal.Remove(slotProposal.idsp);
                                    RemoveSnapshotOfInventorComponents(c, slotProposal.idsp.inventory_detail_slot_id, slotProposal.ProposalVersionDetailQuarterWeekId);
                                }
                            }
                        }

                        deletes.ForEach(o => allocationsForSlot.SlotProposals.Remove(o));

                        foreach (var save in allocation.Adds)
                        {
                            if (allocationsForSlot.SlotProposals.Any())
                            {
                                if (save.SpotLength == 30)
                                {
                                    //Can remove all existing allocations
                                    if (request.ForceSave)
                                    {
                                        c.inventory_detail_slot_proposal.RemoveRange(
                                            allocationsForSlot.SlotProposals.Select(a => a.idsp));
                                    }
                                    else
                                    {
                                        //Add messages for all allocations since they're all conflicts
                                        allocationsForSlot.Messages =
                                            allocationsForSlot.SlotProposals.Select(
                                                a =>
                                                    string.Format("Week {0} {1} reserved by User {2} for Proposal {3}",
                                                        a.WeekStartDate.ToShortDateString(),
                                                        allocationsForSlot.SlotDaypartCode, a.UserName, a.ProposalName))
                                                .ToList();
                                        allConflicts.Add(allocationsForSlot);
                                    }
                                }
                                else if (save.SpotLength == 15)
                                {
                                    var matchingAllocation =
                                        allocationsForSlot.SlotProposals.Where(a => a.idsp.order == save.Order).ToList();

                                    if (matchingAllocation.Any())
                                    {
                                        //Remove the one matching the order
                                        if (request.ForceSave)
                                        {
                                            c.inventory_detail_slot_proposal.RemoveRange(
                                                matchingAllocation.Select(a => a.idsp));
                                        }
                                        else
                                        {
                                            //Only add messages for the specific allocations that are conflicts
                                            allocationsForSlot.Messages =
                                                matchingAllocation.Select(
                                                    a =>
                                                        string.Format(
                                                            "Week {0} {1} reserved by User {2} for Proposal {3}",
                                                            a.WeekStartDate.ToShortDateString(),
                                                            allocationsForSlot.SlotDaypartCode, a.UserName,
                                                            a.ProposalName)).ToList();
                                            allConflicts.Add(allocationsForSlot);
                                        }
                                    }
                                }
                            }

                            var inventoryDetailSlot = GetInventoryDetailSlot(c, allocation.InventoryDetailSlotId);                            

                            c.inventory_detail_slot_proposal.Add(new inventory_detail_slot_proposal
                            {
                                inventory_detail_slot_id = allocation.InventoryDetailSlotId,
                                proprosal_version_detail_quarter_week_id = save.QuarterWeekId,
                                order = save.Order,
                                rolled_up_daypart_id = inventoryDetailSlot.rolled_up_daypart_id,
                                impressions = save.Impressions,
                                slot_cost = inventoryDetailSlot.slot_cost ?? 0,
                                spot_length_id = inventoryDetailSlot.spot_length_id,
                                created_by = request.UserName
                            });

                            SaveSnapshotOfInventoryComponents(c, allocation.InventoryDetailSlotId, save.QuarterWeekId);
                        }
                    }

                    if (allConflicts.Any())
                    {
                        return allConflicts;
                    }

                    c.SaveChanges();

                    return new List<ProprietaryInventoryAllocationConflict>();
                });
            }
        }

        private void RemoveSnapshotOfInventorComponents(BroadcastContext broadcastContext, int inventoryDetailSlotId, int proposalVersionDetailQuarterWeekId)
        {
            var proposalInventoryComponents =
                broadcastContext.inventory_detail_slot_component_proposal.Where(
                    x => x.inventory_detail_slot_id == inventoryDetailSlotId &&
                         x.proprosal_version_detail_quarter_week_id == proposalVersionDetailQuarterWeekId).ToList();

            foreach (var inventoryDetailSlotComponentProposal in proposalInventoryComponents)
            {
                broadcastContext.inventory_detail_slot_component_proposal.Remove(inventoryDetailSlotComponentProposal);
            }
        }

        private void SaveSnapshotOfInventoryComponents(BroadcastContext context, int inventoryDetailSlotId, int quarterWeekId)
        {
            var allSlotComponents =
                context.inventory_detail_slot_components.Where(i => i.inventory_detail_slot_id == inventoryDetailSlotId)
                    .ToList();

            foreach (var component in allSlotComponents)
            {
                context.inventory_detail_slot_component_proposal.Add(new inventory_detail_slot_component_proposal()
                {
                    daypart_id = component.daypart_id,
                    inventory_detail_slot_component_id = component.id,
                    proprosal_version_detail_quarter_week_id = quarterWeekId,
                    inventory_detail_slot_id = inventoryDetailSlotId,
                    station_code = component.station_code,
                    station_program_flight_id = component.station_program_flight_id,
                    station_program_id = component.station_program_flights.station_programs.id
                });
            }
        }

        private inventory_detail_slots GetInventoryDetailSlot(BroadcastContext context, int inventoryDetailSlotId)
        {
            return context.inventory_detail_slots.First(i => i.id == inventoryDetailSlotId);
        }

        private static Dictionary<int, ProprietaryInventoryAllocationConflict> GetRelevantAllocationsGroupedBySlotId(BroadcastContext c, ProprietaryInventoryAllocationRequest inventoryAllocationRequest)
        {
            var slotIds = inventoryAllocationRequest.SlotAllocations.Select(s => s.InventoryDetailSlotId).ToList();

            var relevantAllocations = c.inventory_detail_slots.Include(ids => ids.inventory_detail_slot_proposal.Select(idsp => idsp.proposal_version_detail_quarter_weeks
                                                                                                                                    .proposal_version_detail_quarters
                                                                                                                                    .proposal_version_details
                                                                                                                                    .proposal_versions
                                                                                                                                    .proposal))
                                                              .Where(ids => slotIds.Contains(ids.id))
                                                              .Select(s => new ProprietaryInventoryAllocationConflict
                                                              {
                                                                  MediaWeekId = s.media_week_id,
                                                                  SlotDaypartCode = s.inventory_details.daypart_code,
                                                                  InventoryDetailSlotId = s.id,
                                                                  SlotProposals = s.inventory_detail_slot_proposal.OrderBy(sp => sp.order).Select(idsp => new InventoryDetailSlotProposal
                                                                  {
                                                                      Order = idsp.order,
                                                                      ProposalName = idsp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.proposal_versions.proposal.name,
                                                                      ProposalDetailSpotLengthId = idsp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.spot_length_id,
                                                                      ProposalDetailId = idsp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.id,
                                                                      ProposalVersionDetailQuarterWeekId = idsp.proprosal_version_detail_quarter_week_id,
                                                                      UserName = idsp.created_by,
                                                                      WeekStartDate = idsp.proposal_version_detail_quarter_weeks.start_date,
                                                                      idsp = idsp
                                                                  }).ToList()
                                                              });

            return relevantAllocations.ToDictionary(d => d.InventoryDetailSlotId, d => d);
        }

        public int GetCountOfComponentsForSlot(int slotId, List<int> relevantMediaWeeks, List<LookupDto> spotLengthMappings)
        {
            var validSpotLengths = spotLengthMappings.Where(m => int.Parse(m.Display) <= 30).Select(m => m.Id);

            return
                _InReadUncommitedTransaction(
                    c => c.inventory_detail_slot_components
                        .Include(idsc => idsc.inventory_detail_slots)
                        .Count(
                            idsc =>
                                idsc.inventory_detail_slot_id == slotId &&
                                relevantMediaWeeks.Contains(idsc.inventory_detail_slots.media_week_id) &&
                                validSpotLengths.Contains(idsc.inventory_detail_slots.spot_length_id)));
        }
    }
}
