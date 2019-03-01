using Common.Services;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public abstract class InventoryFileImporterBase
    {
        private readonly string _validTimeExpression =
            @"(^([0-9]|[0-1][0-9]|[2][0-3])(:)?([0-5][0-9])(\s{0,1})(A|AM|P|PM|a|am|p|pm|aM|Am|pM|Pm{2,2})$)|(^([0-9]|[1][0-9]|[2][0-3])(\s{0,1})(A|AM|P|PM|a|am|p|pm|aM|Am|pM|Pm{2,2})$)";

        protected BroadcastDataDataRepositoryFactory _BroadcastDataRepositoryFactory;
        protected IDaypartCache _DaypartCache;
        protected IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        protected IBroadcastAudiencesCache _AudiencesCache;
        protected Dictionary<int, double> _SpotLengthMultipliers;

        private Dictionary<int, int> _SpotLengths;

        public List<InventoryFileProblem> FileProblems { get; set; } = new List<InventoryFileProblem>();
        public InventoryFileSaveRequest Request { get; private set; }

        public BroadcastDataDataRepositoryFactory BroadcastDataDataRepository
        {
            set { _BroadcastDataRepositoryFactory = value; }
        }

        public IDaypartCache DaypartCache
        {
            set { _DaypartCache = value; }
        }

        public IMediaMonthAndWeekAggregateCache MediaMonthAndWeekAggregateCache
        {
            set { _MediaMonthAndWeekAggregateCache = value; }
        }

        public IBroadcastAudiencesCache AudiencesCache
        {
            set { _AudiencesCache = value; }
        }

        public IInventoryDaypartParsingEngine InventoryDaypartParsingEngine { get; set; }

        /// <summary>
        /// Spot lengths dictionary where key is the length and value is the id
        /// </summary>
        public Dictionary<int, int> SpotLengths
        {
            get
            {
                if (_SpotLengths == null)
                {
                    _SpotLengths = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
                }
                return _SpotLengths;
            }
        }

        public void CheckFileHash()
        {
            //check if file has already been loaded
            if (_BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>()
                    .GetInventoryFileIdByHash(FileHash) > 0)
            {
                throw new BroadcastDuplicateInventoryFileException(
                    "Unable to load rate file. The selected rate file has already been loaded or is already loading.");
            }
        }

        public void LoadFromSaveRequest(InventoryFileSaveRequest request)
        {
            Request = request;
            FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(request.StreamData));
            _SpotLengthMultipliers = _GetSpotLengthAndMultipliers();
        }

        public string FileHash { get; private set; }

        public InventorySource InventorySource { get; set; }

        public InventoryFile GetPendingInventoryFile()
        {
            var result = new InventoryFile();
            return HydrateInventoryFile(result);
        }

        private DisplayBroadcastStation _GetDisplayBroadcastStation(string stationName)
        {
            var _stationRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            return _stationRepository.GetBroadcastStationByLegacyCallLetters(stationName) ??
                   _stationRepository.GetBroadcastStationByCallLetters(stationName);
        }

        protected virtual DisplayBroadcastStation ParseStationCallLetters(string stationName)
        {
            stationName = stationName.Replace("-TV", "").Trim();
            return _GetDisplayBroadcastStation(stationName);
        }

        protected Dictionary<string, DisplayBroadcastStation> FindStations(List<string> stationNameList)
        {
            var foundStations = new Dictionary<string, DisplayBroadcastStation>();

            foreach (var stationName in stationNameList)
            {
                var station = ParseStationCallLetters(stationName);

                if (station != null)
                {
                    foundStations.Add(stationName, station);
                }
            }

            return foundStations;
        }

        protected InventoryFile HydrateInventoryFile(InventoryFile inventoryFileToHydrate)
        {
            inventoryFileToHydrate.FileName = Request.FileName == null ? "unknown" : Request.FileName;
            inventoryFileToHydrate.FileStatus = FileStatusEnum.Pending;
            inventoryFileToHydrate.Hash = FileHash;
            inventoryFileToHydrate.InventorySource = InventorySource;
            return inventoryFileToHydrate;
        }

        public abstract void ExtractFileData(
            System.IO.Stream stream,
            InventoryFile inventoryFile,
            DateTime effectiveDate);

        private Dictionary<int, double> _GetSpotLengthAndMultipliers()
        {
            // load spot lenght ids and multipliers
            var spotMultipliers = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsAndCostMultipliers();

            return (from c in SpotLengths
                    join d in spotMultipliers on c.Value equals d.Key
                    select new { c.Key, d.Value }).ToDictionary(x => x.Key, y => y.Value);

        }

        protected List<StationInventoryManifestRate> _GetManifestRatesFromMultipliers(decimal periodRate)
        {
            var manifestRates = new List<StationInventoryManifestRate>();

            foreach (var spotLength in SpotLengths)
            {
                var manifestRate = new StationInventoryManifestRate
                {
                    SpotLengthId = spotLength.Value,
                    SpotCost = periodRate * (decimal)_SpotLengthMultipliers[spotLength.Key]
                };
                manifestRates.Add(manifestRate);
            }

            return manifestRates;
        }

        protected List<DisplayDaypart> ParseDayparts(string daypartText, string station)
        {
            if (InventoryDaypartParsingEngine.TryParse(daypartText, out var displayDayparts) && displayDayparts.Any() && displayDayparts.All(x => x.IsValid))

            {
                return displayDayparts;
            }

            AddProblem($"Invalid daypart '{daypartText}' for station: {station}");
            return new List<DisplayDaypart>();
        }

        protected void AddProblem(string description, string stationLetters = null)
        {
            FileProblems.Add(new InventoryFileProblem()
            {
                ProblemDescription = description,
                StationLetters = stationLetters
            });
        }
    }
}
