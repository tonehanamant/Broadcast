using System.Data.Entity;
using System.IO.Compression;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Repositories
{

    public interface IStationProgramRepository : IDataRepository
    {
        List<ProposalProgramDto> GetStationProgramsForProposalDetail(DateTime flightStart, DateTime flightEnd,
            int spotLength, int rateSource, List<short> proposalMarketIds, int proposalDetailId);
    }

    public class StationProgramRepository : BroadcastRepositoryBase, IStationProgramRepository
    {
        public StationProgramRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<ProposalProgramDto> GetStationProgramsForProposalDetail(DateTime flightStart, DateTime flightEnd,
            int spotLength, int rateSource, List<short> proposalMarketIds, int proposalDetailId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        var programs = context.station_inventory_manifest
                            .Include(a => a.station_inventory_manifest_dayparts)
                            .Include(b => b.station_inventory_manifest_audiences)
                            .Include(m => m.station_inventory_manifest_rates)
                            .Include(s => s.station)
                            .Include(i => i.inventory_sources)
                            .Where(p => p.inventory_source_id == rateSource)
                            .Where(a => (a.effective_date >= flightStart.Date && a.effective_date <= flightEnd.Date)
                                        || (a.end_date >= flightStart.Date && a.end_date <= flightEnd.Date)
                                        || (a.effective_date < flightStart.Date && a.end_date > flightEnd.Date))
                            .ToList();

                        if (proposalMarketIds != null & proposalMarketIds.Count > 0)
                            programs = programs.Where(b => proposalMarketIds.Contains(b.station.market_code)).ToList();

                        return (from manifest in programs
                            from manifestDaypart in manifest.station_inventory_manifest_dayparts
                            select new ProposalProgramDto()
                            {
                                ManifestId = manifest.id,
                                DayPartId = manifestDaypart.daypart_id,
                                ProgramName = manifestDaypart.program_name,
                                StartDate = manifest.effective_date,
                                EndDate = manifest.end_date,
                                TotalSpots = manifest.spots_per_week ?? 0,
                                Station = new DisplayScheduleStation
                                {
                                    StationCode = manifest.station_code,
                                    LegacyCallLetters = manifest.station.legacy_call_letters,
                                    Affiliation = manifest.station.affiliation,
                                    CallLetters = manifest.station.station_call_letters
                                },
                                Market = new LookupDto
                                {
                                    Id = manifest.station.market_code,
                                    Display = manifest.station.market.geography_name
                                },
                                // todo : still undefined
                                //Genres = 
                                FlightWeeks = new List<ProposalProgramFlightWeek>()
                            }).ToList();

                        /*
                        // build up the list of stationprograms based on the filters above
                        var query = programs.Select(
                            sp => new ProposalProgramDto
                            {
                                ProgramId = sp.id,
                                DayPartId = sp.station_inventory_manifest_dayparts.daypart_id,
                                ProgramName = sp.program_name,
                                StartDate = sp.start_date,
                                EndDate = sp.end_date,

                                Station = new DisplayScheduleStation
                                {
                                    StationCode = sp.station_code,
                                    LegacyCallLetters = sp.station.legacy_call_letters,
                                    Affiliation = sp.station.affiliation,
                                    CallLetters = sp.station.station_call_letters
                                },
                                Market = new LookupDto
                                {
                                    Id = sp.station.market_code,
                                    Display = sp.station.market.geography_name
                                },
                                Genres = sp.genres.Select(
                                    genre => new LookupDto
                                    {
                                        Id = genre.id,
                                        Display = genre.name
                                    }).ToList(),
                                FlightWeeks = sp.station_program_flights.Select(fw => new ProposalProgramFlightWeek()
                                {
                                    MediaWeekId = fw.media_week_id,
                                    IsHiatus = !fw.active,
                                    Rate = (spotLength == 15
                                        ? fw.C15s_rate ?? 0
                                        : spotLength == 30
                                            ? fw.C30s_rate ?? 0
                                            : spotLength == 60
                                                ? fw.C60s_rate ?? 0
                                                : spotLength == 90
                                                    ? fw.C90s_rate ?? 0
                                                    : spotLength == 120
                                                        ? fw.C120s_rate ?? 0
                                                        : 0),
                                    Allocations =
                                        context.station_program_flight_proposal.Where(
                                            fp => fp.station_program_flight_id == fw.id &&
                                                  fp.proposal_version_detail_quarter_weeks
                                                      .proposal_version_detail_quarters.proposal_version_detail_id ==
                                                  proposalDetailId)
                                            .Select(
                                                fp =>
                                                    new OpenMarketAllocationDto
                                                    {
                                                        MediaWeekId =
                                                            fp.proposal_version_detail_quarter_weeks.media_week_id,
                                                        Spots = fp.spots
                                                    }).ToList()
                                }).ToList()
                            });

                        return query.ToList();
                         * */
                    });
            }
        }

    }
}
