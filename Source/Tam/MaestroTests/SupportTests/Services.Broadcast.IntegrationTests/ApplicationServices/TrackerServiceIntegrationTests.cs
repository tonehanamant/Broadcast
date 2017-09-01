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
using System.Text;
using System.Transactions;
using Services.Broadcast.Entities.spotcableXML;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class TrackerServiceIntegrationTests
    {
        private ITrackerService _sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

        private ITrackingEngine _sutEngine =
            IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackingEngine>();

        private IScheduleRepository _ScheduleRepository =
            IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();

        private IRatingAdjustmentsRepository _RatingAdjustmentsRepository =
            IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IRatingAdjustmentsRepository>();

        private IBvsRepository _BvsRepository =
            IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();

        [Test]
        [Ignore]
        public void CreateLeadInAndBlockTestData()
        {
            using (new TransactionScopeWrapper())
            {

                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IBvsTestDataGeneratorRepository>().CreateLeadInAndBlockTestData();
                _sutEngine.TrackBvsByEstimateId(8675309);
            }
        }

        [Test]
        [Ignore]
        public void CreateOverflowSpotAssignTestData()
        {
            using (new TransactionScopeWrapper())
            {

                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IBvsTestDataGeneratorRepository>().CreateOverflowSpotAssignTestData();
            }
        }

        [Test]
        [Ignore]
        public void CreateStationMatchWithoutTimeMatch()
        {
            using (new TransactionScopeWrapper())
            {

                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IBvsTestDataGeneratorRepository>().CreateStationMatchWithoutTimeMatchData();
            }
        }

        [Test]
        public void AcceptAsLeadin()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new AcceptScheduleLeadinRequest();
                request.BvsDetailId = 9629;
                request.ScheduleDetailWeekId = 208275;

                var bvsDetail = _sutEngine.AcceptScheduleLeadIn(request);

                Assert.AreEqual(request.ScheduleDetailWeekId, bvsDetail.ScheduleDetailWeekId);
                Assert.AreEqual(true, bvsDetail.LinkedToLeadin);
                Assert.AreEqual(TrackingStatus.InSpec, bvsDetail.Status);
            }
        }

        [Test]
        public void AcceptAsBlock()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new AcceptScheduleBlockRequest();
                request.BvsDetailId = 9629;
                request.ScheduleDetailWeekId = 208275;

                var bvsDetail = _sutEngine.AcceptScheduleBlock(request);

                Assert.AreEqual(request.ScheduleDetailWeekId, bvsDetail.ScheduleDetailWeekId);
                Assert.AreEqual(true, bvsDetail.LinkedToBlock);
                Assert.AreEqual(TrackingStatus.InSpec, bvsDetail.Status);
            }
        }

        [Test]
        [Ignore]
        public void DoIt()
        {
            _sutEngine.TrackBvsByBvsDetails(new List<int>() {8090}, 3622);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_FollowingScheduleMatch()
        {
            using (new TransactionScopeWrapper())
            {

                var actual = _sutEngine.GetProgramMappingDto(10181);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_PrimaryScheduleMatch()
        {
            var response = _sutEngine.GetProgramMappingDto(10053);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramMappingDto_FullScenario()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sutEngine.GetProgramMappingDto(10545);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDisplaySchedules()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {

                var actual = _sut.GetSchedulesByDate(new DateTime(2000, 1, 1), new DateTime(2016, 01, 01));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplaySchedule), "Id");
                jsonResolver.Ignore(typeof(DisplaySchedule), "IsBlank");
                var jsonSettings = new JsonSerializerSettings()
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

                var months = _sut.GetNsiPostingBookMonths();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(months));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubBvsData()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {

                var response = _sut.GetBvsScrubbingData(3417);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
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
                _sutEngine.TrackBvsByEstimateId(7618419);

                _sutEngine.TrackBvsByBvsDetails(new List<int> {10550}, 7618419);

                var scheduleDetailWeek1 =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                        .GetDataRepository<IScheduleRepository>().GetScheduleDetailWeek(369430);
                var scheduleDetailWeek2 =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                        .GetDataRepository<IScheduleRepository>().GetScheduleDetailWeek(369431);

                var bvsDetails =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>()
                        .GetBvsTrackingDetailsByEstimateId(7618419);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(new
                {
                    Week1 = scheduleDetailWeek1,
                    Week2 = scheduleDetailWeek2,
                    BvsDetails = bvsDetails,
                }));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TrackBvsData_OverflowLink()
        {
            using (new TransactionScopeWrapper())
            {
                _sutEngine.TrackBvsByEstimateId(7618419);

                var actual =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>()
                        .GetBvsTrackingDetailsByEstimateId(7618419);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        public void test()
        {
            using (new TransactionScopeWrapper())
            {
                // this guy has a schedule
                var actual = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IBvsRepository>()
                    .GetEstimateIdsWithSchedulesByFileIds(new List<int>() {313});
                Assert.AreEqual(3909, actual.First());

                // this guy no schedule
                actual = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IBvsRepository>()
                    .GetEstimateIdsWithSchedulesByFileIds(new List<int>() {48});

                Assert.IsEmpty(actual);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TrackBvsData_NoTimeOverlap()
        {
            using (new TransactionScopeWrapper())
            {
                _sutEngine.TrackBvsByEstimateId(7985133);

                var actual =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>()
                        .GetBvsTrackingDetailsByEstimateId(7985133);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TrackBvsData()
        {
            using (new TransactionScopeWrapper())
            {
                _sutEngine.TrackBvsByEstimateId(3447);

                var actual =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>()
                        .GetBvsTrackingDetailsByEstimateId(3447);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubBvsData_WithNSIDateChanged()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {

                var response = _sut.GetBvsScrubbingData(3416);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TrackBvsData_WithNSIDateChanged()
        {
            using (new TransactionScopeWrapper())
            {
                _sutEngine.TrackBvsByEstimateId(3416);

                var actual =
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>()
                        .GetBvsTrackingDetailsByEstimateId(3416);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(actual));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleDetails()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sut.GetScheduleDetailsByEstimateId(3417);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleHeader()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sut.GetScheduleHeader(3417);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleStations()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sut.GetScheduleStations(69);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
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

                var response = _sut.GetSchedulePrograms(69);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadSchedule()
        {
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = 3390;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Checkers 2Q16 SYN - Scheduler (tv)";
                schedule.UserName = "User";
                schedule.FileName = @"Checkers 2Q16 SYN - Scheduler (tv).scx";
                schedule.FileStream = new FileStream(@".\Files\Checkers 2Q16 SYN - Scheduler (tv).scx", FileMode.Open,
                    FileAccess.Read);

                schedule.MarketRestrictions = new List<int>() {101, 102};
                // restrict NYC and Binghamton just because reasons
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,
                    endTime = 86400 - 1,
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
                        House = "1111",
                        Client = "2222"
                    }
                };

                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;

                saveRequest.Schedule = schedule;

                _sut.SaveSchedule(saveRequest);

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
        [UseReporter(typeof(DiffReporter))]
        public void LoadAssemblySchedule()
        {
            const int Estimate_Id = 3390;
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = Estimate_Id;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Assembly Schedule Template";
                schedule.UserName = "User";
                schedule.FileName = @"Assembly Schedule Template.csv";
                schedule.FileStream = new FileStream(@".\Files\Assembly Schedule Template.csv", FileMode.Open,
                    FileAccess.Read);

                schedule.MarketRestrictions = new List<int>() {101, 102};
                // restrict NYC and Binghamton just because reasons
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,
                    endTime = 86400 - 1,
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
                        House = "1111",
                        Client = "2222"
                    }
                };
                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;

                saveRequest.Schedule = schedule;

                _sut.SaveSchedule(saveRequest);

                var response = _ScheduleRepository.GetScheduleTrackingDetails(Estimate_Id);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ScheduleDetailWeek), "ScheduleDetailWeekId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(response, jsonSettings);
                Approvals.Verify(json);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadAssemblySchedule_WithInvalidWomanAndFemaleDemos()
        {
            const int Estimate_Id = 3390;
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = Estimate_Id;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Assembly Schedule Template Invalid Woman and Female Demos";
                schedule.UserName = "User";
                schedule.FileName = @"Assembly Schedule Template Invalid Woman and Female Demos.csv";
                schedule.FileStream =
                    new FileStream(@".\Files\Assembly Schedule Template Invalid Woman and Female Demos.csv",
                        FileMode.Open,
                        FileAccess.Read);

                schedule.MarketRestrictions = new List<int>() {101, 102};
                // restrict NYC and Binghamton just because reasons
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,
                    endTime = 86400 - 1,
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
                        House = "1111",
                        Client = "2222"
                    }
                };
                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;

                saveRequest.Schedule = schedule;

                try
                {
                    _sut.SaveSchedule(saveRequest);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("Could not find a matching audience for "))
                        throw;
                }
            }
        }

        [Test]
        public void LoadBvs()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                var fileName = "BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});
                _sut.SaveBvsFiles(bvsRequest);
            }
        }

        [Test]
        public void LoadBvs_Bad_Campaign()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Bad Campaign.xlsx",
                    FileMode.Open,
                    FileAccess.Read);
                var fileName = "BVS Bad Campaign.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});
                try
                {
                    _sut.SaveBvsFiles(bvsRequest);
                }
                catch (Exception e)
                {
                    Assert.IsTrue(e.Message.Contains("Invalid campaign field in row="),
                        "Epected to find exception message saying campaign was invalid.");
                    return;
                }
                throw new Exception("Expected error regarding campaign number, but didn't happen.");
            }
        }

        [Test]
        public void LoadBvs_With_Bad_Rank()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Empty Rank.xlsx", FileMode.Open,
                    FileAccess.Read);
                var fileName = "BVS Beiersdorf_Hersheys Empty Rank.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});
                try
                {
                    _sut.SaveBvsFiles(bvsRequest);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Invalid rank value "))
                        return;
                }
            }
            throw new Exception("Expected rank exception, but never happened.");
        }

        [Test]
        public void LoadBvs_Bad_Spot_Length_Lookup()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Bad Spot Length.xlsx", FileMode.Open,
                    FileAccess.Read);
                var fileName = "BVS Beiersdorf_Hersheys Bad Spot Length.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});

                Assert.Throws<Exception>(() => _sut.SaveBvsFiles(bvsRequest));
            }
        }

        [Test]
        public void LoadBvs_MultiFile_WithErrors()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Bad Spot Length.xlsx", FileMode.Open,
                    FileAccess.Read);
                var fileName = "BVS Beiersdorf_Hersheys Bad Spot Length.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});

                stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx", FileMode.Open,
                    FileAccess.Read);
                fileName = "BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx";
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});

                stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Empty Rank.xlsx", FileMode.Open,
                    FileAccess.Read);
                fileName = "BVS Beiersdorf_Hersheys Empty Rank.xlsx";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});

                try
                {
                    _sut.SaveBvsFiles(bvsRequest);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Invalid rank value ") &&
                        e.Message.Contains("<br /> Error processing file "))
                    {
                        Console.WriteLine(e);
                        return;
                    }
                }
                throw new Exception("Expected an error but didn't happen");
            }
        }

        [Test]
        public void LoadBvs_Empty_File_Error()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS empty file.xlsx", FileMode.Open, FileAccess.Read);
                var fileName = "BVS empty file.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});

                try
                {
                    _sut.SaveBvsFiles(bvsRequest);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("File does not contain valid BVS"))
                        return;
                }
            }
            throw new Exception("Expected empty file error but didn't happen");
        }


        [Test]
        [Ignore]
        public void LoadFTP()
        {
            // not part of auto-run test, only for manual testing
            string msg;
            using (new TransactionScopeWrapper())
            {
                msg = _sut.SaveBvsViaFtp("test_user");
            }
            Console.WriteLine(msg);
        }

        [Test]
        public void ScheduleExists()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sut.ScheduleExists(3417);

                Assert.AreEqual(response, true);
            }
        }

        [Test]
        public void ScheduleDoesNotExists()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sut.ScheduleExists(876165456);

                Assert.AreEqual(response, false);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScrubbingData()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var response = _sut.GetBvsScrubbingData(3417);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
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

                var response = _sut.GetScheduleStations(69);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
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

                var response = _sut.GetSchedulePrograms(69);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
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
            var scrub = new ScrubbingMap()
            {
                BvsStation = "WNBA",
                ScheduleStation = "WNBA-TV",
            };
            scrub.DetailIds.Add(9629);

            _sut.SaveScrubbingMapping(scrub);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleAudiences()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sut.GetScheduleAudiences(3417);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramScrubbingMapList()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sut.GetBvsMapByType("Program");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStationScrubbingMapList()
        {
            using (new TransactionScopeWrapper())
            {

                var response = _sut.GetBvsMapByType("Station");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DeleteProgramMapping()
        {
            using (new TransactionScopeWrapper())
            {
                var mapping = new TrackingMapValue()
                {
                    BvsValue = "10 NEWS 6PM SATURDAY",
                    ScheduleValue = "10 NEWS 6P SAT"
                };
                _sut.DeleteMapping("Program", mapping);
                var response = _sut.GetBvsMapByType("Program");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DeleteStationMapping()
        {
            using (new TransactionScopeWrapper())
            {
                var mapping = new TrackingMapValue()
                {
                    BvsValue = "ADSM",
                    ScheduleValue = "ADSM+S2"
                };
                _sut.DeleteMapping("Station", mapping);
                var response = _sut.GetBvsMapByType("Station");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBvsInitialOptions()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var response = _sut.GetBvsLoadData(new DateTime(2017, 05, 05));
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(Quarter), "Id");
                jsonResolver.Ignore(typeof(BvsLoadDto), "CurrentQuarter");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
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
                schedule.InventorySource = RatesFile.RateSourceType.TVB;
                schedule.PostType = SchedulePostType.NTI;
                saveRequest.Schedule = schedule;

                _sut.SaveSchedule(saveRequest);

            }
            finally
            {
                //_ScheduleRepository.DeleteSchedule(3390);
            }
        }

        [Test]
        [Ignore] //For loading data only - not for testing
        public void LoadScheduleBvsDataForTestingClientReport()
        {

            var bvsRequest = new BvsSaveRequest();

            var stream = new FileStream(@".\Files\Checkers BVS Report - Estimate4401.DAT", FileMode.Open,
                FileAccess.Read);
            bvsRequest.BvsFiles.Add(new BvsFile()
            {
                BvsStream = stream,
                BvsFileName = "User"
            });

            _sut.SaveBvsFiles(bvsRequest);
        }

        const int Tracker_Test_Estimate_Id = 121220;

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadBvs_Load_Assembly_Tracker()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS_Data_for_Tracking.xlsx", FileMode.Open, FileAccess.Read);

                var fileName = "BVS_Data_For_Tracking.DAT";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});

                _sut.SaveBvsFiles(bvsRequest);

                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = Tracker_Test_Estimate_Id;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Assembly Schedule Template";
                schedule.UserName = "User";
                schedule.FileName = @"BVS Assembly Schedule For Tracking.csv";
                schedule.FileStream = new FileStream(@".\Files\BVS Assembly Schedule For Tracking.csv", FileMode.Open,
                    FileAccess.Read);

                schedule.MarketRestrictions = new List<int>() {101, 102};
                    // restrict NYC and Binghamton just because reasons
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,
                    endTime = 86400 - 1,
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

                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;

                saveRequest.Schedule = schedule;

                _sut.SaveSchedule(saveRequest);
                var actual = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<IBvsRepository>()
                    .GetBvsTrackingDetailsByEstimateId(Tracker_Test_Estimate_Id);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(BvsTrackingDetail), "Id");
                jsonResolver.Ignore(typeof(BvsTrackingDetail), "ScheduleDetailWeekId");
                var jsonSettings = new JsonSerializerSettings()
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
        public void Load_SCXWithBlankSpots()
        { //BCOP-1571
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();
                schedule.AdvertiserId = 39279;
                schedule.EstimateId = 44001;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Checkers 2Q16 SYN - Estimate44001 With Empty Spots.scx";
                schedule.UserName = "User";
                schedule.FileName = @"Checkers 2Q16 SYN - Estimate44001 With Empty Spots.scx";
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
                schedule.FileStream = new FileStream(@".\Files\Checkers 2Q16 SYN - Estimate44001 With Empty Spots.scx", FileMode.Open,
                    FileAccess.Read);
                schedule.InventorySource = RatesFile.RateSourceType.OpenMarket;
                schedule.PostType = SchedulePostType.NSI;
                saveRequest.Schedule = schedule;
                var scheduleId = _sut.SaveSchedule(saveRequest);

                ISchedulesReportService reportService =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();

                var actual = reportService.GenerateScheduleReportDto(scheduleId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ScheduleReportDto), "ScheduleId");
                var jsonSettings = new JsonSerializerSettings()
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
        public void CreateBlankSchedule()
        {
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Blank Schedule";
                schedule.UserName = "User";
                schedule.MarketRestrictions = new List<int>() {101, 102};
                    // restrict NYC and Binghamton just because reasons
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,
                    endTime = 86400 - 1,
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

                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;
                schedule.Audiences = new List<int> {31, 42};
                schedule.StartDate = new DateTime(2017, 1, 20);
                schedule.EndDate = new DateTime(2018, 1, 20);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId = _sut.SaveSchedule(saveRequest);
                var actual = _ScheduleRepository.GetDisplayScheduleById(scheduleId);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(DisplaySchedule), "Id");
                jsonResolver.Ignore(typeof(IsciDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
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

                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;
                schedule.Audiences = new List<int> {31, 42};
                schedule.StartDate = new DateTime(2016, 3, 28);
                schedule.EndDate = new DateTime(2016, 4, 24);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId = _sut.SaveSchedule(saveRequest);

                Console.WriteLine(scheduleId);
                tran.Complete();
            }
        }

        [Test]
        [Ignore]
        public void LoadBvs_Quickload()
        {
            var stream = new FileStream(@".\Files\BVS Quick Load.xlsx", FileMode.Open,
                FileAccess.Read);
            var fileName = "BVS Quick Load.xlsx";

            var bvsRequest = new BvsSaveRequest();
            bvsRequest.UserName = "User";
            bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});
            _sut.SaveBvsFiles(bvsRequest);
        }

        /// <summary>
        /// This test loads 2 schdules (using BVS from "BVS Quick Load.xlsx" which should already be loaded)
        /// First schedule has 2 ISCIS and 2 audiences.
        /// Second schedule has 1 ISCI and 1 audience.
        /// The point of this test is to ensure the post logic of the second schedule does 
        /// not affect the first schedule.
        /// </summary>
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
                schedule.MarketRestrictions = new List<int>() {101, 102};
                    // restrict NYC and Binghamton just because reasons
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,
                    endTime = 86400 - 1,
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
                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;
                schedule.Audiences = new List<int> {31, 42};
                schedule.StartDate = new DateTime(2017, 1, 20);
                schedule.EndDate = new DateTime(2018, 1, 20);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId1 = _sut.SaveSchedule(saveRequest);

                // create second schedule contain one of the 2 ISCIS from the first schedule and one of the 2 audiences
                saveRequest = new ScheduleSaveRequest();
                schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Blank Schedule 2";
                schedule.UserName = "User";
                schedule.MarketRestrictions = new List<int>() {101, 102};
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,
                    endTime = 86400 - 1,
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
                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;
                schedule.Audiences = new List<int> {31};
                schedule.StartDate = new DateTime(2017, 1, 20);
                schedule.EndDate = new DateTime(2018, 1, 20);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId2 = _sut.SaveSchedule(saveRequest);

                // now we assert results
                // first schedule
                var efSchedule = _sut.GetDisplayScheduleById(scheduleId1);
                var estimates = _BvsRepository.GetEstimateIdsByIscis(efSchedule.Iscis.Select(ib => ib.House).ToList());
                var actual = new List<BvsPostDetailAudience>();

                estimates.ForEach(
                    estimateId => actual.AddRange(_BvsRepository.GetBvsPostDetailAudienceByEstimateId(estimateId)));

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(BvsPostDetailAudience), "BvsDetailId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                // second schedule
                efSchedule = _sut.GetDisplayScheduleById(scheduleId2);

                estimates = _BvsRepository.GetEstimateIdsByIscis(efSchedule.Iscis.Select(ib => ib.House).ToList());
                //actual = new List<BvsPostDetailAudience>();

                estimates.ForEach(
                    estimateId => actual.AddRange(_BvsRepository.GetBvsPostDetailAudienceByEstimateId(estimateId)));
                
                var json = IntegrationTestHelper.ConvertToJson(actual, jsonSettings);
                Console.WriteLine(json);
                Approvals.Verify(json);
            }
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "All ISCIs must have a client ISCI",
             MatchType = MessageMatch.Contains)]
        public void CreateScheduleWithHouseIscWithoutClientIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Incomplete ISCI Schedule";
                schedule.UserName = "User";
                schedule.Equivalized = true;

                schedule.ISCIs = new List<IsciDto>
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
                };

                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;
                schedule.Audiences = new List<int> {31, 42};
                schedule.StartDate = new DateTime(2017, 1, 20);
                schedule.EndDate = new DateTime(2018, 1, 20);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId = _sut.SaveSchedule(saveRequest);
            }
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "All ISCIs must have a house ISCI",
             MatchType = MessageMatch.Contains)]
        public void CreateScheduleWithClientIscWithoutHouseIsci()
        {
            using (new TransactionScopeWrapper())
            {
                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "Incomplete ISCI Schedule";
                schedule.UserName = "User";
                schedule.Equivalized = true;

                schedule.ISCIs = new List<IsciDto>
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
                };

                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.CNN;
                schedule.Audiences = new List<int> {31, 42};
                schedule.StartDate = new DateTime(2017, 1, 20);
                schedule.EndDate = new DateTime(2018, 1, 20);
                schedule.IsBlank = true;

                saveRequest.Schedule = schedule;

                var scheduleId = _sut.SaveSchedule(saveRequest);
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
            schedule.MarketRestrictions = new List<int>() {101, 102};
                // restrict NYC and Binghamton just because reasons
            schedule.DaypartRestriction = new DaypartDto()
            {
                startTime = 0,
                endTime = 86400 - 1,
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

            schedule.PostType = SchedulePostType.NTI;
            schedule.InventorySource = RatesFile.RateSourceType.CNN;
            schedule.Audiences = new List<int> {31, 42};
            schedule.StartDate = new DateTime(2017, 1, 20);
            schedule.EndDate = new DateTime(2018, 1, 20);
            schedule.IsBlank = true;

            saveRequest.Schedule = schedule;

            _sut.SaveSchedule(saveRequest);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetScheduleSettings()
        {
            var schedule = _ScheduleRepository.GetScheduleDtoByEstimateId(3910);
            var displaySchedule = _sut.GetDisplayScheduleById(schedule.Id);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplaySchedule), "Id");
            jsonResolver.Ignore(typeof(IsciDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
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
            schedule.MarketRestrictions = new List<int>() {101, 102};
                // restrict NYC and Binghamton just because reasons
            schedule.DaypartRestriction = new DaypartDto()
            {
                startTime = 0,
                endTime = 86400 - 1,
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

            schedule.PostType = SchedulePostType.NTI;
            schedule.InventorySource = RatesFile.RateSourceType.CNN;
            schedule.Audiences = new List<int> {31, 42};
            schedule.StartDate = new DateTime(2017, 1, 20);
            schedule.EndDate = new DateTime(2018, 1, 20);
            schedule.IsBlank = true;

            saveRequest.Schedule = schedule;

            _sut.SaveSchedule(saveRequest);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateSchedule()
        {
            const int Estimate_Id = 4000;
            using (new TransactionScopeWrapper())
            {
                var efSchedule = _ScheduleRepository.GetScheduleDtoByEstimateId(Estimate_Id);
                string json = null;

                if (efSchedule != null)
                {
                    var saveRequest = new ScheduleSaveRequest();
                    var schedule = new ScheduleDTO();

                    schedule.EstimateId = Estimate_Id;
                    schedule.IsBlank = false;
                    schedule.ScheduleName = "Test Update (UPDATED)";
                    schedule.UserName = "User";

                    schedule.ISCIs = new List<IsciDto>
                    {
                        new IsciDto
                        {
                            House = "1111",
                            Client = "2222"
                        }
                    };

                    schedule.AdvertiserId = 37674;
                    schedule.PostingBookId = 377;
                    schedule.MarketRestrictions = new List<int> {125, 390};
                    schedule.DaypartRestriction = new DaypartDto()
                    {
                        startTime = 0,
                        endTime = 86400 - 1,
                        mon = false,
                        tue = true,
                        wed = true,
                        thu = true,
                        fri = true,
                        sat = true,
                        sun = true
                    };
                    schedule.PostType = SchedulePostType.NSI;
                    schedule.Equivalized = false;
                    schedule.Audiences = new List<int> {31, 42};
                    schedule.InventorySource = RatesFile.RateSourceType.OpenMarket;

                    saveRequest.Schedule = schedule;
                    _sut.SaveSchedule(saveRequest);

                    var scheduleId = _ScheduleRepository.GetScheduleDtoByEstimateId(Estimate_Id).Id;
                    var response = _ScheduleRepository.GetDisplayScheduleById(scheduleId);

                    var jsonResolver = new IgnorableSerializerContractResolver();
                    jsonResolver.Ignore(typeof(DisplaySchedule), "Id");
                    jsonResolver.Ignore(typeof(IsciDto), "Id");
                    var jsonSettings = new JsonSerializerSettings()
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
            string json = null;
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
        public void SaveBvsFiles_Twice_HasCorrect_DuplicateMessage()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx",
                    FileMode.Open, FileAccess.Read);
                const string fileName = "BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});
                _sut.SaveBvsFiles(bvsRequest);


                var stream2 = new FileStream(@".\Files\BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle2.xlsx",
                    FileMode.Open, FileAccess.Read);
                const string fileName2 = "BVS Beiersdorf_Hersheys Week of 3_6 BVS Test FIle2.xlsx";

                var bvsRequest2 = new BvsSaveRequest();
                bvsRequest2.UserName = "User";
                bvsRequest2.BvsFiles.Add(new BvsFile() {BvsFileName = fileName2, BvsStream = stream2});
                var dontIgnore = _sut.SaveBvsFiles(bvsRequest2).Item2;

                const string correctMessage =
                    @"<p>The following line(s) were previously imported and were ignored:</p><ul><li>Line 10: Station KTVK, Date 3/13/2017 12:00:00 AM, Time Aired 19302, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li><li>Line 11: Station KPLR, Date 3/13/2017 12:00:00 AM, Time Aired 20217, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li><li>Line 12: Station WAXN, Date 3/13/2017 12:00:00 AM, Time Aired 19770, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li><li>Line 14: Station KCWI, Date 3/13/2017 12:00:00 AM, Time Aired 21515, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li><li>Line 15: Station WGMB, Date 3/13/2017 12:00:00 AM, Time Aired 21541, ISCI 38VA45BK95H, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY</li></ul><p>The following line(s) were previously imported and were updated with new program name:</p><ul><li>Line 9: Station WMOR, Date 3/13/2017 12:00:00 AM, Time Aired 18532, ISCI 38VA49CE08, Spot Length 30, Campaign 3638, Advertiser BEIERS HERSHEY, Program Name JUDGE ALEXNEWPROGRAMNAME</li></ul>";
                Assert.AreEqual(correctMessage, dontIgnore);
            }
        }

        private void _ImportBVSFile(string fileName)
        {
            var stream = new FileStream(@".\Files\" + fileName, FileMode.Open, FileAccess.Read);

            var bvsRequest = new BvsSaveRequest();
            bvsRequest.UserName = "INT Test User";
            bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});
            _sut.SaveBvsFiles(bvsRequest);
        }

        private int _ImportOvernightSchedule()
        {
            const int estimateId = 794613;
            const string fileName = "Overnight and lead ins.csv";

            ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

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
            schedule.DaypartRestriction = new DaypartDto()
            {
                startTime = 0,
                endTime = 86400 - 1,
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

            schedule.PostType = SchedulePostType.NSI;
            schedule.InventorySource = RatesFile.RateSourceType.Assembly;
            saveRequest.Schedule = schedule;
            int scheduleId = sut.SaveSchedule(saveRequest);

            return scheduleId;

        }


        /// <summary>
        ///The point of this test is to check air time logic for both normal and overnight with lead in programs.
        /// The bvs record "Sunday Night ALEX" will be out of spec because it's NSI date is on the 19th which is Sunday night
        /// and outside for all schedule dayparts (but closest would be the schedule's record of same program name).
        /// 
        /// This test also covers BCOP-1679 which fails to show BVS out of spec records that fall outside the schedule's weeks.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Tracker_Days_Overnight_And_Lead_Ins()
        {
            using (new TransactionScopeWrapper())
            {
                _ImportBVSFile("Ovenight_Leadin_Stuff.xlsx");
                int scheduleId = _ImportOvernightSchedule();

                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                var reportDto = sut.GenerateClientReportDto(scheduleId);


                //var report = sut.GenerateScheduleReport(scheduleId);
                //File.WriteAllBytes(string.Format("..\\Sched_Report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());


                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ScheduleReportDto), "ScheduleId");
                jsonResolver.Ignore(typeof(IsciDto), "Id");
                var jsonSettings = new JsonSerializerSettings()
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
                var bvsDetails = _BvsRepository.GetBvsTrackingDetailsByDetailIds(new List<int>() {86438});

                var detail = bvsDetails.First();

                ScrubbingMap map = new ScrubbingMap();
                map.EstimateId = detail.EstimateId;

                map.BvsProgram = "GOOD MORNING ARIZONA 5AM";
                map.ScheduleProgram = "VARIOUS";
                map.DetailIds = new List<int>() { 86438 };

                var trackingDetails = _sut.SaveScrubbingMapping(map).First();

                // ensure it mapped properly
                Assert.AreEqual(TrackingStatus.InSpec,trackingDetails.Status,"Test failed due to bad mapping");
                // remove map
                _sut.DeleteMapping("Program", new TrackingMapValue() {BvsValue = "GOOD MORNING ARIZONA 5AM", ScheduleValue = "VARIOUS"});

                _sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

                // retract and ensure the bvs detail status is back to outofspec
                _sut.TrackSchedule(3420);

                detail = _BvsRepository.GetBvsTrackingDetailsByDetailIds(new List<int>() { 86438 }).First();
                Assert.AreEqual(TrackingStatus.OutOfSpec, detail.Status);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]

        public void Delete_BVS_File()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Load For Various Tests.xlsx", FileMode.Open, FileAccess.Read);
                var fileName = "BVS Load For Various Tests.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "LoadBvsFile User";
                bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});
                ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

                int bvsFileId = sut.SaveBvsFiles(bvsRequest).Item1.First();

                sut.DeleteBvsFile(bvsFileId);
                var bvsFiles = sut.GetBvsFileSummaries().ToList();
                // ensure newly created bvs file cannot be read.
                Assert.IsEmpty(bvsFiles.Where(b => b.Id == bvsFileId));
            }
        }
    }
}