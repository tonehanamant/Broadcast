using System;
using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IProposalOpenMarketInventoryRepository : IDataRepository
    {
        List<OpenMarketInventoryAllocation> GetProposalDetailAllocations(int proposalVersionDetailId);
        void RemoveAllocations(List<OpenMarketInventoryAllocation> allocationToRemove);
        void UpdateAllocations(List<OpenMarketInventoryAllocation> allocationsToUpdate, string username, int spotLength);
        void AddAllocations(List<OpenMarketInventoryAllocation> allocationToAdd, string username, int spotLength);
        void RemoveAllocations(List<int> programIds, int proposalDetailId);
        List<OpenMarketInventoryAllocationSnapshotDto> GetOpenMarketInventoryAllocationSnapshot(List<int> programIds, int proposalDetailId);
    }

    public class ProposalOpenMarketInventoryRepository : BroadcastRepositoryBase, IProposalOpenMarketInventoryRepository
    {
        public ProposalOpenMarketInventoryRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<OpenMarketInventoryAllocation> GetProposalDetailAllocations(int proposalVersionDetailId)
        {
            //TODO: Fixme or remove.
            return new List<OpenMarketInventoryAllocation>();
            //return _InReadUncommitedTransaction(
            //    context =>
            //    {
            //        return (from a in context.station_program_flight_proposal
            //                join f in context.station_program_flights on a.station_program_flight_id equals f.id
            //            join w in context.proposal_version_detail_quarter_weeks on a.proprosal_version_detail_quarter_week_id equals
            //                w.id
            //                join q in context.proposal_version_detail_quarters on w.proposal_version_quarter_id equals q.id
            //                where q.proposal_version_detail_id == proposalVersionDetailId
            //                select new OpenMarketInventoryAllocation()
            //                {
            //                    ProposalVersionDetailId = proposalVersionDetailId,
            //                    StationProgramId = f.station_program_id,
            //                    StationProgramFlightId = a.station_program_flight_id,
            //                    MediaWeekId = f.media_week_id,
            //                    ProposalVersionDetailQuarterWeekId = a.proprosal_version_detail_quarter_week_id,
            //                    Spots = a.spots
            //                }).ToList();
            //    });
        }


        public void RemoveAllocations(List<OpenMarketInventoryAllocation> allocations)
        {
            //TODO: Fixme or remove.
            
            //_InReadUncommitedTransaction(
            //    context =>
            //    {
            //        foreach (var allocation in allocations)
            //        {
            //            var programAllocation =
            //                context.station_program_flight_proposal.Single(
            //                    f => f.proprosal_version_detail_quarter_week_id ==
            //                         allocation.ProposalVersionDetailQuarterWeekId &&
            //                         f.station_program_flight_id == allocation.StationProgramFlightId);

            //            context.station_program_flight_proposal.Remove(programAllocation);
            //        }

            //        context.SaveChanges();
            //    });
        }

        public void UpdateAllocations(List<OpenMarketInventoryAllocation> allocations, string username, int spotLength)
        {
            //TODO: Fixme or remove.
            //_InReadUncommitedTransaction(
            //    context =>
            //    {
            //        foreach (var allocation in allocations)
            //        {
            //            var programFlight =
            //                context.station_program_flights.Include(s => s.station_programs)
            //                .Single(f => f.media_week_id == allocation.MediaWeekId &&
            //                                                            f.station_program_id ==
            //                                                            allocation.StationProgramId);

            //            var currentAllocation =
            //                context.station_program_flight_proposal.Where(
            //                    a =>
            //                        a.proprosal_version_detail_quarter_week_id ==
            //                        allocation.ProposalVersionDetailQuarterWeekId &&
            //                        a.station_program_flight_id == allocation.StationProgramFlightId)
            //                    .Single("Unable to find inventory allocation to update");
                        
            //            currentAllocation.station_program_flight_id = programFlight.id;
            //            currentAllocation.proprosal_version_detail_quarter_week_id = allocation.ProposalVersionDetailQuarterWeekId;
            //            currentAllocation.spots = allocation.Spots;
            //            currentAllocation.created_by = username;
            //            currentAllocation.impressions = allocation.Impressions;
            //            currentAllocation.spot_cost = _GetSpotCost(programFlight, spotLength);
            //            currentAllocation.spot_length_id = programFlight.station_programs.spot_length_id;
            //            currentAllocation.station_code = programFlight.station_programs.station_code;
            //            currentAllocation.start_date = programFlight.station_programs.start_date;
            //            currentAllocation.end_date = programFlight.station_programs.end_date;
            //            currentAllocation.daypart_code = programFlight.station_programs.daypart_code;
            //            currentAllocation.rate_source = programFlight.station_programs.rate_source;
            //            currentAllocation.daypart_id = programFlight.station_programs.daypart_id;
                        
            //            context.SaveChanges();
            //        }
            //    });
        }

        public void AddAllocations(List<OpenMarketInventoryAllocation> allocations, string username, int spotLength)
        {
            //TODO: Fixme or remove.
            //_InReadUncommitedTransaction(
            //    context =>
            //    {
            //        foreach (var allocation in allocations)
            //        {
            //            var programFlight =
            //                context.station_program_flights.Include(s => s.station_programs)
            //                    .Single(f => f.media_week_id == allocation.MediaWeekId &&
            //                                 f.station_program_id ==
            //                                 allocation.StationProgramId);

            //            var proposalQuarterWeekId =
            //                context.proposal_version_detail_quarter_weeks.Where(
            //                    w =>
            //                        w.proposal_version_detail_quarters.proposal_version_detail_id ==
            //                        allocation.ProposalVersionDetailId && w.media_week_id == allocation.MediaWeekId)
            //                    .Select(w => w.id)
            //                    .Single();

            //            var programAllocation = new station_program_flight_proposal
            //            {
            //                station_program_flight_id = programFlight.id,
            //                proprosal_version_detail_quarter_week_id = proposalQuarterWeekId,
            //                spots = allocation.Spots,
            //                created_by = username,
            //                impressions = allocation.Impressions,
            //                spot_cost = _GetSpotCost(programFlight, spotLength),
            //                spot_length_id = programFlight.station_programs.spot_length_id,
            //                station_code = programFlight.station_programs.station_code,
            //                start_date = programFlight.station_programs.start_date,
            //                end_date = programFlight.station_programs.end_date,
            //                daypart_code = programFlight.station_programs.daypart_code,
            //                rate_source = programFlight.station_programs.rate_source,
            //                daypart_id = programFlight.station_programs.daypart_id
            //            };

            //            context.station_program_flight_proposal.Add(programAllocation);
            //        }
            //        context.SaveChanges();
            //    });
        }

        public void RemoveAllocations(List<int> programIds, int proposalDetailId)
        {
            //TODO: Fixme or remove.

            //_InReadUncommitedTransaction(c =>
            //{
            //    var proposalVersionDetailQuarterWeekIds = c.proposal_version_details.Where(pvd => pvd.id == proposalDetailId).SelectMany(pvd => pvd.proposal_version_detail_quarters.SelectMany(q => q.proposal_version_detail_quarter_weeks.Select(qw => qw.id)));

            //    var stationProgramFlightIds = c.station_programs.Where(sp => programIds.Contains(sp.id)).SelectMany(sp => sp.station_program_flights.Select(f => f.id));

            //    var matchingAllocations = c.station_program_flight_proposal.Where(spfp => proposalVersionDetailQuarterWeekIds.Contains(spfp.proprosal_version_detail_quarter_week_id)
            //                                                                           && stationProgramFlightIds.Contains(spfp.station_program_flight_id));

            //    c.station_program_flight_proposal.RemoveRange(matchingAllocations);

            //    c.SaveChanges();
            //});
        }

        public List<OpenMarketInventoryAllocationSnapshotDto> GetOpenMarketInventoryAllocationSnapshot(List<int> programIds, int proposalDetailId)
        {
            //TODO: Fixme or remove.
            return new List<OpenMarketInventoryAllocationSnapshotDto>();
            //return _InReadUncommitedTransaction(c =>
            //{
            //    var proposalVersionDetailQuarterWeekIds =
            //        c.proposal_version_details.Where(pvd => pvd.id == proposalDetailId)
            //            .SelectMany(
            //                pvd =>
            //                    pvd.proposal_version_detail_quarters.SelectMany(
            //                        q => q.proposal_version_detail_quarter_weeks.Select(qw => qw.id)));

            //    var stationProgramFlightIds =
            //        c.station_programs.Where(sp => programIds.Contains(sp.id))
            //            .SelectMany(sp => sp.station_program_flights.Select(f => f.id));

            //    return
            //        c.station_program_flight_proposal.Where(
            //            spfp =>
            //                proposalVersionDetailQuarterWeekIds.Contains(spfp.proprosal_version_detail_quarter_week_id)
            //                && stationProgramFlightIds.Contains(spfp.station_program_flight_id))
            //            .Select(s => new OpenMarketInventoryAllocationSnapshotDto
            //            {
            //                StationProgramFlightId = s.station_program_flight_id,
            //                ProposalVersionDetailQuarterWeekId = s.proprosal_version_detail_quarter_week_id,
            //                Spots = s.spots,
            //                CreatedBy = s.created_by,
            //                Isci = s.isci,
            //                Impressions = s.impressions,
            //                SpotCost = s.spot_cost,
            //                SpotLengthId = s.spot_length_id,
            //                StationCode = s.station_code,
            //                StartDate = s.start_date,
            //                EndDate = s.end_date,
            //                DaypartCode = s.daypart_code,
            //                RateSource = (RatesFile.RateSourceType) s.rate_source,
            //                DaypartId = s.daypart_id
            //            }).ToList();
            //});
        }

        //private decimal _GetSpotCost(station_program_flights stationProgramFlights, int spotLength)
        //{
        //    decimal? spotCost = 0;

        //    if (spotLength == 15)
        //    {
        //        spotCost = stationProgramFlights.C15s_rate;
        //    }
        //    else if (spotLength == 30)
        //    {
        //        spotCost = stationProgramFlights.C30s_rate;
        //    }
        //    else if (spotLength == 60)
        //    {
        //        spotCost = stationProgramFlights.C60s_rate;
        //    }
        //    else if (spotLength == 90)
        //    {
        //        spotCost = stationProgramFlights.C90s_rate;
        //    }
        //    else if (spotLength == 120)
        //    {
        //        spotCost = stationProgramFlights.C120s_rate;
        //    }

        //    return spotCost ?? 0;
        //}
    }
}
