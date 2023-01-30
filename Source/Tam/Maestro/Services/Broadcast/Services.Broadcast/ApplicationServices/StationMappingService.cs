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
using Amazon.S3.Model.Internal.MarshallTransformations;

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

        /// <summary>
        /// Gets the station by call letters.
        /// </summary>
        /// <param name="stationCallLetters">The station call letters.</param>
        /// <param name="throwIfNotFound">Boolean value. When set to true exception is thrown if station not found</param>
        /// <returns></returns>
        DisplayBroadcastStation GetStationByCallLetters(string stationCallLetters, bool throwIfNotFound = true);
        /// <summary>
        /// Gets the station by call letters.
        /// </summary>
        DisplayBroadcastStation GetStationByCallLetterWithoutError(string stationCallLetters);
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

            _ValidateMaxLength(stationMappings);
            _PopulateIndependentStationsOwnershipGroupName(stationMappings);
            _ProcessStationMappings(userName, createdDate, stationMappings);
        }

        private void _ValidateMaxLength(List<StationMappingsFileRequestDto> stationMappings)
        {
            var exceedingLimit = stationMappings.Where(x => x.OwnershipGroupName != null && x.OwnershipGroupName.Length > 100)
                .Select(x => x.CadentCallLetters);
            if (exceedingLimit.Count() > 0)
            {
                bool multiple = exceedingLimit.Count() > 1;
                throw new ApplicationException(
                    $"Station{(multiple ? "s" : string.Empty)} {string.Join(",", exceedingLimit)} {(multiple ? "have" : "has")} ownership name greater than 100 chars.");
            }
            exceedingLimit = stationMappings.Where(x => x.RepFirmName != null && x.RepFirmName.Length > 100)
                .Select(x => x.CadentCallLetters);
            if (exceedingLimit.Count() > 0)
            {
                bool multiple = exceedingLimit.Count() > 1;
                throw new ApplicationException(
                    $"Station{(multiple ? "s" : string.Empty)} {string.Join(",", exceedingLimit)} {(multiple ? "have" : "has")} sales group name greater than 100 chars.");
            }
        }

        private void _PopulateIndependentStationsOwnershipGroupName(List<StationMappingsFileRequestDto> stationMappings)
        {
            foreach (var mapping in stationMappings
                .Where(x=> x.Affiliate != null && x.Affiliate.Equals("IND", StringComparison.OrdinalIgnoreCase)
                            && string.IsNullOrWhiteSpace(x.OwnershipGroupName)))
            {
                mapping.OwnershipGroupName = mapping.CadentCallLetters;
            }
        }

        private void _ProcessStationMappings(string userName, DateTime createdDate, List<StationMappingsFileRequestDto> stationMappings)
        {
            var stationGroups = stationMappings.GroupBy(x => x.CadentCallLetters);
            foreach (var stationGroup in stationGroups)
            {
                if (stationGroup.Key != null)
                {
                    _ValidateSalesAndOwnerGroupName(stationGroup.ToList());
                    SaveStationMappings(stationGroup, userName, createdDate);
                }
            }
        }

        private void _ValidateSalesAndOwnerGroupName(List<StationMappingsFileRequestDto> items)
        {
            string cadentCallLetters = items.First().CadentCallLetters;
            if (items.Any(x => string.IsNullOrWhiteSpace(x.RepFirmName)))
            {
                throw new ApplicationException($"Station {cadentCallLetters} does not have sales group populated");
            }
            if (items.Any(x => string.IsNullOrWhiteSpace(x.OwnershipGroupName)))
            {
                throw new ApplicationException($"Station {cadentCallLetters} does not have ownership group populated");
            }
            if (items.Select(x=>x.RepFirmName.Trim()).Distinct().Count() > 1)
            {
                throw new ApplicationException($"Station {cadentCallLetters} cannot have multiple sales groups");
            }
            if (items.Select(x => x.OwnershipGroupName.Trim()).Distinct().Count() > 1)
            {
                throw new ApplicationException($"Station {cadentCallLetters} cannot have multiple ownership groups");
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

            // prep a record to save
            var currentStationGroup = stationGroup.First();
            var stationDtoToSave = new DisplayBroadcastStation
            {
                CallLetters = nsiCallLettersList.FirstOrDefault() ?? currentStationGroup.CadentCallLetters,
                Affiliation = currentStationGroup.Affiliate,
                LegacyCallLetters = currentStationGroup.CadentCallLetters,
                ModifiedDate = createdDate,
                IsTrueInd = _IsStationATrueInd(currentStationGroup),
                RepFirmName = currentStationGroup.RepFirmName,
                OwnershipGroupName = currentStationGroup.OwnershipGroupName
            };

            // look to update existing...
            // If the station doesn't exist, create it
            var station = _StationRepository.GetBroadcastStationByLegacyCallLetters(cadentCallLetters);
            if (station == null)
            {
                var stationMediaMonthId = _StationRepository.GetLatestMediaMonthIdFromStationMonthDetailsList();
                station = _StationRepository.CreateStationWithMonthDetails(stationDtoToSave, stationMediaMonthId, userName);
            }
            else
            {
                _StationRepository.UpdateStation(stationDtoToSave.OwnershipGroupName,
                    stationDtoToSave.RepFirmName, station.Id, stationDtoToSave.IsTrueInd, userName, createdDate);
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

        private bool _IsStationATrueInd(StationMappingsFileRequestDto s)
        {
            var thisMeansYes = new List<string> { "YES" };

            if (thisMeansYes.Contains(s.IsTrueInd?.Trim()?.ToUpper()))
            {
                return true;
            }

            return false;
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
            var rawStationMappings = worksheet.ConvertSheetToObjects<StationMappingsFileRequestDto>();
            // filter out the empty rows the above can pick up
            var stationMappings = rawStationMappings.Where(s => s.CadentCallLetters != null).ToList();
            return stationMappings;
        }

        /// <inheritdoc />
        public DisplayBroadcastStation GetStationByCallLetters(string stationCallLetters, bool throwIfNotFound = true)
        {
            // Check station (Cadent Call Letters, a.k.a. legacy call letters)
            var station = _StationRepository.GetBroadcastStationByLegacyCallLetters(stationCallLetters);

            // If not found, check exact match in mappings
            if (station == null)
            {
                station = _StationMappingRepository.GetBroadcastStationByCallLetters(stationCallLetters);
            }

            // If not found, check starts with in mappings
            // If not found or multiple stations found, throw an error
            if (station == null)
            {
                station = _StationMappingRepository.GetBroadcastStationStartingWithCallLetters(stationCallLetters, throwIfNotFound);
            }

            return station;
        }

        public DisplayBroadcastStation GetStationByCallLetterWithoutError(string stationCallLetters)
        {
            // Check station (Cadent Call Letters, a.k.a. legacy call letters)
            var station = _StationRepository.GetBroadcastStationByLegacyCallLetters(stationCallLetters);

            // If not found, check exact match in mappings
            if (station == null)
            {
                station = _StationMappingRepository.GetBroadcastStationByCallLetters(stationCallLetters);
            }

            // If not found, check starts with in mappings
            // If not found or multiple stations found, throw an error
            if (station == null)
            {               
               station = _StationMappingRepository.GetBroadcastStationStartingWithCallLetters(stationCallLetters);
            }

            //Insert into staion table
            if(station == null)
            {
                var createStation = new DisplayBroadcastStation
                {
                    CallLetters = stationCallLetters,
                    LegacyCallLetters = stationCallLetters,
                    ModifiedDate = DateTime.Now,
                    IsTrueInd = false
                };
                station = _StationRepository.CreateStation(createStation, "Inventory Ingest Process");
            }
            return station;
        }
    }
}
