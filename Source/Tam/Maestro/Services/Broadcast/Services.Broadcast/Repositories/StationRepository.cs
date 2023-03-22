using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IStationRepository : IDataRepository
    {
        DisplayBroadcastStation GetBroadcastStationByCode(int code);
        List<DisplayBroadcastStation> GetBroadcastStationsByCodes(IEnumerable<int> codes);
        DisplayBroadcastStation GetBroadcastStationByLegacyCallLetters(string callLetters);
        DisplayBroadcastStation GetBroadcastStationByCallLetters(string stationCallLetters);
        List<DisplayBroadcastStation> GetBroadcastStationListByLegacyCallLetters(List<string> stationNameList);

        DisplayBroadcastStation GetBroadcastStationById(int id);

        List<DisplayBroadcastStation> GetBroadcastStationsByMarketCodes(List<short> marketCodes);

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

        /// <summary>
        /// Update the station info.
        /// </summary>
        void UpdateStation(DisplayBroadcastStation station, string user, DateTime updateDate);

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
        /// Creates the station with month details.
        /// </summary>
        /// <param name="station">The station.</param>
        /// <param name="mediaMonthId">The media month identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        DisplayBroadcastStation CreateStationWithMonthDetails(DisplayBroadcastStation station, int mediaMonthId, string user);

        /// <summary>
        /// Saves the station month details.
        /// </summary>
        /// <param name="stationMonthDetail">The station month detail.</param>
        void SaveStationMonthDetails(StationMonthDetailDto stationMonthDetail);

        /// <summary>
        /// Gets the station month details for a station.
        /// </summary>
        /// <param name="stationId">The station identifier.</param>
        List<StationMonthDetailDto> GetStationMonthDetailsForStation(int stationId);

        /// <summary>
        /// Gets the latests month details for stations
        /// </summary>
        /// <param name="stationsId">The station ids to get the month details</param>
        List<StationMonthDetailDto> GetLatestStationMonthDetailsForStations(List<int> stationsId);

        /// <summary>
        /// Gets the stations with the latest month details
        /// </summary>
        /// <param name="marketCodes">Market codes to the filter the stations</param>
        List<DisplayBroadcastStation> GetBroadcastStationsWithLatestDetailsByMarketCodes(List<short> marketCodes);

        /// <summary>
        /// Updates the station owner and sales group names.
        /// </summary>
        /// <param name="ownerName">Name of the owner.</param>
        /// <param name="repFirmName">Name of the sales group.</param>
        /// <param name="stationId">Station id</param>
        /// <param name="isTrueInd">True if the station is a 'True Independent' station.</param>
        /// <param name="user">The user making the update.</param>
        /// <param name="timeStamp">The time stamp when the update was done.</param>
        void UpdateStation(string ownerName, string repFirmName, int stationId, bool isTrueInd, string user, DateTime timeStamp);

        List<DisplayBroadcastStation> GetBroadcastStationsByIds(IEnumerable<int> stationIds);

        int DeleteStationMonthDetailsForMonth(int mediaMonthId);

        /// <summary>
        /// Get Station Missing Market and Affiliations data.
        /// </summary>      
        List<StationsGapDto> GetStationsMissingMarkets();
    }

    public class StationRepository : BroadcastRepositoryBase, IStationRepository
    {
        public StationRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public DisplayBroadcastStation GetBroadcastStationByCode(int code)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var station = (from s in context.stations
                            where s.station_code == code
                            select s)
                        .Single("No station found with code: " + code);

                    return _MapToDto(station);
                });
        }

        public DisplayBroadcastStation GetBroadcastStationById(int id)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var station = (from s in context.stations
                        where s.id == id
                        select s).Single("No station found with id: " + id);

                    return _MapToDto(station);
                });
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsByIds(IEnumerable<int> stationIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.stations
                        .Where(x => stationIds.Contains(x.id))
                        .ToList()
                        .Select(_MapToDto)
                        .ToList();
                });
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsByCodes(IEnumerable<int> codes)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var stations =  (from s in context.stations
                        where codes.Contains(s.station_code.Value)
                        select s).ToList();

                    return stations.Select(_MapToDto).ToList();
                });
        }

        public DisplayBroadcastStation GetBroadcastStationByLegacyCallLetters(string callLetters)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var station = (from s in context.stations
                            where s.legacy_call_letters == callLetters
                            select s).FirstOrDefault();

                    if (station == null)
                    {
                        return null;
                    }

                    return _MapToDto(station);
                });
        }

        public DisplayBroadcastStation GetBroadcastStationByCallLetters(string stationCallLetters)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var station = (from s in context.stations
                        where s.station_call_letters == stationCallLetters
                        select s).FirstOrDefault();

                    return _MapToDto(station);
                });
        }

        public List<DisplayBroadcastStation> GetBroadcastStationListByLegacyCallLetters(List<string> stationNameList)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var stations= (from s in context.stations
                            where stationNameList.Contains(s.legacy_call_letters)
                            select s).ToList();

                    return stations.Select(_MapToDto).ToList();
                });
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsByMarketCodes(List<short> marketCodes)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var stations = (from s in context.stations
                        where s.market_code.HasValue
                            && marketCodes.Contains(s.market_code.Value)
                        select s).ToList();

                    return stations.Select(_MapToDto).ToList();
                });
        }

        public List<DisplayBroadcastStation> GetBroadcastStationsWithLatestDetailsByMarketCodes(List<short> marketCodes)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var latestMediaMonth = (from s in context.station_month_details
                                            orderby s.media_month_id descending
                                            select (int?)s.media_month_id).FirstOrDefault();

                    return (from s in context.stations
                            from sd in s.station_month_details.DefaultIfEmpty()
                            where 
                                // Station has month details.
                                (sd != null &&
                                latestMediaMonth != null &&
                                sd.media_month_id == latestMediaMonth &&
                                sd.market_code != null &&
                                marketCodes.Contains(sd.market_code.Value)) ||
                                // Station doesn't have month details.
                                (sd == null && 
                                s.market_code != null &&
                                marketCodes.Contains(s.market_code.Value))
                            select new DisplayBroadcastStation
                            {
                                Id = s.id,
                                Code = s.station_code,
                                Affiliation = sd != null ? sd.affiliation : s.affiliation,
                                CallLetters = s.station_call_letters,
                                LegacyCallLetters = s.legacy_call_letters,
                                OriginMarket = sd != null ? 
                                    sd.market == null ? null : sd.market.geography_name :
                                    s.market == null ? null : s.market.geography_name,
                                MarketCode = sd != null ? sd.market_code : s.market_code,
                                ModifiedDate = s.modified_date,
                                IsTrueInd = s.is_true_ind
                            }).ToList();
                });
        }

        /// <inheritdoc/>
        public List<DisplayBroadcastStation> GetBroadcastStations()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var stations = (from s in context.stations
                        select s).ToList();

                    return stations.Select(_MapToDto).ToList();
                });
        }

        /// <inheritdoc/>
        public List<DisplayBroadcastStation> GetUnratedBroadcastStations()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var stations = (from s in context.stations
                        where s.station_code == null
                        select s).ToList();

                    return stations.Select(_MapToDto).ToList();
                });
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

        public void UpdateStation(string ownerName, string repFirmName, int stationId, bool isTrueInd, string user, DateTime timeStamp)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var station = context.stations.First(s => s.id == stationId);
                    station.modified_by = user;
                    station.modified_date = timeStamp;
                    station.owner_name = ownerName;
                    station.rep_firm_name = repFirmName;
                    station.is_true_ind = isTrueInd;
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

        /// <inheritdoc/>
        public void UpdateStation(DisplayBroadcastStation station, string user, DateTime updateDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var existingStation = context.stations
                        .Where(s => s.legacy_call_letters.Equals(station.LegacyCallLetters))
                        .Single($"No station found for station id '{station.Id}' with legacy call letters '{station.LegacyCallLetters}'.");

                    existingStation.station_code = station.Code;
                    existingStation.station_call_letters = station.CallLetters;
                    existingStation.affiliation = station.Affiliation;
                    existingStation.market_code = (short?)station.MarketCode;
                    existingStation.modified_by = user;
                    existingStation.modified_date = updateDate;
                    existingStation.is_true_ind = station.IsTrueInd;

                    context.SaveChanges();
                });
        }

        /// <inheritdoc/>
        public DisplayBroadcastStation CreateStationWithMonthDetails(DisplayBroadcastStation station, int mediaMonthId, string user)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var newStation = _MapFromDto(station, user);
                    newStation.station_month_details.Add(new station_month_details
                    {
                        media_month_id = mediaMonthId,
                        affiliation = newStation.affiliation,
                        market_code = newStation.market_code,
                        distributor_code = newStation.station_code
                    });
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
                OriginMarket = newStation.market?.geography_name,
                LegacyCallLetters = newStation.legacy_call_letters,
                ModifiedDate = newStation.modified_date,
                OwnershipGroupName = newStation.owner_name,
                RepFirmName = newStation.rep_firm_name,
                IsTrueInd = newStation.is_true_ind
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
                modified_by = user,
                owner_name = station.OwnershipGroupName,
                rep_firm_name = station.RepFirmName,
                is_true_ind = station.IsTrueInd
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

        public List<StationMonthDetailDto> GetStationMonthDetailsForStation(int stationId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var montDetails = context.station_month_details
                        .Where(m => m.station_id == stationId)
                        .Select(m => new StationMonthDetailDto
                        {
                            StationId = m.station_id,
                            MediaMonthId = m.media_month_id,
                            Affiliation = m.affiliation,
                            MarketCode = m.market_code,
                            DistributorCode = m.distributor_code
                        })
                        .ToList();

                    return montDetails;
                });
        }

        public List<StationMonthDetailDto> GetLatestStationMonthDetailsForStations(List<int> stationsId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var latestMediaMonth = (from s in context.station_month_details
                                            orderby s.media_month_id descending
                                            select (int?)s.media_month_id).FirstOrDefault();

                    if (latestMediaMonth == null)
                        return new List<StationMonthDetailDto>();

                    var montDetails = context.station_month_details
                        .Where(m => stationsId.Contains(m.station_id) && m.media_month_id == latestMediaMonth)
                        .Select(m => new StationMonthDetailDto
                        {
                            StationId = m.station_id,
                            MediaMonthId = m.media_month_id,
                            Affiliation = m.affiliation,
                            MarketCode = m.market_code,
                            DistributorCode = m.distributor_code
                        })
                        .ToList();

                    return montDetails;
                });
        }

        public int DeleteStationMonthDetailsForMonth(int mediaMonthId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var deletedCount = context.station_month_details
                        .RemoveRange(context.station_month_details.Where(s => s.media_month_id == mediaMonthId))
                        .Count();
                    context.SaveChanges();
                    return deletedCount;
                });
        }

        public List<StationsGapDto> GetStationsMissingMarkets()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var stnDetails = (from stn in context.stations 
                                      join mkt in context.markets 
                                      on stn.market_code equals mkt.market_code
                                      into StationMarketGroup 
                                      from market in StationMarketGroup.DefaultIfEmpty() 
                                      where stn.market==null || stn.affiliation == null
                                      orderby stn.legacy_call_letters
                                      select new StationsGapDto
                                      {
                                          LegacyCallLetters = stn.legacy_call_letters,
                                          MarketCode = (int)stn.market_code,
                                          MarketName = market.geography_name,
                                          Affiliation = stn.affiliation

                                      }).ToList();
                    return stnDetails;
                });
        }
    }
}
