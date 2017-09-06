using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Aggregates;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IScheduleRepository : IDataRepository
    {
        schedule GetById(int id);
        schedule FindByEstimateId(int estimateId);
        List<DisplaySchedule> GetDisplaySchedules(DateTime? startDate, DateTime? endDate);
        int SaveSchedule(schedule efSchedule);
        void DeleteSchedule(int estimateId);
        void DeleteScheduleById(int scheduleId);
        ScheduleDTO GetScheduleDtoByEstimateId(int estimateId);
        ScheduleDTO GetScheduleDtoById(int id);
        List<int> GetScheduleEstimateIdsByBvsFile(int bvsFileId);
        Dictionary<int, int> GetDictionaryOfScheduleAudiencesByRank(int estimateId);
        List<ScheduleAudience> GetListOfScheduleAudiences(int scheduleId);
        List<ScheduleDetail> GetScheduleTrackingDetails(int estimateId);
        List<LookupDto> GetScheduleLookupPrograms(int scheduleId);
        List<LookupDto> GetScheduleLookupStations(int scheduleId);
        bool UpdateSchedule(schedule schedule);
        DisplaySchedule GetDisplayScheduleById(int scheduleId);
        bool ScheduleDetailWeekExists(int scheduleDetailWeekId);
        void PersistScheduleSpotTargets(List<ScheduleSpotTarget> scheduleSpotTargets);
        ScheduleDetailWeek GetScheduleDetailWeek(int scheduleDetailWeekId);
        bool ScheduleExists(int estimateId);
        ScheduleHeaderDto GetScheduleHeaderByEstimateId(int estimateId);
        List<schedule_audiences> GetScheduleAudiencesById(int scheduleId, List<int> audiencesIds);
        void UpdateScheduleIscis(int scheduleId, List<schedule_iscis> newIscis);
        List<string> GetScheduleIscis(int scheduleId);
        List<int> GetScheduleIdsByIscis(List<string> isciList);
        List<schedule_audiences> GetScheduleAudiences(int scheduleId);
        void ClearScheduleAudiences(int scheduleId);
        void ClearScheduleIscis(int scheduleId);
        void ClearScheduleDaypartRestrictions(int scheduleId);
        void ClearScheduleMarketRestrictions(int scheduleId);
        void UpdateScheduleMarketRestrictions(int scheduleId, List<int> marketIds);
        void UpdateSchedulePostingBook(int scheduleId, int postingBookId);
        DateTime GetMaxEndDate();
    }

    public class ScheduleRepository : BroadcastRepositoryBase, IScheduleRepository
    {
        public ScheduleRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public schedule GetById(int id)
        {
            return _InReadUncommitedTransaction(context => context.schedules
                .Include(s => s.schedule_audiences)
                .Include(s => s.schedule_iscis)
                .Include(s => s.schedule_restriction_dayparts)
                .Include(s => s.markets)
                .Single(s => s.id == id));
        }

        public schedule FindByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(context => context.schedules
                .Include(s => s.schedule_audiences)
                .Include(s => s.schedule_iscis)
                .Include(s => s.schedule_restriction_dayparts)
                .Include(s => s.markets)
                .FirstOrDefault(s => s.estimate_id == estimateId));
        }

        public List<DisplaySchedule> GetDisplaySchedules(DateTime? startDate, DateTime? endDate)
        {
            var ret = _InReadUncommitedTransaction(
                context =>
                {
                    if (startDate == null && endDate == null)
                    {
                        startDate = new DateTime(1901, 1, 1);
                        endDate = new DateTime(2999, 12, 31);
                    }

                    var query = context.schedules.AsQueryable();
                    if (startDate.HasValue)
                        query = query.Where(s => s.start_date >= startDate);
                    if (endDate.HasValue)
                        query = query.Where(s => s.start_date <= endDate);

                    return query.Select(s => new DisplaySchedule
                    {
                        Id = s.id,
                        Name = s.name,
                        AdvertiserId = s.advertiser_id,
                        Estimate = s.estimate_id,
                        StartDate = s.start_date,
                        EndDate = s.end_date,
                        PostType = (SchedulePostType)s.post_type,
                        SpotsBooked = (from sd in context.schedule_details
                                       where sd.schedule_id == s.id
                                       select sd.total_spots).Sum(),
                        SpotsDelivered = (from x in context.bvs_file_details
                                          where x.estimate_id == s.estimate_id
                                                && x.status == (int)TrackingStatus.InSpec
                                          select x).Count(),
                        OutOfSpec = (from x in context.bvs_file_details
                                     where x.estimate_id == s.estimate_id
                                           && x.status != (int)TrackingStatus.InSpec
                                     select x).Count(),
                        PostingBookId = s.posting_book_id,
                        PrimaryDemoBooked = (from sda in context.schedule_detail_audiences
                                             join sd in context.schedule_details on sda.schedule_detail_id equals sd.id
                                             where sd.schedule_id == s.id
                                                   && sda.demo_rank == 1
                                             select sda.impressions).Sum(),
                        PrimaryDemoDelivered = (from bfd in context.bvs_file_details
                                                join pd in context.bvs_post_details on bfd.id equals
                                                pd.bvs_file_detail_id
                                                where bfd.estimate_id == s.estimate_id
                                                      && bfd.status == (int)TrackingStatus.InSpec
                                                      && pd.audience_rank == 1
                                                select (double?)pd.delivery).Sum() ?? 0

                    }).ToList();
                });

            return ret;
        }

        public int SaveSchedule(schedule efSchedule)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.Configuration.AutoDetectChangesEnabled = false;

                    if (efSchedule.markets != null)
                    {
                        foreach (var market in efSchedule.markets)
                        {
                            context.markets.Attach(market);
                        }
                    }

                    context.schedules.Add(efSchedule);
                    context.SaveChanges();
                });

            return efSchedule.id;
        }

        public void DeleteSchedule(int estimateId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var schedule = context.schedules.Single(x => x.estimate_id == estimateId);
                    context.schedules.Remove(schedule);
                    context.SaveChanges();
                });
        }

        public void DeleteScheduleById(int scheduleId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var schedule = context.schedules.Single(x => x.id == scheduleId);
                    context.schedules.Remove(schedule);
                    context.SaveChanges();
                });
        }

        public bool ScheduleExists(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.schedules.Any(x => x.estimate_id == estimateId);
                });
        }

        public ScheduleDTO GetScheduleDtoById(int id)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var efSchedule = (from s in context.schedules
                                      where s.id == id
                                      select s).SingleOrDefault();

                    if (efSchedule == null)
                    {
                        return null;
                    }

                    var scheduleDto = new ScheduleDTO
                    {
                        ScheduleName = efSchedule.name,
                        AdvertiserId = efSchedule.advertiser_id,
                        EstimateId = efSchedule.estimate_id,
                        PostingBookId = efSchedule.posting_book_id,
                        Id = efSchedule.id,
                        StartDate = efSchedule.start_date,
                        EndDate = efSchedule.end_date
                    };

                    scheduleDto.ISCIs = (from i in context.schedule_iscis
                                         where i.schedule_id == scheduleDto.Id
                                         select new IsciDto
                                         {
                                             Id = i.id,
                                             Client = i.client_isci,
                                             House = i.house_isci
                                         }).ToList();

                    return scheduleDto;
                });
        }

        public ScheduleDTO GetScheduleDtoByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var efSchedule = (from s in context.schedules
                                      where s.estimate_id == estimateId
                                      select s).SingleOrDefault();

                    if (efSchedule == null)
                    {
                        throw new Exception(string.Format("Cannot load the Schedule data for Estimate Id: {0}", estimateId));
                    }

                    // FileName
                    var scheduleDto = new ScheduleDTO
                    {
                        ScheduleName = efSchedule.name,
                        AdvertiserId = efSchedule.advertiser_id,
                        EstimateId = efSchedule.estimate_id,
                        PostingBookId = efSchedule.posting_book_id,
                        Id = efSchedule.id,
                        StartDate = efSchedule.start_date,
                        EndDate = efSchedule.end_date,
                        PostType = (SchedulePostType)efSchedule.post_type
                    };

                    scheduleDto.ISCIs = (from i in context.schedule_iscis
                                         where i.schedule_id == scheduleDto.Id
                                         select new IsciDto
                                         {
                                             Id = i.id,
                                             Client = i.client_isci,
                                             House = i.house_isci
                                         }).ToList();

                    return scheduleDto;
                });
        }

        public List<int> GetScheduleEstimateIdsByBvsFile(int bvsFileId)
        {
            return _InReadUncommitedTransaction(context => (from bvs in context.bvs_file_details
                                                            join s in context.schedules on bvs.estimate_id equals s.estimate_id
                                                            where s.estimate_id != null && bvs.bvs_file_id == bvsFileId
                                                            select (int)s.estimate_id).Distinct().ToList());
        }

        public Dictionary<int, int> GetDictionaryOfScheduleAudiencesByRank(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var ret = (from sa in context.schedule_audiences
                               join s in context.schedules on sa.schedule_id equals s.id
                               where s.estimate_id == estimateId
                               select sa).ToList();

                    return ret.ToDictionary(x => x.rank.GetValueOrDefault(), x => x.audience_id); //weird duplicate key issue here.
                });
        }

        public bool ScheduleDetailWeekExists(int scheduleDetailWeekId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.schedule_detail_weeks.Any(x => x.id == scheduleDetailWeekId);
            });
        }

        public void PersistScheduleSpotTargets(List<ScheduleSpotTarget> scheduleSpotTargets)
        {
            var dict = scheduleSpotTargets.ToDictionary(t => t.ScheduleDetailWeek.ScheduleDetailWeekId);
            var ids = dict.Keys.ToList();
            _InReadUncommitedTransaction(context =>
                {
                    var applicableObjs = context.schedule_detail_weeks.Where(w => ids.Contains(w.id));
                    foreach (var scheduleDetailWeekse in applicableObjs)
                    {
                        scheduleDetailWeekse.filled_spots = dict[scheduleDetailWeekse.id].ScheduleDetailWeek.FilledSpots;
                    }

                    context.SaveChanges();
                });
        }

        public ScheduleDetailWeek GetScheduleDetailWeek(int scheduleDetailWeekId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var ret = (from x in context.schedule_detail_weeks
                               where x.id == scheduleDetailWeekId
                               select new ScheduleDetailWeek
                               {
                                   StartDate = x.start_date,
                                   EndDate = x.end_date,
                                   ScheduleDetailWeekId = x.id,
                                   Spots = x.spots,
                                   FilledSpots = x.filled_spots,
                               }).SingleOrDefault();

                    if (ret == null)
                    {
                        throw new Exception(string.Format("The Schedule Detail Week with id: {0} does not exist.", scheduleDetailWeekId));
                    }

                    return ret;
                });
        }

        public List<ScheduleDetail> GetScheduleTrackingDetails(int estimateId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var results = (from sd in context.schedule_details
                               join s in context.schedules on sd.schedule_id equals s.id
                               where s.estimate_id == estimateId
                               select new ScheduleDetail
                               {
                                   Market = sd.market,
                                   Station = sd.network,
                                   Program = sd.program,
                                   SpotLength = sd.spot_length,
                                   SpotLengthId = sd.spot_length_id,
                                   DaypartId = sd.daypart_id,
                                   DetailWeeks = (from x in context.schedule_detail_weeks
                                                  where x.schedule_detail_id == sd.id && x.spots > 0
                                                  select new ScheduleDetailWeek
                                                  {
                                                      StartDate = x.start_date,
                                                      EndDate = x.end_date,
                                                      ScheduleDetailWeekId = x.id,
                                                      Spots = x.spots,
                                                      FilledSpots = x.filled_spots,
                                                  }).ToList()
                               }).ToList();

                results.ForEach(r => r.DetailWeeks = r.DetailWeeks.OrderBy(dw => dw.StartDate).ToList());
                return results;
            });
        }

        public List<LookupDto> GetScheduleLookupPrograms(int scheduleId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var ret = (from s in context.schedule_details
                               where s.schedule_id == scheduleId
                               && s.total_spots > 0
                               select new LookupDto
                               {
                                   Display = s.program,
                                   Id = s.id
                               }).DistinctBy(x => x.Display).OrderBy(x => x.Display).ToList();
                    return ret;
                });
        }

        public List<LookupDto> GetScheduleLookupStations(int scheduleId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var ret = (from s in context.schedule_details
                               where s.schedule_id == scheduleId
                               && s.total_spots > 0
                               select new LookupDto
                               {
                                   Display = s.network,
                                   Id = s.id
                               }).DistinctBy(x => x.Display).OrderBy(x => x.Display).ToList();
                    return ret;
                });

        }

        public bool UpdateSchedule(schedule schedule)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.Configuration.AutoDetectChangesEnabled = false;

                    foreach (var scheduleAudience in schedule.schedule_audiences)
                    {
                        context.Entry(scheduleAudience).State = EntityState.Added;
                    }

                    foreach (var isci in schedule.schedule_iscis)
                    {
                        context.Entry(isci).State = EntityState.Added;
                    }

                    foreach (var daypartRestriction in schedule.schedule_restriction_dayparts)
                    {
                        context.Entry(daypartRestriction).State = EntityState.Added;
                    }

                    context.Entry(schedule).State = EntityState.Modified;
                    context.SaveChanges();
                });

            return true;
        }

        public void ClearScheduleAudiences(int scheduleId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var audiences = context.schedule_audiences.Where(a => a.schedule_id == scheduleId);
                    if (audiences.Any())
                    {
                        context.schedule_audiences.RemoveRange(audiences);
                        context.SaveChanges();
                    }
                });
        }

        public void ClearScheduleIscis(int scheduleId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var iscis = context.schedule_iscis.Where(a => a.schedule_id == scheduleId);
                    if (iscis.Any())
                    {
                        context.schedule_iscis.RemoveRange(iscis);
                        context.SaveChanges();
                    }
                });
        }

        public void ClearScheduleDaypartRestrictions(int scheduleId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var daypartRestrictions = context.schedule_restriction_dayparts.Where(a => a.schedule_id == scheduleId);
                    if (daypartRestrictions.Any())
                    {
                        context.schedule_restriction_dayparts.RemoveRange(daypartRestrictions);
                        context.SaveChanges();
                    }
                });
        }

        public void ClearScheduleMarketRestrictions(int scheduleId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var currentSchedule = context.schedules.FirstOrDefault(s => s.id == scheduleId);
                    if (currentSchedule != null)
                    {
                        foreach (var market in currentSchedule.markets.ToList())
                        {
                            currentSchedule.markets.Remove(market);
                        }
                    }

                    context.SaveChanges();
                });
        }

        public void UpdateScheduleMarketRestrictions(int scheduleId, List<int> marketIds)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var currentSchedule = context.schedules.FirstOrDefault(s => s.id == scheduleId);
                    if (currentSchedule != null)
                    {
                        foreach (var marketId in marketIds)
                        {
                            var market = context.markets.Single(m => m.market_code == marketId);
                            currentSchedule.markets.Add(market);
                        }
                    }

                    context.SaveChanges();
                });
        }

        public void UpdateSchedulePostingBook(int scheduleId, int postingBookId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var currentSchedule = context.schedules.FirstOrDefault(s => s.id == scheduleId);
                    if (currentSchedule != null)
                    {
                        currentSchedule.posting_book_id = postingBookId;
                        context.SaveChanges();
                    }
                });
        }

        public DisplaySchedule GetDisplayScheduleById(int scheduleId)
        {
            var ret = _InReadUncommitedTransaction(
                context =>
                {
                    var displaySchedule =
                       (from s in context.schedules
                        where s.id == scheduleId
                        select new DisplaySchedule
                        {
                            Id = s.id,
                            Name = s.name,
                            AdvertiserId = s.advertiser_id,
                            Estimate = s.estimate_id,
                            StartDate = s.start_date,
                            EndDate = s.end_date,
                            IsEquivalized = s.equivalized,
                            PostType = (SchedulePostType)s.post_type,
                            InventorySource = (RatesFile.RateSourceType)s.inventory_source,

                            SpotsBooked = (from sd in context.schedule_details
                                           where sd.schedule_id == s.id
                                           select sd.total_spots).Sum(),

                            SpotsDelivered = (from x in context.bvs_file_details
                                              where x.estimate_id == s.estimate_id && x.status == (int)TrackingStatus.InSpec
                                              select x).Count(),

                            OutOfSpec = (from x in context.bvs_file_details
                                         where x.estimate_id == s.estimate_id && x.status != (int)TrackingStatus.InSpec
                                         select x).Count(),

                            PostingBookId = s.posting_book_id,

                            PrimaryDemoBooked = (from sda in context.schedule_detail_audiences
                                                 join sd in context.schedule_details on sda.schedule_detail_id equals sd.id
                                                 where sd.schedule_id == s.id && sda.demo_rank == 1
                                                 select sda.impressions).Sum(),

                            PrimaryDemoDelivered = (from bfd in context.bvs_file_details
                                                    join pd in context.bvs_post_details on bfd.id equals pd.bvs_file_detail_id
                                                    where bfd.estimate_id == s.estimate_id && bfd.status == (int)TrackingStatus.InSpec && pd.audience_rank == 1
                                                    select (double?)pd.delivery).Sum() ?? 0,

                            MarketRestrictions = (from m in s.markets
                                                  select m.market_code).ToList(),

                            DaypartRestrictionId = (from d in s.schedule_restriction_dayparts
                                                    select d.daypart_id).FirstOrDefault(),

                            Audiences = (from a in s.schedule_audiences
                                         select a.audience_id).ToList()
                        }).First();

                    var efIscis = (from si in context.schedule_iscis
                                   where si.schedule_id == displaySchedule.Id
                                   select si).ToList();

                    displaySchedule.Iscis = efIscis.Select(i => new IsciDto
                    {
                        Id = i.id,
                        Client = i.client_isci,
                        House = i.house_isci,
                        Brand = i.brand
                    }).ToList();

                    return displaySchedule;
                });

            return ret;
        }

        public ScheduleHeaderDto GetScheduleHeaderByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var isciList = from s in context.schedule_iscis
                                   where s.schedule.estimate_id == estimateId
                                   select s;

                    var scheduleHeader = new ScheduleHeaderDto
                    {
                        ISCIs = isciList.Select(i => new IsciDto
                        {
                            Id = i.id,
                            Client = i.client_isci,
                            House = i.house_isci,
                            Brand = i.brand
                        }).ToList()
                    };

                    return scheduleHeader;
                });
        }

        public List<schedule_audiences> GetScheduleAudiencesById(int estimateId, List<int> audiencesIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                    (from a in context.schedule_audiences
                     join s in context.schedules on a.schedule_id equals s.id
                     where s.estimate_id == estimateId && audiencesIds.Contains(a.audience_id)
                     select a).ToList());
        }

        public void UpdateScheduleIscis(int scheduleId, List<schedule_iscis> newIscis)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var iscis = context.schedule_iscis.Where(s => s.schedule_id == scheduleId).ToList();
                    foreach (var isci in iscis)
                    {
                        context.schedule_iscis.Remove(isci);
                    }
                    foreach (var newIsci in newIscis)
                    {
                        newIsci.schedule_id = scheduleId;
                        context.schedule_iscis.Add(newIsci);
                    }
                    context.SaveChanges();

                });
        }

        public List<ScheduleAudience> GetListOfScheduleAudiences(int scheduleId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var scheduleAudiences = context.schedule_audiences.Where(sa => sa.schedule_id == scheduleId).Select(
                        sa => new ScheduleAudience
                        {
                            AudienceId = sa.audience_id,
                            Rank = sa.rank ?? 0,
                            Population = sa.population
                        }).ToList();
                    return scheduleAudiences;
                });
        }

        public List<string> GetScheduleIscis(int scheduleId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var scheduleIscis =
                        context.schedule_iscis.Where(si => si.schedule_id == scheduleId)
                            .Select(si => si.house_isci)
                            .ToList();
                    return scheduleIscis;
                });
        }

        public List<int> GetScheduleIdsByIscis(List<string> isciList)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var scheduleIscis =
                        context.schedule_iscis.Where(si => isciList.Contains(si.house_isci))
                            .Select(si => si.schedule_id)
                            .Distinct()
                            .ToList();
                    return scheduleIscis;
                });
        }

        public List<schedule_audiences> GetScheduleAudiences(int scheduleId)
        {
            return _InReadUncommitedTransaction(
                 context =>
                     (from a in context.schedule_audiences
                      join s in context.schedules on a.schedule_id equals s.id
                      where s.id == scheduleId
                      select a).ToList());
        }

        public DateTime GetMaxEndDate()
        {
            return _InReadUncommitedTransaction(context => context.schedules.Max(s => s.end_date));
        }
    }
}
