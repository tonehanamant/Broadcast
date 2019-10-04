using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IStationRepository : IDataRepository
    {
        DisplayBroadcastStation GetBroadcastStationByCode(int code);
        List<DisplayBroadcastStation> GetBroadcastStationsByCodes(IEnumerable<int> codes);
        DisplayBroadcastStation GetBroadcastStationByLegacyCallLetters(string callLetters);
        DisplayBroadcastStation GetBroadcastStationByCallLetters(string stationCallLetters);
        List<DisplayBroadcastStation> GetBroadcastStationListByLegacyCallLetters(List<string> stationNameList);

        /// <summary>
        /// Gets all the broadcast stations.
        /// </summary>
        /// <returns>List of DisplayBroadcastStation objects</returns>
        List<DisplayBroadcastStation> GetBroadcastStations();

        int GetBroadcastStationCodeByContactId(int stationContactId);
        void UpdateStation(int code, string user, DateTime timeStamp, int inventorySourceId);
        void UpdateStationList(List<int> stationIds, string user, DateTime timeStamp, int inventorySourceId);
        short? GetStationCode(string stationName);
        List<DisplayBroadcastStation> CreateStations(IEnumerable<DisplayBroadcastStation> stations, string user);
    }

    public class StationRepository : BroadcastRepositoryBase, IStationRepository
    {
        public StationRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public DisplayBroadcastStation GetBroadcastStationByCode(int code)
        {
            return _InReadUncommitedTransaction(
                context => (from s in context.stations
                            where s.station_code == code
                            select new DisplayBroadcastStation
                            {
                                Id = s.id,
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market == null ? null : s.market.geography_name,
                                MarketCode = s.market_code,
                                ModifiedDate = s.modified_date
                            }).Single("No station found with code: " + code));
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsByCodes(IEnumerable<int> codes)
        {
            return _InReadUncommitedTransaction(
                context => (from s in context.stations
                            where codes.Contains(s.station_code.Value)
                            select new DisplayBroadcastStation
                            {
                                Id = s.id,
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market == null ? null : s.market.geography_name,
                                MarketCode = s.market_code,
                                ModifiedDate = s.modified_date
                            }).ToList());
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
                                Id = s.id,
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market == null ? null : s.market.geography_name,
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
                                Id = s.id,
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market == null ? null : s.market.geography_name,
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
                                Id = s.id,
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market == null ? null : s.market.geography_name,
                                MarketCode = s.market_code,
                                ModifiedDate = s.modified_date
                            }).ToList();
                });
        }

        /// <inheritdoc/>
        public List<DisplayBroadcastStation> GetBroadcastStations()
        {
            return _InReadUncommitedTransaction(
                context => (from s in context.stations
                            select new DisplayBroadcastStation
                            {
                                Id = s.id,
                                Code = s.station_code,
                                Affiliation = s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = s.market == null ? null : s.market.geography_name,
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

        public void UpdateStationList(List<int> stationIds, string user, DateTime timeStamp, int inventorySourceId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var stations = context.stations.Where(x => stationIds.Contains(x.id)).ToList();                    
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

                    return stationContacts.station.station_code.Value;
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

                    _StationCache[stationName] = (short)firstOrDefault.station_code.Value;
                    return (short?)firstOrDefault.station_code;
                });
            }
        }

        public List<DisplayBroadcastStation> CreateStations(IEnumerable<DisplayBroadcastStation> stations, string user)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var newStations = stations.Select(x => new station
                    {
                        station_call_letters = x.CallLetters,
                        legacy_call_letters = x.LegacyCallLetters,
                        modified_date = x.ModifiedDate.Value,
                        modified_by = user
                    }).ToList();

                    context.stations.AddRange(newStations);
                    context.SaveChanges();

                    return newStations.Select(s => new DisplayBroadcastStation
                    {
                        Id = s.id,
                        Code = s.station_code,
                        CallLetters = s.station_call_letters,
                        LegacyCallLetters = s.legacy_call_letters,
                        ModifiedDate = s.modified_date
                    }).ToList();
                });
        }
    }
}
