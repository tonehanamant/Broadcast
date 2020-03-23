using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Microsoft.Practices.Unity;
using Moq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class TrackerServiceIntegrationTests
    {
        private ITrackerService _TrackerService;
        private readonly ITrackingEngine _SutEngine;
        private readonly IScheduleRepository _ScheduleRepository;
        private readonly IRatingAdjustmentsRepository _RatingAdjustmentsRepository;
        private readonly IDetectionRepository _DetectionRepository;
        private const int TRACKER_TEST_ESTIMATE_ID = 121220;

        public TrackerServiceIntegrationTests()
        {
            _TrackerService = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();
            _SutEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackingEngine>();
            _ScheduleRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();
            _RatingAdjustmentsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IRatingAdjustmentsRepository>();
            _DetectionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>();
        }

        [Test]
        [Ignore]
        public void CreateLeadInAndBlockTestData()
        {
            using (new TransactionScopeWrapper())
            {

                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionTestDataGeneratorRepository>().CreateLeadInAndBlockTestData();
                _SutEngine.TrackDetectionByEstimateId(8675309);
            }
        }

        [Test]
        [Ignore]
        public void CreateOverflowSpotAssignTestData()
        {
            using (new TransactionScopeWrapper())
            {

                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IDetectionTestDataGeneratorRepository>().CreateOverflowSpotAssignTestData();
            }
        }

        [Test]
        [Ignore]
        public void CreateStationMatchWithoutTimeMatch()
        {
            using (new TransactionScopeWrapper())
            {

                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IDetectionTestDataGeneratorRepository>().CreateStationMatchWithoutTimeMatchData();
            }
        }

        [Test]
        public void AcceptScheduleLeadIn_AcceptAsLeadin()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new AcceptScheduleLeadinRequest
                {
                    DetectionDetailId = 9629,
                    ScheduleDetailWeekId = 208275
                };

                var detectionDetail = _SutEngine.AcceptScheduleLeadIn(request);

                Assert.AreEqual(request.ScheduleDetailWeekId, detectionDetail.ScheduleDetailWeekId);
                Assert.IsTrue(detectionDetail.LinkedToLeadin);
                Assert.AreEqual(TrackingStatus.InSpec, detectionDetail.Status);
            }
        }

        [Test]
        public void AcceptScheduleBlock_AcceptAsBlock()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new AcceptScheduleBlockRequest
                {
                    DetectionDetailId = 9629,
                    ScheduleDetailWeekId = 208275
                };

                var detectionDetail = _SutEngine.AcceptScheduleBlock(request);

                Assert.AreEqual(request.ScheduleDetailWeekId, detectionDetail.ScheduleDetailWeekId);
                Assert.IsTrue(detectionDetail.LinkedToBlock);
                Assert.AreEqual(TrackingStatus.InSpec, detectionDetail.Status);
            }
        }

        [Test]
        [Ignore]
        public void DoIt()
        {
            _SutEngine.TrackDetectionByDetectionDetails(new List<int> { 8090 }, 3622);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_FollowingScheduleMatch()
        {
            using (new TransactionScopeWrapper())
            {
                var actual = _SutEngine.GetProgramMappingDto(10181);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_PrimaryScheduleMatch()
        {
            var response = _SutEngine.GetProgramMappingDto(10053);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_FullScenario()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _SutEngine.GetProgramMappingDto(10545);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDisplaySchedulesWithAdjustedImpressions()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var displayScheduleTrackingDetails = new ScheduleDeliveryDetails()
                {
                    Impressions = 9999,
                    SpotLength = 30
                };

                var displaySchedule = new DisplaySchedule
                {
                    PrimaryDemoBooked = 9999,
                    PostingBookId = 420,
                    PostType = PostingTypeEnum.NTI,
                    DeliveryDetails =
                        new List<ScheduleDeliveryDetails>
                        {
                            displayScheduleTrackingDetails
                        }
                };

                var startDate = new DateTime(2000, 1, 1);
                var dateTime = new DateTime(2016, 01, 01);

                var repo = new Mock<IScheduleRepository>();
                repo.Setup(r => r.GetDisplaySchedules(startDate, dateTime)).Returns(new List<DisplaySchedule> { displaySchedule });
                var oldRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance(repo.Object);

                var engine = new Mock<IImpressionAdjustmentEngine>();
                engine.Setup(
                    e =>
                        e.AdjustImpression(displayScheduleTrackingDetails.Impressions.Value,
                            displaySchedule.IsEquivalized, displayScheduleTrackingDetails.SpotLength,
                            displaySchedule.PostType, displaySchedule.PostingBookId, true)).Returns(9999);
                engine.Setup(e => e.AdjustImpression(displaySchedule.PrimaryDemoDelivered, displaySchedule.PostType, displaySchedule.PostingBookId, false)).Returns(99999);

                var nsiPostingBookService = IntegrationTestApplicationServiceFactory.GetApplicationService<INsiPostingBookService>();

                var actual = _TrackerService.GetDisplaySchedulesWithAdjustedImpressions(startDate, dateTime);
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance(oldRepo);
                Assert.AreEqual(9999, actual.Single().PrimaryDemoBooked);
                Assert.AreEqual(9999, actual.Single().PrimaryDemoDelivered);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDetectionDetailsWithAdjustedImpressions()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var dto = new ScheduleDTO
                {
                    EstimateId = 124124,
                    PostType = PostingTypeEnum.NTI,
                    PostingBookId = 12412
                };

                var detectionTrackingDetail = new DetectionTrackingDetail
                {
                    Impressions = 9249234,
                    SpotLength = 30
                };

                var repo = new Mock<IDetectionRepository>();
                repo.Setup(r => r.GetDetectionTrackingDetailsByEstimateId(dto.EstimateId.Value)).Returns(new List<DetectionTrackingDetail> { detectionTrackingDetail });
                var oldRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>();
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance(repo.Object);

                var engine = new Mock<IImpressionAdjustmentEngine>();
                engine.Setup(e => e.AdjustImpression(detectionTrackingDetail.Impressions.Value, dto.Equivalized, detectionTrackingDetail.SpotLength, dto.PostType, dto.PostingBookId, true)).Returns(9999);

                var nsiPostingBookService = IntegrationTestApplicationServiceFactory.GetApplicationService<INsiPostingBookService>();
                var actual = _TrackerService.GetDetectionDetailsWithAdjustedImpressions(dto.EstimateId.Value, dto);
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance(oldRepo);

                Assert.AreEqual(9249234, actual.Single().Impressions);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDetectionDetailsWithAdjustedImpressionsEquivalized()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var dto = new ScheduleDTO
                {
                    EstimateId = 124124,
                    PostType = PostingTypeEnum.NTI,
                    PostingBookId = 12412,
                    Equivalized = true
                };

                var detectionTrackingDetail = new DetectionTrackingDetail
                {
                    Impressions = 9249234,
                    SpotLength = 15,
                };

                var repo = new Mock<IDetectionRepository>();
                repo.Setup(r => r.GetDetectionTrackingDetailsByEstimateId(dto.EstimateId.Value)).Returns(new List<DetectionTrackingDetail> { detectionTrackingDetail });
                var oldRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>();
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance(repo.Object);

                var nsiPostingBookService = IntegrationTestApplicationServiceFactory.GetApplicationService<INsiPostingBookService>();
                var actual = _TrackerService.GetDetectionDetailsWithAdjustedImpressions(dto.EstimateId.Value, dto);
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance(oldRepo);

                Assert.AreEqual(4624617.0d, actual.Single().Impressions);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDisplaySchedules()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var actual = _TrackerService.GetSchedulesByDate(new DateTime(2000, 1, 1), new DateTime(2016, 01, 01));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplaySchedule), "Id");
                jsonResolver.Ignore(typeof(DisplaySchedule), "IsBlank");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetNsiPostingBookMonths()
        {
            using (new TransactionScopeWrapper())
            {
                var nsiPostingBookService = IntegrationTestApplicationServiceFactory.GetApplicationService<INsiPostingBookService>();
                var months = nsiPostingBookService.GetNsiPostingBookMonths();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(months));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubDetectionData()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {

                var response = _TrackerService.GetDetectionScrubbingData(3417);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Track_ReRun_RememberFilledSpots()
        {
            using (new TransactionScopeWrapper())
            {
                _SutEngine.TrackDetectionByEstimateId(7618419);

                _SutEngine.TrackDetectionByDetectionDetails(new List<int> { 10550 }, 7618419);

                var scheduleDetailWeek1 =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                        .GetDataRepository<IScheduleRepository>().GetScheduleDetailWeek(369430);
                var scheduleDetailWeek2 =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                        .GetDataRepository<IScheduleRepository>().GetScheduleDetailWeek(369431);

                var detectionDetails =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>()
                        .GetDetectionTrackingDetailsByEstimateId(7618419);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(new
                {
                    Week1 = scheduleDetailWeek1,
                    Week2 = scheduleDetailWeek2,
                    DetectionDetails = detectionDetails,
                }));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TrackDetectionData_OverflowLink()
        {
            using (new TransactionScopeWrapper())
            {
                _SutEngine.TrackDetectionByEstimateId(7618419);

                var actual =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>()
                        .GetDetectionTrackingDetailsByEstimateId(7618419);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        public void GetEstimateIdsWithSchedulesByFileIds_HasSchedule()
        {
            using (new TransactionScopeWrapper())
            {
                // this guy has a schedule
                var actual = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IDetectionRepository>()
                    .GetEstimateIdsWithSchedulesByFileIds(new List<int> { 313 });
                Assert.AreEqual(3909, actual.First());
            }
        }

        [Test]
        public void GetEstimateIdsWithSchedulesByFileIds_DoesntHaveSchedule()
        {
            using (new TransactionScopeWrapper())
            {
                // this guy no schedule
                var actual = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IDetectionRepository>()
                    .GetEstimateIdsWithSchedulesByFileIds(new List<int> { 48 });

                Assert.IsEmpty(actual);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TrackDetectionData_NoTimeOverlap()
        {
            using (new TransactionScopeWrapper())
            {
                _SutEngine.TrackDetectionByEstimateId(7985133);

                var actual =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>()
                        .GetDetectionTrackingDetailsByEstimateId(7985133);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TrackDetectionData()
        {
            using (new TransactionScopeWrapper())
            {
                _SutEngine.TrackDetectionByEstimateId(3447);

                var actual =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>()
                        .GetDetectionTrackingDetailsByEstimateId(3447);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubDetectionData_WithNSIDateChanged()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {

                var response = _TrackerService.GetDetectionScrubbingData(3416);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TrackDetectionData_WithNSIDateChanged()
        {
            using (new TransactionScopeWrapper())
            {
                _SutEngine.TrackDetectionByEstimateId(3416);

                var actual =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>()
                        .GetDetectionTrackingDetailsByEstimateId(3416);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleDetails()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetScheduleDetailsByEstimateId(3417);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleHeader()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetScheduleHeader(3417);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleStations()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetScheduleStations(69);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetSchedulePrograms()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetSchedulePrograms(69);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleTrackingDetails()
        {
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();

                saveRequest.Schedule = new ScheduleDTO
                {
                    AdvertiserId = 39279,
                    EstimateId = 3390,
                    PostingBookId = 413,
                    ScheduleName = "Checkers 2Q16 SYN - Scheduler (tv)",
                    UserName = "User",
                    FileName = @"Checkers 2Q16 SYN - Scheduler (tv).scx",
                    FileStream = new FileStream(@".\Files\Checkers 2Q16 SYN - Scheduler (tv).scx", FileMode.Open,
                    FileAccess.Read),

                    MarketRestrictions = new List<int> { 101, 102 },
                    // restrict NYC and Binghamton just because reasons
                    DaypartRestriction = new DaypartDto
                    {
                        startTime = 0,
                        endTime = BroadcastConstants.OneDayInSeconds - 1,
                        mon = true,
                        tue = true,
                        wed = true,
                        thu = true,
                        fri = true,
                        sat = true,
                        sun = true
                    },
                    Equivalized = true,
                    ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "1111",
                        Client = "2222"
                    }
                },

                    PostType = PostingTypeEnum.NTI,
                    InventorySource = InventorySourceEnum.CNN
                };

                _TrackerService.SaveSchedule(saveRequest);

                var response = _ScheduleRepository.GetScheduleTrackingDetails(3390);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ScheduleDetailWeek), "ScheduleDetailWeekId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(response, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        public void SaveDetectionFiles()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                const string fileName = "BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });
                _TrackerService.SaveDetectionFiles(detectionRequest, "User");
            }
        }

        [Test]
        public void SaveDetectionFiles_BadCampaign()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Bad Campaign.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                const string fileName = "BVS Bad Campaign.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                Assert.That(() => _TrackerService.SaveDetectionFiles(detectionRequest, "User"),
                Throws.TypeOf<ExtractDetectionException>().With.Message.Contains("Invalid campaign field in row="));
            }
        }

        [Test]
        public void SaveDetectionFiles_WithBadRank()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Empty Rank.xlsx", FileMode.Open,
                    FileAccess.Read);
                const string fileName = "BVS Beiersdorf_Hersheys Empty Rank.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                Assert.That(() => _TrackerService.SaveDetectionFiles(detectionRequest, "User"),
               Throws.TypeOf<ExtractDetectionException>().With.Message.Contains("Invalid rank value "));
            }
        }

        [Test]
        public void SaveDetectionFiles_BadSpotLengthLookup()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Bad Spot Length.xlsx", FileMode.Open,
                    FileAccess.Read);
                const string fileName = "BVS Beiersdorf_Hersheys Bad Spot Length.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                Assert.That(() => _TrackerService.SaveDetectionFiles(detectionRequest, "User"),
               Throws.TypeOf<ExtractDetectionException>().With.Message.Contains("Invalid spot length"));
            }
        }

        [Test]
        public void SaveDetectionFiles_MultiFileWithErrors()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Bad Spot Length.xlsx", FileMode.Open,
                    FileAccess.Read);
                var fileName = "BVS Beiersdorf_Hersheys Bad Spot Length.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx", FileMode.Open,
                    FileAccess.Read);
                fileName = "BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx";
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Empty Rank.xlsx", FileMode.Open,
                    FileAccess.Read);
                fileName = "BVS Beiersdorf_Hersheys Empty Rank.xlsx";
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                Assert.That(() => _TrackerService.SaveDetectionFiles(detectionRequest, "User"),
               Throws.TypeOf<ExtractDetectionException>().With.Message.Contains("Invalid rank value ").And.Message.Contains("<br /> Error processing file "));
            }
        }

        [Test]
        public void SaveDetectionFiles_EmptyFileError()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS empty file.xlsx", FileMode.Open, FileAccess.Read);
                const string fileName = "BVS empty file.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                Assert.That(() => _TrackerService.SaveDetectionFiles(detectionRequest, "User"),
               Throws.TypeOf<ExtractDetectionException>().With.Message.Contains("File does not contain valid detection detail data."));
            }
        }

        [Test]
        [Ignore]
        public void LoadFtp()
        {
            // not part of auto-run test, only for manual testing
            string msg;
            using (new TransactionScopeWrapper())
            {
                msg = _TrackerService.SaveDetectionFileViaFtp("test_user");
            }
            Console.WriteLine(msg);
        }

        [Test]
        public void ScheduleExists_Exist()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.ScheduleExists(3417);

                Assert.IsTrue(response);
            }
        }

        [Test]
        public void ScheduleExists_DoesNotExists()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.ScheduleExists(876165456);

                Assert.IsFalse(response);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void GetScrubbingData()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var response = _TrackerService.GetDetectionScrubbingData(3417);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMappingStations()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetScheduleStations(69);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetMappingPrograms()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetSchedulePrograms(69);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [Ignore]
        public void CreateScrubbingMapping()
        {
            var scrub = new ScrubbingMap
            {
                BvsStation = "WNBA",
                ScheduleStation = "WNBA-TV",
            };
            scrub.DetailIds.Add(9629);

            _TrackerService.SaveScrubbingMapping(scrub);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetScheduleAudiences(3417);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramScrubbingMapList()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetDetectionMapByType("Program");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationScrubbingMapList()
        {
            using (new TransactionScopeWrapper())
            {
                var response = _TrackerService.GetDetectionMapByType("Station");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DeleteProgramMapping()
        {
            using (new TransactionScopeWrapper())
            {
                var mapping = new TrackingMapValue
                {
                    DetectionValue = "10 NEWS 6PM SATURDAY",
                    ScheduleValue = "10 NEWS 6P SAT"
                };
                _TrackerService.DeleteMapping("Program", mapping);
                var response = _TrackerService.GetDetectionMapByType("Program");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DeleteStationMapping()
        {
            using (new TransactionScopeWrapper())
            {
                var mapping = new TrackingMapValue
                {
                    DetectionValue = "ADSM",
                    ScheduleValue = "ADSM+S2"
                };
                _TrackerService.DeleteMapping("Station", mapping);
                var response = _TrackerService.GetDetectionMapByType("Station");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDetectionInitialOptions()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var response = _TrackerService.GetDetectionLoadData(new DateTime(2019, 01, 01));
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(Quarter), "Id");
                jsonResolver.Ignore(typeof(DetectionLoadDto), "CurrentQuarter");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                Assert.AreEqual(response.CurrentQuarter.Display, "2019Q1");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [Ignore] //For loading data only - not for testing
        public void LoadScheduleDataForTestClientReport()
        {
            try
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = 4401;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Checkers 2Q16 - Estimtate 4401";
                schedule.UserName = "User";
                schedule.FileName = @"Checkers 2Q16 SYN - Estimate4401.scx";

                schedule.ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "4400ABCD1H",
                        Client = "4400ABCD1H"
                    },
                    new IsciDto
                    {
                        House = "4400ABCD2H",
                        Client = "4400ABCD2H"
                    }
                };

                schedule.FileStream = new FileStream(@".\Files\Checkers 2Q16 SYN - Estimate4401.scx", FileMode.Open,
                    FileAccess.Read);
                schedule.InventorySource = InventorySourceEnum.TVB;
                schedule.PostType = PostingTypeEnum.NTI;
                saveRequest.Schedule = schedule;

                _TrackerService.SaveSchedule(saveRequest);

            }
            finally
            {
                //_ScheduleRepository.DeleteSchedule(3390);
            }
        }

        [Test]
        [Ignore] //For loading data only - not for testing
        public void LoadScheduleDetectionDataForTestingClientReport()
        {

            var detectionRequest = new FileSaveRequest();

            var stream = new FileStream(@".\Files\Checkers BVS Report - Estimate4401.DAT", FileMode.Open,
                FileAccess.Read);
            detectionRequest.Files.Add(new FileRequest
            {
                StreamData = stream
            });

            _TrackerService.SaveDetectionFiles(detectionRequest, "User");
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadDetection_Load_Assembly_Tracker()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS_Data_for_Tracking.xlsx", FileMode.Open, FileAccess.Read);

                const string fileName = "BVS_Data_For_Tracking.DAT";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });

                _TrackerService.SaveDetectionFiles(detectionRequest, "User");

                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = TRACKER_TEST_ESTIMATE_ID;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Assembly Schedule Template";
                schedule.UserName = "User";
                schedule.FileName = @"BVS Assembly Schedule For Tracking.csv";
                schedule.FileStream = new FileStream(@".\Files\BVS Assembly Schedule For Tracking.csv", FileMode.Open,
                    FileAccess.Read);

                schedule.MarketRestrictions = new List<int> { 101, 102 };
                // restrict NYC and Binghamton just because reasons
                schedule.DaypartRestriction = new DaypartDto
                {
                    startTime = 0,
                    endTime = BroadcastConstants.OneDayInSeconds - 1,
                    mon = true,
                    tue = true,
                    wed = true,
                    thu = true,
                    fri = true,
                    sat = true,
                    sun = true
                };
                schedule.Equivalized = true;
                schedule.ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "2TWNWEST000H",
                        Client = "2TWNWEST000H"
                    },
                    new IsciDto
                    {
                        House = "37WEST1605H",
                        Client = "37WEST1605H"
                    }
                };

                schedule.PostType = PostingTypeEnum.NTI;
                schedule.InventorySource = InventorySourceEnum.CNN;

                saveRequest.Schedule = schedule;

                _TrackerService.SaveSchedule(saveRequest);
                var actual = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IDetectionRepository>()
                    .GetDetectionTrackingDetailsByEstimateId(TRACKER_TEST_ESTIMATE_ID);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DetectionTrackingDetail), "Id");
                jsonResolver.Ignore(typeof(DetectionTrackingDetail), "ScheduleDetailWeekId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(actual, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateScheduleReportDto_SCXWithBlankSpots()
        { //BCOP-1571
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest
                {
                    Schedule = new ScheduleDTO
                    {
                        AdvertiserId = 39279,
                        EstimateId = 44001,
                        PostingBookId = 413,
                        ScheduleName = "Checkers 2Q16 SYN - Estimate44001 With Empty Spots.scx",
                        UserName = "User",
                        FileName = @"Checkers 2Q16 SYN - Estimate44001 With Empty Spots.scx",
                        ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "4400ABCD1H",
                        Client = "4400ABCD1H"
                    },
                    new IsciDto
                    {
                        House = "4400ABCD2H",
                        Client = "4400ABCD2H"
                    }
                },
                        FileStream = new FileStream(@".\Files\Checkers 2Q16 SYN - Estimate44001 With Empty Spots.scx", FileMode.Open,
       FileAccess.Read),
                        InventorySource = InventorySourceEnum.OpenMarket,
                        PostType = PostingTypeEnum.NSI
                    }
                };
                var scheduleId = _TrackerService.SaveSchedule(saveRequest);

                var reportService = IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();

                var actual = reportService.GenerateScheduleReportDto(scheduleId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ScheduleReportDto), "ScheduleId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(actual, jsonSettings);
                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveSchedule_CreateBlankSchedule()
        {
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest
                {
                    Schedule = new ScheduleDTO
                    {
                        AdvertiserId = 39279,
                        PostingBookId = 413,
                        ScheduleName = "Blank Schedule",
                        UserName = "User",
                        MarketRestrictions = new List<int> { 101, 102 },
                        // restrict NYC and Binghamton just because reasons
                        DaypartRestriction = new DaypartDto
                        {
                            startTime = 0,
                            endTime = BroadcastConstants.OneDayInSeconds - 1,
                            mon = true,
                            tue = true,
                            wed = true,
                            thu = true,
                            fri = true,
                            sat = true,
                            sun = true
                        },
                        Equivalized = true,
                        ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "2TWNWEST000H",
                        Client = "2TWNWEST000H"
                    },
                    new IsciDto
                    {
                        House = "37WEST1605H",
                        Client = "37WEST1605H"
                    }
                },

                        PostType = PostingTypeEnum.NTI,
                        InventorySource = InventorySourceEnum.CNN,
                        Audiences = new List<DetectionTrackingAudience>
                {
                   new DetectionTrackingAudience() {AudienceId = 31, Rank = 1},
                   new DetectionTrackingAudience() {AudienceId = 42, Rank = 2}
                },
                        StartDate = new DateTime(2017, 1, 20),
                        EndDate = new DateTime(2018, 1, 20),
                        IsBlank = true
                    }
                };

                var scheduleId = _TrackerService.SaveSchedule(saveRequest);
                var actual = _ScheduleRepository.GetDisplayScheduleById(scheduleId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplaySchedule), "Id");
                jsonResolver.Ignore(typeof(IsciDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(actual, jsonSettings);
                Console.WriteLine(json);
                Approvals.Verify(json);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void CreateBlankScheduleDataForClientReportTesting()
        {
            using (var tran = new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Blank Schedule for 4400ABCD1H, 4400ABCD2H";
                schedule.UserName = "User";
                schedule.Equivalized = false;
                schedule.ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "4400ABCD1H",
                        Client = "4400ABCD1H"
                    },
                    new IsciDto
                    {
                        House = "4400ABCD2H",
                        Client = "4400ABCD2H"
                    }
                };

                schedule.PostType = PostingTypeEnum.NTI;
                schedule.InventorySource = InventorySourceEnum.CNN;
                schedule.Audiences = new List<DetectionTrackingAudience>
                {
                   new DetectionTrackingAudience() {AudienceId = 31, Rank = 1},
                   new DetectionTrackingAudience() {AudienceId = 42, Rank = 2}
                };
                schedule.StartDate = new DateTime(2016, 3, 28);
                schedule.EndDate = new DateTime(2016, 4, 24);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId = _TrackerService.SaveSchedule(saveRequest);

                Console.WriteLine(scheduleId);
                tran.Complete();
            }
        }

        [Test]
        [Ignore]
        public void LoadDetection_Quickload()
        {
            var stream = new FileStream(@".\Files\BVS Quick Load.xlsx", FileMode.Open,
                FileAccess.Read);
            const string fileName = "BVS Quick Load.xlsx";

            var detectionRequest = new FileSaveRequest();
            detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });
            _TrackerService.SaveDetectionFiles(detectionRequest, "User");
        }

        /// <summary>
        /// This test loads 2 schdules (using BVS from "BVS Quick Load.xlsx" which should already be loaded)
        /// First schedule has 2 ISCIS and 2 audiences.
        /// Second schedule has 1 ISCI and 1 audience.
        /// The point of this test is to ensure the post logic of the second schedule does 
        /// not affect the first schedule.
        /// </summary>
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateBlankSchedule_CheckPost()
        {
            using (new TransactionScopeWrapper())
            {
                // create a schedule, one with 2 ISCIS and 2 audiences.
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Best";
                schedule.UserName = "User";
                schedule.MarketRestrictions = new List<int> { 101, 102 };
                // restrict NYC and Binghamton just because reasons
                schedule.DaypartRestriction = new DaypartDto
                {
                    startTime = 0,
                    endTime = BroadcastConstants.OneDayInSeconds - 1,
                    mon = true,
                    tue = true,
                    wed = true,
                    thu = true,
                    fri = true,
                    sat = true,
                    sun = true
                };
                schedule.Equivalized = true;
                schedule.ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "QRVA49CE08",
                        Client = "QRVA49CE08"
                    },
                    new IsciDto
                    {
                        House = "QRVA45BK95H",
                        Client = "QRVA45BK95H"
                    }
                };
                schedule.PostType = PostingTypeEnum.NTI;
                schedule.InventorySource = InventorySourceEnum.CNN;
                schedule.Audiences = new List<DetectionTrackingAudience>
                {
                   new DetectionTrackingAudience() {AudienceId = 31, Rank = 1},
                   new DetectionTrackingAudience() {AudienceId = 42, Rank = 2}
                };
                schedule.StartDate = new DateTime(2017, 1, 20);
                schedule.EndDate = new DateTime(2018, 1, 20);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId1 = _TrackerService.SaveSchedule(saveRequest);

                // create second schedule contain one of the 2 ISCIS from the first schedule and one of the 2 audiences
                saveRequest = new ScheduleSaveRequest();
                schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Blank Schedule 2";
                schedule.UserName = "User";
                schedule.MarketRestrictions = new List<int> { 101, 102 };
                schedule.DaypartRestriction = new DaypartDto
                {
                    startTime = 0,
                    endTime = BroadcastConstants.OneDayInSeconds - 1,
                    mon = true,
                    tue = true,
                    wed = true,
                    thu = true,
                    fri = true,
                    sat = true,
                    sun = true
                };
                schedule.Equivalized = true;
                schedule.ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "QRVA49CE08",
                        Client = "QRVA49CE08"
                    }
                };
                schedule.PostType = PostingTypeEnum.NTI;
                schedule.InventorySource = InventorySourceEnum.CNN;
                schedule.Audiences = new List<DetectionTrackingAudience>
                {
                   new DetectionTrackingAudience() {AudienceId = 31, Rank = 1}
                };
                schedule.StartDate = new DateTime(2017, 1, 20);
                schedule.EndDate = new DateTime(2018, 1, 20);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId2 = _TrackerService.SaveSchedule(saveRequest);

                // now we assert results
                // first schedule
                var efSchedule = _TrackerService.GetDisplayScheduleById(scheduleId1);
                var estimates = _DetectionRepository.GetEstimateIdsByIscis(efSchedule.Iscis.Select(ib => ib.House).ToList());
                var actual = new List<DetectionPostDetailAudience>();

                estimates.ForEach(
                    estimateId => actual.AddRange(_DetectionRepository.GetDetectionPostDetailAudienceByEstimateId(estimateId)));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DetectionPostDetailAudience), "DetectionDetailId");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                // second schedule
                efSchedule = _TrackerService.GetDisplayScheduleById(scheduleId2);

                estimates = _DetectionRepository.GetEstimateIdsByIscis(efSchedule.Iscis.Select(ib => ib.House).ToList());
                //actual = new List<DetectionPostDetailAudience>();

                estimates.ForEach(
                    estimateId => actual.AddRange(_DetectionRepository.GetDetectionPostDetailAudienceByEstimateId(estimateId)));

                var json = IntegrationTestHelper.ConvertToJson(actual, jsonSettings);
                Console.WriteLine(json);
                Approvals.Verify(json);
            }
        }

        [Test]
        public void SaveSchedule_WithHouseIscWithoutClientIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest
                {
                    Schedule = new ScheduleDTO
                    {
                        AdvertiserId = 39279,
                        PostingBookId = 413,
                        ScheduleName = "Incomplete ISCI Schedule",
                        UserName = "User",
                        Equivalized = true,

                        ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "ABCD1234",
                        Client = "ABCD1234"
                    },
                    new IsciDto
                    {
                        House = "EFGH5678",
                        Client = null
                    }
                },

                        PostType = PostingTypeEnum.NTI,
                        InventorySource = InventorySourceEnum.CNN,
                        Audiences = new List<DetectionTrackingAudience>
                {
                   new DetectionTrackingAudience() {AudienceId = 31, Rank = 1},
                   new DetectionTrackingAudience() {AudienceId = 42, Rank = 2}
                },
                        StartDate = new DateTime(2017, 1, 20),
                        EndDate = new DateTime(2018, 1, 20),
                        IsBlank = true
                    }
                };

                Assert.That(() => _TrackerService.SaveSchedule(saveRequest),
                    Throws.TypeOf<ApplicationException>().With.Message.EqualTo("All ISCIs must have a client ISCI"));
            }
        }

        [Test]
        public void SaveSchedule_WithClientIscWithoutHouseIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest
                {
                    Schedule = new ScheduleDTO
                    {
                        AdvertiserId = 39279,
                        PostingBookId = 413,
                        ScheduleName = "Incomplete ISCI Schedule",
                        UserName = "User",
                        Equivalized = true,

                        ISCIs = new List<IsciDto>
                        {
                            new IsciDto
                            {
                                House = null,
                                Client = "ABCD1234"
                            },
                            new IsciDto
                            {
                                House = "EFGH5678",
                                Client = "EFGH5678"
                            }
                        },

                        PostType = PostingTypeEnum.NTI,
                        InventorySource = InventorySourceEnum.CNN,
                        Audiences = new List<DetectionTrackingAudience>
                    {
                       new DetectionTrackingAudience() {AudienceId = 31, Rank = 1},
                       new DetectionTrackingAudience() {AudienceId = 42, Rank = 2}
                    },
                        StartDate = new DateTime(2017, 1, 20),
                        EndDate = new DateTime(2018, 1, 20),
                        IsBlank = true
                    }
                };

                Assert.That(() => _TrackerService.SaveSchedule(saveRequest),
                    Throws.TypeOf<ApplicationException>().With.Message.EqualTo("All ISCIs must have a house ISCI"));
            }
        }

        [Test]
        [Ignore] //For loading data only - not for testing
        [UseReporter(typeof(DiffReporter))]
        public void LoadScheduleSettingsTest()
        {
            var saveRequest = new ScheduleSaveRequest();
            var schedule = new ScheduleDTO();

            schedule.EstimateId = 3910;
            schedule.AdvertiserId = 39279;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Test Settings Schedule";
            schedule.UserName = "User";
            schedule.MarketRestrictions = new List<int> { 101, 102 };
            // restrict NYC and Binghamton just because reasons
            schedule.DaypartRestriction = new DaypartDto
            {
                startTime = 0,
                endTime = BroadcastConstants.OneDayInSeconds - 1,
                mon = true,
                tue = true,
                wed = true,
                thu = true,
                fri = true,
                sat = true,
                sun = true
            };
            schedule.Equivalized = true;
            schedule.ISCIs = new List<IsciDto>
            {
                new IsciDto
                {
                    House = "2TWNWEST000H",
                    Client = "2TWNWEST000H"
                },
                new IsciDto
                {
                    House = "37WEST1605H",
                    Client = "37WEST1605H"
                }
            };

            schedule.PostType = PostingTypeEnum.NTI;
            schedule.InventorySource = InventorySourceEnum.CNN;
            schedule.Audiences = new List<DetectionTrackingAudience>
                {
                   new DetectionTrackingAudience() {AudienceId = 31, Rank = 1},
                   new DetectionTrackingAudience() {AudienceId = 42, Rank = 2}
                };
            schedule.StartDate = new DateTime(2017, 1, 20);
            schedule.EndDate = new DateTime(2018, 1, 20);
            schedule.IsBlank = true;

            saveRequest.Schedule = schedule;

            _TrackerService.SaveSchedule(saveRequest);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleSettings()
        {
            var schedule = _ScheduleRepository.GetScheduleDtoByEstimateId(3910);
            var displaySchedule = _TrackerService.GetDisplayScheduleById(schedule.Id);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplaySchedule), "Id");
            jsonResolver.Ignore(typeof(IsciDto), "Id");
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(displaySchedule, jsonSettings);
            Console.WriteLine(json);
            Approvals.Verify(json);
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void LoadScheduleForUpdate()
        {
            var saveRequest = new ScheduleSaveRequest();
            var schedule = new ScheduleDTO();

            schedule.EstimateId = 4000;
            schedule.AdvertiserId = 39279;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Test Update";
            schedule.UserName = "User";
            schedule.MarketRestrictions = new List<int> { 101, 102 };
            // restrict NYC and Binghamton just because reasons
            schedule.DaypartRestriction = new DaypartDto
            {
                startTime = 0,
                endTime = BroadcastConstants.OneDayInSeconds - 1,
                mon = true,
                tue = true,
                wed = true,
                thu = true,
                fri = true,
                sat = true,
                sun = true
            };
            schedule.Equivalized = true;
            schedule.ISCIs = new List<IsciDto>
            {
                new IsciDto
                {
                    House = "2TWNWEST000H",
                    Client = "2TWNWEST000H"
                },
                new IsciDto
                {
                    House = "37WEST1605H",
                    Client = "37WEST1605H"
                }
            };

            schedule.PostType = PostingTypeEnum.NTI;
            schedule.InventorySource = InventorySourceEnum.CNN;
            schedule.Audiences = new List<DetectionTrackingAudience>
                {
                   new DetectionTrackingAudience() {AudienceId = 31, Rank = 1},
                   new DetectionTrackingAudience() {AudienceId = 42, Rank = 2}
                };
            schedule.StartDate = new DateTime(2017, 1, 20);
            schedule.EndDate = new DateTime(2018, 1, 20);
            schedule.IsBlank = true;

            saveRequest.Schedule = schedule;

            _TrackerService.SaveSchedule(saveRequest);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateSchedule()
        {
            const int estimateId = 4000;
            using (new TransactionScopeWrapper())
            {
                var efSchedule = _ScheduleRepository.GetScheduleDtoByEstimateId(estimateId);
                string json = null;

                if (efSchedule != null)
                {
                    var saveRequest = new ScheduleSaveRequest
                    {
                        Schedule = new ScheduleDTO
                        {
                            EstimateId = estimateId,
                            IsBlank = false,
                            ScheduleName = "Test Update (UPDATED)",
                            UserName = "User",

                            ISCIs = new List<IsciDto>
                    {
                        new IsciDto
                        {
                            House = "1111",
                            Client = "2222"
                        }
                    },

                            AdvertiserId = 37674,
                            PostingBookId = 377,
                            MarketRestrictions = new List<int> { 125, 390 },
                            DaypartRestriction = new DaypartDto
                            {
                                startTime = 0,
                                endTime = BroadcastConstants.OneDayInSeconds - 1,
                                mon = false,
                                tue = true,
                                wed = true,
                                thu = true,
                                fri = true,
                                sat = true,
                                sun = true
                            },
                            PostType = PostingTypeEnum.NSI,
                            Equivalized = false,
                            Audiences = new List<DetectionTrackingAudience>
                    {
                        new DetectionTrackingAudience() {AudienceId = 31, Rank = 1},
                        new DetectionTrackingAudience() {AudienceId = 42, Rank = 2}
                    },
                            InventorySource = InventorySourceEnum.OpenMarket
                        }
                    };
                    _TrackerService.SaveSchedule(saveRequest);

                    var scheduleId = _ScheduleRepository.GetScheduleDtoByEstimateId(estimateId).Id;
                    var response = _ScheduleRepository.GetDisplayScheduleById(scheduleId);

                    var jsonResolver = new IgnorableSerializerContractResolver();
                    jsonResolver.Ignore(typeof(DisplaySchedule), "Id");
                    jsonResolver.Ignore(typeof(IsciDto), "Id");
                    var jsonSettings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = jsonResolver
                    };
                    json = IntegrationTestHelper.ConvertToJson(response, jsonSettings);
                }

                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateRatingAdjustments()
        {
            using (new TransactionScopeWrapper())
            {
                var ratingAdjustments = new List<RatingAdjustmentsDto>
                {
                    new RatingAdjustmentsDto
                    {
                        MediaMonthId = 377,
                        AnnualAdjustment = 0.5M,
                        NtiAdjustment = 10
                    },
                    new RatingAdjustmentsDto
                    {
                        MediaMonthId = 380,
                        AnnualAdjustment = 20.5M,
                        NtiAdjustment = 35
                    },
                    new RatingAdjustmentsDto
                    {
                        MediaMonthId = 382,
                        AnnualAdjustment = 0.01M,
                        NtiAdjustment = 100
                    }
                };

                _RatingAdjustmentsRepository.SaveRatingAdjustments(ratingAdjustments);

                var response = _RatingAdjustmentsRepository.GetRatingAdjustments();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        public void SaveDetectionFiles_TwiceHasCorrectDuplicateMessage()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx",
                    FileMode.Open, FileAccess.Read);
                const string fileName = "BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });
                _TrackerService.SaveDetectionFiles(detectionRequest, "User");


                var stream2 = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle2.xlsx",
                    FileMode.Open, FileAccess.Read);
                const string fileName2 = "BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle2.xlsx";

                var detectionRequest2 = new FileSaveRequest();
                detectionRequest2.Files.Add(new FileRequest { FileName = fileName2, StreamData = stream2 });
                var dontIgnore = _TrackerService.SaveDetectionFiles(detectionRequest2, "User").Item2;

                const string correctMessage =
                    @"<p>The following line(s) were previously imported and were ignored:</p><ul><li>Line 10: Station KTVK, Date 3/13/2017 12:00:00 AM, Time Aired 19302, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li><li>Line 11: Station KPLR, Date 3/13/2017 12:00:00 AM, Time Aired 20217, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li><li>Line 12: Station WAXN, Date 3/13/2017 12:00:00 AM, Time Aired 19770, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li><li>Line 14: Station KCWI, Date 3/13/2017 12:00:00 AM, Time Aired 21515, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li><li>Line 15: Station WGMB, Date 3/13/2017 12:00:00 AM, Time Aired 21541, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li></ul><p>The following line(s) were previously imported and were updated with new program name:</p><ul><li>Line 9: Station WMOR, Date 3/13/2017 12:00:00 AM, Time Aired 18532, ISCI 38VA49CE08, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY, Program Name JUDGE ALEXNEWPROGRAMNAME</li></ul>";
                Assert.AreEqual(correctMessage, dontIgnore);
            }
        }

        private void _ImportBVSFile(string fileName)
        {
            var stream = new FileStream(@".\Files\" + fileName, FileMode.Open, FileAccess.Read);

            var detectionRequest = new FileSaveRequest();
            detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });
            _TrackerService.SaveDetectionFiles(detectionRequest, "INT Test User");
        }

        private static int _ImportOvernightSchedule()
        {
            const int estimateId = 794613;
            const string fileName = "Overnight and lead ins.csv";

            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

            var saveRequest = new ScheduleSaveRequest();
            var schedule = new ScheduleDTO();

            schedule.AdvertiserId = 39279;
            schedule.EstimateId = estimateId;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Schedule For Reporting Tests";
            schedule.UserName = "Assembly INT Test User";
            schedule.FileName = fileName;
            schedule.FileStream = new FileStream(@".\Files\" + fileName, FileMode.Open, FileAccess.Read);

            schedule.MarketRestrictions = new List<int>();
            schedule.DaypartRestriction = new DaypartDto
            {
                startTime = 0,
                endTime = BroadcastConstants.OneDayInSeconds - 1,
                mon = true,
                tue = true,
                wed = true,
                thu = true,
                fri = true,
                sat = true,
                sun = true
            };
            schedule.Equivalized = true;
            schedule.ISCIs = new List<IsciDto>
            {
                new IsciDto {House = "O_N_RR", Client = "CO_N_RR"}
            };

            schedule.PostType = PostingTypeEnum.NSI;
            schedule.InventorySource = InventorySourceEnum.Assembly;
            saveRequest.Schedule = schedule;
            var scheduleId = sut.SaveSchedule(saveRequest);

            return scheduleId;

        }


        /// <summary>
        ///The point of this test is to check air time logic for both normal and overnight with lead in programs.
        /// The detection record "Sunday Night ALEX" will be out of spec because it's NSI date is on the 19th which is Sunday night
        /// and outside for all schedule dayparts (but closest would be the schedule's record of same program name).
        /// 
        /// This test also covers BCOP-1679 which fails to show BVS out of spec records that fall outside the schedule's weeks.
        /// </summary>
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Tracker_Days_Overnight_And_Lead_Ins()
        {
            using (new TransactionScopeWrapper())
            {
                _ImportBVSFile("Ovenight_Leadin_Stuff.xlsx");
                var scheduleId = _ImportOvernightSchedule();

                var sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                var reportDto = sut.GenerateClientReportDto(scheduleId);


                //var report = sut.GenerateScheduleReport(scheduleId);
                //File.WriteAllBytes(string.Format("..\\Sched_Report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"detectionreport.xlsx", reportStream.GetBuffer());


                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ScheduleReportDto), "ScheduleId");
                jsonResolver.Ignore(typeof(IsciDto), "Id");
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportDto, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Tracker_Track_Schedule()
        {
            using (new TransactionScopeWrapper())
            {
                var detectionDetails = _DetectionRepository.GetDetectionTrackingDetailsByDetailIds(new List<int> { 86438 });

                var detail = detectionDetails.First();

                var map = new ScrubbingMap();
                map.EstimateId = detail.EstimateId;

                map.BvsProgram = "GOOD MORNING ARIZONA 5AM";
                map.ScheduleProgram = "VARIOUS";
                map.DetailIds = new List<int> { 86438 };

                var trackingDetails = _TrackerService.SaveScrubbingMapping(map).First();

                // ensure it mapped properly
                Assert.AreEqual(TrackingStatus.InSpec, trackingDetails.Status, "Test failed due to bad mapping");
                // remove map
                _TrackerService.DeleteMapping("Program", new TrackingMapValue { DetectionValue = "GOOD MORNING ARIZONA 5AM", ScheduleValue = "VARIOUS" });

                _TrackerService = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

                // retract and ensure the detection detail status is back to outofspec
                _TrackerService.TrackSchedule(3420);

                detail = _DetectionRepository.GetDetectionTrackingDetailsByDetailIds(new List<int> { 86438 }).First();
                Assert.AreEqual(TrackingStatus.OutOfSpec, detail.Status);
            }
        }

        [Test]
        [Ignore]
        public void Delete_BVS_File()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Load For Various Tests.xlsx", FileMode.Open, FileAccess.Read);
                const string fileName = "BVS Load For Various Tests.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = fileName, StreamData = stream });
                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

                var detectionFileId = sut.SaveDetectionFiles(detectionRequest, "LoadDetectionFile User").Item1.First();

                sut.DeleteDetectionFile(detectionFileId);
                var detectionFiles = sut.GetDetectionFileSummaries().ToList();
                // ensure newly created detection file cannot be read.
                Assert.IsEmpty(detectionFiles.Where(b => b.Id == detectionFileId));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]

        public void Tracker_Overlapping_Fields_And_Choose_Higher_Rate()
        {
            const int estimate_id = 333123;
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS For Overlapping Fields.xlsx", FileMode.Open, FileAccess.Read);
                var fileName = "BVS For Overlapping Fields.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest() { FileName = fileName, StreamData = stream });

                int detectionFileId = _TrackerService.SaveDetectionFiles(detectionRequest, "BVS For Overlapping Fields").Item1.First();

                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = estimate_id;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "SCX Overlapping Fields.scx";
                schedule.UserName = "User";
                schedule.FileName = @"SCX Overlapping Fields.scx";

                schedule.ISCIs = new List<IsciDto>
                {
                    new IsciDto
                    {
                        House = "AAABBB",
                        Client = "AAABBB"
                    }
                };

                schedule.FileStream = new FileStream(@".\Files\SCX Overlapping Fields.scx", FileMode.Open,
                    FileAccess.Read);
                schedule.InventorySource = InventorySourceEnum.OpenMarket;
                schedule.PostType = PostingTypeEnum.NTI;
                saveRequest.Schedule = schedule;

                _TrackerService.SaveSchedule(saveRequest);

                var output = _DetectionRepository.GetDetectionTrackingDetailsByEstimateId(estimate_id);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DetectionTrackingDetail), "Id");
                jsonResolver.Ignore(typeof(DetectionTrackingDetail), "ScheduleDetailWeekId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(output, jsonSettings));
            }
        }

        /// <summary>
        /// The idea of this test to to *not* include detection details as part of delivery counts for 
        /// detection records that are not in spec.
        /// </summary>
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Remove_OutofSpec_Deliveries_BCOP_1873()
        {
            const int estimate_id = 333444;
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Load For Various Tests.xlsx", FileMode.Open, FileAccess.Read);
                var detectionFileName = "BVS Load For Various Tests.xlsx";

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest() { FileName = detectionFileName, StreamData = stream });

                int detectionFileId = _TrackerService.SaveDetectionFiles(detectionRequest, detectionFileName).Item1.First();

                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                var scxFileName = "SCX Various Tests.scx";
                schedule.AdvertiserId = 39279;
                schedule.EstimateId = estimate_id;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = scxFileName;
                schedule.UserName = "User";
                schedule.FileName = scxFileName;

                schedule.ISCIs = new List<IsciDto> { new IsciDto { House = "AAABBB", Client = "AAABBB" } };

                schedule.FileStream = new FileStream(@".\Files\" + scxFileName, FileMode.Open,
                    FileAccess.Read);
                schedule.InventorySource = InventorySourceEnum.OpenMarket;
                schedule.PostType = PostingTypeEnum.NTI;
                saveRequest.Schedule = schedule;

                var scheduleId = _TrackerService.SaveSchedule(saveRequest);


                var trackingDetails = _DetectionRepository.GetDetectionTrackingDetailsByEstimateId(estimate_id);
                // grab first detail record.
                int outofSpecDetailId = trackingDetails.First().Id;
                // mark it as out of spec.
                var officiallyOutOfSpecDetectionItems = _DetectionRepository.GetDetectionTrackingDetailsByDetailIds(new List<int>() { outofSpecDetailId });
                officiallyOutOfSpecDetectionItems.ForEach(c => c.Status = TrackingStatus.OfficialOutOfSpec);
                _DetectionRepository.PersistDetectionDetails(officiallyOutOfSpecDetectionItems);

                // generate the report and the "DeliveredSpots" field for the above record should be 0.
                var dtoReportService = IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                var output = dtoReportService.GenerateScheduleReportDto(scheduleId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ScheduleReportDto), "ScheduleId");
                jsonResolver.Ignore(typeof(IsciDto), "Id");
                jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(output, jsonSettings));
            }
        }

        /// <summary>
        /// The various tests.scx file has 3 audiences.  Upload it and post data will have data for all audiences.
        /// Then update schedule with only one audience, the post data should only have audiences for only the one.
        /// </summary>
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_GenerateScheduleReportDto_BCOP1900()
        {
            const int estimateId = 112233;

            using (new TransactionScopeWrapper())
            {
                ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                var scxFileName = "SCX Various Tests.scx";
                schedule.AdvertiserId = 39279;
                schedule.EstimateId = estimateId;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = scxFileName;
                schedule.UserName = "SCX User";
                schedule.FileName = scxFileName;
                schedule.FileStream = new FileStream(@".\Files\" + scxFileName, FileMode.Open, FileAccess.Read);

                schedule.MarketRestrictions = new List<int>();
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,
                    endTime = BroadcastConstants.OneDayInSeconds - 1,
                    mon = true,
                    tue = true,
                    wed = true,
                    thu = true,
                    fri = true,
                    sat = true,
                    sun = true
                };

                schedule.Equivalized = true;
                schedule.ISCIs = new List<IsciDto>
                {
                    new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                    new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}
                };

                schedule.PostType = PostingTypeEnum.NTI;
                schedule.InventorySource = InventorySourceEnum.OpenMarket;
                saveRequest.Schedule = schedule;
                int scheduleId = sut.SaveSchedule(saveRequest);

                schedule.Audiences = new List<DetectionTrackingAudience>
                {
                   new DetectionTrackingAudience() {AudienceId = 31, Rank = 1}
                };
                schedule.FileStream = null;
                schedule.FileName = null;
                scheduleId = sut.SaveSchedule(saveRequest);

                var postDetails = _DetectionRepository.GetDetectionPostDetailAudienceByEstimateId(estimateId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(IsciDto), "DetectionDetailId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(postDetails, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadSigmaFile()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\SigmaImport.csv", FileMode.Open, FileAccess.Read);

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = "SigmaImport.csv", StreamData = stream });
                var result = _TrackerService.SaveDetectionFiles(detectionRequest, "User", true);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadSigmaFile_InvalidDates()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\SigmaImport_BCOP3447.csv", FileMode.Open, FileAccess.Read);

                var detectionRequest = new FileSaveRequest();
                detectionRequest.Files.Add(new FileRequest { FileName = "SigmaImport_BCOP3447.csv", StreamData = stream });
                var result = _TrackerService.SaveDetectionFiles(detectionRequest, "User", true);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }
    }
}