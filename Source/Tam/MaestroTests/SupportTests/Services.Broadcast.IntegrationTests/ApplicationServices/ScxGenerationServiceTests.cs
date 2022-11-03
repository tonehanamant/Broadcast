using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public class ScxGenerationServiceTests
    {
        private readonly IScxGenerationService _ScxGenerationService;
        private readonly IScxGenerationJobRepository _ScxGenerationJobRepository;
        private readonly InMemoryFileServiceStubb _FileService;
        private readonly JsonSerializerSettings _JsonSettings;
        private readonly IInventoryRatingsProcessingService _InventoryRatingsProcessingService;
        private readonly IInventoryFileRatingsJobsRepository _InventoryFileRatingsJobsRepository;

        public ScxGenerationServiceTests()
        {
            _FileService = new InMemoryFileServiceStubb();

            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(_FileService);
            
            _InventoryRatingsProcessingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
            _InventoryFileRatingsJobsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRatingsJobsRepository>();
            _ScxGenerationService = IntegrationTestApplicationServiceFactory.GetApplicationService<IScxGenerationService>();
            _ScxGenerationJobRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IScxGenerationJobRepository>();
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ScxGenerationJob), "Id");
            jsonResolver.Ignore(typeof(ScxFileGenerationDetail), "FullFilePath");
            _JsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ScxGenerationQueueJobTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    StandardDaypartId = 1,
                    InventorySourceId = 1,
                    StartDate = new DateTime(2019, 3, 1),
                    EndDate = new DateTime(2019, 3, 15),
                    UnitNames = new List<string> { "EM 1 " }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetJobById(jobId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(job, _JsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ScxGenerationProcessJobTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    StandardDaypartId = 1,
                    InventorySourceId = 7,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetJobById(jobId);

                _ScxGenerationService.ProcessScxGenerationJob(job, new DateTime(2019, 7, 11));

                var jobAfterProcessing = _ScxGenerationJobRepository.GetJobById(jobId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobAfterProcessing, _JsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ScxGenerationProcessJobFileResultTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    StandardDaypartId = 1,
                    InventorySourceId = 7,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetJobById(jobId);

                _ScxGenerationService.ProcessScxGenerationJob(job, new DateTime(2019, 7, 11));

                var jobAfterProcessing = _ScxGenerationJobRepository.GetJobById(jobId);

                var paths = _FileService.Paths;
                var streams = _FileService.Streams;

                Assert.NotNull(paths.Count > 0 );
                Assert.NotNull(streams.Count > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ScxGenerationGetQueuedJobsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    StandardDaypartId = 1,
                    InventorySourceId = 7,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var jobs = _ScxGenerationService.GetQueuedJobs(5);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobs, _JsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ScxGenerationGetHistoryTest()
        {
            const int inventorySourceId = 5;
            using (new TransactionScopeWrapper())
            {
                string fileName = "Barter_A25-54_Q1 CNN.xlsx";
                var fileSaveRequest = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(fileSaveRequest, "sroibu", now);

                var ratingsJob = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(ratingsJob.Id);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 1, 1),
                    StartDate = new DateTime(2018, 3, 20),
                    StandardDaypartId = 2,
                    InventorySourceId = inventorySourceId,
                    UnitNames = new List<string> { "Unit 1" }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", now);
                var scxGenerationJob = _ScxGenerationJobRepository.GetJobById(jobId);
                _ScxGenerationService.ProcessScxGenerationJob(scxGenerationJob, now);

                var history = _ScxGenerationService.GetScxFileGenerationHistory(inventorySourceId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(history, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ScxGenerationGetHistoryTestWithFailedFileGeneration()
        {
            const int inventorySourceId = 5;
            using (new TransactionScopeWrapper())
            {
                var failingFileService = new FailingFileServiceStub();
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IFileService>(failingFileService);
                var scxGenerationService = IntegrationTestApplicationServiceFactory.GetApplicationService<IScxGenerationService>();

                string fileName = "Barter_A25-54_Q1 CNN.xlsx";
                var fileSaveRequest = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(fileSaveRequest, "sroibu", now);

                var ratingsJob = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(ratingsJob.Id);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 1, 1),
                    StartDate = new DateTime(2018, 3, 20),
                    StandardDaypartId = 2,
                    InventorySourceId = inventorySourceId,
                    UnitNames = new List<string> { "Unit 1" }
                };

                var jobId = scxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", now);
                var scxGenerationJob = _ScxGenerationJobRepository.GetJobById(jobId);

                try
                {
                    scxGenerationService.ProcessScxGenerationJob(scxGenerationJob, now);
                }
                catch (Exception ex)
                {
                    Assert.That(ex.Message, Is.EqualTo("FailingFileServiceStub never creates files"));
                }

                var history = _ScxGenerationService.GetScxFileGenerationHistory(inventorySourceId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(history, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ScxGenerationDownloadFileTest()
        {
            const int inventorySourceId = 5;
            using (new TransactionScopeWrapper())
            {
                string fileName = "Barter_A25-54_Q1 CNN.xlsx";
                var fileSaveRequest = new FileRequest
                {
                    StreamData = new FileStream($@".\Files\ProprietaryDataFiles\{fileName}", FileMode.Open, FileAccess.Read),
                    FileName = fileName
                };

                var now = new DateTime(2019, 02, 02);
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(fileSaveRequest, "sroibu", now);

                var ratingsJob = _InventoryFileRatingsJobsRepository.GetLatestJob();
                _InventoryRatingsProcessingService.ProcessInventoryRatingsJob(ratingsJob.Id);

                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 1, 1),
                    StartDate = new DateTime(2018, 3, 20),
                    StandardDaypartId = 2,
                    InventorySourceId = inventorySourceId,
                    UnitNames = new List<string> { "Unit 1" }
                };
                
                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", now);
                var job = _ScxGenerationJobRepository.GetJobById(jobId);
                _ScxGenerationService.ProcessScxGenerationJob(job, now);
                var history = _ScxGenerationService.GetScxFileGenerationHistory(inventorySourceId);
                var fileId = history.First().FileId;

                var result = _ScxGenerationService.DownloadGeneratedScxFile(fileId.Value);

                Assert.AreEqual("CNN_Unit 1_20180319_20190106.scx", result.Item1);
                Assert.IsTrue(result.Item2.Length > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void ScxGenerationGetHistoryWithQueuedJobsTest()
        {
            const int inventorySourceId = 7;
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    StandardDaypartId = 1,
                    InventorySourceId = inventorySourceId,
                    UnitNames = new List<string> { "ExpiresGroupsTest" }
                };

                var jobId = _ScxGenerationService.QueueScxGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var history = _ScxGenerationService.GetScxFileGenerationHistory(inventorySourceId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(history, _JsonSettings));
            }
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(ScxFileGenerationDetail), "FileId");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void OpenMarketScxGenerationQueueJobTest()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxOpenMarketsDownloadRequest
                {
                    StandardDaypartId = 1,
                    StartDate = new DateTime(2019, 3, 1),
                    EndDate = new DateTime(2019, 3, 15),
                    Affiliates = new List<string> { "ABC","NBC"},
                    GenreType = OpenMarketInventoryExportGenreTypeEnum.News,
                    MarketCode = 390
                };
                
                var jobId = _ScxGenerationService.QueueScxOpenMarketsGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetOpenMarketsJobById(jobId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(job, _GetJsonSettingsForOpenMarket()));
            } 
        }
        private JsonSerializerSettings _GetJsonSettingsForOpenMarket()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(ScxOpenMarketsGenerationJob), "Id");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ScxGenerationProcessJobTestForOpenMarket()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxOpenMarketsDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    StandardDaypartId = 1,
                    Affiliates = new List<string> { "ABC","NBC"},
                    GenreType = OpenMarketInventoryExportGenreTypeEnum.News,
                    MarketCode = 390
                };

                var jobId = _ScxGenerationService.QueueScxOpenMarketsGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetOpenMarketsJobById(jobId);

                _ScxGenerationService.ProcessScxOpenMarketGenerationJob(job, new DateTime(2019, 7, 11));

                var jobAfterProcessing = _ScxGenerationJobRepository.GetOpenMarketsJobById(jobId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(jobAfterProcessing, _GetJsonSettingsForOpenMarket()));
            }
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ScxGenerationProcessJobFileResultTestForOpenMarket()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new InventoryScxOpenMarketsDownloadRequest
                {
                    EndDate = new DateTime(2019, 03, 31),
                    StartDate = new DateTime(2018, 12, 31),
                    StandardDaypartId = 1,
                    Affiliates = new List<string> { "ABC", "NBC" },
                    GenreType = OpenMarketInventoryExportGenreTypeEnum.News,
                    MarketCode = 390
                };

                var jobId = _ScxGenerationService.QueueScxOpenMarketsGenerationJob(request, "IntegrationTestUser", new DateTime(2019, 7, 11));

                var job = _ScxGenerationJobRepository.GetOpenMarketsJobById(jobId);

                _ScxGenerationService.ProcessScxOpenMarketGenerationJob(job, new DateTime(2019, 7, 11));

                var jobAfterProcessing = _ScxGenerationJobRepository.GetOpenMarketsJobById(jobId);

                var paths = _FileService.Paths;
                var streams = _FileService.Streams;

                Assert.NotNull(paths.Count > 0);
                Assert.NotNull(streams.Count > 0);
            }
        }
    }
}
