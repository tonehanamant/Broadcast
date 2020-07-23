using Common.Services;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines.InventoryProgramsProcessing;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity;

namespace Services.Broadcast.IntegrationTests.Helpers
{
    public class InventoryFileTestHelper
    {
        private readonly IInventoryRatingsProcessingService _InventoryRatingsProcessingService;
        private readonly IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;
        private readonly IProprietaryInventoryService _ProprietaryInventoryService;
        private readonly IInventoryService _InventoryService;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;
        private readonly IInventoryProgramsProcessingService _InventoryProgramsProcessingService;
        private IInventoryRepository _InventoryRepository;

        public InventoryFileTestHelper()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IFileService, FileServiceStub>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IProgramGuideApiClient, ProgramGuideApiClientStub>();
            _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
            _InventoryFileRatingsJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
            _InventoryProgramsByFileJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
            _ProprietaryInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>();
            _InventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();
            _InventoryProgramsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
            _InventoryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        public void UploadProprietaryInventoryFile(
            string fileName, 
            DateTime? date = null, 
            bool processInventoryRatings = true,
            bool processInventoryProgramsJob = false)
        {
            var request = new FileRequest
            {
                StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                FileName = fileName
            };

            var now = date ?? new DateTime(2019, 02, 02);
            _ProprietaryInventoryService.SaveProprietaryInventoryFile(request, "IntegrationTestUser", now);

            if (processInventoryRatings)
            {
                var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);
            }

            if (processInventoryProgramsJob)
            {
                var job = _InventoryProgramsByFileJobsRepository.GetLatestJob();
                _InventoryProgramsProcessingService.ProcessInventoryProgramsByFileJob(job.Id);
            }
        }

        public int UploadOpenMarketInventoryFile(string fileName, DateTime? date = null, bool enhanceFilePrograms = false)
        {
            var request = new InventoryFileSaveRequest
            {
                StreamData = new FileStream($@".\Files\ImportingRateData\{fileName}", FileMode.Open, FileAccess.Read),
                FileName = fileName
            };

            var now = date ?? new DateTime(2019, 02, 02);

            var result = _InventoryService.SaveInventoryFile(request, "IntegrationTestUser", now);

            var job = _InventoryFileRatingsJobsRepository.GetLatestJob();
            var source = _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(job.Id);

            return result.FileId;
        }

        public void EnhanceProgramsForFileId(int fileId, List<string> enhancements)
        {
            var csvFileContents = new StringBuilder();
            var headerFields = InventoryProgramsProcessingEngineBase.GetProgramsImportFileHeaderFields();
            csvFileContents.AppendLine(string.Join(",", headerFields));
            var programs = _InventoryRepository.GetInventoryByFileIdForProgramsProcessing(fileId);

            foreach (var program in programs)
            {
                foreach (var week in program.ManifestWeeks)
                {
                    foreach (var daypart in program.ManifestDayparts)
                    {
                        csvFileContents.AppendLine($"{program.Id}, " +
                            $"{week.Id}, " +
                            $"{daypart.Id}, " +
                            $"{program.Station.CallLetters}, " +
                            $"{program.Station.Affiliation}, " +
                            $"{week.StartDate}, " +
                            $"{week.EndDate}, " +
                            $"{daypart.Daypart.ToString()}," +
                            $"{daypart.Daypart.Monday}," +
                            $"{daypart.Daypart.Tuesday}," +
                            $"{daypart.Daypart.Wednesday}," +
                            $"{daypart.Daypart.Thursday}," +
                            $"{daypart.Daypart.Friday}," +
                            $"{daypart.Daypart.Saturday}," +
                            $"{daypart.Daypart.Sunday}," +
                            $"{daypart.Daypart.StartTime}," +
                            $"{daypart.Daypart.EndTime}," +
                            $"{daypart.ProgramName}," +
                            $"{enhancements[0]}," +
                            $"{enhancements[1]}," +
                            $"{enhancements[2]}," +
                            $"{enhancements[3]}," +
                            $"{enhancements[4]}," +
                            $"{enhancements[5]}");
                    }
                }
            }

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            streamWriter.Write(csvFileContents.ToString());

            _InventoryProgramsProcessingService.ImportInventoryProgramsResults(memoryStream, "OpenMarket.csv");
        }
    }
}
