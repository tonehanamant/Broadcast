using System.Data.Entity.Core;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using System.Data.Entity;

namespace Services.Broadcast.Repositories
{
    public interface IStationRepository : IDataRepository
    {
        List<DisplayBroadcastStation> GetBroadcastStationsWithFlightWeeksForRateSource(InventoryFile.InventorySource inventorySource);
        DisplayBroadcastStation GetBroadcastStationByCode(int code);
        DisplayBroadcastStation GetBroadcastStationByLegacyCallLetters(string callLetters);
        DisplayBroadcastStation GetBroadcastStationByCallLetters(string stationCallLetters);
        List<DisplayBroadcastStation> GetBroadcastStationListByLegacyCallLetters(List<string> stationNameList);
        List<DisplayBroadcastStation> GetBroadcastStationsByFlightWeek(InventoryFile.InventorySource inventorySource, int mediaWeekId, bool isIncluded);
        List<DisplayBroadcastStation> GetBroadcastStationListByStationCode(List<int> fileStationCodes);
        int GetBroadcastStationCodeByContactId(int stationContactId);
        void UpdateStation(int code, string user, DateTime timeStamp);
        void UpdateStationList(List<int> stationCodes, string user, DateTime timeStamp);
        short? GetStationCode(string stationName);
    }

    public class StationRepository : BroadcastRepositoryBase, IStationRepository
    {
        public StationRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsWithFlightWeeksForRateSource(InventoryFile.InventorySource inventorySource)
        {
            return _InReadUncommitedTransaction(
                context =>
                {

                    return (from s in context.stations.Include(sa=>sa.station_inventory_manifest)
                        select new DisplayBroadcastStation
                        {
                            Code = s.station_code,
                            Affiliation = s.affiliation,
                            CallLetters = s.station_call_letters,
                            LegacyCallLetters = s.legacy_call_letters,
                            OriginMarket = s.market.geography_name,
                            MarketCode = s.market_code,
                            ModifiedDate = s.modified_date,
                            FlightWeeks = (from m in s.station_inventory_manifest
                                           join g in context.inventory_sources on m.inventory_source_id equals g.id
                                           join p in context.station_inventory_manifest_generation on m.id equals p.station_inventory_manifest_id
                                           // todo: review this equality
                                where g.name.ToLower().Equals(inventorySource.ToString().ToLower())
                                select new FlightWeekDto
                                {
                                    Id = p.media_week_id
                                }).Distinct().OrderBy(a=>a.Id).ToList()
                        }).ToList();
                });
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsByFlightWeek(InventoryFile.InventorySource inventorySource, int mediaWeekId, bool isIncluded)
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
                context =>
                {
                    return (from s in context.stations
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
                            }).FirstOrDefault();
                });
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
                                ModifiedDate = s.modified_date
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
                                ModifiedDate = s.modified_date
                            }).ToList();
                });
        }


        public List<DisplayBroadcastStation> GetBroadcastStationListByStationCode(List<int> fileStationCodes)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from s in context.stations
                            where fileStationCodes.Contains(s.station_code)
                            select new DisplayBroadcastStation
                            {
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market.geography_name,
                                ModifiedDate = s.modified_date
                            }).ToList();
                });
        }

        public void UpdateStation(int code, string user, DateTime timeStamp)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var station = context.stations.First(s => s.station_code == code);
                    station.modified_by = user;
                    station.modified_date = timeStamp;

                    context.SaveChanges();
                });
        }

        public void UpdateStationList(List<int> stationCodes, string user, DateTime timeStamp)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var code in stationCodes)
                    {
                        var stationUpdate = context.stations.Where(q => q.station_code == code).Single();
                        stationUpdate.modified_by = user;
                        stationUpdate.modified_date = timeStamp;
                    }

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
