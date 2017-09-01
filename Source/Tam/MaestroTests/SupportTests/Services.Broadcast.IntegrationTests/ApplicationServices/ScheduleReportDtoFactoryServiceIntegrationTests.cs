using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ScheduleReportDtoFactoryServiceIntegrationTests
    {
        private readonly IScheduleAggregateFactoryService _scheduleAggregateFactoryService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IScheduleAggregateFactoryService>();

        readonly IScheduleReportDtoFactoryService _Sut =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IScheduleReportDtoFactoryService>();

        private ScheduleReportDto _NotEquivalizedScheduleReportDto;
        private ScheduleReportDto _EquivalizedNtiScheduleReportDto;
        private ScheduleReportDto _3rdPartyScheduleReportDto;
        public ScheduleReportDto _BlankScheduleClientReportDto;

        public const int EquivalizedNtiScheduleId = 96;
        public const int NotEquivalizedNtiScheduleId = 69;
        public const int ThirdPartyClientScheduleId = 975;
        public const int BlankScheduleWithRelatedIscis = 2365;

        private void GetEquivalizedNtiScheduleReportDto()
        {
            if (_EquivalizedNtiScheduleReportDto == null)
            {
                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                _EquivalizedNtiScheduleReportDto = sut.GenerateScheduleReportDto(EquivalizedNtiScheduleId);
            }
        }

        private void _GetNotEquivalizedScheduleReportDto()
        {
            if (_NotEquivalizedScheduleReportDto == null)
            {
                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                _NotEquivalizedScheduleReportDto = sut.GenerateScheduleReportDto(NotEquivalizedNtiScheduleId);
            }

        }

        private void _Get3rdPartyScheduleReportDto()
        {
            if (_3rdPartyScheduleReportDto == null)
            {
                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                _3rdPartyScheduleReportDto = sut.Generate3rdPartyProviderReportDto(ThirdPartyClientScheduleId);
            }

        }

        private void _GetBlankScheduleClientReportDto()
        {
            if (_BlankScheduleClientReportDto == null)
            {
                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                _BlankScheduleClientReportDto = sut.GenerateClientReportDto(BlankScheduleWithRelatedIscis);
            }

        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_WithMarketRestrictions()
        {
            const int scheduleId = NotEquivalizedNtiScheduleId;
            using (new TransactionScopeWrapper())
            {
                var scheduleRepo = IntegrationTestApplicationServiceFactory
                    .BroadcastDataRepositoryFactory
                    .GetDataRepository<IScheduleRepository>();
                var marketRepo = IntegrationTestApplicationServiceFactory
                    .BroadcastDataRepositoryFactory
                    .GetDataRepository<IMarketRepository>();

                var schedule = GetSchedule(scheduleRepo, scheduleId);

                var markets = marketRepo.GetMarketsByMarketCodes(new List<int>() {106, 104, 165});

                scheduleRepo.UpdateSchedule(schedule);
                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                var report = sut.GenerateScheduleReport(scheduleId);
                File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", report.Stream.GetBuffer());
                //Approvals.Verify(IntegrationTestHelper.ConvertToJson(dto));
            }
        }

        private static schedule GetSchedule(IScheduleRepository scheduleRepo, int scheduleId)
        {
            var schedule = scheduleRepo.GetById(scheduleId);
            var returnSchedule = new schedule();
            returnSchedule.id = schedule.id;
            returnSchedule.name = schedule.name;
            returnSchedule.estimate_id = schedule.estimate_id;
            returnSchedule.advertiser_id = schedule.advertiser_id;
            returnSchedule.posting_book_id = schedule.posting_book_id;

            returnSchedule.start_date = schedule.start_date;
            returnSchedule.end_date = schedule.end_date;
            returnSchedule.created_by = schedule.created_by;
            returnSchedule.created_date = schedule.created_date;

            returnSchedule.modified_by = "Unit test";
            returnSchedule.modified_date = DateTime.Now;
            returnSchedule.post_type = schedule.post_type;
            returnSchedule.equivalized = schedule.equivalized;

            returnSchedule.inventory_source = schedule.inventory_source;
            return returnSchedule;
        }
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_AdvertiserData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.AdvertiserData));
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_WeeklyData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.WeeklyData));
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_StationSummaryData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.StationSummaryData));
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_SpotDetailData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.SpotDetailData));
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_OutOfSpecData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.OutOfSpecToDate));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateEquivalizedNtiReportDto_AdvertiserData()
        {
            GetEquivalizedNtiScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_EquivalizedNtiScheduleReportDto.AdvertiserData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateEquivalizedNtiReportDto_WeeklyData()
        {
            GetEquivalizedNtiScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_EquivalizedNtiScheduleReportDto.WeeklyData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateEquivalizedNtiReportDto_StationSummaryData()
        {
            GetEquivalizedNtiScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_EquivalizedNtiScheduleReportDto.StationSummaryData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateEquivalizedNtiReportDto_SpotDetailData()
        {
            GetEquivalizedNtiScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_EquivalizedNtiScheduleReportDto.SpotDetailData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateEquivalizedNtiReportDto_OutOfSpecData()
        {
            GetEquivalizedNtiScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_EquivalizedNtiScheduleReportDto.OutOfSpecToDate));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Generate3rdPartyReportDto_Advertiser()
        {
            _Get3rdPartyScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_3rdPartyScheduleReportDto.AdvertiserData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Generate3rdPartyReportDto_Weekly()
        {
            _Get3rdPartyScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_3rdPartyScheduleReportDto.WeeklyData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Generate3rdPartyReportDto_StationSummary()
        {
            _Get3rdPartyScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_3rdPartyScheduleReportDto.StationSummaryData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Generate3rdPartyReportDto_SpotDetail()
        {
            _Get3rdPartyScheduleReportDto();
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(IsciDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_3rdPartyScheduleReportDto.SpotDetailData, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Generate3rdPartyReportDto_SpotsAndImpressionsBySource()
        {
            _Get3rdPartyScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_3rdPartyScheduleReportDto.SpotsAndImpressionsBySource));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Generate3rdPartyReportDto_SpotsAndImpressionsByAdvertiser()
        {
            _Get3rdPartyScheduleReportDto();
            Approvals.Verify(
                IntegrationTestHelper.ConvertToJson(_3rdPartyScheduleReportDto.SpotsAndImpressionsDeliveryByAdvertiser));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Generate3rdPartyReportDto_OutOfSpecData()
        {
            _Get3rdPartyScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_3rdPartyScheduleReportDto.OutOfSpecToDate));
        }

        private void _LoadBvsFile()
        {
            var stream = new FileStream(@".\Files\BVS Load For Various Tests.xlsx", FileMode.Open, FileAccess.Read);
            var fileName = "BVS Load For Various Tests.xlsx";

            var bvsRequest = new BvsSaveRequest();
            bvsRequest.UserName = "LoadBvsFile User";
            bvsRequest.BvsFiles.Add(new BvsFile() {BvsFileName = fileName, BvsStream = stream});
            ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

            sut.SaveBvsFiles(bvsRequest);
        }

        private int _CreateBlankScheduleWithRelatedSchedules()
        {
            var blankScheduleId = _CreateBlankSchedule();

            const int estimateId = 333444;
            ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

            var saveRequest = new ScheduleSaveRequest();
            var schedule = new ScheduleDTO();

            schedule.AdvertiserId = 39279;
            schedule.EstimateId = estimateId;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Assembly Schedule For Reporting Tests With Extra Audience Info 1";
            schedule.UserName = "Assembly User";
            schedule.FileName = @"Assembly Schedule For Reporting Tests With Extra Audience Info 1.csv";
            schedule.FileStream =
                new FileStream(@".\Files\Assembly Schedule For Reporting Tests With Extra Audience Info 1.csv",
                    FileMode.Open,
                    FileAccess.Read);

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
                new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}
            };

            schedule.PostType = SchedulePostType.NTI;
            schedule.InventorySource = RatesFile.RateSourceType.Assembly;
            saveRequest.Schedule = schedule;
            sut.SaveSchedule(saveRequest);

            // import the second related schedule
            saveRequest = new ScheduleSaveRequest();
            schedule = new ScheduleDTO();

            schedule.AdvertiserId = 39279;
            schedule.EstimateId = 555666;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Assembly Schedule For Reporting Tests With Extra Audience Info 2";
            schedule.UserName = "Assembly User";
            schedule.FileName = @"Assembly Schedule For Reporting Tests With Extra Audience Info 2.csv";
            schedule.FileStream =
                new FileStream(@".\Files\Assembly Schedule For Reporting Tests With Extra Audience Info 2.csv",
                    FileMode.Open,
                    FileAccess.Read);

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
                new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}
            };

            schedule.PostType = SchedulePostType.NTI;
            schedule.InventorySource = RatesFile.RateSourceType.Assembly;
            saveRequest.Schedule = schedule;
            sut.SaveSchedule(saveRequest);


            return blankScheduleId;
        }

        private int _CreateBlankSchedule()
        {
            int scheduleId;
            var saveRequest = new ScheduleSaveRequest();
            var schedule = new ScheduleDTO();

            schedule.AdvertiserId = 39279;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Blank Schedule";
            schedule.UserName = "User";
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
                new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}
            };
            schedule.PostType = SchedulePostType.NTI;
            schedule.Audiences = new List<int> {33, 61, 53};
            schedule.StartDate = new DateTime(2017, 1, 20);
            schedule.EndDate = new DateTime(2018, 1, 20);
            schedule.IsBlank = true;

            saveRequest.Schedule = schedule;

            ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();
            scheduleId = sut.SaveSchedule(saveRequest);

            return scheduleId;
        }

        private int _ImportSchedule(List<int> marketRestrictions = null,DaypartDto daypartRestrictions = null )
        {
            const int estimateId = 333444;
            _LoadBvsFile();

            ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

            var saveRequest = new ScheduleSaveRequest();
            var schedule = new ScheduleDTO();

            schedule.AdvertiserId = 39279;
            schedule.EstimateId = estimateId;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Assembly Schedule For Reporting Tests";
            schedule.UserName = "Assembly User";
            schedule.FileName = @"Assembly Schedule For Reporting Tests.csv";
            schedule.FileStream = new FileStream(@".\Files\Assembly Schedule For Reporting Tests.csv", FileMode.Open,
                FileAccess.Read);

            schedule.MarketRestrictions = new List<int>();
            if (marketRestrictions != null)
            {
                schedule.MarketRestrictions.AddRange(marketRestrictions);
            }
            schedule.DaypartRestriction = new DaypartDto()
            {
                startTime = 0,endTime = 86400 - 1,mon = true,tue = true,wed = true,thu = true,fri = true,sat = true,sun = true
            };
            if (daypartRestrictions != null)
            {
                schedule.DaypartRestriction = daypartRestrictions;
            }
            schedule.Equivalized = true;
            schedule.ISCIs = new List<IsciDto>
            {
                new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}
            };

            schedule.PostType = SchedulePostType.NTI;
            schedule.InventorySource = RatesFile.RateSourceType.Assembly;
            saveRequest.Schedule = schedule;
            int scheduleId = sut.SaveSchedule(saveRequest);

            return scheduleId;
        }

        private int _ImportRestrictedSchedule()
        {
            const int estimateId = 333444;
            _LoadBvsFile();

            ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

            var saveRequest = new ScheduleSaveRequest();
            var schedule = new ScheduleDTO();

            schedule.AdvertiserId = 39279;
            schedule.EstimateId = estimateId;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Assembly Schedule For Reporting Tests";
            schedule.UserName = "Assembly User";
            schedule.FileName = @"Assembly Schedule For Reporting Tests.csv";
            schedule.FileStream = new FileStream(@".\Files\Assembly Schedule For Reporting Tests.csv", FileMode.Open,
                FileAccess.Read);

            schedule.MarketRestrictions = new List<int>() {209, 420};
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
                new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}
            };

            schedule.PostType = SchedulePostType.NTI;
            schedule.InventorySource = RatesFile.RateSourceType.Assembly;
            saveRequest.Schedule = schedule;
            int scheduleId = sut.SaveSchedule(saveRequest);

            return scheduleId;
        }

        private static void _VerifyReportData(ScheduleReportDto reportDto)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ScheduleReportDto), "ScheduleId");
            jsonResolver.Ignore(typeof(IsciDto), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(reportDto, jsonSettings);
            Approvals.Verify(json);
        }

        /// <summary>
        /// This test now covers issue BB-458.  Earlier it wasn't due to _CreateBlankSchedule not being set up properly.
        /// </summary>
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_BlankSchedule_ClientReport()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                _ImportSchedule(); // basic setup
                var scheduleId = _CreateBlankSchedule();

                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                reportDto = sut.GenerateClientReportDto(scheduleId);
                // 2 commented lines below are useful for debugging full report
                var report = sut.GenerateClientReport(scheduleId);
                File.WriteAllBytes(string.Format("..\\Client_Report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            }
            _VerifyReportData(reportDto);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_GenerateScheduleReportDto()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                var scheduleId = _ImportSchedule();
                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                reportDto = sut.GenerateScheduleReportDto(scheduleId);
                // 2 commented lines below are useful for debugging full report
                //var report = sut.GenerateScheduleReport(scheduleId);
                //File.WriteAllBytes(string.Format("..\\Schdule_Report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            }
            _VerifyReportData(reportDto);
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_GenerateClientReportDto()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                var scheduleId = _ImportSchedule();
                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                reportDto = sut.GenerateClientReportDto(scheduleId);
            }
            _VerifyReportData(reportDto);
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_Generate3rdPartyProviderReportDto()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                var scheduleId = _ImportSchedule();
                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                reportDto = sut.Generate3rdPartyProviderReportDto(scheduleId);
                //var report = sut.Generate3rdPartyProviderReport(scheduleId);
                //File.WriteAllBytes(string.Format("..\\3rdPartyReport{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            }
            _VerifyReportData(reportDto);
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_Generate3rdPartyProviderReportDto_With_Restrictions_BB554()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                var scheduleId = _ImportRestrictedSchedule(); // basic setup

                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();

                reportDto = sut.Generate3rdPartyProviderReportDto(scheduleId);
                //var report = sut.Generate3rdPartyProviderReport(scheduleId);
                //File.WriteAllBytes(string.Format("..\\3rdPartyReport{0}.xlsx", scheduleId), report.Stream.GetBuffer());
            }
            _VerifyReportData(reportDto);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_Market_Datepart_Restrictions_BB294()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                var scheduleId = _ImportRestrictedSchedule(); // basic setup

                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();

                reportDto = sut.GenerateScheduleReportDto(scheduleId);
//                var report = sut.GenerateScheduleReport(scheduleId);
//                File.WriteAllBytes(string.Format("..\\report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            }
            _VerifyReportData(reportDto);
        }

        private int _CreateScheduleWithRelatedSchedules()
        {
            var mainScheduleId = _ImportSchedule(new List<int>() { 209, 117 }); // markets ST LOUIS and Charlotte
            
            const int estimateId = 333441;
            ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

            var saveRequest = new ScheduleSaveRequest();
            var schedule = new ScheduleDTO();

            schedule.AdvertiserId = 39279;
            schedule.EstimateId = estimateId;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Assembly Schedule For Reporting Tests With Extra Audience Info 1";
            schedule.UserName = "Assembly User";
            schedule.FileName = @"Assembly Schedule For Reporting Tests With Extra Audience Info 1.csv";
            schedule.FileStream =
                new FileStream(@".\Files\Assembly Schedule For Reporting Tests With Extra Audience Info 1.csv",
                    FileMode.Open,
                    FileAccess.Read);

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
                new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}    
            };

            schedule.PostType = SchedulePostType.NTI;
            schedule.InventorySource = RatesFile.RateSourceType.Assembly;
            saveRequest.Schedule = schedule;
            sut.SaveSchedule(saveRequest);

            // import the second related schedule
            saveRequest = new ScheduleSaveRequest();
            schedule = new ScheduleDTO();

            schedule.AdvertiserId = 39279;
            schedule.EstimateId = 555666;
            schedule.PostingBookId = 413;
            schedule.ScheduleName = "Assembly Schedule For Reporting Tests With Extra Audience Info 2";
            schedule.UserName = "Assembly User";
            schedule.FileName = @"Assembly Schedule For Reporting Tests With Extra Audience Info 2.csv";
            schedule.FileStream =
                new FileStream(@".\Files\Assembly Schedule For Reporting Tests With Extra Audience Info 2.csv",
                    FileMode.Open,
                    FileAccess.Read);

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
                new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}
            };

            schedule.PostType = SchedulePostType.NTI;
            schedule.InventorySource = RatesFile.RateSourceType.Assembly;
            saveRequest.Schedule = schedule;
            sut.SaveSchedule(saveRequest);

            return mainScheduleId;
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_ClientReport_Market_Datepart_Restrictions_BB562()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                var scheduleId = _CreateScheduleWithRelatedSchedules();

                ISchedulesReportService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();

                reportDto = sut.GenerateClientReportDto(scheduleId);
                //var report = sut.GenerateClientReport(scheduleId);
                //File.WriteAllBytes(string.Format("..\\report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            }
            _VerifyReportData(reportDto);
        }


        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_BlankSchedule_ClientReport_Additional_Audiences_BB459()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                _ImportSchedule(); // basic setup
                var scheduleId = _CreateBlankScheduleWithRelatedSchedules();

                ISchedulesReportService sut =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                reportDto = sut.GenerateClientReportDto(scheduleId);
                // 2 commented lines below are useful for debugging full report
                //var report = sut.GenerateClientReport(scheduleId);
                //File.WriteAllBytes(string.Format("..\\Client_Report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
            }
            _VerifyReportData(reportDto);
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_BlankSchedule_ClientReport_Additional_Audiences_BB368_481()
        {
            _GetBlankScheduleClientReportDto();
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(IsciDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_BlankScheduleClientReportDto, jsonSettings));
        }

        /// <summary>
        /// This test uses already loaded files:
        ///     BVS_For_Mapped.xlsx
        ///     Assembly Schedule For Mapping.csv
        /// It also already mapped bvs program names "JUDGE ALEX" and "CRIME WATCH DAILY WITH CHRIS HANSEN" to "VARIOUS" schedule program
        /// </summary>
        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_ClientReport_Handle_Mapped_ProgramNames_BB507()
        {
            const int preloadedScheduleId = 3420;

            ISchedulesReportService sut =
                IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
            var reportDto = sut.GenerateClientReportDto(preloadedScheduleId);
            // 2 commented lines below are useful for debugging full report
            //var report = sut.GenerateClientReport(scheduleId);
            //File.WriteAllBytes(string.Format("..\\Client_Report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());

            _VerifyReportData(reportDto);
    }
}
}
