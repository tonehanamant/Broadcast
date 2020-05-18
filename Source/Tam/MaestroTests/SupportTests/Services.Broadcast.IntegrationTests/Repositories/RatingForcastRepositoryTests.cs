using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    public class RatingForcastRepositoryTests
    {
        IRatingForecastRepository _Repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
        const int SecPerHour = 60 * 60;

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Test_Projected()
        {
            int posting_media_month_id = 437;
            string demos = "6,7,347,348,13,14,15,21,22,28,29,30,284,290";//', --A18 +

            List<ManifestDetailDaypart> details = new List<ManifestDetailDaypart>();
            DisplayDaypart dp = new DisplayDaypart(1, 32400, 35999, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 5,
                LegacyCallLetters = "WMAR",
                DisplayDaypart = dp,
            });
            dp = new DisplayDaypart(1, 36000, 35999, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 6,
                LegacyCallLetters = "WMAR",
                DisplayDaypart = dp,
            });
            dp = new DisplayDaypart(1, 39600, 43199, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 7,
                LegacyCallLetters = "WMAR",
                DisplayDaypart = dp,
            });
            dp = new DisplayDaypart(1, 32400, 35999, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 71,
                LegacyCallLetters = "WESH",
                DisplayDaypart = dp,
            });
            dp = new DisplayDaypart(1, 36000, 35999, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 74,
                LegacyCallLetters = "WESH",
                DisplayDaypart = dp,
            });
            dp = new DisplayDaypart(1, 39600, 43199, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 77,
                LegacyCallLetters = "WESH",
                DisplayDaypart = dp,
            });
            dp = new DisplayDaypart(1, 32400, 35999, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 89,
                LegacyCallLetters = "EESH",
                DisplayDaypart = dp,
            });
            dp = new DisplayDaypart(1, 36000, 39599, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 90,
                LegacyCallLetters = "EESH",
                DisplayDaypart = dp,
            });
            dp = new DisplayDaypart(1, 39600, 43199, true, true, true, true, true, false, false);
            details.Add(new ManifestDetailDaypart()
            {
                Id = 91,
                LegacyCallLetters = "EESH",
                DisplayDaypart = dp,
            });

            var results = _Repo.GetImpressionsDaypart(posting_media_month_id, demos.Split(',').Select(i => Int32.Parse(i)).ToList(), details,ProposalEnums.ProposalPlaybackType.LivePlus3);

            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, jsonSettings));

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Test_PointInTime()
        {
            int posting_media_month_id = 437;
            string demos = "6,7,347,348,13,14,15,21,22,28,29,30,284,290";//', --A18 +

            List<StationDetailPointInTime> details = new List<StationDetailPointInTime>();
            details.Add(new StationDetailPointInTime()
            {
                Id = 5,
                LegacyCallLetters = "WMAR",
                TimeAired = 32400,
                DayOfWeek = DayOfWeek.Thursday
            });
            details.Add(new StationDetailPointInTime()
            {
                Id = 6,
                LegacyCallLetters = "WMAR",
                TimeAired = 36000,
                DayOfWeek = DayOfWeek.Thursday
            });
            details.Add(new StationDetailPointInTime()
            {
                Id = 7,
                LegacyCallLetters = "WMAR",
                TimeAired = 39600,
                DayOfWeek = DayOfWeek.Thursday
            });
            details.Add(new StationDetailPointInTime()
            {
                Id = 71,
                LegacyCallLetters = "WESH",
                TimeAired = 32400,
                DayOfWeek = DayOfWeek.Thursday
            });
            details.Add(new StationDetailPointInTime()
            {
                Id = 74,
                LegacyCallLetters = "WESH",
                TimeAired = 36000,
                DayOfWeek = DayOfWeek.Thursday
            });
            details.Add(new StationDetailPointInTime()
            {
                Id = 77,
                LegacyCallLetters = "WESH",
                TimeAired = 39600,
                DayOfWeek = DayOfWeek.Thursday
            });
            details.Add(new StationDetailPointInTime()
            {
                Id = 89,
                LegacyCallLetters = "EESH",
                TimeAired = 32400,
                DayOfWeek = DayOfWeek.Thursday
            });
            details.Add(new StationDetailPointInTime()
            {
                Id = 90,
                LegacyCallLetters = "EESH",
                TimeAired = 36000,
                DayOfWeek = DayOfWeek.Thursday
            });
            details.Add(new StationDetailPointInTime()
            {
                Id = 91,
                LegacyCallLetters = "EESH",
                TimeAired = 39600,
                DayOfWeek = DayOfWeek.Thursday
            });

            var results = _Repo.GetImpressionsPointInTime(posting_media_month_id, demos.Split(',').Select(i => Int32.Parse(i)).ToList(), details, ProposalEnums.ProposalPlaybackType.LivePlus3);

            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, jsonSettings));

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Test_AdjustDayparts_TimeAdjustments()
        {
            using (new TransactionScopeWrapper())
            {
                List<ManifestDetailDaypart> manifestDayparts = new List<ManifestDetailDaypart>();
                int startTime;
                int endTime;
                DisplayDaypart daypart;

                // cover four timeslots with 2 adjustmments equ to 10:00 - 11:00
                startTime = (SecPerHour * 11) - (60 * 2);  // 10:58
                endTime = (SecPerHour * 12) + (60 * 5) - 1;    // 12:05
                daypart = new DisplayDaypart(2, startTime, endTime, true, false, false, false, false, false, false);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 2, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });

                // cover 1 timeslots with  2 adjustments equ to 10:00 - 11:15 (extra added at end)
                // should be adjusted to 11:05-11:11
                startTime = (SecPerHour * 11) - (60 * 2);  // 10:58
                endTime = (SecPerHour * 11) + (60 * 18) - 1;    // 11:18
                daypart = new DisplayDaypart(2, startTime, endTime, true, false, false, false, false, false, false);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 2, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });

                // cover 1 timeslots should make no changes as it is too small
                startTime = SecPerHour * 10;  // 10:00
                endTime = (SecPerHour * 10) + 15 * 60 - 1;    // 10:15
                daypart = new DisplayDaypart(2, startTime, endTime, true, false, false, false, false, false, false);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 1, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });


                // cover 1 timeslots should make no changes as it is too small
                startTime = SecPerHour * 10;  // 10:00
                endTime = (SecPerHour * 10) + 14 * 60 - 1;    // 10:14
                daypart = new DisplayDaypart(2, startTime, endTime, true, false, false, false, false, false, false);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 1, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });

                // cover four timeslots should be 10:07 - 10:53
                startTime = SecPerHour * 10;  // 10:00
                endTime = (SecPerHour * 11) - 1;    // 11:00
                daypart = new DisplayDaypart(2, startTime, endTime, true, false, false, false, false, false, false);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 1, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });


                var result = _Repo.AdjustDayparts(manifestDayparts);

                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Test_AdjustDayparts_DayAdjustments()
        {
            using (new TransactionScopeWrapper())
            {
                const int SecPerHour = 60 * 60;
                
                List<ManifestDetailDaypart> manifestDayparts = new List<ManifestDetailDaypart>();
                int startTime;
                int endTime;
                DisplayDaypart daypart;


                // all days of week no changes
                startTime = (SecPerHour);  // 1:00
                endTime = ((SecPerHour * 2) + (60 * 8)) - 1;    // 2:00
                daypart = new DisplayDaypart(2, startTime, endTime, true, true, true, true, true, true, true);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 4, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });

                // mon and Sun active
                // should become mon and tues active
                startTime = (SecPerHour);  // 1:00
                endTime = ((SecPerHour * 2) + (60 * 8)) - 1;    // 2:00
                daypart = new DisplayDaypart(2, startTime, endTime, true, false, false, false, false, false, true);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 4, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });

                // Sat and Sun active
                // should become sun and mon
                startTime = (SecPerHour);  // 1:00
                endTime = ((SecPerHour * 2) + (60 * 8)) - 1;    // 2:00
                daypart = new DisplayDaypart(2, startTime, endTime, false, false, false, false, false, true, true);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 4, LegacyCallLetters = "WNBC", DisplayDaypart = daypart }); 

                // mon-su w/o wed...so should be m,t,w,f,sa,su
                startTime = (SecPerHour);  // 1:00
                endTime = ((SecPerHour * 2) + (60 * 8)) - 1;    // 2:00
                daypart = new DisplayDaypart(2, startTime, endTime, true, true, false, true, true, true, true);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 4, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });

                // t-sa ...so should be w,th,f,sa,su
                startTime = (SecPerHour);  // 1:00
                endTime = ((SecPerHour * 2) + (60 * 8)) - 1;    // 2:00
                daypart = new DisplayDaypart(2, startTime, endTime, false, true, true, true, true, true, false);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 4, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });

                // days of week should be adjusted +1. IN this cast mon becomes tues
                startTime = SecPerHour * 1;  // 1:00
                endTime = (SecPerHour * 3) - 1;    // 3:00
                daypart = new DisplayDaypart(2, startTime, endTime, true, false, false, false, false, false, false);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 3, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });

                // days of week should be adjusted +1. IN this cast tues becomes wed and fri becomes sat and sun becomes mon
                startTime = SecPerHour * 1;  // 1:00
                endTime = (SecPerHour * 3) - 1;    // 3:00
                daypart = new DisplayDaypart(2, startTime, endTime, false, true, false, false, true, false, true);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 4, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });


                // cover five timeslots with  2 adjustments equ to 2:05 - 3:01 (extra added at end)
                // but days of week should be adjusted +1. IN this cast tues becomes wed and fri becomes sat and sun becomes mon
                startTime = (SecPerHour * 2) - (60 * 2);  // 1:58
                endTime = ((SecPerHour * 3) + (60 * 8)) -1;    // 3:08
                daypart = new DisplayDaypart(2, startTime, endTime, false, true, false, false, true, false, true);
                manifestDayparts.Add(new ManifestDetailDaypart() { Id = 4, LegacyCallLetters = "WNBC", DisplayDaypart = daypart });


                var result = _Repo.AdjustDayparts(manifestDayparts);

                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }
        [Test]
        public void Test_TwoBook_Divide_Zero_PRI4396()
        {

            var uniqueRatingsAudiences = new List<int>() {13};
            var stationDetails = new List<ManifestDetailDaypart>()
            {
                new ManifestDetailDaypart()
                {
                    DisplayDaypart =
                        new DisplayDaypart(0, 19800, 21599, false, false, false, false, false, true, false),
                    LegacyCallLetters = "KRBC"
                }
            };
            _Repo.GetImpressionsDaypart(434, 437, uniqueRatingsAudiences, stationDetails, ProposalEnums.ProposalPlaybackType.LivePlus3);
        }

    }
}
