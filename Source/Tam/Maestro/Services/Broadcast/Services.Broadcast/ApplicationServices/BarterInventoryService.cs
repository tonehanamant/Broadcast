using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IBarterInventoryService : IApplicationService
    {
        /// <summary>
        /// Saves a barter inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing a barter inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>InventoryFileSaveResult object</returns>
        InventoryFileSaveResult SaveBarterInventoryFile(InventoryFileSaveRequest request, string userName, DateTime now);
    }

    public class BarterInventoryService : IBarterInventoryService
    {
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IBarterRepository _BarterRepository;
        private readonly IBarterFileImporter _BarterFileImporter;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IStationRepository _StationRepository;
        private readonly ILockingEngine _LockingEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IInventoryDaypartParsingEngine _DaypartParsingEngine;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IStationInventoryGroupService _StationInventoryGroupService;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IImpressionsService _ImpressionsService;

        public BarterInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IBarterFileImporter barterFileImporter
            , IStationProcessingEngine stationProcessingEngine
            , ILockingEngine lockingEngine
            , ISpotLengthEngine spotLengthEngine
            , IInventoryDaypartParsingEngine inventoryDaypartParsingEngine
            , IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine
            , IStationInventoryGroupService stationInventoryGroupService
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IImpressionsService impressionsService)
        {
            _BarterRepository = broadcastDataRepositoryFactory.GetDataRepository<IBarterRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _BarterFileImporter = barterFileImporter;
            _StationProcessingEngine = stationProcessingEngine;
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _LockingEngine = lockingEngine;
            _SpotLengthEngine = spotLengthEngine;
            _DaypartParsingEngine = inventoryDaypartParsingEngine;
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _StationInventoryGroupService = stationInventoryGroupService;
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _ImpressionsService = impressionsService;
        }

        /// <summary>
        /// Saves a barter inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing a barter inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>InventoryFileSaveResult object</returns>
        public InventoryFileSaveResult SaveBarterInventoryFile(InventoryFileSaveRequest request, string userName, DateTime now)
        {
            if (!request.FileName.EndsWith(".xlsx"))
            {
                return new InventoryFileSaveResult
                {
                    Status = FileStatusEnum.Failed,
                    ValidationProblems = new List<string> { "Invalid file format. Please, provide a .xlsx File" }
                };
            }

            var stationLocks = new List<IDisposable>();
            var lockedStationIds = new List<int>();

            _BarterFileImporter.LoadFromSaveRequest(request);
            _BarterFileImporter.CheckFileHash();
            BarterInventoryFile barterFile = _BarterFileImporter.GetPendingBarterInventoryFile(userName);

            _CheckValidationProblems(barterFile);

            barterFile.Id = _InventoryFileRepository.CreateInventoryFile(barterFile, userName);
            try
            {
                _BarterFileImporter.ExtractData(barterFile);
                barterFile.FileStatus = barterFile.ValidationProblems.Any() ? FileStatusEnum.Failed : FileStatusEnum.Loaded;

                if (barterFile.ValidationProblems.Any())
                {
                    _BarterRepository.AddValidationProblems(barterFile);
                }
                else
                {
                    using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                    {
                        var stations = _GetFileStationsOrCreate(barterFile, userName);
                        var stationsDict = stations.ToDictionary(x => x.Id, x => x.LegacyCallLetters);
                        barterFile.InventoryGroups = _GetStationInventoryGroups(barterFile, stations);

                        _LockingEngine.LockStations(stationsDict, lockedStationIds, stationLocks);

                        var manifests = barterFile.InventoryGroups.SelectMany(x => x.Manifests);
                        _ImpressionsService.GetProjectedStationImpressions(manifests, barterFile.Header.PlaybackType, barterFile.Header.ShareBookId, barterFile.Header.HutBookId);
                        _ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests);

                        _StationInventoryGroupService.AddNewStationInventoryGroups(barterFile, barterFile.Header.EffectiveDate);

                        _StationRepository.UpdateStationList(stationsDict.Keys.ToList(), userName, now, barterFile.InventorySource.Id);

                        _BarterRepository.SaveBarterInventoryFile(barterFile);
                        transaction.Complete();

                        _LockingEngine.UnlockStations(lockedStationIds, stationLocks);
                    }
                }
            }
            catch (Exception ex)
            {
                _LockingEngine.UnlockStations(lockedStationIds, stationLocks);
                barterFile.ValidationProblems.Add(ex.Message);
                barterFile.FileStatus = FileStatusEnum.Failed;
                _BarterRepository.AddValidationProblems(barterFile);
            }

            _CheckValidationProblems(barterFile);

            return new InventoryFileSaveResult
            {
                FileId = barterFile.Id,
                ValidationProblems = barterFile.ValidationProblems,
                Status = barterFile.FileStatus
            };
        }

        private static void _CheckValidationProblems(BarterInventoryFile barterFile)
        {
            if (barterFile.ValidationProblems.Any())
            {
                var fileProblems = barterFile.ValidationProblems.Select(x => new InventoryFileProblem(x)).ToList();
                throw new Exceptions.FileUploadException<InventoryFileProblem>(fileProblems);
            }
        }

        private List<StationInventoryGroup> _GetStationInventoryGroups(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations)
        {
            var fileHeader = barterFile.Header;
            var stationCodes = stations.ToDictionary(x => x.LegacyCallLetters, x => x, StringComparer.OrdinalIgnoreCase);
            return barterFile.DataLines
                .SelectMany(x => x.Units, (dataLine, unit) => new
                {
                    dataLine,
                    unit.BarterInventoryUnit,
                    unit.Spots
                })
                .GroupBy(x => new { x.BarterInventoryUnit.Name, x.BarterInventoryUnit.SpotLength })
                .Select(groupingByUnit => new
                {
                    UnitName = groupingByUnit.Key.Name,
                    groupingByUnit.Key.SpotLength,
                    Manifests = groupingByUnit.Select(x => new
                    {
                        x.Spots,
                        x.dataLine.Station,
                        x.dataLine.Daypart,
                        x.dataLine.Comment
                    })
                })
                .Where(x => x.Manifests.Any(y => y.Spots != null))  //exclude empty manifest groups
                .Select(manifestGroup => new StationInventoryGroup
                {
                    Name = manifestGroup.UnitName,
                    DaypartCode = Regex.Match(manifestGroup.UnitName, @"[a-z]+", RegexOptions.IgnoreCase).Value,
                    InventorySource = barterFile.InventorySource,
                    StartDate = fileHeader.EffectiveDate,
                    EndDate = fileHeader.EndDate,
                    SlotNumber = _ParseSlotNumber(manifestGroup.UnitName),
                    Manifests = manifestGroup.Manifests
                        .Where(x => x.Spots != null) //exclude empty manifests
                        .Select(manifest => new StationInventoryManifest
                    {
                        EffectiveDate = fileHeader.EffectiveDate,
                        EndDate = fileHeader.EndDate,
                        InventorySourceId = barterFile.InventorySource.Id,
                        FileId = barterFile.Id,
                        Station = stationCodes[_StationProcessingEngine.StripStationSuffix(manifest.Station)],
                        SpotLengthId = _SpotLengthEngine.GetSpotLengthIdByValue(manifestGroup.SpotLength),
                        Comment = manifest.Comment,
                        ManifestWeeks = _GetManifestWeeksInRange(fileHeader.EffectiveDate, fileHeader.EndDate, manifest.Spots.Value),
                        ManifestDayparts = _ParseDayparts(manifest.Daypart),
                        ManifestAudiences = new List<StationInventoryManifestAudience>
                        {
                            new StationInventoryManifestAudience
                            {
                                Audience = new DisplayAudience { Id = fileHeader.AudienceId },
                                CPM = fileHeader.Cpm
                            }
                        }
                    }).ToList()
                }).ToList();
        }

        private int _ParseSlotNumber(string unitName)
        {
            var slotNumberString = Regex.Match(unitName, @"[0-9]+", RegexOptions.IgnoreCase).Value;
            return int.TryParse(slotNumberString, out var slotNumber) ? slotNumber : 0;
        }

        private List<StationInventoryManifestWeek> _GetManifestWeeksInRange(DateTime startDate, DateTime endDate, int spots)
        {
            var mediaWeeks = _MediaMonthAndWeekCache.GetMediaWeeksInRange(startDate, endDate);
            return mediaWeeks.Select(x => new StationInventoryManifestWeek { MediaWeek = x, Spots = spots }).ToList();
        }

        private List<StationInventoryManifestDaypart> _ParseDayparts(string daypartText)
        {
            var result = new List<StationInventoryManifestDaypart>();

            if (_DaypartParsingEngine.TryParse(daypartText, out var dayparts))
            {
                foreach (var daypart in dayparts)
                {
                    var manifestDaypart = new StationInventoryManifestDaypart
                    {
                        Daypart = daypart
                    };

                    result.Add(manifestDaypart);
                }
            }
            else
            {
                throw new Exception("Cannot parse dayparts");
            }

            return result;
        }

        private List<DisplayBroadcastStation> _GetFileStationsOrCreate(BarterInventoryFile barterFile, string userName)
        {
            var now = DateTime.Now;
            var allStationNames = barterFile.DataLines.Select(x => x.Station).Distinct();
            var allLegacyStationNames = allStationNames.Select(_StationProcessingEngine.StripStationSuffix).Distinct().ToList();
            var existingStations = _StationRepository.GetBroadcastStationListByLegacyCallLetters(allLegacyStationNames);
            var existingStationNames = existingStations.Select(x => x.LegacyCallLetters);
            var notExistingStations = allLegacyStationNames.Except(existingStationNames, StringComparer.OrdinalIgnoreCase).ToList();
            var stationsToCreate = new List<DisplayBroadcastStation>();

            foreach (var stationName in allStationNames)
            {
                var legacyStationName = _StationProcessingEngine.StripStationSuffix(stationName);

                if (notExistingStations.Contains(legacyStationName))
                {
                    stationsToCreate.Add(new DisplayBroadcastStation
                    {
                        CallLetters = stationName,
                        LegacyCallLetters = legacyStationName,
                        ModifiedDate = now
                    });
                }
            }

            var newStations = _StationRepository.CreateStations(stationsToCreate, userName);
            existingStations.AddRange(newStations);

            return existingStations;
        }
    }
}
