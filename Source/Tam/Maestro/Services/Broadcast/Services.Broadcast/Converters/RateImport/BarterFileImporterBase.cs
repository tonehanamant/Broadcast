using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IBarterFileImporter
    {
        void CheckFileHash();
        BarterInventoryFile GetPendingBarterInventoryFile(string userName, InventorySource inventorySource);
        void ExtractData(BarterInventoryFile barterFile);
        void LoadFromSaveRequest(InventoryFileSaveRequest request);
        void LoadAndValidateDataLines(ExcelWorksheet worksheet, BarterInventoryFile barterFile);
        void PopulateManifests(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations);
    }

    public abstract class BarterFileImporterBase : IBarterFileImporter
    {
        protected const string CPM_FORMAT = "##.##";
        protected readonly string[] BOOK_DATE_FORMATS = new string[] { "MMM yy", "MMM-yy", "MMM/yy", "yy-MMM", "yy/MMM" };
        protected readonly string[] DATE_FORMATS = new string[] { "MM/dd/yyyy", "M/dd/yyyy" };

        private string _FileHash { get; set; }

        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryRepository _InventoryRepository;

        protected InventoryFileSaveRequest Request { get; set; }

        protected readonly IBroadcastAudiencesCache AudienceCache;
        protected readonly IInventoryDaypartParsingEngine DaypartParsingEngine;
        protected readonly IMediaMonthAndWeekAggregateCache MediaMonthAndWeekAggregateCache;
        protected readonly IStationProcessingEngine StationProcessingEngine;
        protected readonly ISpotLengthEngine SpotLengthEngine;

        public BarterFileImporterBase(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine)
        {
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            AudienceCache = broadcastAudiencesCache;
            DaypartParsingEngine = inventoryDaypartParsingEngine;
            MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            StationProcessingEngine = stationProcessingEngine;
            SpotLengthEngine = spotLengthEngine;
        }

        public void CheckFileHash()
        {
            //check if file has already been loaded
            if (_InventoryFileRepository.GetInventoryFileIdByHash(_FileHash) > 0)
            {
                throw new BroadcastDuplicateInventoryFileException(
                    "Unable to load file. The selected file has already been loaded or is already loading.");
            }
        }

        public void LoadFromSaveRequest(InventoryFileSaveRequest request)
        {
            Request = request;
            _FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(request.StreamData));
        }

        public BarterInventoryFile GetPendingBarterInventoryFile(string userName, InventorySource inventorySource)
        {
            return new BarterInventoryFile
            {
                FileName = Request.FileName ?? "unknown",
                FileStatus = FileStatusEnum.Pending,
                Hash = _FileHash,
                CreatedBy = userName,
                CreatedDate = DateTime.Now,
                InventorySource = inventorySource
            };
        }

        public void ExtractData(BarterInventoryFile barterFile)
        {
            using (var package = new ExcelPackage(Request.StreamData))
            {
                var worksheet = package.Workbook.Worksheets.First();
                LoadAndValidateHeaderData(worksheet, barterFile);
                LoadAndValidateDataLines(worksheet, barterFile);
            }
        }

        protected abstract void LoadAndValidateHeaderData(ExcelWorksheet worksheet, BarterInventoryFile barterFile);

        public abstract void LoadAndValidateDataLines(ExcelWorksheet worksheet, BarterInventoryFile barterFile);

        public abstract void PopulateManifests(BarterInventoryFile barterFile, List<DisplayBroadcastStation> stations);
    }
}
