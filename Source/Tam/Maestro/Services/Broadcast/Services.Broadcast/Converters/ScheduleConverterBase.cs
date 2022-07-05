using Common.Services;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using market = EntityFrameworkMapping.Broadcast.market;

namespace Services.Broadcast.Converters
{
    public interface IScheduleConverter
    {
        schedule Convert(ScheduleDTO scheduleDto);
    }

    public abstract class ScheduleConverterBase : IScheduleConverter
    {
        protected readonly IDataRepositoryFactory _DataRepositoryFactory;
        protected readonly IDaypartCache _DaypartCache;
        protected readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        protected readonly IBroadcastAudiencesCache _AudienceCache;

        protected ScheduleConverterBase(
                    IDataRepositoryFactory broadcastDataRepositoryFactory, 
                    IDaypartCache daypartCache,
                    IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                    IBroadcastAudiencesCache audienceCache)
        {
            _DataRepositoryFactory = broadcastDataRepositoryFactory;
            _DaypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _AudienceCache = audienceCache;
        }
        public abstract schedule Convert(ScheduleDTO scheduleDto);

        protected virtual schedule _MapSchedule(ScheduleDTO scheduleDto)
        {
            schedule originalSchedule = null;
            var isUpdating = !(scheduleDto.EstimateId != null && scheduleDto.FileStream != null || scheduleDto.IsBlank);
            if (isUpdating)
            {
                if (scheduleDto.EstimateId.HasValue && _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().ScheduleExists(scheduleDto.EstimateId.Value))
                {
                    originalSchedule = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().FindByEstimateId(scheduleDto.EstimateId.Value);
                }

                if (originalSchedule == null && scheduleDto.Id > 0)
                {
                    originalSchedule = _DataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetById(scheduleDto.Id);
                }
            }

            // mapping to EF entity
            var returnSchedule = originalSchedule ?? new schedule();  
            returnSchedule.name = scheduleDto.ScheduleName;
            returnSchedule.estimate_id = scheduleDto.EstimateId;
            returnSchedule.advertiser_id = scheduleDto.AdvertiserId;
            returnSchedule.posting_book_id = scheduleDto.PostingBookId;
            returnSchedule.advertiser_master_id = scheduleDto.AdvertiserMasterId;

            var startDate = scheduleDto.StartDate != null ? (DateTime) scheduleDto.StartDate : DateTime.Now;
            returnSchedule.start_date = originalSchedule != null ? originalSchedule.start_date : startDate;

            var endDate = scheduleDto.EndDate != null ? (DateTime)scheduleDto.EndDate : DateTime.Now;
            returnSchedule.end_date = originalSchedule != null ? originalSchedule.end_date : endDate;

            returnSchedule.modified_date = DateTime.Now;
            if (returnSchedule.created_date == new DateTime())
            {
                returnSchedule.created_date = DateTime.Now;
            }

            returnSchedule.created_by = scheduleDto.UserName;
            returnSchedule.modified_by = scheduleDto.UserName;
            returnSchedule.post_type = (byte)scheduleDto.PostType;
            returnSchedule.equivalized = scheduleDto.Equivalized;

            if (scheduleDto.InventorySource != null)
            {
                returnSchedule.inventory_source =  (byte)scheduleDto.InventorySource;
            }

            if (scheduleDto.MarketRestrictions != null)
            {
                var efMarkets = _DataRepositoryFactory.GetDataRepository<IMarketRepository>().GetMarketsByMarketCodes(scheduleDto.MarketRestrictions).ToList();
                returnSchedule.markets = new List<market>();

                foreach (var market in efMarkets)
                {
                    returnSchedule.markets.Add(new market
                    {
                        market_code = market.market_code,
                        geography_name = market.geography_name
                    });
                }
            }

            returnSchedule.schedule_restriction_dayparts = new List<schedule_restriction_dayparts>();
            if (scheduleDto.DaypartRestriction != null)
            {
                var displayDaypart = DaypartDto.ConvertDaypartDto(scheduleDto.DaypartRestriction);
                var daypartId = DaypartCache.Instance.GetIdByDaypart(displayDaypart);
                returnSchedule.schedule_restriction_dayparts.Add(new schedule_restriction_dayparts
                {
                    schedule_id = returnSchedule.id,
                    daypart_id = daypartId
                });
            }

            if (scheduleDto.ISCIs != null)
            {
                returnSchedule.schedule_iscis =
                    scheduleDto.ISCIs.Select(i => new schedule_iscis
                    {
                        schedule_id = returnSchedule.id,
                        house_isci = i.House != null ? i.House.Trim() : null,
                        client_isci = i.Client != null ? i.Client.Trim() : null,
                        brand = i.Brand != null ? i.Brand.Trim() : null
                    }).ToList();
            }
           
            return returnSchedule;
        }

        internal static List<schedule_iscis> GetScheduleIscisFromString(string iscisString)
        {
            if (iscisString == null)
                return null;

            var scheduleIscis = new List<schedule_iscis>();

            var iscis = iscisString.Split(',');
            foreach (var isci in iscis)
            {
                string houseIsci;
                string clientIsci;

                var clientIsciStart = isci.IndexOf('(');
                var isClientISciMissing = (clientIsciStart == -1);

                if (isClientISciMissing)
                {
                    houseIsci = isci.Trim();
                    clientIsci = houseIsci;
                }
                else
                {
                    houseIsci = isci.Substring(0, isci.IndexOf('('));
                    var clientIsciEnd = isci.IndexOf(')');
                    clientIsci = isci.Substring(clientIsciStart + 1, clientIsciEnd - clientIsciStart - 1).Trim();
                }

                scheduleIscis.Add(new schedule_iscis() { house_isci = houseIsci, client_isci = clientIsci });

            }

            return scheduleIscis;
        }


        internal static bool HasSameIscis(schedule efSchedule, string iscis)
        {
            var isciList = GetScheduleIscisFromString(iscis).Select(i => new Tuple<string, string>(i.house_isci, i.client_isci)).ToList();
            var scheduleIsciList =
                efSchedule.schedule_iscis.Select(i => new Tuple<string, string>(i.house_isci, i.client_isci)).ToList();

            if (scheduleIsciList.Intersect(isciList).Count() == isciList.Count)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
