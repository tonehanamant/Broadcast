﻿using Common.Services;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Repositories
{
    public interface IBvsTestDataGeneratorRepository : IDataRepository
    {
        void CreateLeadInAndBlockTestData();
        void CreateOverflowSpotAssignTestData();
        void CreateStationMatchWithoutTimeMatchData();
    }

    public class BvsTestDataGeneratorRepository : BroadcastRepositoryBase, IBvsTestDataGeneratorRepository
    {
        public BvsTestDataGeneratorRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public void CreateLeadInAndBlockTestData()
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var isci = "SOMEISCI";
                    var schedule = context.schedules.Create();
                    schedule.estimate_id = 8675309;
                    schedule.advertiser_id = 37674;
                    schedule.name = "LeadInBlockTest";
                    schedule.posting_book_id = 413;
                    schedule.start_date = new DateTime(2016, 1, 1);
                    schedule.end_date = new DateTime(2016, 12, 24);
                    schedule.created_by = "";
                    schedule.created_date = DateTime.Now;
                    schedule.modified_by = "";
                    schedule.modified_date = DateTime.Now;
                    schedule.schedule_iscis.Add(new schedule_iscis(){house_isci = isci, client_isci = isci});
                    context.schedules.Add(schedule);

                    context.SaveChanges();

                    var scheduleDetail = context.schedule_details.Create();
                    scheduleDetail.schedule_id = schedule.id;
                    scheduleDetail.market = "New York";
                    scheduleDetail.network = "WCBS-TV";
                    scheduleDetail.program = "Friends";
                    scheduleDetail.daypart_id = DaypartCache.Instance.GetIdByDaypart(DisplayDaypart.ParseMsaDaypart("M-SU 6:30PM-7:00PM"));
                    scheduleDetail.total_cost = 400;
                    scheduleDetail.total_spots = 2;
                    scheduleDetail.spot_cost = 800;

                    {
                        var week = context.schedule_detail_weeks.Create();
                        week.media_week_id = 649;
                        week.start_date = new DateTime(2016, 5, 30);
                        week.end_date = new DateTime(2016, 6, 5);
                        week.spots = 2;
                        scheduleDetail.schedule_detail_weeks.Add(week);

                        var audience = context.schedule_detail_audiences.Create();
                        audience.audience_id = 31;
                        audience.audience_rank = 1;
                        audience.audience_population = 7368320;
                        audience.impressions = 127859;
                        scheduleDetail.schedule_detail_audiences.Add(audience);
                    }
                    context.schedule_details.Add(scheduleDetail);

                    var scheduleDetail2 = context.schedule_details.Create();
                    scheduleDetail2.schedule_id = schedule.id;
                    scheduleDetail2.market = "New York";
                    scheduleDetail2.network = "WCBS-TV";
                    scheduleDetail2.program = "Another Block";
                    scheduleDetail2.daypart_id = DaypartCache.Instance.GetIdByDaypart(DisplayDaypart.ParseMsaDaypart("M-SU 5:00PM-7:00PM"));
                    scheduleDetail2.total_cost = 400;
                    scheduleDetail2.total_spots = 2;
                    scheduleDetail2.spot_cost = 800;

                    {
                        var week = context.schedule_detail_weeks.Create();
                        week.media_week_id = 649;
                        week.start_date = new DateTime(2016, 5, 30);
                        week.end_date = new DateTime(2016, 6, 5);
                        week.spots = 2;
                        scheduleDetail2.schedule_detail_weeks.Add(week);

                        var audience = context.schedule_detail_audiences.Create();
                        audience.audience_id = 31;
                        audience.audience_rank = 1;
                        audience.audience_population = 7368320;
                        audience.impressions = 127859;
                        scheduleDetail2.schedule_detail_audiences.Add(audience);
                    }
                    context.schedule_details.Add(scheduleDetail2);

                    var scheduleDetail3 = context.schedule_details.Create();
                    scheduleDetail3.schedule_id = schedule.id;
                    scheduleDetail3.market = "New York";
                    scheduleDetail3.network = "WCBS-TV";
                    scheduleDetail3.program = "Gummybears";
                    scheduleDetail3.daypart_id = DaypartCache.Instance.GetIdByDaypart(DisplayDaypart.ParseMsaDaypart("SA-SU 7:00PM-7:30PM"));
                    scheduleDetail3.total_cost = 400;
                    scheduleDetail3.total_spots = 2;
                    scheduleDetail3.spot_cost = 800;

                    {
                        var week = context.schedule_detail_weeks.Create();
                        week.media_week_id = 649;
                        week.start_date = new DateTime(2016, 5, 30);
                        week.end_date = new DateTime(2016, 6, 5);
                        week.spots = 2;
                        scheduleDetail3.schedule_detail_weeks.Add(week);

                        var audience = context.schedule_detail_audiences.Create();
                        audience.audience_id = 31;
                        audience.audience_rank = 1;
                        audience.audience_population = 7368320;
                        audience.impressions = 127859;
                        scheduleDetail3.schedule_detail_audiences.Add(audience);
                    }
                    context.schedule_details.Add(scheduleDetail3);

                    var scheduleDetail4 = context.schedule_details.Create();
                    scheduleDetail4.schedule_id = schedule.id;
                    scheduleDetail4.market = "New York";
                    scheduleDetail4.network = "WCBS-TV";
                    scheduleDetail4.program = "Cartoon Block";
                    scheduleDetail4.daypart_id = DaypartCache.Instance.GetIdByDaypart(DisplayDaypart.ParseMsaDaypart("M-SU 7:00PM-9:00PM"));
                    scheduleDetail4.total_cost = 400;
                    scheduleDetail4.total_spots = 2;
                    scheduleDetail4.spot_cost = 800;

                    {
                        var week = context.schedule_detail_weeks.Create();
                        week.media_week_id = 649;
                        week.start_date = new DateTime(2016, 5, 30);
                        week.end_date = new DateTime(2016, 6, 5);
                        week.spots = 2;
                        scheduleDetail4.schedule_detail_weeks.Add(week);

                        var audience = context.schedule_detail_audiences.Create();
                        audience.audience_id = 31;
                        audience.audience_rank = 1;
                        audience.audience_population = 7368320;
                        audience.impressions = 127859;
                        scheduleDetail4.schedule_detail_audiences.Add(audience);
                    }
                    context.schedule_details.Add(scheduleDetail4);

                    context.SaveChanges();

                    var bvsDetail = context.bvs_files.Create();
                    bvsDetail.name = schedule.name;
                    bvsDetail.start_date = schedule.start_date;
                    bvsDetail.end_date = schedule.end_date;
                    bvsDetail.file_hash = "FILE HASH";
                    bvsDetail.created_by = "";
                    bvsDetail.created_date = DateTime.Now;
                    context.bvs_files.Add(bvsDetail);

                    context.SaveChanges();

                    var bvsFileDetail = context.bvs_file_details.Create();
                    bvsFileDetail.bvs_file_id = bvsDetail.id;
                    bvsFileDetail.rank = 1;
                    bvsFileDetail.program_name = "Friends (Season Finale)";
                    bvsFileDetail.market = "New York";
                    bvsFileDetail.station = "WCBS";
                    bvsFileDetail.affiliate = "CBS";
                    bvsFileDetail.date_aired = new DateTime(2016, 6, 5);
                    bvsFileDetail.nsi_date = new DateTime(2016, 6, 5);
                    bvsFileDetail.nti_date = new DateTime(2016, 6, 5);
                    bvsFileDetail.time_aired = 68399;
                    bvsFileDetail.spot_length = 30;
                    bvsFileDetail.isci = isci;
                    bvsFileDetail.estimate_id = (int)schedule.estimate_id;

                    bvsDetail.bvs_file_details.Add(bvsFileDetail);

                    context.SaveChanges();
                });
        }

        public void CreateOverflowSpotAssignTestData()
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var isci = "SOMEISCI";
                    var schedule = context.schedules.Create();
                    schedule.estimate_id = 7618419;
                    schedule.advertiser_id = 37674;
                    schedule.name = "OverflowSpotTest";
                    schedule.posting_book_id = 413;
                    schedule.start_date = new DateTime(2016, 1, 1);
                    schedule.end_date = new DateTime(2016, 12, 24);
                    schedule.created_by = "";
                    schedule.created_date = DateTime.Now;
                    schedule.modified_by = "";
                    schedule.modified_date = DateTime.Now;
                    schedule.schedule_iscis.Add(new schedule_iscis(){house_isci = isci, client_isci = isci});
                    context.schedules.Add(schedule);

                    context.SaveChanges();

                    var scheduleDetail = context.schedule_details.Create();
                    scheduleDetail.schedule_id = schedule.id;
                    scheduleDetail.market = "New York";
                    scheduleDetail.network = "WCBS-TV";
                    scheduleDetail.program = "Friends";
                    scheduleDetail.daypart_id = DaypartCache.Instance.GetIdByDaypart(DisplayDaypart.ParseMsaDaypart("M-SU 6:30PM-7:00PM"));
                    scheduleDetail.spot_cost = 400;
                    scheduleDetail.total_spots = 4;
                    scheduleDetail.total_cost = scheduleDetail.spot_cost * scheduleDetail.total_spots;

                    {
                        var week = context.schedule_detail_weeks.Create();
                        week.media_week_id = 649;
                        week.start_date = new DateTime(2016, 5, 30);
                        week.end_date = new DateTime(2016, 6, 5);
                        week.spots = 2;
                        scheduleDetail.schedule_detail_weeks.Add(week);

                        var audience = context.schedule_detail_audiences.Create();
                        audience.audience_id = 31;
                        audience.audience_rank = 1;
                        audience.audience_population = 7368320;
                        audience.impressions = 127859;
                        scheduleDetail.schedule_detail_audiences.Add(audience);
                    }
                    context.schedule_details.Add(scheduleDetail);

                    var scheduleDetail2 = context.schedule_details.Create();
                    scheduleDetail2.schedule_id = schedule.id;
                    scheduleDetail2.market = "New York";
                    scheduleDetail2.network = "WCBS-TV";
                    scheduleDetail2.program = "Friends";
                    scheduleDetail2.daypart_id = DaypartCache.Instance.GetIdByDaypart(DisplayDaypart.ParseMsaDaypart("SA-SU 7:00PM-7:30PM"));
                    scheduleDetail2.total_cost = 400;
                    scheduleDetail2.total_spots = 2;
                    scheduleDetail2.spot_cost = 800;

                    {
                        var week = context.schedule_detail_weeks.Create();
                        week.media_week_id = 649;
                        week.start_date = new DateTime(2016, 5, 30);
                        week.end_date = new DateTime(2016, 6, 5);
                        week.spots = 2;
                        scheduleDetail2.schedule_detail_weeks.Add(week);

                        var audience = context.schedule_detail_audiences.Create();
                        audience.audience_id = 31;
                        audience.audience_rank = 1;
                        audience.audience_population = 7368320;
                        audience.impressions = 127859;
                        scheduleDetail2.schedule_detail_audiences.Add(audience);
                    }
                    context.schedule_details.Add(scheduleDetail2);

                    context.SaveChanges();

                    var bvsFile = context.bvs_files.Create();
                    bvsFile.name = schedule.name;
                    bvsFile.start_date = schedule.start_date;
                    bvsFile.end_date = schedule.end_date;
                    bvsFile.file_hash = "FILE HASH2";
                    bvsFile.created_by = "";
                    bvsFile.created_date = DateTime.Now;
                    context.bvs_files.Add(bvsFile);

                    context.SaveChanges();

                    //Create 5 spots all at the same time.
                    _CreateBvsDetail(context, bvsFile, schedule, isci);
                    _CreateBvsDetail(context, bvsFile, schedule, isci);
                    _CreateBvsDetail(context, bvsFile, schedule, isci);
                    _CreateBvsDetail(context, bvsFile, schedule, isci);
                    _CreateBvsDetail(context, bvsFile, schedule, isci);

                    context.SaveChanges();
                });
        }

        public void CreateStationMatchWithoutTimeMatchData()
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var isci = "SOMEISCI";
                    var schedule = context.schedules.Create();
                    schedule.estimate_id = 7985133;
                    schedule.advertiser_id = 37674;
                    schedule.name = "BvsAndScheduleNoDateMatch";
                    schedule.posting_book_id = 413;
                    schedule.start_date = new DateTime(2016, 1, 1);
                    schedule.end_date = new DateTime(2016, 12, 24);
                    schedule.created_by = "";
                    schedule.created_date = DateTime.Now;
                    schedule.modified_by = "";
                    schedule.modified_date = DateTime.Now;
                    schedule.schedule_iscis.Add(new schedule_iscis(){house_isci = isci, client_isci = isci});
                    context.schedules.Add(schedule);

                    context.SaveChanges();

                    var scheduleDetail = context.schedule_details.Create();
                    scheduleDetail.schedule_id = schedule.id;
                    scheduleDetail.market = "New York";
                    scheduleDetail.network = "WCBS-TV";
                    scheduleDetail.program = "Friends";
                    scheduleDetail.daypart_id = DaypartCache.Instance.GetIdByDaypart(DisplayDaypart.ParseMsaDaypart("M-SU 6:30PM-7:00PM"));
                    scheduleDetail.spot_cost = 400;
                    scheduleDetail.total_spots = 4;
                    scheduleDetail.total_cost = scheduleDetail.spot_cost * scheduleDetail.total_spots;

                    {
                        var week = context.schedule_detail_weeks.Create();
                        week.media_week_id = 649;
                        week.start_date = new DateTime(2016, 5, 30);
                        week.end_date = new DateTime(2016, 6, 5);
                        week.spots = 2;
                        scheduleDetail.schedule_detail_weeks.Add(week);

                        var audience = context.schedule_detail_audiences.Create();
                        audience.audience_id = 31;
                        audience.audience_rank = 1;
                        audience.audience_population = 7368320;
                        audience.impressions = 127859;
                        scheduleDetail.schedule_detail_audiences.Add(audience);
                    }
                    context.schedule_details.Add(scheduleDetail);

                    var bvsFile = context.bvs_files.Create();
                    bvsFile.name = schedule.name;
                    bvsFile.start_date = schedule.start_date;
                    bvsFile.end_date = schedule.end_date;
                    bvsFile.file_hash = "FILE HASH2";
                    bvsFile.created_by = "";
                    bvsFile.created_date = DateTime.Now;
                    context.bvs_files.Add(bvsFile);

                    context.SaveChanges();

                    //Create 5 spots all at the same time.
                    var bvsDetail = _CreateBvsDetail(context, bvsFile, schedule, isci);
                    bvsDetail.date_aired = new DateTime(2015, 1, 1);
                    bvsDetail.nsi_date = new DateTime(2015, 6, 5);
                    bvsDetail.nti_date = new DateTime(2015, 6, 5);

                    context.SaveChanges();
                });
        }

        private bvs_file_details _CreateBvsDetail(QueryHintBroadcastContext context, bvs_files bvsDetail, schedule schedule, String isci)
        {
            if (schedule.estimate_id == null)
            {
                return null;
            }
            
            var bvsFileDetail = context.bvs_file_details.Create();
            bvsFileDetail.bvs_file_id = bvsDetail.id;
            bvsFileDetail.rank = 1;
            bvsFileDetail.program_name = "Friends";
            bvsFileDetail.market = "New York";
            bvsFileDetail.station = "WCBS";
            bvsFileDetail.affiliate = "CBS";
            bvsFileDetail.date_aired = new DateTime(2016, 6, 5);
            bvsFileDetail.nsi_date = new DateTime(2016, 6, 5);
            bvsFileDetail.nti_date = new DateTime(2016, 6, 5);
            bvsFileDetail.time_aired = 68399;
            bvsFileDetail.spot_length = 30;
            bvsFileDetail.isci = isci;
            bvsFileDetail.estimate_id = (int)schedule.estimate_id;
            bvsDetail.bvs_file_details.Add(bvsFileDetail);

            return bvsFileDetail;
        }
    }
}
