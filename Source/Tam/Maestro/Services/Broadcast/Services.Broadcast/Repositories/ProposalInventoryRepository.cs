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
        List<StationProgramFlightDto> GetOpenMarketInventoryAllocations(int proposalId);
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
