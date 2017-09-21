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
using Tam.Maestro.Services.ContractInterfaces.Common;

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
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_AdvertiserData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.AdvertiserData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_WeeklyData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.WeeklyData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_StationSummaryData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.StationSummaryData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_SpotDetailData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.SpotDetailData));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GenerateReportDto_OutOfSpecData()
        {
            _GetNotEquivalizedScheduleReportDto();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_NotEquivalizedScheduleReportDto.OutOfSpecToDate));
        }

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
            jsonResolver.Ignore(typeof(DisplayDaypart), "_Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(reportDto, jsonSettings);
            Approvals.Verify(json);
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

        /// <summary>
        /// This test tests the removal logic of NTI's monday between 3/6 exclusion and NTI date adjustments between 3/6.
        /// 
        /// The bvs Records effected are the second JUDGE ALEX 
        /// (which would have had date adjusted w/ old logic)  and first "CRIME WATCH..." 
        /// (which would have been excluded with old logic) bvs records.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FullReport_GenerateScheduleReportDto_NTI_ClockDateAdjustment()
        {
            ScheduleReportDto reportDto = null;
            const int estimateId = 333444;

            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Load For Various Tests NTI NSI Adjustments.xlsx", FileMode.Open, FileAccess.Read);
                var fileName = "BVS Load For Various Tests NTI NSI Adjustments.xlsx";

                var bvsRequest = new BvsSaveRequest();
                bvsRequest.UserName = "LoadBvsFile User";
                bvsRequest.BvsFiles.Add(new BvsFile() { BvsFileName = fileName, BvsStream = stream });
                ITrackerService trackerService = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

                trackerService.SaveBvsFiles(bvsRequest);

                ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = estimateId;
                schedule.PostingBookId = 413;
                schedule.ScheduleName = "SCX File For Various tests NTI NSI.scx";
                schedule.UserName = "SCX User";
                schedule.FileName = @"SCX File For Various tests NTI NSI.scx";
                schedule.FileStream = new FileStream(@".\Files\SCX File For Various tests NTI NSI.scx", FileMode.Open,FileAccess.Read);

                schedule.MarketRestrictions = new List<int>();
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,endTime = 86400 - 1,
                    mon = true,tue = true,wed = true,thu = true,fri = true,sat = true,sun = true
                };
                schedule.Equivalized = true;
                schedule.ISCIs = new List<IsciDto>
                {
                    new IsciDto {House = "AAABBB", Client = "cl_AAABBB"},
                    new IsciDto {House = "CCCDDD", Client = "cl_CCCDDD"}
                };

                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.OpenMarket;
                saveRequest.Schedule = schedule;
                int scheduleId = sut.SaveSchedule(saveRequest);

                ISchedulesReportService reportService =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();

                reportDto = reportService.GenerateScheduleReportDto(scheduleId);
                //var report = reportService.GenerateScheduleReport(scheduleId);
                //File.WriteAllBytes(string.Format("..\\Report{0}.xlsx",scheduleId), report.Stream.GetBuffer());//AppDomain.CurrentDomain.BaseDirectory + @"bvsreport.xlsx", reportStream.GetBuffer());
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

 
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Schedule_Upload_Case_Sensitive_ISCIs_BCOP1927()
        {
            ScheduleReportDto reportDto = null;
            using (new TransactionScopeWrapper())
            {
                _LoadBvsFile();

                const int estimateId = 333444;
                ITrackerService sut = IntegrationTestApplicationServiceFactory.GetApplicationService<ITrackerService>();

                var saveRequest = new ScheduleSaveRequest();
                var schedule = new ScheduleDTO();

                schedule.AdvertiserId = 39279;
                schedule.EstimateId = estimateId;
                schedule.PostingBookId = 413;
                
                schedule.ScheduleName = "SCX Various Tests.scx";
                schedule.UserName = "Assembly User";
                schedule.FileName = @"SCX Various Tests.scx";
                schedule.FileStream =
                    new FileStream(@".\Files\SCX Various Tests.scx",
                        FileMode.Open,
                        FileAccess.Read);

                schedule.MarketRestrictions = new List<int>();
                schedule.DaypartRestriction = new DaypartDto()
                {
                    startTime = 0,endTime = 86400 - 1,mon = true,tue = true,wed = true,thu = true,fri = true,sat = true,
                    sun = true
                };
                schedule.Equivalized = true;
                schedule.ISCIs = new List<IsciDto>
                {   // lower case house isci is the point of this test ;)
                    new IsciDto {House = "aaabbb", Client = "cl_AAABBB"},
                    new IsciDto {House = "cccddd", Client = "cl_CCCDDD"}
                };

                schedule.PostType = SchedulePostType.NTI;
                schedule.InventorySource = RatesFile.RateSourceType.Assembly;
                saveRequest.Schedule = schedule;
                var scheduleId = sut.SaveSchedule(saveRequest);

                ISchedulesReportService reportService =
                    IntegrationTestApplicationServiceFactory.GetApplicationService<ISchedulesReportService>();
                reportDto = reportService.GenerateScheduleReportDto(scheduleId);
            }
            _VerifyReportData(reportDto);

    }
}
}
