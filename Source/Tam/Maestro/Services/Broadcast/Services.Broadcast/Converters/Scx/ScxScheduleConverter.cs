using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.Scx
{
    public interface IScxScheduleConverter : IApplicationService, IScheduleConverter
    {
    }
    public class ScxScheduleConverter : ScheduleConverterBase, IScxScheduleConverter
    {
        protected adx _RawScx;
        public ScxScheduleConverter(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                IDaypartCache daypartCache,
                                IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                IBroadcastAudiencesCache audienceCache)
                : base(broadcastDataRepositoryFactory, daypartCache, mediaMonthAndWeekAggregateCache, audienceCache)
        {
        }

        public override schedule Convert(ScheduleDTO scheduleDto)
        {
            // deserialize xml into object
            var scxFile = new ScxFile(scheduleDto.FileStream);
            _RawScx = scxFile.RawScx;

            var convertedSchedule = _MapSchedule(scheduleDto);
            convertedSchedule.schedule_audiences = _MapAudiences();
            convertedSchedule.schedule_details = _MapDetails();

            return convertedSchedule;
        }

        protected override schedule _MapSchedule(ScheduleDTO scheduleDto)
        {
            var returnSchedule = base._MapSchedule(scheduleDto);
            returnSchedule.start_date = _RawScx.campaign.dateRange.startDate;
            returnSchedule.end_date = _RawScx.campaign.dateRange.endDate;

            return returnSchedule;
        }

        private List<schedule_audiences> _MapAudiences()
        {
            var audiences = new List<schedule_audiences>();
            var demos = _FindMatchingAudiences(_RawScx.campaign.demo.ToList());

            foreach (var demo in demos)
            {
                var audience = new schedule_audiences
                {
                    audience_id = demo.Value,
                    rank = demo.Key,
                    population = int.Parse(_RawScx.campaign.populations.Single(x => x.demoRank == demo.Key).Value)
                };
                audiences.Add(audience);
            }
            return audiences;
        }

        private List<schedule_details> _MapDetails()
        {
            Dictionary<int, int> spotLengthDict = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsByDuration();

            var demos = _FindMatchingAudiences(_RawScx.campaign.demo.ToList());
            var details = new List<schedule_details>();
            foreach (var order in _RawScx.campaign.order)
            {
                foreach (var systemOrder in order.systemOrder)
                {
                    foreach (var detailLine in systemOrder.detailLine)
                    {
                        var detail = new schedule_details();
                        if (detailLine.network == null)
                            continue;
                        detail.market = order.market.name;
                        detail.network = detailLine.network.First().name;
                        detail.program = detailLine.program;
                        detail.spot_length = detailLine.length;
                        detail.spot_length_id = _LookupSpotLengthId(detail.spot_length, spotLengthDict);
                        var daypart = _GetDisplayDaypartFromOrderDetail(detailLine);
                        detail.daypart_id = _DaypartCache.GetIdByDaypart(daypart);
                        if (detailLine.totals == null)
                            continue;
                        detail.total_cost = detailLine.totals.cost.Value;
                        detail.total_spots = int.Parse(detailLine.totals.spots);
                        detail.spot_cost = detailLine.spotCost.Value;

                        foreach (var spot in detailLine.spot)
                        {
                            var detailWeek = new schedule_detail_weeks();
                            detailWeek.start_date = systemOrder.weeks.week.Single(x => x.number == spot.weekNumber).startDate;
                            detailWeek.end_date = detailWeek.start_date.AddDays(6);
                            detailWeek.media_week_id = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(detailWeek.start_date).Id;
                            int quantity;
                            if (int.TryParse(spot.quantity, out quantity))
                                detailWeek.spots = quantity;
                            detail.schedule_detail_weeks.Add(detailWeek);
                        }

                        var audiences = new List<schedule_detail_audiences>();
                        if (detailLine.demoValue != null)
                        {
                            foreach (var demoValue in detailLine.demoValue)
                            {
                                var demoRank = int.Parse(demoValue.demoRank);
                                audiences.Add(new schedule_detail_audiences()
                                {
                                    audience_rank = demoRank
                                    ,
                                    impressions = double.Parse(demoValue.value.Single(pVal => pVal.type == demoValueValueType.Impressions).Value)
                                    ,
                                    audience_population = systemOrder.populations.Single(pop => pop.demoRank == demoRank).Value != null ? int.Parse(systemOrder.populations.Single(pop => pop.demoRank == demoRank).Value) : 0
                                    ,
                                    audience_id = demos[demoRank]
                                });
                            }
                        }
                        detail.schedule_detail_audiences = audiences;
                        details.Add(detail);
                    }
                }
            }
            return details;
        }

        private int _LookupSpotLengthId(string spot_length_string, Dictionary<int, int> spotLengthsDict)
        {
            var convertedSpotLength = new StringBuilder(spot_length_string);
            convertedSpotLength = convertedSpotLength.Replace("PT", "").Replace("S", "");

            int spotLength;
            if (!Int32.TryParse(convertedSpotLength.ToString(), out spotLength))
                throw new Exception(string.Format("Cannot convert spot length, {0}, to number ", spot_length_string));

            return spotLengthsDict[spotLength];
        }

        public static string OverwriteDateTimeToHoursMinutesSeconds(string pXml)
        {
            var lXmlDocument = new XmlDocument();
            lXmlDocument.LoadXml(pXml);
            var startTimes = lXmlDocument.DocumentElement.GetElementsByTagName("startTime");
            var endTimes = lXmlDocument.DocumentElement.GetElementsByTagName("endTime");
            foreach (XmlNode timeNod in startTimes)
            {
                if (timeNod.InnerText.Length > 7)
                    timeNod.InnerText = timeNod.InnerText.Substring(0, 8);
            }
            foreach (XmlNode timeNod in endTimes)
            {
                if (timeNod.InnerText.Length > 7)
                    timeNod.InnerText = timeNod.InnerText.Substring(0, 8);
            }
            var sw = new StringWriter();
            var tx = new XmlTextWriter(sw);
            lXmlDocument.WriteTo(tx);

            return sw.ToString();
        }

        private DisplayDaypart _GetDisplayDaypartFromOrderDetail(detailLine orderDetail)
        {
            var dayPart = new DisplayDaypart();
            dayPart.Monday = orderDetail.dayOfWeek.Monday == "Y";
            dayPart.Tuesday = orderDetail.dayOfWeek.Tuesday == "Y";
            dayPart.Wednesday = orderDetail.dayOfWeek.Wednesday == "Y";
            dayPart.Thursday = orderDetail.dayOfWeek.Thursday == "Y";
            dayPart.Friday = orderDetail.dayOfWeek.Friday == "Y";
            dayPart.Saturday = orderDetail.dayOfWeek.Saturday == "Y";
            dayPart.Sunday = orderDetail.dayOfWeek.Sunday == "Y";
            dayPart.StartTime = (int)orderDetail.startTime.TimeOfDay.TotalSeconds;
            dayPart.EndTime = (int)orderDetail.endTime.TimeOfDay.TotalSeconds == 0 ? 86399 : (int)orderDetail.endTime.TimeOfDay.TotalSeconds - 1; //handling for midnight
            return dayPart;
        }

        private Dictionary<int, int> _FindMatchingAudiences(List<demo> demos)
        {
            var matchedDemos = new Dictionary<int, int>();
            foreach (var demo in demos)
            {
                var audiences = _AudienceCache.FindByAgeRange(int.Parse(demo.ageFrom), int.Parse(demo.ageTo)).Select(a => new LookupDto(a.Id, a.SubCategoryCode)).ToList();
                LookupDto bestMatch = null;
                if (audiences.Any())
                {
                    foreach (var lAudience in audiences)
                    {
                        switch (demo.group.ToString())
                        {
                            case "Households":
                                {
                                    if (lAudience.Display == "H")
                                        bestMatch = lAudience;
                                    break;
                                }
                            case "Adults":
                                {
                                    if (lAudience.Display == "A")
                                        bestMatch = lAudience;
                                    break;
                                }
                            case "Men":
                                {
                                    if (lAudience.Display == "M")
                                        bestMatch = lAudience;
                                    break;
                                }
                            case "Women":
                                {
                                    if (lAudience.Display == "W")
                                        bestMatch = lAudience;
                                    break;
                                }
                            case "Children":
                                {
                                    if (lAudience.Display == "C")
                                        bestMatch = lAudience;
                                    break;
                                }
                            case "Persons":
                                {
                                    if (lAudience.Display == "P")
                                        bestMatch = lAudience;
                                    break;
                                }
                        }
                    }
                }

                if (bestMatch == null)
                    throw new InvalidOperationException(string.Format("Audience components were unable to be determined for the following demographic: {0} {1}-{2}. Please contact the help desk for assistance.", demo.group, demo.ageFrom, demo.ageTo));

                matchedDemos.Add(demo.demoRank, bestMatch.Id);
            }

            return matchedDemos;
        }
    }
}
