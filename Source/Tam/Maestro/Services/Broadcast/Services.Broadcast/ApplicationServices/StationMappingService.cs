using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IStationMappingService : IApplicationService
    {
        /// <summary>
        /// Loads the station mappings from an xlsx file to the broadcast database
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="userName">The name of the current user</param>
        /// <param name="createdDate">Created date</param>
        void LoadStationMappings(Stream fileStream, string fileName, string userName, DateTime createdDate);

        /// <summary>
        /// Saves the station mappings.
        /// </summary>
        /// <param name="stationGroup">The station group.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        void SaveStationMappings(IGrouping<string, StationMappingsFileRequestDto> stationGroup, string userName, DateTime createdDate);

        /// <summary>
        /// Adds the new station mapping.
        /// </summary>
        /// <param name="stationMapping">The station mapping.</param>
        /// <param name="updatedBy">The updated by.</param>
        /// <param name="updatedDate">The updated date.</param>
        void AddNewStationMapping(StationMappingsDto stationMapping, string updatedBy, DateTime updatedDate);

        /// <summary>
        /// Gets the station mappings for the given cadent call letters.
        /// </summary>
        /// <param name="cadentCallLetters">The cadent call letters.</param>
        /// <returns></returns>
        List<StationMappingsDto> GetStationMappingsByCadentCallLetter(string cadentCallLetters);

        /// <summary>
        /// Adds the new station mappings.
        /// </summary>
        /// <param name="stationMappingList">The station mapping list.</param>
        /// <param name="updatedBy">The updated by.</param>
        /// <param name="updatedDate">The updated date.</param>
        void AddNewStationMappings(List<StationMappingsDto> stationMappingList, string updatedBy, DateTime updatedDate);
    }

    public class StationMappingService : IStationMappingService
    {
        private readonly IStationMappingRepository _StationMappingRepository;
        private readonly IStationRepository _StationRepository;

        public StationMappingService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _StationMappingRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationMappingRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
        }

        /// <inheritdoc />
        public void LoadStationMappings(Stream fileStream, string fileName, string userName, DateTime createdDate)
        {
            var stationMappings = _ReadStationMappingsFile(fileStream);

            var stationGroups = stationMappings.GroupBy(x => x.CadentCallLetters);
            foreach (var stationGroup in stationGroups)
            {
                SaveStationMappings(stationGroup, userName, createdDate);
            }
        }

        public void SaveStationMappings(IGrouping<string, StationMappingsFileRequestDto> stationGroup, string userName, DateTime createdDate)
        {
            // Get all the mappings for a cadent call letter
            var cadentCallLetters = stationGroup.Key;
            var extendedCallLettersList = stationGroup
                .Where(x => !string.IsNullOrWhiteSpace(x.ExtendedCallLetters))
                .Select(x => x.ExtendedCallLetters)
                .Distinct().ToList();
            var sigmaCallLettersList = stationGroup
                .Where(x => !string.IsNullOrWhiteSpace(x.SigmaCallLetters))
                .Select(x => x.SigmaCallLetters)
                .Distinct().ToList();
            var nsiLegacyCallLettersList = stationGroup
                .Where(x => !string.IsNullOrWhiteSpace(x.NSILegacyCallLetters))
                .Select(x => x.NSILegacyCallLetters)
                .Distinct().ToList();
            var nsiCallLettersList = stationGroup
                .Where(x => !string.IsNullOrWhiteSpace(x.NSICallLetters))
                .Select(x => x.NSICallLetters)
                .Distinct().ToList();

            // Get the station
            var station = _StationRepository.GetBroadcastStationByLegacyCallLetters(cadentCallLetters);
            // If the station doesn't exist, and we have an NSI call letter for it, create it
            if (station == null && !string.IsNullOrEmpty(nsiCallLettersList.FirstOrDefault()))
            {
                station = _StationRepository.CreateStation(
                    new DisplayBroadcastStation
                    {
                        CallLetters = nsiCallLettersList.FirstOrDefault(),
                        Affiliation = stationGroup.First().Affiliate,
                        LegacyCallLetters = stationGroup.First().CadentCallLetters,
                        ModifiedDate = createdDate,
                    },
                    userName);
            }

            // If we still don't have a station, don't save mappings
            if (station == null)
            {
                return;
            }

            // Since we allow multiple map sets for a station, remove the existing ones, and add the new ones, to avoid duplicate mappings
            _StationMappingRepository.RemoveAllMappingsForCadentCallLetters(cadentCallLetters);

            // Create the mappings
            var newMappings = new List<StationMappingsDto>();
            extendedCallLettersList.ForEach(callLetter =>
            {
                newMappings.Add(new StationMappingsDto
                {
                    StationId = station.Id,
                    MapSet = StationMapSetNamesEnum.Extended,
                    MapValue = callLetter
                });
            });
            sigmaCallLettersList.ForEach(callLetter =>
            {
                newMappings.Add(new StationMappingsDto
                {
                    StationId = station.Id,
                    MapSet = StationMapSetNamesEnum.Sigma,
                    MapValue = callLetter
                });
            });
            nsiCallLettersList.ForEach(callLetter =>
            {
                newMappings.Add(new StationMappingsDto
                {
                    StationId = station.Id,
                    MapSet = StationMapSetNamesEnum.NSI,
                    MapValue = callLetter
                });
            });
            nsiLegacyCallLettersList.ForEach(callLetter =>
            {
                newMappings.Add(new StationMappingsDto
                {
                    StationId = station.Id,
                    MapSet = StationMapSetNamesEnum.NSILegacy,
                    MapValue = callLetter
                });
            });

            AddNewStationMappings(newMappings, userName, createdDate);
        }

        /// <inheritdoc />
        public List<StationMappingsDto> GetStationMappingsByCadentCallLetter(string cadentCallLetters)
        {
            return _StationMappingRepository.GetStationMappingsByCadentCallLetters(cadentCallLetters);
        }

        /// <inheritdoc />
        public void AddNewStationMappings(List<StationMappingsDto> stationMappingList, string updatedBy, DateTime updatedDate)
        {
            _StationMappingRepository.SaveNewStationMappings(stationMappingList, updatedBy, updatedDate);
        }

        public void AddNewStationMapping(StationMappingsDto stationMapping, string updatedBy, DateTime updatedDate)
        {
            _StationMappingRepository.SaveNewStationMapping(stationMapping, updatedBy, updatedDate);
        }

        private List<StationMappingsFileRequestDto> _ReadStationMappingsFile(Stream stream)
        {
            var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[1];
            var stationMappings = worksheet.ConvertSheetToObjects<StationMappingsFileRequestDto>();

            return stationMappings.ToList();
        }
    }
}
