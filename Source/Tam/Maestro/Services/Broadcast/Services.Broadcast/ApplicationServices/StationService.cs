using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Services.Broadcast.BusinessEngines;
using System.IO;
using Services.Broadcast.Extensions;

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

        int ResetLastStationsFromForecastDatabaseJob(string username);

        /// <summary>
        /// Get Station Missing Market and Affiliations data.
        /// </summary>   
        MemoryStream GetStationsMissingMarkets();
    }

    public class StationService : BroadcastBaseClass, IStationService
    {
        private readonly INsiStationRepository _NsiStationRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IStationMappingService _StationMappingService;
        private readonly IDateTimeEngine _DateTimeEngine;

        public StationService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IStationMappingService stationMappingService,
            IDateTimeEngine dateTimeEngine, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper
            ) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _NsiStationRepository = broadcastDataRepositoryFactory.GetDataRepository<INsiStationRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            broadcastDataRepositoryFactory.GetDataRepository<IStationMappingRepository>();
            _StationMappingService = stationMappingService;
            _DateTimeEngine = dateTimeEngine;
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
            _LogInfo("Station ingest process starting...");
            var latestNsiStationListMediaMonthId = _NsiStationRepository.GetLatestMediaMonthIdFromStationList();
            var latestBroadcastStationListMediaMonthId = _StationRepository.GetLatestMediaMonthIdFromStationMonthDetailsList();

            _LogInfo($"Nsi.Stations.MaxMediaMonthId : {latestNsiStationListMediaMonthId}; Broadcast.Station.MaxMediaMonthId : {latestBroadcastStationListMediaMonthId};");

            if (latestNsiStationListMediaMonthId > latestBroadcastStationListMediaMonthId)
            {
                // We have new data in the broadcast_forecast database that we can start importing
                var forecastStationList = _NsiStationRepository.GetNsiStationListByMediaMonth(latestNsiStationListMediaMonthId);

                var ingestedCount = forecastStationList.Count;
                _LogInfo($"Identified {ingestedCount} nsi stations for ingest. Beginning processing...");

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

                            if (!forecastStation.DistributorCode.Equals(broadcastStation.Code) ||
                                !forecastStation.PrimaryAffiliation.Equals(broadcastStation.Affiliation) ||
                                !forecastStation.MarketCode.Equals(broadcastStation.MarketCode))
                            {
                                broadcastStation.Code = forecastStation.DistributorCode;
                                broadcastStation.Affiliation = forecastStation.PrimaryAffiliation;
                                broadcastStation.MarketCode = forecastStation.MarketCode;

                                _StationRepository.UpdateStation(broadcastStation, createdBy, createdDate);
                            }
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

                    var unratedStationsCount = unratedStations.Count;
                    _LogInfo($"Identified {unratedStationsCount} unrated broadcast stations for processing. Beginning processing...");

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
                _LogInfo($"Completed.");
            }
            else
            {
                _LogInfo("Stopping. Ingest has run for the latest month ({latestNsiStationListMediaMonthId}=={latestBroadcastStationListMediaMonthId}).");
            }
        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void ImportStationsFromForecastDatabaseJobEntryPoint(string userName)
        {
            var createdDate = _DateTimeEngine.GetCurrentMoment();

            ImportStationsFromForecastDatabase(userName, createdDate);
        }

        public int ResetLastStationsFromForecastDatabaseJob(string username)
        {
            var latestSourceMediaMonthId = _NsiStationRepository.GetLatestMediaMonthIdFromStationList();
            _LogInfo($"Deleting station month detail records for media month id {latestSourceMediaMonthId}.", username);
            var deletedCount = _StationRepository.DeleteStationMonthDetailsForMonth(latestSourceMediaMonthId);
            _LogInfo($"Deleted {deletedCount} station month detail records.", username);
            return deletedCount;
        }
        public MemoryStream GetStationsMissingMarkets()
        {
            _LogInfo($"Gathering the missing station report data...");
            var stationDetails = _StationRepository.GetStationsMissingMarkets();
            _LogInfo($"Preparing to generate the missing station report file.");
            var report = MissingStationsReportGenerator.Generate(stationDetails);
            _LogInfo($"Exporting generated missing station report file");
            return report;
        }
    }
}
