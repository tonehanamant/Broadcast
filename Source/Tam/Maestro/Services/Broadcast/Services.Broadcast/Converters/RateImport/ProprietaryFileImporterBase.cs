using Common.Services;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IProprietaryFileImporter : IBaseInventoryFileImporter
    {
        /// <summary>
        /// Check if the file was already processed by using file hash
        /// </summary>
        void CheckFileHash();

        ProprietaryInventoryFile GetPendingProprietaryInventoryFile(string userName, InventorySource inventorySource);

        /// <summary>
        /// Extracts the data in the request stream and loads the proprietary inventory file
        /// </summary>
        /// <param name="proprietaryFile">Proprietary inventory file object to load</param>
        /// <returns>Stream containing the file and the validation errors if there are any</returns>
        Stream ExtractData(ProprietaryInventoryFile proprietaryFile);

        void LoadFromSaveRequest(FileRequest request);

        /// <summary>
        /// Load and validate data lines
        /// </summary>
        /// <param name="worksheet">Excel worksheet to process</param>
        /// <param name="proprietaryFile">ProprietaryInventoryFile to be loaded</param>
        void LoadAndValidateDataLines(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile);

        /// <summary>
        /// Load and validate header lines
        /// </summary>
        /// <param name="worksheet">Excel worksheet to process</param>
        /// <param name="proprietaryFile">ProprietaryInventoryFile to be loaded</param>
        void LoadAndValidateHeaderData(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile);

        void PopulateManifests(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations);
    }

    public abstract class ProprietaryFileImporterBase : BaseInventoryFileImporter, IProprietaryFileImporter
    {
        protected const string CPM_FORMAT = "##.##";
        protected readonly string[] BOOK_DATE_FORMATS = new string[] { "MMM yy", "MMM-yy", "MMM/yy", "yy-MMM", "yy/MMM", "MMM yyyy" };
        protected readonly string[] DATE_FORMATS = new string[] { "MM/dd/yyyy", "M/dd/yyyy", "M/d/yyyy" };
        protected const string HEADER_ERROR_COLUMN = "E";
        protected const string FILE_MULTIPLE_ERRORS_SEPARATOR = "; ";
        private string _FileHash { get; set; }

        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryRepository _InventoryRepository;

        protected FileRequest Request { get; set; }

        protected readonly IBroadcastAudiencesCache AudienceCache;
        protected readonly IInventoryDaypartParsingEngine DaypartParsingEngine;
        protected readonly IMediaMonthAndWeekAggregateCache MediaMonthAndWeekAggregateCache;
        protected readonly IStationProcessingEngine StationProcessingEngine;
        protected readonly ISpotLengthEngine SpotLengthEngine;
        protected readonly IDaypartDefaultRepository DaypartDefaultRepository;
        protected readonly IStationMappingService _StationMappingService;
        private readonly IFileService _FileService;


        /// <summary>
        /// Indicates that a second worksheet needs to be processed.
        /// Override in your base class and return true to opt-in.
        /// </summary>
        public virtual bool HasSecondWorksheet { get; } = false;
        /// <summary>
        /// Constructor
        /// </summary>        
        public ProprietaryFileImporterBase(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine,
            IFileService fileService,
            IStationMappingService stationMappingService)
        {
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
            AudienceCache = broadcastAudiencesCache;
            DaypartParsingEngine = inventoryDaypartParsingEngine;
            MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            StationProcessingEngine = stationProcessingEngine;
            SpotLengthEngine = spotLengthEngine;
            _FileService = fileService;
            _StationMappingService = stationMappingService;
        }

        /// <summary>
        /// Check if the file was already processed by using file hash
        /// </summary>
        public void CheckFileHash()
        {
            //check if file has already been loaded
            if (_InventoryFileRepository.GetInventoryFileIdByHash(_FileHash) > 0)
            {
                throw new BroadcastDuplicateInventoryFileException(
                    "Unable to load file. The selected file has already been loaded or is already loading.");
            }
        }

        public void LoadFromSaveRequest(FileRequest request)
        {
            Request = request;
            _FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(request.StreamData));
        }

        public ProprietaryInventoryFile GetPendingProprietaryInventoryFile(string userName, InventorySource inventorySource)
        {
            return new ProprietaryInventoryFile
            {
                FileName = Request.FileName ?? "unknown",
                FileStatus = FileStatusEnum.Pending,
                Hash = _FileHash,
                CreatedBy = userName,
                CreatedDate = DateTime.Now,
                InventorySource = inventorySource
            };
        }

        /// <summary>
        /// Extracts the data in the request stream and loads the proprietary inventory file
        /// </summary>
        /// <param name="proprietaryFile">Proprietary inventory file object to load</param>
        /// <returns>Stream containing the file and the validation errors if there are any</returns>
        public Stream ExtractData(ProprietaryInventoryFile proprietaryFile)
        {
            var result = new MemoryStream();
            using (var package = new ExcelPackage(Request.StreamData))
            {
                var worksheet = package.Workbook.Worksheets.First();
                LoadAndValidateHeaderData(worksheet, proprietaryFile);
                if (HasSecondWorksheet)
                {
                    if (package.Workbook.Worksheets.Count > 1)
                    {
                        // worksheets are 1-indexed.
                        LoadAndValidateSecondWorksheet(package.Workbook.Worksheets[2], proprietaryFile);
                    }
                    else
                    {
                        throw new BroadcastDuplicateInventoryFileException($"Unable to load file. Expected a second worksheet that is not in the file.");
                    }
                }
                
                LoadAndValidateDataLines(worksheet, proprietaryFile);

                package.SaveAs(result);
                result.Position = 0;
            }
            return result;
        }

        public virtual List<DisplayBroadcastStation> GetFileStations(ProprietaryInventoryFile proprietaryFile)
        {
            var allStationNames = proprietaryFile.DataLines.Select(x => x.Station).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
            var stationsList = new List<DisplayBroadcastStation>();

            foreach (var stationName in allStationNames)
            {
                var station = _StationMappingService.GetStationByCallLetters(stationName);
                stationsList.Add(station);
            }

            return stationsList;
        }

        protected List<StationInventoryManifestWeek> GetManifestWeeksInRange(DateTime startDate, DateTime endDate, int spots)
        {
            var mediaWeeks = MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(startDate, endDate);
            return mediaWeeks.Select(x => new StationInventoryManifestWeek { MediaWeek = x, Spots = spots }).ToList();
        }

        public virtual void LoadAndValidateSecondWorksheet(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile)
        {
            // implement as needed.
        }

        /// <summary>
        /// Load and validate header data
        /// </summary>
        /// <param name="worksheet">Excel worksheet to process</param>
        /// <param name="proprietaryFile">ProprietaryInventoryFile object to be loaded</param>
        public abstract void LoadAndValidateHeaderData(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile);

        /// <summary>
        /// Load and validate data lines
        /// </summary>
        /// <param name="worksheet">Excel worksheet to process</param>
        /// <param name="proprietaryFile">ProprietaryInventoryFile to be loaded</param>
        public abstract void LoadAndValidateDataLines(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile);

        public abstract void PopulateManifests(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations);
    }
}
