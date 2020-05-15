using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProgramMappingService : IApplicationService
    {
        /// <summary>
        /// Loads the program mappings, and hands it off to background job.
        /// </summary>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>The background jobs Id</returns>
        string LoadProgramMappings(Stream fileStream, string fileName, string userName, DateTime createdDate);

        /// <summary>
        /// Processes the program mappings file.
        /// </summary>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        [Queue("programmappings")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void RunProgramMappingsProcessingJob(Guid fileId, string userName, DateTime createdDate);
    }

    public class ProgramMappingService : BroadcastBaseClass, IProgramMappingService
    {
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IProgramMappingRepository _ProgramMappingRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IShowTypeRepository _ShowTypeRepository;
        private readonly IGenreRepository _GenreRepository;
        private readonly ISharedFolderService _SharedFolderService;

        public ProgramMappingService(IBackgroundJobClient backgroundJobClient,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISharedFolderService sharedFolderService)
        {
            _BackgroundJobClient = backgroundJobClient;
            _ProgramMappingRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ShowTypeRepository = broadcastDataRepositoryFactory.GetDataRepository<IShowTypeRepository>();
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _SharedFolderService = sharedFolderService;
        }

        /// <inheritdoc />
        public string LoadProgramMappings(Stream fileStream, string fileName, string userName, DateTime createdDate)
        {
            var fileId = _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = _GetProgramMappingsDirectoryPath(),
                FileNameWithExtension = fileName,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.CampaignExport,
                CreatedDate = createdDate,
                CreatedBy = userName,
                FileContent = fileStream
            });

            // Hand off to a background job
            var hangfireJobId = _BackgroundJobClient.Enqueue<IProgramMappingService>(x => x.RunProgramMappingsProcessingJob(fileId, userName, createdDate));
            return hangfireJobId;
        }

        public void RunProgramMappingsProcessingJob(Guid fileId, string userName, DateTime createdDate)
        {
            var durationSw = new Stopwatch();
            durationSw.Start();
            var file = _SharedFolderService.GetFile(fileId);
            var ingestedRecordsCount = 0;
            var updatedInventoryCount = 0;

            _LogInfo($"Started processing the program mapping file {file.FileNameWithExtension}");

            var programMappings = _ReadProgramMappingsFile(file.FileContent);
            _LogInfo($"The selected program mapping file has {programMappings.Count} rows");

            _ProcessProgramMappings(programMappings, createdDate, ref updatedInventoryCount, ref ingestedRecordsCount);

            _SharedFolderService.RemoveFile(fileId);

            durationSw.Stop();
            _LogInfo($"Processing of the program mapping file {file.FileNameWithExtension}, finished successfully in {durationSw.ElapsedMilliseconds} ms. Ingested {ingestedRecordsCount} records, updated {updatedInventoryCount} inventory.");
                
        }

        protected void _ProcessProgramMappings(
            List<ProgramMappingsFileRequestDto> programMappings,
            DateTime createdDate,
            ref int updatedInventoryCount, ref int ingestedRecordsCount)
        {
            foreach (var mapping in programMappings)
            {
                using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(30)))
                {
                    if (_ProgramMappingRepository.MappingExistsForOriginalProgramName(mapping.OriginalProgramName))
                    {
                        var existingMapping = _ProgramMappingRepository.GetProgramMappingByOriginalProgramName(mapping.OriginalProgramName);
                        if (existingMapping.OfficialProgramName != mapping.OfficialProgramName ||
                            existingMapping.OfficialGenre.Name != mapping.OfficialGenre ||
                            existingMapping.OfficialShowType.Name != mapping.OfficialShowType)
                        {
                            // There are changes for an existing mapping
                            existingMapping.OfficialProgramName = mapping.OfficialProgramName;
                            existingMapping.OfficialGenre = _GenreRepository.GetGenreByName(mapping.OfficialGenre, GenreSourceEnum.RedBee);
                            existingMapping.OfficialShowType = _ShowTypeRepository.GetShowTypeByName(mapping.OfficialShowType);
                            _ProgramMappingRepository.UpdateProgramMapping(existingMapping);
                            _UpdateInventoryWithEnrichedProgramName(existingMapping, mapping, createdDate, ref updatedInventoryCount);
                            ingestedRecordsCount++;
                        }
                    }
                    else
                    {
                        var newProgramMapping = new ProgramMappingsDto
                        {
                            OriginalProgramName = mapping.OriginalProgramName,
                            OfficialProgramName = mapping.OfficialProgramName,
                            OfficialGenre = _GenreRepository.GetGenreByName(mapping.OfficialGenre, GenreSourceEnum.RedBee),
                            OfficialShowType = _ShowTypeRepository.GetShowTypeByName(mapping.OfficialShowType)
                        };
                        _ProgramMappingRepository.CreateProgramMapping(newProgramMapping);
                        _UpdateInventoryWithEnrichedProgramName(newProgramMapping, mapping, createdDate, ref updatedInventoryCount);
                        ingestedRecordsCount++;
                    }
                    transaction.Complete();
                }
            }
        }

        private void _UpdateInventoryWithEnrichedProgramName(ProgramMappingsDto programMapping, ProgramMappingsFileRequestDto mapping, DateTime createdDate, ref int updatedInventoryCount)
        {
            var durationSw = new Stopwatch();
            durationSw.Start();

            var maestroGenre = _GenreRepository.GetGenreByName(mapping.OfficialGenre, GenreSourceEnum.Maestro);
            var sourceGenre = _GenreRepository.GetGenreByName(mapping.OfficialGenre, GenreSourceEnum.RedBee);

            // Get all StationInventoryManifestDaypart's with ProgramName
            var manifestDayparts = _InventoryRepository.GetManifestDaypartsForProgramName(programMapping.OriginalProgramName);
            foreach (var daypart in manifestDayparts)
            {
                var programWeeksDateRange = _InventoryRepository.GetStationInventoryManifesDaypartWeeksDateRange(daypart.Id.Value);

                // Get all StationInventoryManifestDaypartProgram for these
                var manifestDaypartPrograms = _InventoryRepository.GetDaypartProgramsForInventoryDayparts(new List<int> { daypart.Id.Value });

                // Remove the old programs
                var manifestDaypartIds = manifestDaypartPrograms.Select(x => x.StationInventoryManifestDaypartId).Distinct().ToList();
                if (!manifestDaypartIds.IsEmpty())
                {
                    _InventoryRepository.DeleteInventoryPrograms(manifestDaypartIds);
                }

                // Create the new StationInventoryManifestDaypartProgram
                var newManifestDaypartPrograms = new List<StationInventoryManifestDaypartProgram>
                {
                    new StationInventoryManifestDaypartProgram
                    {
                        StationInventoryManifestDaypartId = daypart.Id.Value,
                        ProgramName = programMapping.OfficialProgramName,
                        GenreSourceId = (int)GenreSourceEnum.RedBee,
                        MaestroGenreId = maestroGenre.Id,
                        SourceGenreId = sourceGenre.Id,
                        ShowType = mapping.OfficialShowType,
                        StartTime = daypart.Daypart.StartTime,
                        EndTime = daypart.Daypart.EndTime,
                        StartDate = programWeeksDateRange.Start.Value,
                        EndDate = programWeeksDateRange.End.Value,
                        CreatedDate = createdDate
                    }
                };
                _InventoryRepository.CreateInventoryPrograms(newManifestDaypartPrograms, createdDate);
                updatedInventoryCount++;
            };

            durationSw.Stop();
            _LogInfo($"Updating inventory for program with {programMapping.OriginalProgramName}, finished successfully in {durationSw.ElapsedMilliseconds} ms.");
        }

        private List<ProgramMappingsFileRequestDto> _ReadProgramMappingsFile(Stream stream)
        {
            var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[1];
            var programMappings = worksheet.ConvertSheetToObjects<ProgramMappingsFileRequestDto>();

            return programMappings
                // Ignore blank rows
                .Where(mapping => !string.IsNullOrEmpty(mapping.OriginalProgramName) && !string.IsNullOrEmpty(mapping.OfficialProgramName))
                .ToList();
        }

        protected string _GetProgramMappingsDirectoryPath()
        {
            const string dirName = "ProgramMappings";
            return Directory.CreateDirectory(Path.Combine(BroadcastServiceSystemParameter.BroadcastSharedFolder, dirName)).FullName;
        }
    }
}
