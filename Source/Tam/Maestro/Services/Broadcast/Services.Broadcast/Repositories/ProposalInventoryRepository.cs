using System;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
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
        int GetCountOfComponentsForSlot(int slotId, List<int> relevantMediaWeeks, List<LookupDto> spotLengthMappings);
        int GetNumberOfProprietaryInventoryAllocationsForProposal(int proposalId);
        ProprietaryInventoryAllocationSnapshotDto GetProprietaryInventoryAllocationSnapshot(int inventoryDetailSlotId);
        int GetCountOfAllocationsForProposal(int proposalId);
    }

    public class ProposalInventoryRepository : BroadcastRepositoryBase, IProposalInventoryRepository
    {
        public ProposalInventoryRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<InventoryDetail> GetSortedFilteredInventoryDetails(ICollection<int> relevantMediaWeeks, List<int> proposalMarketIds, IEnumerable<int> spotLengths)
        {
            //TODO: Fixme or remove.
            return new List<InventoryDetail>();
            
        }

        public Dictionary<int, string> GetAffectedProposalsByUpdatedInventorySlotAndMediaWeekIds(
            List<int> inventoryDetailSloIds)

        {
            return new Dictionary<int, string>();
        }

        public List<int> GetProposalsInventoryDetailSlotIdsByQuarterWeeksIds(List<int> quarterWeeksIds)
        {
            //TODO: Fixme or remove.
            return new List<int>();
        }

        public int GetNumberOfProprietaryInventoryAllocationsForProposal(int proposalId)
        {
            //TODO: Fixme or remove.
            return 0;
        }

        public ProprietaryInventoryAllocationSnapshotDto GetProprietaryInventoryAllocationSnapshot(int inventoryDetailSlotId)
        {
            return new ProprietaryInventoryAllocationSnapshotDto();
        }

        public List<InventoryDetailSlot> GetProposalInventorySlotsByQuarterWeekIds(List<int> quarterWeeksIds)
        {
            return new List<InventoryDetailSlot>();
        }


        public bool DeleteInventoryAllocationsForDetailQuarterWeek(List<int> detailQuarterWeekIds)
        {
            return true;
        }

        public List<int> DeleteInventoryAllocationsByInventory(InventoryDetail inventory)
        {
            //TODO: Fixme or remove
            return new List<int>();
        }

        public List<InventoryDetailSlotProposalDto> GetProprietaryInventoryAllocations(int proposalId)
        {
            //TODO: Fixme or remove
            return new List<InventoryDetailSlotProposalDto>();
        }


        public void DeleteInventoryAllocations(int proposalId)
        {
            _InReadUncommitedTransaction(context =>
            {
                /*
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
                                proposal.proprosal_version_detail_quarter_week_id &&
                                p.order == proposal.order));
                }

                c.inventory_detail_slot_proposal.RemoveRange(inventoryDetailSlotProposals);
                */

                var openMarket = from pv in context.proposals.Find(proposalId).proposal_versions.Where(x => x.snapshot_date == null)
                                 from pvd in pv.proposal_version_details
                                 from pdq in pvd.proposal_version_detail_quarters
                                 from pdqw in pdq.proposal_version_detail_quarter_weeks
                                 from sis in pdqw.station_inventory_spots
                                 select sis;

                context.station_inventory_spots.RemoveRange(openMarket);

                context.SaveChanges();
            });
        }

        public void DeleteInventoryAllocations(List<int> slotIds, List<int> detailQuarterWeekIds)
        {
            //TODO: Fixme or remove.
            _InReadUncommitedTransaction(context =>
            {
                
            });
        }

        public List<ProprietaryInventoryAllocationConflict> SaveInventoryAllocations(ProprietaryInventoryAllocationRequest request, List<LookupDto> spotLengthMappings)
        {
            //TODO: Fixme or remove.
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    return new List<ProprietaryInventoryAllocationConflict>();
                });
            }
        }

        public int GetCountOfComponentsForSlot(int slotId, List<int> relevantMediaWeeks, List<LookupDto> spotLengthMappings)
        {
            return 1; //TODO: This is just a dummy value. Fix or remove.
        }

        public int GetCountOfAllocationsForProposal(int proposalId)
        {
            return 1; //TODO: This is just a dummy value. Fix or remove.

        }
    }
}
