using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IStationService : IApplicationService
    {
        List<NsiStationDto> GetLatestNsiStationList();
        bool StationExistsInBroadcastDatabase(string stationCallLetters);
        int GetLatestStationDetailMediaMonthId();
        /// <summary>
        /// Stations import hangfire job entry point.
        /// </summary>
        [Queue("stationsupdate")]
        void ImportStationsFromForecastDatabaseJobEntryPoint(string userName);
        void ImportStationsFromForecastDatabase(string createdBy, DateTime createdDate);
    }

    public class StationService : IStationService
    {
        private readonly INsiStationRepository _NsiStationRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IStationMappingService _StationMappingService;

        public StationService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IStationMappingService stationMappingService
            )
        {
            _NsiStationRepository = broadcastDataRepositoryFactory.GetDataRepository<INsiStationRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            broadcastDataRepositoryFactory.GetDataRepository<IStationMappingRepository>();
            _StationMappingService = stationMappingService;
        }

        public List<NsiStationDto> GetLatestNsiStationList()
        {
            var latestMediaMonthId = _NsiStationRepository.GetLatestMediaMonthIdFromStationList();
            return _NsiStationRepository.GetNsiStationListByMediaMonth(latestMediaMonthId);
        }

        public int GetLatestStationDetailMediaMonthId()
        {
            return _StationRepository.GetLatestMediaMonthIdFromStationMonthDetailsList();
        }

        public bool StationExistsInBroadcastDatabase(string stationCallLetters)
        {
            return _StationRepository.ExistsStationWithCallLetter(stationCallLetters);
        }

        public void ImportStationsFromForecastDatabase(string createdBy, DateTime createdDate)
        {

            var latestNsiStationListMediaMonthId = _NsiStationRepository.GetLatestMediaMonthIdFromStationList();
            var latestBroadcastStationListMediaMonthId = _StationRepository.GetLatestMediaMonthIdFromStationMonthDetailsList();

            if (latestNsiStationListMediaMonthId > latestBroadcastStationListMediaMonthId)
            {
                // We have new data in the broadcast_forecast database that we can start importing
                var forecastStationList = _NsiStationRepository.GetNsiStationListByMediaMonth(latestNsiStationListMediaMonthId);

                using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                {
                    forecastStationList.ForEach(forecastStation =>
                    {
                        var broadcastStation = new DisplayBroadcastStation();

                        // Check if we are dealing with a new station
                        var stationExist = _StationRepository.ExistsStationWithCallLetter(forecastStation.LegacyCallLetters);
                        if (!stationExist)
                        {
                            // Create the station
                            broadcastStation = _StationRepository.CreateStation(
                                    new DisplayBroadcastStation
                                    {
                                        Code = forecastStation.DistributorCode,
                                        CallLetters = forecastStation.CallLetters,
                                        Affiliation = forecastStation.PrimaryAffiliation,
                                        MarketCode = forecastStation.MarketCode,
                                        LegacyCallLetters = forecastStation.LegacyCallLetters,
                                        ModifiedDate = createdDate,
                                    },
                                    createdBy);
                        }
                        else
                        {
                            broadcastStation = _StationRepository.GetBroadcastStationByLegacyCallLetters(forecastStation.LegacyCallLetters);
                        }

                        if (!string.IsNullOrWhiteSpace(forecastStation.DistributorGroup))
                        {
                            // We are dealing with a child station
                            // We don't save a record for these in the stations table
                            _StationMappingService.AddNewStationMapping(
                                    new StationMappingsDto
                                    {
                                        StationId = broadcastStation.Id,
                                        MapSet = StationMapSetNamesEnum.NSI,
                                        MapValue = forecastStation.CallLetters
                                    },
                                    createdBy,
                                    createdDate);
                        }
                        else
                        {
                            // This is a parent/main station
                            var newStationMonthDetail = new StationMonthDetailDto
                            {
                                StationId = broadcastStation.Id,
                                MediaMonthId = forecastStation.MediaMonthId,
                                Affiliation = forecastStation.PrimaryAffiliation,
                                MarketCode = (short?)forecastStation.MarketCode,
                                DistributorCode = forecastStation.DistributorCode
                            };

                            _StationRepository.SaveStationMonthDetails(newStationMonthDetail);
                        }
                    });

                    // Copy unrated/non-NSI stations from the previous month
                    var unratedStations = _StationRepository.GetUnratedBroadcastStations();
                    foreach (var station in unratedStations)
                    {
                        var newStationMonthDetail = new StationMonthDetailDto
                        {
                            StationId = station.Id,
                            MediaMonthId = latestNsiStationListMediaMonthId,
                            Affiliation = station.Affiliation,
                        };

                        _StationRepository.SaveStationMonthDetails(newStationMonthDetail);
                    }

                    transaction.Complete();
                }
            }
        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void ImportStationsFromForecastDatabaseJobEntryPoint(string userName)
        {
            var createdDate = DateTime.Now;
            ImportStationsFromForecastDatabase(userName, createdDate);
        }
    }
}
