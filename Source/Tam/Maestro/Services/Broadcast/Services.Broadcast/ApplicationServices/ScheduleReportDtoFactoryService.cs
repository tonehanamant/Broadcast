using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Aggregates;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.Extensions;
using Services.Broadcast.BusinessEngines;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IScheduleReportDtoFactoryService : IApplicationService
    {
        ScheduleReportDto GenereteScheduleReportData(SchedulesAggregate schedulesAggregate,
            ScheduleReportType reportType);
    }

    public enum ScheduleReportType
    {
        Schedule,
        Client,
        ThirdPartyProvider
    }

    public class ScheduleReportDtoFactoryService : IScheduleReportDtoFactoryService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IDaypartCache _DaypartCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;

        public ScheduleReportDtoFactoryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache audiencesCache, IImpressionAdjustmentEngine impressionAdjustmentEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _DaypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _AudiencesCache = audiencesCache;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
        }

        public ScheduleReportDto GenereteScheduleReportData(SchedulesAggregate schedulesAggregate,
            ScheduleReportType reportType)
        {
            // fetch audience name from maestro db
            foreach (var audience in schedulesAggregate.GetScheduleAudiences())
            {
                var audienceName = _AudiencesCache.FindDto(audience.AudienceId).Display;
                audience.AudienceName = audienceName;
            }

            //BEGIN Advertiser Data
            var scheduleAudiences = schedulesAggregate.GetScheduleAudiences().ToList();
            var advertiserDataDto = new AdvertiserDataDto(scheduleAudiences);
            var coreDataList = new List<AdvertiserCoreData>();

            var bvsDetailList = _GatherCoreDataAndBvsDetails(schedulesAggregate, scheduleAudiences, coreDataList);

            _GatherAdvertiserData(schedulesAggregate, scheduleAudiences, coreDataList, advertiserDataDto);
            //END Advertiser Data

            //BEGIN Weekly Data
            var weeklyData = _GatherWeeklyData(schedulesAggregate, scheduleAudiences, coreDataList, bvsDetailList);
            //END Weekly Data

            //BEGIN Station Summary
            var stationSummaryData = new StationSummaryDto(scheduleAudiences);
            var outOfSpecBvsDetails = _GatherStationSummaryData(schedulesAggregate, scheduleAudiences, coreDataList,
                stationSummaryData, bvsDetailList);
            //END Station Summary

            // BEGIN out of spec to date
            var outOfSpecToDate =
                _GatherOutOfSpecToDateData(schedulesAggregate, scheduleAudiences, outOfSpecBvsDetails);
            // END out of spec to date

            //BEGIN spot details
            var prePostDataList = schedulesAggregate.GetBroadcastPrePostData(bvsDetailList);

            //Order of adjustments is important. If main delivery is calculated first, the NsiDelivery would get discounted twice. We may need to consider how to make this more resilient.
            prePostDataList.ForEach(d => d.AudienceImpressions.ForEach(a =>
                a.NsiDelivery = _ImpressionAdjustmentEngine.AdjustImpression(a.Delivery,
                    schedulesAggregate.IsEquivalized, d.Length, SchedulePostType.NSI,
                    schedulesAggregate.PostingBookId)));
            prePostDataList.ForEach(d => d.AudienceImpressions.ForEach(a =>
                a.Delivery = _ImpressionAdjustmentEngine.AdjustImpression(a.Delivery, schedulesAggregate.IsEquivalized,
                    d.Length, schedulesAggregate.PostType, schedulesAggregate.PostingBookId)));
            var spotDetailsData = new SpotDetailDto(scheduleAudiences, prePostDataList, schedulesAggregate);
            _SetPrePostDetailIscisByReportType(spotDetailsData, reportType);
            //END spot details

            var deliveryBySource = new SpotsAndImpressionsDeliveryBySource
            {
                Source = schedulesAggregate.InventorySource,
                Spots = advertiserDataDto.DeliveredSpots ?? 0,
                AudienceImpressions =
                    advertiserDataDto.ImpressionsAndDelivey.ToDictionary(a => a.AudienceId, i => i.DeliveredImpressions)
            };

            var deliveryByAdvertiser =
                _GatherDeliveryByAdvertiser(schedulesAggregate, bvsDetailList, scheduleAudiences);

            // construct dtos
            var reportDto = new ScheduleReportDto
            {
                ScheduleId = schedulesAggregate.ScheduleId,
                AdvertiserData = advertiserDataDto,
                WeeklyData = weeklyData,
                StationSummaryData = stationSummaryData,
                SpotDetailData = spotDetailsData,
                OutOfSpecToDate = outOfSpecToDate,
                SpotsAndImpressionsBySource = new List<SpotsAndImpressionsDeliveryBySource> {deliveryBySource},
                SpotsAndImpressionsDeliveryByAdvertiser = deliveryByAdvertiser
            };

            return reportDto;
        }

        private static void _SetPrePostDetailIscisByReportType(SpotDetailDto spotDetailsData,
            ScheduleReportType reportType)
        {
            switch (reportType)
            {
                case ScheduleReportType.Schedule:
                    spotDetailsData.ReportData.ForEach(
                        d => d.Isci = d.IsciDto.Count > 1
                            ? d.IsciDto.Select(i => i.House).FirstOrDefault()
                            : d.IsciDto.Select(i => i.Client).FirstOrDefault());
                    break;

                case ScheduleReportType.Client:
                    spotDetailsData.ReportData.ForEach(
                        d => d.Isci = d.IsciDto.Count > 1
                            ? d.IsciDto.Select(i => string.Format("{0}(M)", i.House)).FirstOrDefault()
                            : d.IsciDto.Select(i => i.Client).FirstOrDefault());
                    break;

                case ScheduleReportType.ThirdPartyProvider:
                    spotDetailsData.ReportData.ForEach(d => d.Isci = d.IsciDto.Select(i => i.House).FirstOrDefault());
                    break;
            }
        }

        private List<SpotsAndImpressionsDeliveryByAdvertiser> _GatherDeliveryByAdvertiser(
            SchedulesAggregate schedulesAggregate,
            IEnumerable<bvs_file_details> bvsDetailList,
            List<ScheduleAudience> scheduleAudiences)
        {
            var inSpecDetailsByAdvertiserAndSpotLength =
                bvsDetailList.Where(
                        d => d.status == 1 &&
                             schedulesAggregate.AllowedForReport(d.station, d.date_aired, d.time_aired))
                    .GroupBy(
                        d => new
                        {
                            Advertiser = d.advertiser,
                            SpotLength = d.spot_length
                        }).ToList();

            var deliveryByAdvertiserAndSpotLength = new List<SpotsAndImpressionsDeliveryByAdvertiser>();

            foreach (var advertiserGroup in inSpecDetailsByAdvertiserAndSpotLength)
            {
                var deliveryByAdvertiser = new SpotsAndImpressionsDeliveryByAdvertiser();
                deliveryByAdvertiser.AdvertiserName = advertiserGroup.Key.Advertiser;
                deliveryByAdvertiser.Spots = advertiserGroup.Count();
                deliveryByAdvertiser.AudienceImpressions = scheduleAudiences.ToDictionary(
                    a => a.AudienceId,
                    a => _ImpressionAdjustmentEngine.AdjustImpression(
                        schedulesAggregate.GetRestrictedDeliveredImpressionsByAudienceAndAdvertiserName(
                            advertiserGroup.Key.Advertiser, a.AudienceId),
                        schedulesAggregate.IsEquivalized, advertiserGroup.Key.SpotLength, schedulesAggregate.PostType,
                        schedulesAggregate.PostingBookId)
                );
                deliveryByAdvertiserAndSpotLength.Add(deliveryByAdvertiser);
            }

            var result =
                deliveryByAdvertiserAndSpotLength.GroupBy(d => d.AdvertiserName)
                    .Select(
                        d => new SpotsAndImpressionsDeliveryByAdvertiser
                        {
                            AdvertiserName = d.Key,
                            Spots = d.Sum(x => x.Spots),
                            AudienceImpressions = d.SelectMany(x => x.AudienceImpressions).GroupBy(i => i.Key)
                                .ToDictionary(a => a.Key, a => a.Sum(y => y.Value))
                        }).ToList();

            return result;
        }

        private OutOfSpecToDateDto _GatherOutOfSpecToDateData(SchedulesAggregate schedulesAggregate,
            List<ScheduleAudience> scheduleAudiences,
            IEnumerable<bvs_file_details> outOfSpecBvsDetails)
        {
            var outOfSpecToDate = new OutOfSpecToDateDto(scheduleAudiences);

            var outOfSpecGroupedData = outOfSpecBvsDetails.GroupBy(x => new
            {
                Rank = x.rank,
                Market = x.market,
                Station = x.station,
                Affiliate = x.affiliate,
                ProgramName = x.program_name,
                SpotLength = x.spot_length,
                Isci = x.isci,
                BvsDateAired = x.date_aired,
                BroadcastDate = _GetBroadcastDate(x, schedulesAggregate),
                TimeAired = new DateTime().AddSeconds(x.time_aired),
                MatchAirTime = x.match_airtime,
                MatchIsci = x.match_isci,
                MatchProgram = x.match_program,
                MatchStation = x.match_station,
                MatchSpotLength = x.match_spot_length
            });

            foreach (var bvsDetailGroup in outOfSpecGroupedData)
            {
                var bvsReportData = new BvsReportOutOfSpecData
                {
                    Rank = bvsDetailGroup.Key.Rank,
                    Market = bvsDetailGroup.Key.Market,
                    Station = bvsDetailGroup.Key.Station,
                    Affiliate = bvsDetailGroup.Key.Affiliate,
                    SpotLength = bvsDetailGroup.Key.SpotLength,
                    ProgramName = bvsDetailGroup.Key.ProgramName,
                    Isci = bvsDetailGroup.Key.Isci,
                    Status = 0,
                    BvsDate = bvsDetailGroup.Key.BvsDateAired,
                    BroadcastDate = bvsDetailGroup.Key.BroadcastDate,
                    TimeAired = bvsDetailGroup.Key.TimeAired,
                    MatchAirTime = bvsDetailGroup.Key.MatchAirTime,
                    MatchIsci = bvsDetailGroup.Key.MatchIsci,
                    MatchProgram = bvsDetailGroup.Key.MatchProgram,
                    MatchStation = bvsDetailGroup.Key.MatchStation,
                    MatchSpotLength = bvsDetailGroup.Key.MatchSpotLength,
                    OutOfSpecSpots = bvsDetailGroup.Count(),
                };
                bvsReportData.SpecStatus = schedulesAggregate.GetSpecStatusText(bvsReportData);

                var bvsDetails = bvsDetailGroup.Select(x => x).ToList();
                foreach (var audience in scheduleAudiences)
                {
                    var audienceImpressionsAndDeliveries = new AudienceImpressionsAndDelivery();
                    audienceImpressionsAndDeliveries.AudienceId = audience.AudienceId;
                    audienceImpressionsAndDeliveries.Delivery = 0;

                    foreach (var bvsDetail in bvsDetails)
                    {
                        if (schedulesAggregate.AllowedForReport(bvsReportData.Station, bvsDetail.date_aired,
                            bvsDetail.time_aired))
                        {
                            var data = GetOutOfScopeTotalDeliveryDetailsByAudienceId(bvsDetail, audience.AudienceId);
                            audienceImpressionsAndDeliveries.Delivery +=
                                _ImpressionAdjustmentEngine.AdjustImpression(data.Item2,
                                    schedulesAggregate.IsEquivalized, bvsReportData.SpotLength,
                                    schedulesAggregate.PostType, schedulesAggregate.PostingBookId);
                        }
                    }

                    bvsReportData.AudienceImpressions.Add(audienceImpressionsAndDeliveries);
                }

                outOfSpecToDate.ReportData.Add(bvsReportData);
            }

            //OUT OF SPEC TOTALS
            foreach (var audience in outOfSpecToDate.ScheduleAudiences)
            {
                var audienceData = outOfSpecToDate.ReportData
                    .SelectMany(row => row.AudienceImpressions)
                    .Where(ai => ai.AudienceId == audience.AudienceId)
                    .ToList();
                var outOfSpecAudienceData = outOfSpecToDate.GetOutOfSpec()
                    .SelectMany(row => row.AudienceImpressions)
                    .Where(ai => ai.AudienceId == audience.AudienceId)
                    .Sum(s => s.Delivery);

                outOfSpecToDate.ImpressionsAndDelivery.Add(
                    new ImpressionAndDeliveryDto
                    {
                        AudienceId = audience.AudienceId,
                        AudienceName = audience.AudienceName,
                        OrderedImpressions = audienceData.Sum(x => x.Impressions),
                        DeliveredImpressions = audienceData.Sum(x => x.Delivery),
                        OutOfSpecDeliveredImpressions = outOfSpecAudienceData
                    });
            }

            return outOfSpecToDate;
        }

        private List<bvs_file_details> _GatherStationSummaryData(SchedulesAggregate schedulesAggregate,
            List<ScheduleAudience> scheduleAudiences,
            List<AdvertiserCoreData> coreDataList,
            StationSummaryDto stationSummaryData,
            List<bvs_file_details> bvsDetailList)
        {
            foreach (var coreData in coreDataList)
            {
                var scheduleDetail = coreData.ScheduleDetail;

                var orderedSpots = scheduleDetail.schedule_detail_weeks.Sum(w => w.spots);
                int deliveredSpots = schedulesAggregate.GetDeliveredCountFromScheduleWeeks(
                    scheduleDetail.schedule_detail_weeks.Select(w => w.id));

                if (orderedSpots == 0 && deliveredSpots == 0)
                    continue;

                var specStatus = deliveredSpots > 0 ? "Match" : string.Empty;
                var bvsReportData = new BvsReportData
                {
                    Rank = coreData.Rank,
                    Market = coreData.Market,
                    Station = coreData.Station,
                    Affiliate = coreData.Affiliate,
                    SpotLength = coreData.SpotLength,
                    ProgramName = scheduleDetail.program,
                    DisplayDaypart = _DaypartCache.GetDisplayDaypart(scheduleDetail.daypart_id),
                    Cost = scheduleDetail.spot_cost * deliveredSpots,
                    SpotCost = scheduleDetail.spot_cost,
                    OrderedSpots = orderedSpots,
                    DeliveredSpots = deliveredSpots,
                    SpotClearance = (double) deliveredSpots / orderedSpots,
                    Status = 1,
                    SpecStatus = specStatus,
                };

                foreach (var audience in scheduleAudiences)
                {
                    var audienceData =
                        scheduleDetail.schedule_detail_audiences.Where(a => a.audience_id == audience.AudienceId);

                    var audienceImpressionAndDelivery = new AudienceImpressionsAndDelivery
                    {
                        AudienceId = audience.AudienceId,
                        Impressions = 0,
                    };

                    var audienceImpressions = audienceData.Sum(a => a.impressions);
                    audienceImpressionAndDelivery.Impressions += scheduleDetail.total_spots * audienceImpressions;

                    var audienceAndDeliver =
                        coreData.BvsDetails
                            .SelectMany(x => x.bvs_post_details)
                            .Where(x => x.audience_id == audience.AudienceId)
                            .ToList();
                    audienceImpressionAndDelivery.Delivery = _ImpressionAdjustmentEngine.AdjustImpression(
                        audienceAndDeliver.Sum(x => x.delivery), schedulesAggregate.IsEquivalized,
                        bvsReportData.SpotLength,
                        schedulesAggregate.PostType, schedulesAggregate.PostingBookId);

                    bvsReportData.AudienceImpressions.Add(audienceImpressionAndDelivery);
                }

                stationSummaryData.ReportData.Add(bvsReportData);
            }

            //Out of Spec
            var outOfSpecBvsDetails = bvsDetailList.Where(x => x.IsOutOfSpec()).ToList();

            var groupedOutOfSpecBvsDetailsList = outOfSpecBvsDetails.GroupBy(x => new
            {
                Rank = x.rank,
                Market = x.market,
                Station = x.station,
                Affiliate = x.affiliate,
                SpotLength = x.spot_length,
                MatchAirTime = x.match_airtime,
                MatchIsci = x.match_isci,
                MatchProgram = x.match_program,
                MatchStation = x.match_station,
                MatchSpotLength = x.match_spot_length
            }).ToList();

            foreach (var groupedOutOfSpecBvsDetails in groupedOutOfSpecBvsDetailsList)
            {
                var bvsReportData = new BvsReportOutOfSpecData
                {
                    Rank = groupedOutOfSpecBvsDetails.Key.Rank,
                    Market = groupedOutOfSpecBvsDetails.Key.Market,
                    Station = groupedOutOfSpecBvsDetails.Key.Station,
                    Affiliate = groupedOutOfSpecBvsDetails.Key.Affiliate,
                    SpotLength = groupedOutOfSpecBvsDetails.Key.SpotLength,
                    ProgramName = null,
                    Status = 0,
                    MatchAirTime = groupedOutOfSpecBvsDetails.Key.MatchAirTime,
                    MatchIsci = groupedOutOfSpecBvsDetails.Key.MatchIsci,
                    MatchProgram = groupedOutOfSpecBvsDetails.Key.MatchProgram,
                    MatchStation = groupedOutOfSpecBvsDetails.Key.MatchStation,
                    MatchSpotLength = groupedOutOfSpecBvsDetails.Key.MatchSpotLength,
                    OutOfSpecSpots = groupedOutOfSpecBvsDetails.Count(),
                };
                bvsReportData.SpecStatus = schedulesAggregate.GetSpecStatusText(bvsReportData);

                var bvsDetails = groupedOutOfSpecBvsDetails.Select(x => x).ToList();
                foreach (var audience in scheduleAudiences)
                {
                    var audienceImpressionsAndDeliveries = new AudienceImpressionsAndDelivery();
                    audienceImpressionsAndDeliveries.AudienceId = audience.AudienceId;
                    audienceImpressionsAndDeliveries.Delivery = 0;

                    foreach (var bvsDetail in bvsDetails)
                    {
                        if (schedulesAggregate.AllowedForReport(bvsDetail.station, bvsDetail.date_aired,
                            bvsDetail.time_aired))
                        {
                            var data = GetOutOfScopeTotalDeliveryDetailsByAudienceId(bvsDetail, audience.AudienceId);
                            audienceImpressionsAndDeliveries.Delivery +=
                                _ImpressionAdjustmentEngine.AdjustImpression(data.Item2,
                                    schedulesAggregate.IsEquivalized, bvsReportData.SpotLength,
                                    schedulesAggregate.PostType, schedulesAggregate.PostingBookId);
                        }
                    }

                    bvsReportData.AudienceImpressions.Add(audienceImpressionsAndDeliveries);
                }

                stationSummaryData.ReportData.Add(bvsReportData);
            }

            stationSummaryData.ReportData =
                stationSummaryData.ReportData.OrderBy(rd => rd.Station).ThenBy(rd => rd.Rank).ToList();

            //STATION SUMMARY TOTALS
            foreach (var audience in stationSummaryData.ScheduleAudiences)
            {
                var audienceData = stationSummaryData.GetInSpec()
                    .SelectMany(row => row.AudienceImpressions)
                    .Where(ai => ai.AudienceId == audience.AudienceId)
                    .ToList();
                var deliveredImpressions = stationSummaryData.GetInSpec()
                    .SelectMany(row => row.AudienceImpressions)
                    .Where(ai => ai.AudienceId == audience.AudienceId)
                    .Sum(row => row.Delivery);
                var outOfSpecAudienceData = stationSummaryData.GetOutOfSpec()
                    .SelectMany(row => row.AudienceImpressions)
                    .Where(ai => ai.AudienceId == audience.AudienceId)
                    .Sum(s => s.Delivery);

                stationSummaryData.ImpressionsAndDelivery.Add(
                    new ImpressionAndDeliveryDto
                    {
                        AudienceId = audience.AudienceId,
                        AudienceName = audience.AudienceName,
                        OrderedImpressions = audienceData.Sum(x => x.Impressions),
                        DeliveredImpressions = deliveredImpressions,
                        OutOfSpecDeliveredImpressions = outOfSpecAudienceData
                    });
            }

            return outOfSpecBvsDetails;
        }


        private List<bvs_file_details> _GatherCoreDataAndBvsDetails(SchedulesAggregate schedulesAggregate,
            List<ScheduleAudience> scheduleAudiences,
            List<AdvertiserCoreData> coreDataList)
        {
            var marketNamedRanks = GetMarketRanks(schedulesAggregate);
            var spotLengthRepo = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            var bvsDetailList = schedulesAggregate.GetBvsDetails();
            var details = schedulesAggregate.GetScheduleDetails();

            foreach (var scheduleDetail in details) //in-spec
            {
                int rank;
                var affiliate = schedulesAggregate.GetDetailAffiliateFromScheduleDetailId(scheduleDetail.network);
                var spotLength = spotLengthRepo.GetSpotLengthById(scheduleDetail.spot_length_id.Value);

                var bvsDetails = schedulesAggregate.GetBvsDetailsByScheduleId(scheduleDetail.id);

                if (!marketNamedRanks.TryGetValue(scheduleDetail.market.ToLower(), out rank))
                    if (bvsDetails.Any())
                        rank = bvsDetails.First().rank;

                var coreReportData = new AdvertiserCoreData
                {
                    Rank = rank,
                    Market = scheduleDetail.market,
                    Station = scheduleDetail.network,
                    Affiliate = affiliate,
                    ProgramName = scheduleDetail.program,
                    GroupedByName = scheduleDetail.program,
                    SpotLength = spotLength,
                    ScheduleDetail = scheduleDetail,
                    BvsDetails = bvsDetails
                };

                coreDataList.Add(coreReportData);
            }

            foreach (var bvsFileDetail in bvsDetailList)
            {
                int rank;

                if (marketNamedRanks.TryGetValue(bvsFileDetail.market.ToLower(), out rank))
                    bvsFileDetail.rank = rank;
            }

            return bvsDetailList;
        }

        private void _GatherAdvertiserData(SchedulesAggregate schedulesAggregate,
            List<ScheduleAudience> scheduleAudiences,
            List<AdvertiserCoreData> coreDataList,
            AdvertiserDataDto advertiserDataDto)
        {
            foreach (var coreData in coreDataList)
            {
                var scheduleDetail = coreData.ScheduleDetail;

                var orderedSpots = scheduleDetail.total_spots;
                var deliveredSpots =
                    schedulesAggregate.GetDeliveredCountFromScheduleWeeks(
                        scheduleDetail.schedule_detail_weeks.Select(w => w.id));

                if (orderedSpots == 0 && deliveredSpots == 0)
                    continue;

                var bvsReportData = new BvsReportData
                {
                    Rank = coreData.Rank,
                    Market = coreData.Market,
                    Station = coreData.Station,
                    Affiliate = coreData.Affiliate,
                    ProgramName = coreData.ProgramName,
                    SpotLength = coreData.SpotLength,
                    DisplayDaypart = _DaypartCache.GetDisplayDaypart(scheduleDetail.daypart_id),
                    Cost = scheduleDetail.spot_cost * deliveredSpots,
                    SpotCost = scheduleDetail.spot_cost,
                    OrderedSpots = scheduleDetail.total_spots,
                    DeliveredSpots = deliveredSpots,
                    SpotClearance = deliveredSpots / (double) orderedSpots,
                };
                advertiserDataDto.ReportData.Add(bvsReportData);

                foreach (var audience in scheduleAudiences)
                {
                    var audienceData =
                        scheduleDetail.schedule_detail_audiences.SingleOrDefault(
                            a => a.audience_id == audience.AudienceId);

                    var delivery =
                        coreData.BvsDetails.SelectMany(b => b.bvs_post_details)
                            .Where(p => p.audience_id == audience.AudienceId)
                            .Sum(p => p.delivery);

                    var audienceImpressionAndDelivery = new AudienceImpressionsAndDelivery
                    {
                        AudienceId = audience.AudienceId,
                        Impressions =
                            scheduleDetail.total_spots * (audienceData == null ? 0 : audienceData.impressions),
                        Delivery =
                            _ImpressionAdjustmentEngine.AdjustImpression(delivery,
                                schedulesAggregate.IsEquivalized, bvsReportData.SpotLength, schedulesAggregate.PostType,
                                schedulesAggregate.PostingBookId),
                    };
                    bvsReportData.AudienceImpressions.Add(audienceImpressionAndDelivery);
                }
            }

            //TOTALS FOR ADVERTISER
            foreach (var scheduleAudience in scheduleAudiences)
            {
                var audienceData = advertiserDataDto.ReportData
                    .Where(row => row.DisplayDaypart != null || row.Status == 1)
                    .SelectMany(row => row.AudienceImpressions)
                    .Where(ai => ai.AudienceId == scheduleAudience.AudienceId)
                    .ToList();
                var deliveredImpression = advertiserDataDto.ReportData
                    .Where(row =>
                        schedulesAggregate.AllowedForReport(row.Station, row.DisplayDaypart) &&
                        (row.DisplayDaypart != null || row.Status == 1))
                    .SelectMany(row => row.AudienceImpressions)
                    .Where(ai => ai.AudienceId == scheduleAudience.AudienceId)
                    .Sum(x => x.Delivery);

                advertiserDataDto.ImpressionsAndDelivey.Add(
                    new ImpressionAndDeliveryDto
                    {
                        AudienceId = scheduleAudience.AudienceId,
                        AudienceName = scheduleAudience.AudienceName,
                        OrderedImpressions = audienceData.Sum(x => x.Impressions),
                        DeliveredImpressions = deliveredImpression,
                    });
            }
        }

        private WeeklyDataDto _GatherWeeklyData(SchedulesAggregate schedulesAggregate,
            List<ScheduleAudience> scheduleAudiences,
            List<AdvertiserCoreData> coreDataList,
            List<bvs_file_details> bvsDetailList)
        {
            var weeklyData = new WeeklyDataDto(scheduleAudiences);
            var scheduleWeeks = schedulesAggregate.GetScheduleWeeks().ToList();
            var bvsDetailDateAired = schedulesAggregate.GetBvsDetailDateAired();
            bvsDetailDateAired.ForEach(mw =>
            {
                var mediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(mw);
                if (!scheduleWeeks.Contains(mediaWeek.Id))
                    scheduleWeeks.Add(mediaWeek.Id);
            });
            var weeks = scheduleWeeks.Distinct().OrderBy(w => w)
                .Select(weekId => _MediaMonthAndWeekAggregateCache.FindMediaWeekLookup(weekId)).ToList();

            foreach (var week in weeks)
            {
                var weeklyDto = new WeeklyImpressionAndDeliveryDto
                {
                    Week = week,
                };
                weeklyData.ReportDataByWeek.Add(weeklyDto);

                //In Spec
                foreach (var detail in coreDataList)
                {
                    var scheduleDetail = detail.ScheduleDetail;
                    ;

                    var weekDetail = scheduleDetail.schedule_detail_weeks.Where(w => w.media_week_id == week.Id);
                    var orderedSpots = weekDetail.Sum(w => w.spots);
                    var deliveredSpots =
                        schedulesAggregate.GetDeliveredCountFromScheduleWeeks(weekDetail.Select(w => w.id));

                    if (orderedSpots == 0 && deliveredSpots == 0)
                        continue;

                    var status = deliveredSpots > 0 ? "Match" : string.Empty;
                    var bvsReportData = new BvsReportData
                    {
                        Rank = detail.Rank,
                        Market = detail.Market,
                        Station = detail.Station,
                        Affiliate = detail.Affiliate,
                        ProgramName = detail.ProgramName,
                        SpotLength = detail.SpotLength,
                        DisplayDaypart = _DaypartCache.GetDisplayDaypart(scheduleDetail.daypart_id),
                        Cost = scheduleDetail.spot_cost * deliveredSpots,
                        SpotCost = scheduleDetail.spot_cost,
                        OrderedSpots = orderedSpots,
                        DeliveredSpots = deliveredSpots,
                        SpotClearance = (double) deliveredSpots / orderedSpots,
                        Status = 1, //in-spec
                        SpecStatus = status,
                    };

                    foreach (var audience in scheduleAudiences)
                    {
                        var audienceData =
                            schedulesAggregate._ScheduleDetailAudiences.SingleOrDefault(
                                x => x.schedule_detail_id == scheduleDetail.id && x.audience_id == audience.AudienceId);
                        var audienceImpressions = (audienceData == null ? 0 : audienceData.impressions);
                        var deliveries =
                            detail.BvsDetails.Where(dg => dg.schedule_detail_weeks.media_week_id == week.Id)
                                .SelectMany(x => x.bvs_post_details)
                                .Where(x => x.audience_id == audience.AudienceId)
                                .ToList();

                        var audienceImpressionAndDelivery = new AudienceImpressionsAndDelivery
                        {
                            AudienceId = audience.AudienceId,
                            Impressions = bvsReportData.OrderedSpots * audienceImpressions,
                            Delivery =
                                _ImpressionAdjustmentEngine.AdjustImpression(deliveries.Sum(x => x.delivery),
                                    schedulesAggregate.IsEquivalized, bvsReportData.SpotLength,
                                    schedulesAggregate.PostType, schedulesAggregate.PostingBookId)
                        };
                        bvsReportData.AudienceImpressions.Add(audienceImpressionAndDelivery);
                    }

                    weeklyDto.ReportData.Add(bvsReportData);
                }

                //Out of Spec
                foreach (var bvsDetail in bvsDetailList.OutOfSpec().ToList())
                {
                    var mediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(bvsDetail.date_aired);
                    if (mediaWeek.Id != week.Id) continue;

                    var bvsReportData = new BvsReportOutOfSpecData
                    {
                        Rank = bvsDetail.rank,
                        Market = bvsDetail.market,
                        Station = bvsDetail.station,
                        Affiliate = bvsDetail.affiliate,
                        ProgramName = bvsDetail.program_name,
                        SpotLength = bvsDetail.spot_length,
                        Isci = bvsDetail.isci,
                        Status = 0, //out-of-spec,
                        BvsDate = bvsDetail.date_aired,
                        BroadcastDate = _GetBroadcastDate(bvsDetail, schedulesAggregate),
                        TimeAired = new DateTime().AddSeconds(bvsDetail.time_aired),
                        MatchAirTime = bvsDetail.match_airtime,
                        MatchIsci = bvsDetail.match_isci,
                        MatchProgram = bvsDetail.match_program,
                        MatchStation = bvsDetail.match_station,
                        MatchSpotLength = bvsDetail.match_spot_length
                    };

                    bvsReportData.SpecStatus = schedulesAggregate.GetSpecStatusText(bvsReportData);

                    foreach (var audience in scheduleAudiences)
                    {
                        if (schedulesAggregate.AllowedForReport(bvsDetail.station, bvsDetail.date_aired,
                            bvsDetail.time_aired))
                        {
                            var audienceImpressionsAndDeliveries =
                                GetOutOfScopeTotalDeliveryDetailsByAudienceId(bvsDetail, audience.AudienceId).Item1;

                            audienceImpressionsAndDeliveries.ForEach(
                                a =>
                                    a.Delivery =
                                        _ImpressionAdjustmentEngine.AdjustImpression(a.Delivery,
                                            schedulesAggregate.IsEquivalized,
                                            bvsReportData.SpotLength, schedulesAggregate.PostType,
                                            schedulesAggregate.PostingBookId));
                            bvsReportData.AudienceImpressions.AddRange(audienceImpressionsAndDeliveries);
                        }
                    }

                    weeklyDto.ReportData.Add(bvsReportData);
                }

                //TOTALS FOR WEEKLY
                foreach (var scheduleAudience in scheduleAudiences)
                {
                    var inSpecAudienceData = weeklyDto.GetInSpec()
                        .Where(row => row.Status == 1)
                        .SelectMany(row => row.AudienceImpressions)
                        .Where(ai => ai.AudienceId == scheduleAudience.AudienceId)
                        .ToList();
                    var delivery = weeklyDto.GetInSpec()
                        .Where(row =>
                            schedulesAggregate.AllowedForReport(row.Station, row.DisplayDaypart) && row.Status == 1)
                        .SelectMany(row => row.AudienceImpressions)
                        .Where(ai => ai.AudienceId == scheduleAudience.AudienceId)
                        .Sum(x => x.Delivery);
                    var outOfSpecAudienceData = weeklyDto.GetOutOfSpec()
                        .Where(row => row.Status != 1)
                        .SelectMany(row => row.AudienceImpressions)
                        .Where(ai => ai.AudienceId == scheduleAudience.AudienceId)
                        .ToList();

                    var impAndDelivery = new ImpressionAndDeliveryDto
                    {
                        AudienceId = scheduleAudience.AudienceId,
                        AudienceName = scheduleAudience.AudienceName,
                        OrderedImpressions = inSpecAudienceData.Sum(x => x.Impressions),
                        DeliveredImpressions = delivery,
                        OutOfSpecDeliveredImpressions = outOfSpecAudienceData.Sum(x => x.Delivery),
                    };
                    weeklyDto.ImpressionsAndDelivery.Add(impAndDelivery);
                }
            }

            return weeklyData;
        }

        private DateTime _GetBroadcastDate(bvs_file_details bvsDetail, SchedulesAggregate schedulesAggregate)
        {
            return schedulesAggregate.PostType == SchedulePostType.NSI ? bvsDetail.nsi_date : bvsDetail.nti_date;
        }

        private Dictionary<string, int> GetMarketRanks(SchedulesAggregate schedulesAggregate)
        {
            var marketRanks = _BroadcastDataRepositoryFactory.GetDataRepository<INsiMarketRepository>()
                .GetMarketRankingsByMediaMonth(schedulesAggregate.PostingBookId);
            var dmaMarkets = _BroadcastDataRepositoryFactory.GetDataRepository<IMarketDmaMapRepository>()
                .GetMarketMapFromMarketCodes(marketRanks.Select(m => m.Key).ToList())
                .ToDictionary(k => k.market_code, v => v.dma_mapped_value);

            var marketNamedRanks = new Dictionary<string, int>();
            marketRanks.ForEach(mr => marketNamedRanks.Add(dmaMarkets[(short) mr.Key].ToLower(), mr.Value));

            return marketNamedRanks;
        }

        private string CombineBvsProgramNames(IEnumerable<string> names)
        {
            return string.Join(" / ", names);
        }

        private static Tuple<List<AudienceImpressionsAndDelivery>, double>
            GetOutOfScopeTotalDeliveryDetailsByAudienceId(bvs_file_details bfd, int audienceId)
        {
            var list = new List<AudienceImpressionsAndDelivery>();
            double delivery = 0;

            for (var j = 0; j < bfd.bvs_post_details.Count; j++)
            {
                var bpd = bfd.bvs_post_details.ElementAt(j);
                if (bpd.audience_id != audienceId)
                    continue;
                list.Add(new AudienceImpressionsAndDelivery
                {
                    Impressions = 0,
                    Delivery = bpd.delivery,
                    AudienceId = audienceId
                });
                delivery += bpd.delivery;
            }

            var tuple = Tuple.Create(list, delivery);

            return tuple;
        }
    }

    public class ScheduleReportDto
    {
        public int ScheduleId { get; set; }
        public LookupDto Advertiser { get; set; }
        public AdvertiserDataDto AdvertiserData { get; set; }
        public WeeklyDataDto WeeklyData { get; set; }
        public StationSummaryDto StationSummaryData { get; set; }
        public SpotDetailDto SpotDetailData { get; set; }
        public OutOfSpecToDateDto OutOfSpecToDate { get; set; }
        public List<SpotsAndImpressionsDeliveryBySource> SpotsAndImpressionsBySource { get; set; }
        public List<SpotsAndImpressionsDeliveryByAdvertiser> SpotsAndImpressionsDeliveryByAdvertiser { get; set; }
    }


    public class WeeklyImpressionAndDeliveryDto : BvsReportDataContainer
    {
        public LookupDto Week { get; set; }
    }

    public class SpotDetailDto
    {
        public SpotDetailDto(IEnumerable<ScheduleAudience> scheduleAudiences, List<BvsPrePostReportData> reportData,
            SchedulesAggregate aggregate)
        {
            ImpressionsAndDelivery = new List<ImpressionAndDeliveryDto>();
            ReportData = reportData;
            foreach (var scheduleAudience in scheduleAudiences)
            {
                var audienceData = ReportData.SelectMany(row => row.AudienceImpressions);
                var deliveredImpressions = ReportData
                    .Where(row => aggregate.AllowedForReport(row.Station, row.Date, row.AirTime))
                    .SelectMany(row => row.AudienceImpressions)
                    .Sum(x => x.Delivery);
                ImpressionsAndDelivery.Add(new ImpressionAndDeliveryDto
                {
                    AudienceId = scheduleAudience.AudienceId,
                    AudienceName = scheduleAudience.AudienceName,
                    OrderedImpressions = audienceData.Sum(x => x.Impressions),
                    DeliveredImpressions = deliveredImpressions
                });
            }
        }

        public List<ImpressionAndDeliveryDto> ImpressionsAndDelivery { get; private set; }
        public List<BvsPrePostReportData> ReportData { get; set; }
    }
}