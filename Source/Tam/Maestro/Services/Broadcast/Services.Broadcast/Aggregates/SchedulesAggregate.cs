using Common.Services;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Aggregates
{
    public class SchedulesAggregate
    {
        private List<ScheduleAudience> _ScheduleAudiences;
        private List<schedule_details> _ScheduleDetails;
        private List<schedule_iscis> _ScheduleIscis;
        public readonly List<schedule_detail_audiences> _ScheduleDetailAudiences;
        private List<schedule_detail_weeks> _ScheduleDetailWeeks;
        private List<bvs_file_details> _BvsFileDetails;
        private List<bvs_post_details> _BvsPostDetails;
        private List<DisplayMediaWeek> _MediaWeeks;
        private DateTime _StartDate;
        private DateTime _EndDate;

        public SchedulePostType PostType { get; set; }
        public InventorySourceEnum InventorySource { get; set; }
        public bool IsEquivalized { get; set; }
        private schedule _Schedule;
        private List<DisplayDaypart> _RestrictedDayparts;
        private List<market> _RestrictedMarkets;
        private Dictionary<string, string> _StationToAffiliateDict;

        public SchedulesAggregate(schedule schedule,
                                    List<ScheduleAudience> scheduleAudiences,
                                    List<schedule_details> scheduleDetails,
                                    List<schedule_iscis> scheduleIscis,
                                    List<schedule_detail_audiences> scheduleDetailAudiences,
                                    List<schedule_detail_weeks> scheduleDetailWeeks,
                                    List<bvs_file_details> bvsFileDetails,
                                    List<bvs_post_details> bvsPostDetails,
                                    List<DisplayMediaWeek> mediaWeeks,
                                    SchedulePostType postType,
                                    InventorySourceEnum inventorySource,
                                    bool isEquivalized,
                                    DateTime startDate,
                                    DateTime endDate,
                                    Dictionary<string, string> stationToAffiliateDict)
        {
            _Schedule = schedule;
            _RestrictedDayparts = _Schedule.schedule_restriction_dayparts.Select(rdp => DaypartCache.Instance.GetDisplayDaypart(rdp.daypart_id)).ToList();
            _RestrictedMarkets = _Schedule.markets.ToList();
            _ScheduleAudiences = scheduleAudiences;
            _ScheduleDetails = scheduleDetails;
            _ScheduleIscis = scheduleIscis;
            _ScheduleDetailAudiences = scheduleDetailAudiences;
            _ScheduleDetailWeeks = scheduleDetailWeeks;
            _BvsFileDetails = bvsFileDetails;
            _BvsPostDetails = bvsPostDetails;
            _MediaWeeks = mediaWeeks;
            PostType = postType;
            InventorySource = inventorySource;
            _StationToAffiliateDict = stationToAffiliateDict;
            IsEquivalized = isEquivalized;
            _StartDate = startDate;
            _EndDate = endDate;
        }

        public int ScheduleId
        {
            get { return _Schedule.id; }
        }

        public schedule Schedule
        {
            get { return _Schedule; }
        }

        public int PostingBookId
        {
            get { return _Schedule.posting_book_id; }
        }

        public DateTime StartDate
        {
            get { return _StartDate; }
        }

        public DateTime EndDate
        {
            get { return _EndDate; }
        }

        public int GetOrderedSpots()
        {
            return _ScheduleDetails.Sum(sd => sd.total_spots);
        }

        public int GetOrderedSpotsByMediaWeek(int mediaWeekId)
        {
            return _ScheduleDetailWeeks.Where(sdw => sdw.media_week_id == mediaWeekId).Sum(sdw => sdw.spots);
        }

        public int GetDeliveredSpots()
        {
            return _BvsFileDetails.Count(bfd => bfd.IsInSpec());
        }

        public int GetDeliveredSpotsByMediaWeek(int mediaWeekId)
        {
            return _BvsFileDetails.Count(bfd => bfd.IsInSpec() && bfd.schedule_detail_week_id == mediaWeekId);
        }

        public int GetOutOfSpecSpots()
        {
            return _BvsFileDetails.Count(bfd => bfd.IsOutOfSpec());
        }

        public double GetOrderedImpressionsByAudience(int audienceId)
        {
            return _ScheduleDetailAudiences.Where(sda => sda.audience_id == audienceId).Sum(sda => sda.impressions);
        }

        public double GetOrderedImpressionsByAudienceAndMediaWeek(int mediaWeekId, int audienceId)
        {
            var scheduleAudiencesByWeek = _ScheduleDetailAudiences
                .Where(sda => sda.audience_id == audienceId && sda.schedule_details.schedule_detail_weeks.Select(sdw => sdw.media_week_id).Contains(mediaWeekId))
                .Distinct();
            return scheduleAudiencesByWeek.Sum(sda => sda.impressions);
        }

        public double GetDeliveredImpressionsByAudience(int audienceId)
        {
            return _BvsPostDetails.Where(
                bpd =>
                    bpd.bvs_file_details.IsInSpec() &&
                    bpd.audience_id == audienceId).Sum(bpd => bpd.delivery);
        }

        public double GetDeliveredImpressionsByAudienceAndMediaWeek(int mediaWeekId, int audienceId)
        {
            return _BvsPostDetails.Where(
                bpd =>
                    bpd.bvs_file_details.IsInSpec() &&
                    bpd.audience_id == audienceId &&
                    bpd.bvs_file_details.schedule_detail_week_id == mediaWeekId).Sum(bpd => bpd.delivery);
        }

        public double GetRestrictedDeliveredImpressionsByAudienceAndAdvertiserName(string advertiserName, int audienceId)
        {
            return _BvsPostDetails.Where(
                bpd =>
                    AllowedForReport(bpd.bvs_file_details.station, bpd.bvs_file_details.date_aired, bpd.bvs_file_details.time_aired) &&
                    bpd.bvs_file_details.IsInSpec() &&
                    bpd.audience_id == audienceId &&
                    bpd.bvs_file_details.advertiser == advertiserName).Sum(bpd => bpd.delivery);
        }

        public int GetDaypartIdByScheduleDetailWeek(int scheduleDetailWeekId)
        {
            var week = _ScheduleDetailWeeks.Single(x => x.id == scheduleDetailWeekId);
            var detail = _ScheduleDetails.Single(x => x.id == week.schedule_detail_id);
            return detail.daypart_id;
        }

        public List<schedule_details> GetScheduleDetails()
        {
            return _ScheduleDetails;
        }

        public static string CleanStatioName(string stationName)
        {
            return stationName.ToLower().Replace("-tv", "").Replace("+s2", "").Trim();
        }
        public string GetDetailAffiliateFromScheduleDetailId(string stationName)
        {
            var adjName = CleanStatioName(stationName);

            string affiliate;
            if (_StationToAffiliateDict.TryGetValue(adjName, out affiliate))
                return affiliate;

            //throw new Exception(string.Format("Could not find affiliate from station named \"{0}\"",adjName));
            return string.Empty;
        }

        public List<schedule_details> GetUndeliveredScheduleDetails()
        {
            var deliveredSchduleIds =
                _ScheduleDetails.Where(s => _BvsFileDetails.Select(b => b.schedule_detail_week_id).Contains(s.id))
                    .Select(s => s.id)
                    .ToList();
            var details = _ScheduleDetails.Where(s => !deliveredSchduleIds.Contains(s.id));
            return details.ToList();
        }

        public schedule_details GetScheduleDetailById(int scheduleDetailId)
        {
            var detail = _ScheduleDetails.Single(x => x.id == scheduleDetailId);
            return detail;
        }

        public schedule_details GetScheduleDetailByWeekId(int scheduleDetailWeekId)
        {
            var week = _ScheduleDetailWeeks.Single(x => x.id == scheduleDetailWeekId);
            var detail = _ScheduleDetails.Single(x => x.id == week.schedule_detail_id);
            return detail;
        }

        public schedule_detail_weeks GetScheduleDetailWeekById(int scheduleDetailWeekId)
        {
            var week = _ScheduleDetailWeeks.Single(x => x.id == scheduleDetailWeekId);
            return week;
        }

        public List<bvs_file_details> GetBvsDetailsByScheduleId(int scheduleDetailId)
        {
            var scheduleWeeks = _ScheduleDetailWeeks.Where(x => x.schedule_detail_id == scheduleDetailId).Select(w => w.id).ToList();
            // yes, this can be null
            return _BvsFileDetails.Where(b => b.schedule_detail_week_id.HasValue
                                                        && scheduleWeeks.Contains(b.schedule_detail_week_id.Value)).ToList();
        }

        public int GetDeliveredCountFromScheduleWeeks(IEnumerable<int> scheduleWeekIds)
        {
            return _BvsFileDetails.Count(b => b.status == (int)TrackingStatus.InSpec
                                                && b.schedule_detail_week_id.HasValue
                                                && scheduleWeekIds.Contains(b.schedule_detail_week_id.Value));
        }
        public List<bvs_file_details> GetBvsDetails()
        {
            return _BvsFileDetails.ToList();
        }

        public List<BvsPrePostReportData> GetBroadcastPrePostData(List<bvs_file_details> fileDetails)
        {
            var query = _BuildPrePostDataQuery(fileDetails).ToList();

            query.ForEach(
                x =>
                {
                    x.TimeAired = new DateTime(TimeSpan.FromSeconds(x.AirTime).Ticks).ToShortTimeString();
                    x.DateAired = x.Date.ToString("MM/dd/yyyy");
                });

            return query;
        }

        public string GetSpecStatusText(BvsReportOutOfSpecData row)
        {
            var textList = new List<string>();

            if (!row.MatchAirTime)
                textList.Add("Airtime");

            if (!row.MatchIsci)
                textList.Add("ISCI");

            if (!row.MatchProgram)
                textList.Add("Program Name");

            if (!row.MatchStation)
                textList.Add("Station");

            if (!row.MatchSpotLength)
                textList.Add("Spot Length");

            return string.Join(",", textList);
        }

        public void ConvertDateToMediaWeek(BvsReportOutOfSpecData outOfSpecRow)
        {
            outOfSpecRow.MediaWeekId = _MediaWeeks.First(mw => mw.WeekStartDate <= outOfSpecRow.DateAired && outOfSpecRow.DateAired <= mw.WeekEndDate).Id;
        }


        public IEnumerable<ScheduleAudience> GetScheduleAudiences()
        {
            return _ScheduleAudiences;
        }

        public void SetScheduleAudiences(List<ScheduleAudience> newScheduleAudiences)
        {
            _ScheduleAudiences = newScheduleAudiences;
        }

        private IEnumerable<BvsPrePostReportData> _BuildPrePostDataQuery(List<bvs_file_details> fileDetails)
        {
            var estimateId = _Schedule.estimate_id;
            if (estimateId == null)
            {
                return Enumerable.Empty<BvsPrePostReportData>();
            }

            var query =
                (from bfd in fileDetails
                 where bfd.IsInSpec()
                 select new BvsPrePostReportData
                 {
                     Rank = bfd.rank,
                     Market = bfd.market,
                     Station = bfd.station,
                     Affiliate = bfd.affiliate,
                     ProgramName = bfd.program_name,
                     AirTime = bfd.time_aired,
                     Date = _GetBVSFileDetailDate(bfd),
                     Length = bfd.spot_length,
                     IsciDto = _Schedule.schedule_iscis
                                        .Where(i => i.house_isci == bfd.isci)
                                        .Select(i => new IsciDto
                                        {
                                            Id = i.id,
                                            Client = i.client_isci,
                                            House = i.house_isci,
                                            Brand = i.brand
                                        })
                                        .ToList(),
                     Campaign = (int)estimateId,
                     Advertiser = bfd.advertiser,
                     Brand = String.Join("+", _ScheduleIscis.Where(i => i.house_isci.Equals(bfd.isci, StringComparison.InvariantCultureIgnoreCase)).Select(a => a.brand).Distinct().ToArray()),
                     InventorySource = (InventorySourceEnum)_Schedule.inventory_source,
                     AudienceImpressions = bfd.bvs_post_details
                                                .Where(bpd => AllowedForReport(bfd.station, bfd.date_aired, bfd.time_aired))
                                                .Select(bpd => new AudienceImpressionsAndDelivery
                                                {
                                                    AudienceId = bpd.audience_id,
                                                    Delivery = bpd.delivery
                                                }).ToList()
                 }).OrderBy(x => x.Rank);

            return query;
        }

        private DateTime _GetBVSFileDetailDate(bvs_file_details bvsFile)
        {
            return PostType == SchedulePostType.NSI ? bvsFile.nsi_date : bvsFile.nti_date;
        }

        public List<int> GetScheduleWeeks()
        {
            return _ScheduleDetailWeeks.Select(sdw => sdw.media_week_id).Distinct().ToList();
        }

        public List<DateTime> GetBvsDetailDateAired()
        {
            return _BvsFileDetails.Select(d => d.date_aired).ToList();
        }
        public int GetAdvertiserId()
        {
            return _Schedule.advertiser_id;
        }

        /// <summary>
        /// Sets schedule restrictions which are used in the "AllowedForReport" methods.
        /// </summary>
        public void OverrideScheduleRestrictions(List<market> restrictedMarkets, ICollection<schedule_restriction_dayparts> restrictedDayparts)
        {
            _RestrictedMarkets = restrictedMarkets;
            _RestrictedDayparts = restrictedDayparts.Select(rdp => DaypartCache.Instance.GetDisplayDaypart(rdp.daypart_id)).ToList();
        }

        public bool AllowedForReport(string stationName, DateTime dateAired, int timeAired)
        {
            var allowedStationMarket = _IsAllowedStationMarket(stationName);
            if (!allowedStationMarket) return false;

            bool allowed = true;
            if (_RestrictedDayparts.Any())
                allowed = _RestrictedDayparts.Any(dp => dp.Intersects(dateAired, timeAired));
            return allowed;
        }

        public bool AllowedForReport(string stationName, DisplayDaypart daypart)
        {
            var allowedStationMarket = _IsAllowedStationMarket(stationName);
            if (!allowedStationMarket) return false;

            bool allowed = true;
            if (_RestrictedDayparts.Any())
                allowed = _RestrictedDayparts.Any(dp => dp.Intersects(daypart));
            return allowed;
        }

        private bool _IsAllowedStationMarket(string stationName)
        {
            return !_RestrictedMarkets.Any()
                        || !_RestrictedMarkets.Any(m => m.stations.Any(s => s.legacy_call_letters == stationName));
        }
        public void ApplyBvsDetailsFilter(List<string> iscis, DateTime startDate, DateTime endDate, List<TrackingStatus> statusList)
        {
            _BvsFileDetails =
                _BvsFileDetails.Where(
                    d => d.date_aired >= startDate && d.date_aired <= endDate && iscis.Contains(d.isci) && statusList.Contains((TrackingStatus)d.status)).ToList();
        }
    }

    public class BvsPrePostReportData
    {
        public int Rank { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public string ProgramName { get; set; }
        public string TimeAired { get; set; }
        public int AirTime { get; set; }
        public string DateAired { get; set; }
        public DateTime Date { get; set; }
        public int Length { get; set; }
        public List<IsciDto> IsciDto { get; set; }
        public string Isci { get; set; }
        public int Campaign { get; set; }
        public string Advertiser { get; set; }
        public int DaypartId { get; set; }
        public string Brand { get; set; }
        public InventorySourceEnum InventorySource { get; set; }
        public List<AudienceImpressionsAndDelivery> AudienceImpressions { get; set; }


        public double GetDeliveredImpressions(int audienceId)
        {
            return AudienceImpressions.Where(ai => ai.AudienceId == audienceId).Sum(ai => ai.Delivery);
        }
        public double GetNsiDeliveredImpressions(int audienceId)
        {
            return AudienceImpressions.Where(ai => ai.AudienceId == audienceId).Sum(ai => ai.NsiDelivery);
        }
    }

    public class AudienceImpressionsAndDelivery
    {
        public double Impressions { get; set; }
        public int AudienceId { get; set; }
        public double Delivery { get; set; }
        public double NsiDelivery { get; set; }
    }

    public class ScheduleAudience
    {
        public int AudienceId { get; set; }
        public string AudienceName { get; set; }
        public int Rank { get; set; }
        public int Population { get; set; }
    }
}