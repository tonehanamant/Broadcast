using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using System.Linq.Expressions;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Repositories
{
    public interface IStationRepository : IDataRepository
    {
        List<DisplayBroadcastStation> GetBroadcastStationsWithFlightWeeksForRateSource(InventorySource inventorySource);
        DisplayBroadcastStation GetBroadcastStationByCode(int code);
        DisplayBroadcastStation GetBroadcastStationByLegacyCallLetters(string callLetters);
        DisplayBroadcastStation GetBroadcastStationByCallLetters(string stationCallLetters);
        List<DisplayBroadcastStation> GetBroadcastStationListByLegacyCallLetters(List<string> stationNameList);
        List<DisplayBroadcastStation> GetBroadcastStations();
        List<DisplayBroadcastStation> GetBroadcastStationsByDate(int inventorySourceId, DateTime date, bool isIncluded);
        int GetBroadcastStationCodeByContactId(int stationContactId);
        void UpdateStation(int code, string user, DateTime timeStamp, int inventorySourceId);
        void UpdateStationList(List<int> stationCodes, string user, DateTime timeStamp, int inventorySourceId);
        short? GetStationCode(string stationName);
    }

    public class StationRepository : BroadcastRepositoryBase, IStationRepository
    {
        public StationRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsWithFlightWeeksForRateSource(InventorySource inventorySource)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = (from s in context.stations
                            .Include(sa => sa.station_inventory_manifest)
                            .Include(sl => sl.station_inventory_loaded)
                                 select s);
                    return query.Select(_MapToDisplayBroadcastStation(inventorySource.Id)).ToList();
                });
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsByDate(int inventorySourceId, DateTime date, bool isIncluded)
        {

            return _InReadUncommitedTransaction(
                context =>
                {
                    var query =
                        context.stations
                        .Include(sa => sa.station_inventory_manifest)
                        .Include(sl => sl.station_inventory_loaded)
                        .Where(
                            s =>
                                s.station_inventory_manifest.Any(
                                    m => (m.end_date == null || m.end_date > date)
                                         && m.inventory_source_id == inventorySourceId &&
                                         (m.file_id == null ||
                                          m.inventory_files.status == (byte)FileStatusEnum.Loaded)
                                          ) == isIncluded);

                    return query.Select(_MapToDisplayBroadcastStation(inventorySourceId)).ToList();
                });
        }

        private static Expression<Func<station, DisplayBroadcastStation>> _MapToDisplayBroadcastStation(int inventorySourceId)
        {
            return s => new DisplayBroadcastStation
            {
                Code = s.station_code,
                Affiliation = s.affiliation,
                CallLetters = s.station_call_letters,
                LegacyCallLetters = s.legacy_call_letters,
                OriginMarket = s.market.geography_name,
                ModifiedDate = (from m in s.station_inventory_loaded
                                where m.inventory_source_id == inventorySourceId
                                select m.last_loaded).OrderByDescending(x => x).FirstOrDefault(),
                MarketCode = s.market_code,
                ManifestMaxEndDate = (from m in s.station_inventory_manifest
                                      select m.end_date).Max()
            };
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsByFlightWeek(InventorySource inventorySource, int mediaWeekId, bool isIncluded)
        {
            //TODO: Fixme or remove.
            return new List<DisplayBroadcastStation>();
            //return _InReadUncommitedTransaction(
            //    context =>
            //    {
            //        var query =
            //            context.stations.Where(
            //                s =>
            //                    s.station_programs.Any(
            //                        p => (p.station_program_flights.Any(f => f.media_week_id == mediaWeekId))
            //                             && p.rate_source == (byte) rateSource &&
            //                             (p.rate_file_id == null ||
            //                              p.rate_files.status == (byte)RatesFile.FileStatusEnum.Loaded)
            //                              ) == isIncluded);

            //        var result = query.Select(
            //            s => new DisplayBroadcastStation
            //            {
            //                Code = s.station_code,
            //                Affiliation = s.affiliation,
            //                CallLetters = s.station_call_letters,
            //                LegacyCallLetters = s.legacy_call_letters,
            //                OriginMarket = s.market.geography_name,
            //                ModifiedDate = s.modified_date,
            //                FlightWeeks =
            //                    (from pf in context.station_program_flights
            //                     join sp in context.station_programs on pf.station_program_id equals sp.id
            //                     where sp.station_code == s.station_code && sp.rate_source == (byte) rateSource
            //                     select new FlightWeekDto
            //                     {
            //                         Id = pf.media_week_id,
            //                         IsHiatus = !pf.active
            //                     }).Distinct().OrderBy(fw => fw.Id)
            //            }).ToList();
            //        return result;
            //    });
        }

        public DisplayBroadcastStation GetBroadcastStationByCode(int code)
        {
            return _InReadUncommitedTransaction(
                context => (from s in context.stations
                            where s.station_code == code
                            select new DisplayBroadcastStation
                            {
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market.geography_name,
                                MarketCode = s.market_code,
                                ModifiedDate = s.modified_date
                            }).Single("No station found with code: " + code));
        }

        public DisplayBroadcastStation GetBroadcastStationByLegacyCallLetters(string callLetters)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from s in context.stations
                            where s.legacy_call_letters == callLetters
                            select new DisplayBroadcastStation
                            {
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market.geography_name,
                                ModifiedDate = s.modified_date,
                                MarketCode = s.market_code
                            }).FirstOrDefault();
                });
        }

        public DisplayBroadcastStation GetBroadcastStationByCallLetters(string stationCallLetters)
        {
            return _InReadUncommitedTransaction(
                context => (from s in context.stations
                            where s.station_call_letters == stationCallLetters
                            select new DisplayBroadcastStation
                            {
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market.geography_name,
                                MarketCode = s.market_code,
                                ModifiedDate = s.modified_date
                            }).FirstOrDefault());
        }

        public List<DisplayBroadcastStation> GetBroadcastStationListByLegacyCallLetters(List<string> stationNameList)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from s in context.stations
                            where stationNameList.Contains(s.legacy_call_letters)
                            select new DisplayBroadcastStation
                            {
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market.geography_name,
                                MarketCode = s.market_code,
                                ModifiedDate = s.modified_date
                            }).ToList();
                });
        }

        public List<DisplayBroadcastStation> GetBroadcastStations()
        {
            return _InReadUncommitedTransaction(
                context => (from s in context.stations
                            select new DisplayBroadcastStation
                            {
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market.geography_name,
                                MarketCode = s.market_code,
                                ModifiedDate = s.modified_date
                            }).ToList());
        }
        
        public void UpdateStation(int code, string user, DateTime timeStamp, int inventorySourceId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var station = context.stations.First(s => s.station_code == code);
                    station.modified_by = user;
                    station.modified_date = timeStamp;
                    station.station_inventory_loaded.Add(new station_inventory_loaded
                    {
                        inventory_source_id = (byte)inventorySourceId,
                        last_loaded = timeStamp
                    });
                    context.SaveChanges();
                });
        }

        public void UpdateStationList(List<int> stationCodes, string user, DateTime timeStamp, int inventorySourceId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var stations = context.stations.Where(x => stationCodes.Contains(x.station_code)).ToList();                    
                    stations.ForEach(x =>
                    {
                        x.modified_by = user;
                        x.modified_date = timeStamp;
                        x.station_inventory_loaded.Add(new station_inventory_loaded
                        {
                            inventory_source_id = (byte)inventorySourceId,
                            last_loaded = timeStamp
                        });
                    });
                    context.SaveChanges();
                });
        }

        public int GetBroadcastStationCodeByContactId(int stationContactId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var stationContacts = context.station_contacts.SingleOrDefault(s => s.id == stationContactId);
                    if (stationContacts == null)
                        throw new Exception("No Station Contact found.");

                    return stationContacts.station.station_code;
                });
        }

        private readonly Dictionary<string, short> _StationCache = new Dictionary<string, short>();
        public short? GetStationCode(string stationName)
        {
            short code;
            if (_StationCache.TryGetValue(stationName, out code))
                return code;

            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var firstOrDefault = c.stations.FirstOrDefault(s => s.legacy_call_letters == stationName);
                    if (firstOrDefault == null)
                        return (short?)null;

                    _StationCache[stationName] = firstOrDefault.station_code;
                    return firstOrDefault.station_code;
                });
            }
        }
    }
}
