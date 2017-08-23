using System.Collections.Generic;
using System.Linq;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Entities.OpenMarketInventory;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IProposalOpenMarketInventoryRepository : IDataRepository
    {
        List<OpenMarketInventoryAllocation> GetProposalDetailAllocations(int proposalVersionDetailId);
        void RemoveAllocations(List<OpenMarketInventoryAllocation> allocationToRemove);
        void UpdateAllocations(List<OpenMarketInventoryAllocation> allocationsToUpdate, string username);
        void AddAllocations(List<OpenMarketInventoryAllocation> allocationToAdd, string username);
        void RemoveAllocations(List<int> programIds, int proposalDetailId);
    }

    public class ProposalOpenMarketInventoryRepository : BroadcastRepositoryBase, IProposalOpenMarketInventoryRepository
    {
        public ProposalOpenMarketInventoryRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<OpenMarketInventoryAllocation> GetProposalDetailAllocations(int proposalVersionDetailId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from a in context.station_program_flight_proposal
                            join f in context.station_program_flights on a.station_program_flight_id equals f.id
                        join w in context.proposal_version_detail_quarter_weeks on a.proprosal_version_detail_quarter_week_id equals
                            w.id
                            join q in context.proposal_version_detail_quarters on w.proposal_version_quarter_id equals q.id
                            where q.proposal_version_detail_id == proposalVersionDetailId
                            select new OpenMarketInventoryAllocation()
                            {
                                ProposalVersionDetailId = proposalVersionDetailId,
                                StationProgramId = f.station_program_id,
                                StationProgramFlightId = a.station_program_flight_id,
                                MediaWeekId = f.media_week_id,
                                ProposalVersionDetailQuarterWeekId = a.proprosal_version_detail_quarter_week_id,
                                Spots = a.spots
                            }).ToList();
                });
        }


        public void RemoveAllocations(List<OpenMarketInventoryAllocation> allocations)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var allocation in allocations)
                    {
                        var programAllocation =
                            context.station_program_flight_proposal.Where(
                                f =>
                                    f.proprosal_version_detail_quarter_week_id ==
                                    allocation.ProposalVersionDetailQuarterWeekId &&
                                    f.station_program_flight_id == allocation.StationProgramFlightId).Single();
                        context.station_program_flight_proposal.Remove(programAllocation);
                    }

                    context.SaveChanges();

                });

        }

        public void UpdateAllocations(List<OpenMarketInventoryAllocation> allocations, string username)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var allocation in allocations)
                    {
                        var currentAllocation =
                            context.station_program_flight_proposal.Where(
                                a =>
                                    a.proprosal_version_detail_quarter_week_id ==
                                    allocation.ProposalVersionDetailQuarterWeekId &&
                                    a.station_program_flight_id == allocation.StationProgramFlightId)
                                .Single("Unable to find inventory allocation to update");
                        currentAllocation.spots = allocation.Spots;
                        currentAllocation.created_by = username;
                        context.SaveChanges();
                    }
                });
        }

        public void AddAllocations(List<OpenMarketInventoryAllocation> allocations, string username)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var allocation in allocations)
                    {
                        var programFlightId =
                            context.station_program_flights.Where(
                                f =>
                                    f.media_week_id == allocation.MediaWeekId &&
                                    f.station_program_id == allocation.StationProgramId).Select(f => f.id).Single();
                        var proposalQuarterWeekId =
                            context.proposal_version_detail_quarter_weeks.Where(
                                w =>
                                    w.proposal_version_detail_quarters.proposal_version_detail_id ==
                                    allocation.ProposalVersionDetailId && w.media_week_id == allocation.MediaWeekId)
                                .Select(w => w.id)
                                .Single();
                        var programAllocation = new station_program_flight_proposal()
                        {
                            created_by = username,
                            spots = allocation.Spots,
                            station_program_flight_id = programFlightId,
                            proprosal_version_detail_quarter_week_id = proposalQuarterWeekId
                        };

                        context.station_program_flight_proposal.Add(programAllocation);
                    }
                    context.SaveChanges();
                });
        }

        public void RemoveAllocations(List<int> programIds, int proposalDetailId)
        {
            _InReadUncommitedTransaction(c =>
            {
                var proposalVersionDetailQuarterWeekIds = c.proposal_version_details.Where(pvd => pvd.id == proposalDetailId).SelectMany(pvd => pvd.proposal_version_detail_quarters.SelectMany(q => q.proposal_version_detail_quarter_weeks.Select(qw => qw.id)));

                var stationProgramFlightIds = c.station_programs.Where(sp => programIds.Contains(sp.id)).SelectMany(sp => sp.station_program_flights.Select(f => f.id));

                var matchingAllocations = c.station_program_flight_proposal.Where(spfp => proposalVersionDetailQuarterWeekIds.Contains(spfp.proprosal_version_detail_quarter_week_id)
                                                                                       && stationProgramFlightIds.Contains(spfp.station_program_flight_id));

                c.station_program_flight_proposal.RemoveRange(matchingAllocations);
                c.SaveChanges();
            });
        }
    }
}
