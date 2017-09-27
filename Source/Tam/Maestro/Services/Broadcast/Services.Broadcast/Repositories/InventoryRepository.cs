using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryRepository : IDataRepository
    {
        InventoryDetail GetInventoryDetailByDaypartCodeAndRateSource(string daypartCode, RatesFile.RateSourceType? rateSource);
        int GetMaximunNumberOfSpotsAvailableByDaypartCode(string daypartCode, RatesFile.RateSourceType? rateSource,
            int mediaWeekId, int spotLengthId);
        void SaveInventoryDetail(InventoryDetail inventoryDetail);
        //List<StationProgram> GetStationProgramsPerSpotAvailable(int? spots, int mediaWeekId, RatesFile.RateSourceType? rateSource, int spotLengthId, string daypartCode);
        void UpdateInventoryDetail(InventoryDetail inventory);
    }

    public class InventoryRepository : BroadcastRepositoryBase, IInventoryRepository
    {
        public InventoryRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory
            , ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public InventoryDetail GetInventoryDetailByDaypartCodeAndRateSource(string daypartCode,
            RatesFile.RateSourceType? rateSource)
        {
                return new InventoryDetail();
        }

        public int GetMaximunNumberOfSpotsAvailableByDaypartCode(string daypartCode,
            RatesFile.RateSourceType? rateSource, int mediaWeekId, int spotLengthId)
        {
            //TODO: Fixme or remove.
            throw new NotImplementedException();
            //return _InReadUncommitedTransaction(
            //    context =>
            //    {
            //        //Taking all the programs that match the daypart(s) included in the file, group by daypart, week, station and find maximum spots. 
            //        //That will be the maximum number of spots available.
            //        var programsMatch = (from p in context.station_programs
            //            join f in context.station_program_flights on p.id equals f.station_program_id
            //            where string.Compare(p.daypart_code, daypartCode, StringComparison.OrdinalIgnoreCase) == 0 &&
            //                  p.rate_source == (byte?) rateSource.Value &&
            //                  f.media_week_id == mediaWeekId &&
            //                  p.spot_length_id == spotLengthId
            //            select new {p.daypart_code, f.media_week_id, p.station_code, f.spots}).GroupBy(
            //                a => new {a.daypart_code, a.media_week_id, a.station_code}).ToList();
                    
            //        return programsMatch.Any()
            //            ? programsMatch.Select(z => z.ToList().Max(a => a.spots ?? 0)).Max()
            //            : 0;
            //    });
        }

        public void SaveInventoryDetail(InventoryDetail inventoryDetail)
        {
            _InReadUncommitedTransaction(
                context =>
                {

                });
        }

        //TODO: Fixme or remove.
        //public List<StationProgram> GetStationProgramsPerSpotAvailable(int? spots, int mediaWeekId,
        //    RatesFile.RateSourceType? rateSource, int spotLengthId, string daypartCode)
        //{
        //    return _InReadUncommitedTransaction(
        //        context =>
        //        {
        //            // For each slot and week combination (looking at the weeks specified during file upload) calculate the total number of stations 
        //            // that have same or more spots available as that slot level. Create slot/week records.
        //            return (from p in context.station_programs
        //                    join f in context.station_program_flights on p.id equals f.station_program_id
        //                    where f.media_week_id == mediaWeekId &&
        //                          p.daypart_code == daypartCode &&
        //                          p.rate_source == (byte?)rateSource &&
        //                          p.spot_length_id == spotLengthId &&
        //                          f.spots >= spots
        //                    select new StationProgram()
        //                    {
        //                        DaypartCode = p.daypart_code,
        //                        StationCode = p.station_code,
        //                        Daypart = new DisplayDaypart()
        //                        {
        //                            Id = p.daypart_id
        //                        },
        //                        FixedPrice = p.fixed_price,
        //                        FlightWeeks = p.station_program_flights.Select(flight => new StationProgramFlightWeek()
        //                        {
        //                            Id = flight.id,
        //                            Rate15s = flight.C15s_rate,
        //                            Rate30s = flight.C30s_rate,
        //                            Rate60s = flight.C60s_rate,
        //                            Rate90s = flight.C90s_rate,
        //                            Rate120s = flight.C120s_rate,
        //                            FlightWeek = new DisplayMediaWeek()
        //                            {
        //                                Id = flight.media_week_id
        //                            }
        //                        }).ToList()
        //                    }).ToList();
        //        });
        //}

        public void UpdateInventoryDetail(InventoryDetail inventory)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    
                });
        }

    }
}
