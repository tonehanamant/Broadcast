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

        /// <summary>
        /// Gets all the unrated broadcast stations (the ones with no station code).
        /// </summary>
        /// <returns>List of DisplayBroadcastStation objects</returns>
        List<DisplayBroadcastStation> GetUnratedBroadcastStations();

        int GetBroadcastStationCodeByContactId(int stationContactId);
        void UpdateStation(int code, string user, DateTime timeStamp, int inventorySourceId);
        void UpdateStationList(List<int> stationIds, string user, DateTime timeStamp, int inventorySourceId);
        short? GetStationCode(string stationName);
        List<DisplayBroadcastStation> CreateStations(IEnumerable<DisplayBroadcastStation> stations, string user);

        /// <summary>
        /// Checks if a station exists for the given station call letters.
        /// </summary>
        /// <param name="stationCallLetters">The station call letters.</param>
        /// <returns></returns>
        bool ExistsStationWithCallLetter(string stationCallLetters);

        /// <summary>
        /// Gets the latest media month for which the station list details where imported
        /// </summary>
        /// <returns></returns>
        int GetLatestMediaMonthIdFromStationMonthDetailsList();

        /// <summary>
        /// Creates the station.
        /// </summary>
        /// <param name="station">The station.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        DisplayBroadcastStation CreateStation(DisplayBroadcastStation station, string user);

        /// <summary>
        /// Saves the station month details.
        /// </summary>
        /// <param name="stationMonthDetail">The station month detail.</param>
        void SaveStationMonthDetails(StationMonthDetailDto stationMonthDetail);
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

        /// <inheritdoc/>
        public List<DisplayBroadcastStation> GetUnratedBroadcastStations()
        {
            return _InReadUncommitedTransaction(
                context => (from s in context.stations
                            where s.station_code == null
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

        /// <inheritdoc/>
        public DisplayBroadcastStation CreateStation(DisplayBroadcastStation station, string user)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var newStation = _MapFromDto(station, user);

                    context.stations.Add(newStation);
                    context.SaveChanges();

                    return _MapToDto(newStation);
                });
        }

        private DisplayBroadcastStation _MapToDto(station newStation)
        {
            return new DisplayBroadcastStation
            {
                Id = newStation.id,
                Code = newStation.station_code,
                CallLetters = newStation.station_call_letters,
                Affiliation = newStation.affiliation,
                MarketCode = newStation.market_code,
                LegacyCallLetters = newStation.legacy_call_letters,
                ModifiedDate = newStation.modified_date
            };
        }

        private station _MapFromDto(DisplayBroadcastStation station, string user)
        {
            return new station
            {
                station_call_letters = station.CallLetters,
                legacy_call_letters = station.LegacyCallLetters,
                station_code = station.Code,
                affiliation = station.Affiliation,
                modified_date = station.ModifiedDate.Value,
                market_code =  (short?)station.MarketCode,
                modified_by = user
            };
        }

        /// <inheritdoc/>
        public bool ExistsStationWithCallLetter(string stationCallLetters)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.stations.Any(s => s.legacy_call_letters == stationCallLetters);
                });
        }

        /// <inheritdoc/>
        public int GetLatestMediaMonthIdFromStationMonthDetailsList()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var result = context.station_month_details
                    .OrderByDescending(station => station.media_month_id)
                    .FirstOrDefault();

                    return result != null ? result.media_month_id : 0;
                });
        }

        public void SaveStationMonthDetails(StationMonthDetailDto stationMonthDetail)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var newStationMonthDetail = new station_month_details
                    {
                        station_id = stationMonthDetail.StationId,
                        media_month_id = stationMonthDetail.MediaMonthId,
                        affiliation = stationMonthDetail.Affiliation,
                        market_code = stationMonthDetail.MarketCode,
                        distributor_code = stationMonthDetail.DistributorCode,
                    };

                    context.station_month_details.Add(newStationMonthDetail);
                    context.SaveChanges();
                });
        }
    }
}
