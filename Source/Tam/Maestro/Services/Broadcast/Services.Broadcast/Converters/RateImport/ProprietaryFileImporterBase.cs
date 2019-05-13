﻿using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IProprietaryFileImporter
    {
        void CheckFileHash();
        ProprietaryInventoryFile GetPendingProprietaryInventoryFile(string userName, InventorySource inventorySource);
        void ExtractData(ProprietaryInventoryFile proprietaryFile);
        void LoadFromSaveRequest(FileRequest request);
        void LoadAndValidateDataLines(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile);
        void PopulateManifests(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations);
    }

    public abstract class ProprietaryFileImporterBase : IProprietaryFileImporter
    {
        protected const string CPM_FORMAT = "##.##";
        protected readonly string[] BOOK_DATE_FORMATS = new string[] { "MMM yy", "MMM-yy", "MMM/yy", "yy-MMM", "yy/MMM", "MMM yyyy" };
        protected readonly string[] DATE_FORMATS = new string[] { "MM/dd/yyyy", "M/dd/yyyy", "M/d/yyyy" };

        private string _FileHash { get; set; }

        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryRepository _InventoryRepository;

        protected FileRequest Request { get; set; }

        protected readonly IBroadcastAudiencesCache AudienceCache;
        protected readonly IInventoryDaypartParsingEngine DaypartParsingEngine;
        protected readonly IMediaMonthAndWeekAggregateCache MediaMonthAndWeekAggregateCache;
        protected readonly IStationProcessingEngine StationProcessingEngine;
        protected readonly ISpotLengthEngine SpotLengthEngine;
        protected readonly IDaypartCodeRepository DaypartCodeRepository;

        public ProprietaryFileImporterBase(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IInventoryDaypartParsingEngine inventoryDaypartParsingEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IStationProcessingEngine stationProcessingEngine,
            ISpotLengthEngine spotLengthEngine)
        {
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            DaypartCodeRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartCodeRepository>();
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

        public void ExtractData(ProprietaryInventoryFile proprietaryFile)
        {
            using (var package = new ExcelPackage(Request.StreamData))
            {
                var worksheet = package.Workbook.Worksheets.First();
                LoadAndValidateHeaderData(worksheet, proprietaryFile);
                LoadAndValidateDataLines(worksheet, proprietaryFile);
            }
        }

        protected List<StationInventoryManifestWeek> GetManifestWeeksInRange(DateTime startDate, DateTime endDate, int spots)
        {
            var mediaWeeks = MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(startDate, endDate);
            return mediaWeeks.Select(x => new StationInventoryManifestWeek { MediaWeek = x, Spots = spots }).ToList();
        }

        protected abstract void LoadAndValidateHeaderData(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile);

        public abstract void LoadAndValidateDataLines(ExcelWorksheet worksheet, ProprietaryInventoryFile proprietaryFile);

        public abstract void PopulateManifests(ProprietaryInventoryFile proprietaryFile, List<DisplayBroadcastStation> stations);
    }
}