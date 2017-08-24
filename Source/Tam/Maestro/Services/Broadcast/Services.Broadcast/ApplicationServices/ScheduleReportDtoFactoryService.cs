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
using OfficeOpenXml.FormulaParsing.Utilities;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IScheduleReportDtoFactoryService : IApplicationService
    {
        ScheduleReportDto GenereteScheduleReportData(SchedulesAggregate schedulesAggregate, ScheduleReportType reportType);
    }


    public enum ScheduleReportType
    {
        Schedule,
        Client,
        ThirdPartyProvider
    }

    public class ScheduleReportDtoFactoryService : IScheduleReportDtoFactoryService
    {
        private readonly IScheduleAggregateFactoryService _ScheduleFactoryService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IDaypartCache _DaypartCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private List<RatingAdjustmentsDto> _RatingAdjustments;
        private Dictionary<int, float> _SpotLengthMultipliers;

        public ScheduleReportDtoFactoryService(
            IDataRepositoryFactory broadcastDataRepositoryFactory, 
            IDaypartCache daypartCache, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache audiencesCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _DaypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _AudiencesCache = audiencesCache;
            _RatingAdjustments =
                _BroadcastDataRepositoryFactory.GetDataRepository<IRatingAdjustmentsRepository>().GetRatingAdjustments();
            _SpotLengthMultipliers = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthMultipliers();
        }

        public ScheduleReportDto GenereteScheduleReportData(SchedulesAggregate schedulesAggregate, ScheduleReportType reportType)
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
            var bvsAndScheduleDetailsList = new List<AdvertiserCoreData>();

            var bvsDetailList = _GatherAdvertiserData(schedulesAggregate, scheduleAudiences, bvsAndScheduleDetailsList, advertiserDataDto);
            //END Advertiser Data

            //BEGIN Weekly Data
            var weeklyData = _GatherWeeklyData(schedulesAggregate, scheduleAudiences, bvsAndScheduleDetailsList, bvsDetailList);
            //END Weekly Data

            //BEGIN Station Summary
            var stationSummaryData = new StationSummaryDto(scheduleAudiences);
            var outOfSpecBvsDetails = _GatherStationSummaryData(schedulesAggregate, scheduleAudiences, bvsAndScheduleDetailsList, stationSummaryData, bvsDetailList);
            //END Station Summary

            // BEGIN out of spec to date
            var outOfSpecToDate = _GatherOutOfSpecToDateData(schedulesAggregate, scheduleAudiences, outOfSpecBvsDetails);
            // END out of spec to date

            //BEGIN spot details
            var prePostDataList = schedulesAggregate.GetBroadcastPrePostData(bvsDetailList);

            //Order of adjustments is important. If main delivery is calculated first, the NsiDelivery would get discounted twice. We may need to consider how to make this more resilient.
            prePostDataList.ForEach(d => d.AudienceImpressions.ForEach(a => a.NsiDelivery = _AdjustDeliveredImpressions(a.Delivery, schedulesAggregate.IsEquivalized, d.Length, SchedulePostType.NSI, schedulesAggregate.PostingBookId)));
            prePostDataList.ForEach(d => d.AudienceImpressions.ForEach(a => a.Delivery = _AdjustDeliveredImpressions(a.Delivery, schedulesAggregate.IsEquivalized, d.Length, schedulesAggregate.PostType, schedulesAggregate.PostingBookId)));
            var spotDetailsData = new SpotDetailDto(scheduleAudiences, prePostDataList,schedulesAggregate);
            _SetPrePostDetailIscisByReportType(spotDetailsData, reportType);
            //END spot details

            var deliveryBySource = new SpotsAndImpressionsDeliveryBySource()
            {
                Source = schedulesAggregate.InventorySource,
                Spots = advertiserDataDto.DeliveredSpots ?? 0,
                AudienceImpressions = advertiserDataDto.ImpressionsAndDelivey.ToDictionary(a => a.AudienceId, i => i.DeliveredImpressions)
            };

            var deliveryByAdvertiser = _GatherDeliveryByAdvertiser(schedulesAggregate, bvsDetailList, scheduleAudiences);

            // construct dtos
            var reportDto = new ScheduleReportDto
            {
                ScheduleId = schedulesAggregate.ScheduleId,
                AdvertiserData = advertiserDataDto,
                WeeklyData = weeklyData,
                StationSummaryData = stationSummaryData,
                SpotDetailData = spotDetailsData,
                OutOfSpecToDate = outOfSpecToDate,
                SpotsAndImpressionsBySource = new List<SpotsAndImpressionsDeliveryBySource>() { deliveryBySource },
                SpotsAndImpressionsDeliveryByAdvertiser = deliveryByAdvertiser
            };

            return reportDto;
        }

        private void _SetPrePostDetailIscisByReportType(SpotDetailDto spotDetailsData, ScheduleReportType reportType)
        {
            switch (reportType)
            {
                case ScheduleReportType.Schedule:
                    spotDetailsData.ReportData.ForEach(
                                    d => d.Isci = (d.IsciDto.Count > 1
                                        ? d.IsciDto.Select(i => i.House).FirstOrDefault()
                                        : d.IsciDto.Select(i => i.Client).FirstOrDefault()));
                    break;

                case ScheduleReportType.Client:
                    spotDetailsData.ReportData.ForEach(
                        d => d.Isci = (d.IsciDto.Count > 1
                            ? d.IsciDto.Select(i => string.Format("{0}(M)", i.House)).FirstOrDefault()
                            : d.IsciDto.Select(i => i.Client).FirstOrDefault()));
                    break;

                case ScheduleReportType.ThirdPartyProvider:
                    spotDetailsData.ReportData.ForEach(d => d.Isci = d.IsciDto.Select(i => i.House).FirstOrDefault());
                    break;
            }
            

        }

        private List<SpotsAndImpressionsDeliveryByAdvertiser> _GatherDeliveryByAdvertiser(
            SchedulesAggregate schedulesAggregate,
            List<bvs_file_details> bvsDetailList,
            List<ScheduleAudience> scheduleAudiences)
        {
            var inSpecDetailsByAdvertiserAndSpotLength =
                bvsDetailList.Where(
                    d => d.status == 1 && schedulesAggregate.AllowedForReport(d.station, d.date_aired, d.time_aired))
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
                    a => _AdjustDeliveredImpressions(schedulesAggregate.GetRestrictedDeliveredImpressionsByAudienceAndAdvertiserName(advertiserGroup.Key.Advertiser, a.AudienceId),
                                schedulesAggregate.IsEquivalized, advertiserGroup.Key.SpotLength, schedulesAggregate.PostType, schedulesAggregate.PostingBookId)
                                );
                deliveryByAdvertiserAndSpotLength.Add(deliveryByAdvertiser);
            }

            var result =
                deliveryByAdvertiserAndSpotLength.GroupBy(d => d.AdvertiserName)
                    .Select(
                        d => new SpotsAndImpressionsDeliveryByAdvertiser()
                        {
                            AdvertiserName = d.Key,
                            Spots = d.Sum(x => x.Spots),
                            AudienceImpressions = d.SelectMany(x => x.AudienceImpressions).GroupBy(i => i.Key).ToDictionary(a => a.Key, a => a.Sum(y => y.Value))
                        }).ToList();

            return result;

        }

        private OutOfSpecToDateDto _GatherOutOfSpecToDateData(SchedulesAggregate schedulesAggregate,
                                                                List<ScheduleAudience> scheduleAudiences, 
                                                                List<bvs_file_details> outOfSpecBvsDetails)
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
                        if (schedulesAggregate.AllowedForReport(bvsReportData.Station, bvsDetail.date_aired,bvsDetail.time_aired))
                        {
                            var data = GetOutOfScopeTotalDeliveryDetailsByAudienceId(bvsDetail, audience.AudienceId);
                            audienceImpressionsAndDeliveries.Delivery +=
                                _AdjustDeliveredImpressions(data.Item2,
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
                                                                    List<AdvertiserCoreData> bvsAndScheduleDetailsList,
                                                                    StationSummaryDto stationSummaryData, 
                                                                    List<bvs_file_details> bvsDetailList)
        {
            var inSpecStationSummary = bvsAndScheduleDetailsList.GroupBy(
                x => new
                {
                    x.Rank,
                    x.Market,
                    x.Station,
                    x.Affiliate,
                    x.SpotLength,
                    InSpec = x.IsBvsDetail
                });

            foreach (var bvsDetailGroup in inSpecStationSummary)
            {
                var scheduleDetailWeekIds = bvsDetailGroup.Where(x => x.ScheduleDetailWeekId > 0).Select(x => x.ScheduleDetailWeekId).Distinct().ToList();
                var scheduleDetails = scheduleDetailWeekIds.Select(x => { return schedulesAggregate.GetScheduleDetailByWeekId(x); }).DistinctBy(x => x.id).ToList();

                var orderedSpots = scheduleDetails.Sum(d => d.total_spots);
                if (orderedSpots == 0)
                    continue;

                int deliveredSpots = 0;
                if (bvsDetailGroup.Key.InSpec)
                    deliveredSpots = bvsDetailGroup.Count();

                var specStatus = bvsDetailGroup.Key.InSpec ? "Match" : string.Empty;
                var bvsReportData = new BvsReportData
                {
                    Rank = bvsDetailGroup.Key.Rank,
                    Market = bvsDetailGroup.Key.Market,
                    Station = bvsDetailGroup.Key.Station,
                    Affiliate = bvsDetailGroup.Key.Affiliate,
                    SpotLength = bvsDetailGroup.Key.SpotLength,
                    ProgramName = null,
                    DisplayDaypart = null,
                    Cost = null,
                    OrderedSpots = orderedSpots,
                    DeliveredSpots = deliveredSpots,
                    SpotClearance = (double) deliveredSpots/(double) orderedSpots,
                    Status = 1,
                    SpecStatus = specStatus,
                };

                foreach (var audience in scheduleAudiences)
                {
                    var audienceImpressionAndDelivery = new AudienceImpressionsAndDelivery
                    {
                        AudienceId = audience.AudienceId,
                        Impressions = 0,
                    };

                    foreach (var scheduleDetail in scheduleDetails)
                    {
                        var audienceData =
                            schedulesAggregate._ScheduleDetailAudiences.SingleOrDefault(
                                x => x.schedule_detail_id == scheduleDetail.id && x.audience_id == audience.AudienceId);
                        var audienceImpressions = (audienceData == null ? 0 : audienceData.impressions);
                        audienceImpressionAndDelivery.Impressions += scheduleDetail.total_spots*audienceImpressions;
                    }

                    var audienceAndDeliver =
                        bvsDetailGroup
                            .Where( dg => schedulesAggregate.AllowedForReport(dg.Station,dg.DateAired, dg.TimeAired))
                            .SelectMany(x => x.AudienceImpressions)
                            .Where(x => x.AudienceId == audience.AudienceId)
                            .ToList();
                    audienceImpressionAndDelivery.Delivery = _AdjustDeliveredImpressions(
                        audienceAndDeliver.Sum(x => x.Delivery), schedulesAggregate.IsEquivalized, bvsReportData.SpotLength,
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
                                _AdjustDeliveredImpressions(data.Item2,
                                    schedulesAggregate.IsEquivalized, bvsReportData.SpotLength,
                                    schedulesAggregate.PostType, schedulesAggregate.PostingBookId);
                        }
                    }

                    bvsReportData.AudienceImpressions.Add(audienceImpressionsAndDeliveries);
                }

                stationSummaryData.ReportData.Add(bvsReportData);
            }

            stationSummaryData.ReportData = stationSummaryData.ReportData.OrderBy(rd => rd.Station).ThenBy(rd => rd.Rank).ToList();

            //STATION SUMMARY TOTALS
            foreach (var audience in stationSummaryData.ScheduleAudiences)
            {
                var audienceData = stationSummaryData.GetInSpec()
                    .SelectMany(row => row.AudienceImpressions)
                    .Where(ai => ai.AudienceId == audience.AudienceId)
                    .ToList();
                var deliveredImpressions  = stationSummaryData.GetInSpec()
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


        private List<bvs_file_details> _GatherAdvertiserData(SchedulesAggregate schedulesAggregate,
                                                            List<ScheduleAudience> scheduleAudiences,
                                                            List<AdvertiserCoreData> bvsAndScheduleDetailsList,
                                                            AdvertiserDataDto advertiserDataDto)
        {
            var marketNamedRanks = GetMarketRanks(schedulesAggregate);

            var spotLengthRepo = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            var bvsDetailList = _ApplyNtiExclusions(schedulesAggregate.GetBvsDetails(), schedulesAggregate.PostType);

            var details = schedulesAggregate.GetScheduleDetails();
            foreach (var scheduleDetail in details) //in-spec
            {
                AdvertiserCoreData coreReportData = null;
                var bvsDetails = schedulesAggregate.GetBvsDetailsByScheduleId(scheduleDetail.id).ToList();
                if (!bvsDetails.IsNullOrEmpty())
                {
                    foreach (var bvsDetail in bvsDetails)
                    {
                        int? bvsDetailId = bvsDetail.id;
                        var scheduleDetailWeek = schedulesAggregate.GetScheduleDetailWeekById(bvsDetail.schedule_detail_week_id.Value);

                        var mediaWeekId = scheduleDetailWeek.media_week_id;
                        var scheduleDetailWeekId = scheduleDetailWeek.id;

                        coreReportData = new AdvertiserCoreData
                        {
                            Rank = bvsDetail.rank,
                            Market = scheduleDetail.market,
                            Station = scheduleDetail.network,
                            Affiliate = bvsDetail.affiliate,
                            ProgramName = bvsDetail.program_name,
                            GroupedByName = scheduleDetail.program,
                            SpotLength = bvsDetail.spot_length,
                            ScheduleDetailId = scheduleDetail.id,
                            ScheduleDetailWeekId = scheduleDetailWeekId,
                            MediaWeekId = mediaWeekId,
                            DateAired = bvsDetail.date_aired,
                            TimeAired = bvsDetail.time_aired,
                            IsBvsDetail = true
                        };
                        foreach (var audience in scheduleAudiences)
                        {
                            var audienceImpressionAndDelivery =
                                schedulesAggregate.GetImpressionsDetailsByScheduleDetailAndAudience(scheduleDetail.id,
                                    audience.AudienceId, bvsDetailId);
                            if (audienceImpressionAndDelivery != null)
                            {
                                coreReportData.AudienceImpressions.Add(audienceImpressionAndDelivery);
                            }
                        }

                        bvsAndScheduleDetailsList.Add(coreReportData);
                    }
                }
                else
                {
                    int rank;
                    if (!marketNamedRanks.TryGetValue(scheduleDetail.market, out rank))
                        rank = 0;

                    int spotLength = spotLengthRepo.GetSpotLengthById(scheduleDetail.spot_length_id.Value);
                    foreach (var scheduleWeek in scheduleDetail.schedule_detail_weeks)
                    {
                        string aff = schedulesAggregate.GetDetailAffiliateFromScheduleDetailId(scheduleDetail.network);
                        coreReportData = new AdvertiserCoreData
                        {
                            Rank = rank,
                            Market = scheduleDetail.market,
                            Station = scheduleDetail.network,
                            Affiliate = aff,
                            ProgramName = scheduleDetail.program,
                            GroupedByName = scheduleDetail.program,
                            SpotLength = spotLength,
                            ScheduleDetailId = scheduleDetail.id,
                            ScheduleDetailWeekId = scheduleWeek.id,
                            MediaWeekId = scheduleWeek.media_week_id,
                            DateAired = new DateTime(),
                            TimeAired = 0
                        };
                        foreach (var audience in scheduleAudiences)
                        {
                            var audienceImpressionAndDelivery =
                                schedulesAggregate.GetImpressionsDetailsByScheduleDetailAndAudience(scheduleDetail.id,
                                    audience.AudienceId, null);
                            if (audienceImpressionAndDelivery != null)
                            {
                                coreReportData.AudienceImpressions.Add(audienceImpressionAndDelivery);
                            }
                        }

                        bvsAndScheduleDetailsList.Add(coreReportData);
                    }
                }

            }

            var groupedBvsScheduleDetailsList = bvsAndScheduleDetailsList.GroupBy(
                x => new
                {
                    x.Rank,
                    x.Market,
                    x.Station,
                    x.Affiliate,
                    x.GroupedByName,
                    x.ScheduleDetailId,
                    x.SpotLength,
                    IsBvsDetail = x.IsBvsDetail
                }).ToList();

            foreach (var groupedBvsScheduleDetail in groupedBvsScheduleDetailsList)
            {
                var scheduleDetail = schedulesAggregate.GetScheduleDetailById(groupedBvsScheduleDetail.Key.ScheduleDetailId);

                var orderedSpots = scheduleDetail.total_spots;
                if (orderedSpots == 0)
                    continue;
                var deliveredSpots = 0;
                if (groupedBvsScheduleDetail.Key.IsBvsDetail)
                    deliveredSpots = groupedBvsScheduleDetail.Count(); 

                var bvsReportData = new BvsReportData
                {
                    Rank = groupedBvsScheduleDetail.Key.Rank,
                    Market = groupedBvsScheduleDetail.Key.Market,
                    Station = groupedBvsScheduleDetail.Key.Station,
                    Affiliate = groupedBvsScheduleDetail.Key.Affiliate,
                    ProgramName = CombineBvsProgramNames(groupedBvsScheduleDetail.Select(dg => dg.ProgramName).Distinct()),
                    SpotLength = groupedBvsScheduleDetail.Key.SpotLength,
                    DisplayDaypart = _DaypartCache.GetDisplayDaypart(scheduleDetail.daypart_id),
                    Cost = (double)scheduleDetail.spot_cost * (double)deliveredSpots,
                    OrderedSpots = scheduleDetail.total_spots,
                    DeliveredSpots = deliveredSpots,
                    SpotClearance = (double)deliveredSpots / (double)orderedSpots,
                };
                advertiserDataDto.ReportData.Add(bvsReportData);

                foreach (var audience in scheduleAudiences)
                {
                    var audienceData =
                        schedulesAggregate._ScheduleDetailAudiences.SingleOrDefault(
                            x => x.schedule_detail_id == scheduleDetail.id
                                    && x.audience_id == audience.AudienceId);
                    var audienceAndDeliver =
                        groupedBvsScheduleDetail
                            .Where(x => x.ScheduleDetailWeekId != 0
                                    && schedulesAggregate.AllowedForReport(x.Station, x.DateAired, x.TimeAired))
                            .SelectMany(x => x.AudienceImpressions)
                            .Where(x => x.AudienceId == audience.AudienceId)
                            .ToList();

                    var audienceImpressionAndDelivery = new AudienceImpressionsAndDelivery
                    {
                        AudienceId = audience.AudienceId,
                        Impressions = scheduleDetail.total_spots * (audienceData == null ? 0 : audienceData.impressions),
                        Delivery =
                            _AdjustDeliveredImpressions(audienceAndDeliver.Sum(x => x.Delivery),
                                schedulesAggregate.IsEquivalized, bvsReportData.SpotLength, schedulesAggregate.PostType, schedulesAggregate.PostingBookId),
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
                    .Where(row => schedulesAggregate.AllowedForReport(row.Station, row.DisplayDaypart) && (row.DisplayDaypart != null || row.Status == 1))
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
            return bvsDetailList;
        }

        private WeeklyDataDto _GatherWeeklyData(SchedulesAggregate schedulesAggregate, 
                                        List<ScheduleAudience> scheduleAudiences, 
                                        List<AdvertiserCoreData> bvsAndScheduleDetailsList,
                                        List<bvs_file_details> bvsDetailList)
        {
            var weeklyData = new WeeklyDataDto(scheduleAudiences);
            var weeks = new List<LookupDto>();
            var scheduleWeeks = schedulesAggregate.GetScheduleWeeks().ToList();
            var bvsDetailDateAired = schedulesAggregate.GetBvsDetailDateAired();
            bvsDetailDateAired.ForEach(mw =>
            {
                var mediaWeek = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(mw);
                if (!scheduleWeeks.Contains(mediaWeek.Id))
                    scheduleWeeks.Add(mediaWeek.Id);
            });
            foreach (var weekId in scheduleWeeks.Distinct().OrderBy(w => w))
            {
                weeks.Add(_MediaMonthAndWeekAggregateCache.FindMediaWeekLookup(weekId));
            }

            foreach (var week in weeks)
            {
                var weeklyDto = new WeeklyImpressionAndDeliveryDto
                {
                    Week = week,
                };
                weeklyData.ReportDataByWeek.Add(weeklyDto);

                var weeklyDataToUse = bvsAndScheduleDetailsList.Where(x => x.MediaWeekId == week.Id).GroupBy(
                    x => new
                    {
                        x.Rank,
                        x.Market,
                        x.Station,
                        x.Affiliate,
                        x.GroupedByName,// ProgramName, 
                        x.SpotLength,
                        x.ScheduleDetailWeekId,
                        InSpec = x.IsBvsDetail
                    }).ToList();

                //In Spec
                foreach (var bvsDetailGroup in weeklyDataToUse)
                {
                    var scheduleDetail = schedulesAggregate.GetScheduleDetailByWeekId(bvsDetailGroup.Key.ScheduleDetailWeekId);
                    var scheduleDetailWeek = schedulesAggregate.GetScheduleDetailWeekById(bvsDetailGroup.Key.ScheduleDetailWeekId);

                    var orderedSpots = scheduleDetailWeek.spots;
                    if (orderedSpots == 0)
                        continue;

                    int deliveredSpots = 0;
                    if (bvsDetailGroup.Key.InSpec)
                        deliveredSpots = bvsDetailGroup.Count();

                    var bvsReportData = new BvsReportData
                    {
                        Rank = bvsDetailGroup.Key.Rank,
                        Market = bvsDetailGroup.Key.Market,
                        Station = bvsDetailGroup.Key.Station,
                        Affiliate = bvsDetailGroup.Key.Affiliate,
                        ProgramName = CombineBvsProgramNames(bvsDetailGroup.Select(dg => dg.ProgramName).Distinct()),
                        SpotLength = bvsDetailGroup.Key.SpotLength,
                        DisplayDaypart = _DaypartCache.GetDisplayDaypart(scheduleDetail.daypart_id),
                        Cost = (double) scheduleDetail.spot_cost*(double) deliveredSpots,
                        OrderedSpots = orderedSpots,
                        DeliveredSpots = deliveredSpots,
                        SpotClearance = (double) deliveredSpots/(double) orderedSpots,
                        Status = 1, //in-spec
                        //@todo Fill this out
                        SpecStatus = "Match",
                    };

                    foreach (var audience in scheduleAudiences)
                    {
                        var audienceData =
                            schedulesAggregate._ScheduleDetailAudiences.SingleOrDefault(
                                x => x.schedule_detail_id == scheduleDetail.id && x.audience_id == audience.AudienceId);
                        var audienceImpressions = (audienceData == null ? 0 : audienceData.impressions);
                        var audienceAndDeliver =
                            bvsDetailGroup.Where(dg => schedulesAggregate.AllowedForReport(dg.Station,dg.DateAired,dg.TimeAired))
                                .SelectMany(x => x.AudienceImpressions)
                                .Where(x => x.AudienceId == audience.AudienceId)
                                .ToList();

                        var audienceImpressionAndDelivery = new AudienceImpressionsAndDelivery
                        {
                            AudienceId = audience.AudienceId,
                            Impressions = bvsReportData.OrderedSpots*audienceImpressions,
                            Delivery =
                                _AdjustDeliveredImpressions(audienceAndDeliver.Sum(x => x.Delivery),
                                    schedulesAggregate.IsEquivalized, bvsReportData.SpotLength, schedulesAggregate.PostType, schedulesAggregate.PostingBookId)
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
                        Status = 0, //out-of-spec
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
                            var audienceImpressionsAndDeliveries =GetOutOfScopeTotalDeliveryDetailsByAudienceId(bvsDetail, audience.AudienceId).Item1;

                            audienceImpressionsAndDeliveries.ForEach(
                                a =>
                                    a.Delivery =
                                        _AdjustDeliveredImpressions(a.Delivery, schedulesAggregate.IsEquivalized,
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
                        .Where(row => schedulesAggregate.AllowedForReport(row.Station,row.DisplayDaypart) && row.Status == 1)
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

        private Dictionary<string, int> GetMarketRanks(SchedulesAggregate schedulesAggregate)
        {
            var marketRanks = _BroadcastDataRepositoryFactory.GetDataRepository<INsiMarketRepository>()
                .GetMarketRankingsByMediaMonth(schedulesAggregate.PostingBookId);
            var dmaMarkets = _BroadcastDataRepositoryFactory.GetDataRepository<IMarketDmaMapRepository>()
                .GetMarketMapFromMarketCodes(marketRanks.Select(m => m.Key).ToList())
                .ToDictionary(k => k.market_code,v => v.dma_mapped_value);

            var marketNamedRanks = new Dictionary<string, int>();
            marketRanks.ForEach(mr => marketNamedRanks.Add(dmaMarkets[(short) mr.Key], mr.Value));

            return marketNamedRanks;
        }

        private List<bvs_file_details> _GatherAdvertiserData_Old(SchedulesAggregate schedulesAggregate, 
                                                                List<ScheduleAudience> scheduleAudiences,
                                                                List<AdvertiserCoreData> bvsAndScheduleDetailsList, 
                                                                AdvertiserDataDto advertiserDataDto)
        {
            var bvsDetailList = _ApplyNtiExclusions(schedulesAggregate.GetBvsDetails(), schedulesAggregate.PostType);
            foreach (var bvsDetail in bvsDetailList.InSpec().ToList()) //in-spec
            {
                var scheduleDetailWeek = schedulesAggregate.GetScheduleDetailWeekById(bvsDetail.schedule_detail_week_id.Value);
                var scheduleDetail = schedulesAggregate.GetScheduleDetailByWeekId(bvsDetail.schedule_detail_week_id.Value);

                var coreReportData = new AdvertiserCoreData
                {
                    Rank = bvsDetail.rank,
                    Market = bvsDetail.market,
                    Station = bvsDetail.station,
                    Affiliate = bvsDetail.affiliate,
                    ProgramName = bvsDetail.program_name,
                    GroupedByName = scheduleDetail.program,
                    SpotLength = bvsDetail.spot_length,
                    ScheduleDetailId = scheduleDetail.id,
                    ScheduleDetailWeekId = scheduleDetailWeek.id,
                    MediaWeekId = scheduleDetailWeek.media_week_id,
                    DateAired = bvsDetail.date_aired,
                    TimeAired = bvsDetail.time_aired
                };

                foreach (var audience in scheduleAudiences)
                {
                    var audienceImpressionAndDelivery =
                        schedulesAggregate.GetImpressionsDetailsByScheduleDetailAndAudience(scheduleDetail.id,
                            audience.AudienceId, bvsDetail.id);
                    if (audienceImpressionAndDelivery != null)
                    {
                        coreReportData.AudienceImpressions.Add(audienceImpressionAndDelivery);
                    }
                }

                bvsAndScheduleDetailsList.Add(coreReportData);
            }

            var groupedBvsScheduleDetailsList = bvsAndScheduleDetailsList.GroupBy(
                x => new
                {
                    x.Rank,
                    x.Market,
                    x.Station,
                    x.Affiliate,
                    x.GroupedByName,
                    x.ScheduleDetailId,
                    x.SpotLength
                }).ToList();

            foreach (var groupedBvsScheduleDetail in groupedBvsScheduleDetailsList)
            {
                var scheduleDetail = schedulesAggregate.GetScheduleDetailById(groupedBvsScheduleDetail.Key.ScheduleDetailId);

                var orderedSpots = scheduleDetail.total_spots;
                var deliveredSpots = groupedBvsScheduleDetail.Count();

                var bvsReportData = new BvsReportData
                {
                    Rank = groupedBvsScheduleDetail.Key.Rank,
                    Market = groupedBvsScheduleDetail.Key.Market,
                    Station = groupedBvsScheduleDetail.Key.Station,
                    Affiliate = groupedBvsScheduleDetail.Key.Affiliate,
                    ProgramName = CombineBvsProgramNames(groupedBvsScheduleDetail.Select(dg => dg.ProgramName).Distinct()),
                    SpotLength = groupedBvsScheduleDetail.Key.SpotLength,
                    DisplayDaypart = _DaypartCache.GetDisplayDaypart(scheduleDetail.daypart_id),
                    Cost = (double) scheduleDetail.spot_cost*(double) groupedBvsScheduleDetail.Count(),
                    OrderedSpots = scheduleDetail.total_spots,
                    DeliveredSpots = groupedBvsScheduleDetail.Count(),
                    SpotClearance = (double) deliveredSpots/(double) orderedSpots,
                };
                advertiserDataDto.ReportData.Add(bvsReportData);

                foreach (var audience in scheduleAudiences)
                {
                    var audienceData =
                        schedulesAggregate._ScheduleDetailAudiences.SingleOrDefault(
                            x => x.schedule_detail_id == scheduleDetail.id && x.audience_id == audience.AudienceId);
                    var audienceAndDeliver =
                        groupedBvsScheduleDetail
                            .Where(x => schedulesAggregate.AllowedForReport(x.Station,x.DateAired,x.TimeAired))
                            .SelectMany(x => x.AudienceImpressions)
                            .Where(x => x.AudienceId == audience.AudienceId)
                            .ToList();

                    var audienceImpressionAndDelivery = new AudienceImpressionsAndDelivery
                    {
                        AudienceId = audience.AudienceId,
                        Impressions = scheduleDetail.total_spots*(audienceData == null ? 0 : audienceData.impressions),
                        Delivery =
                            _AdjustDeliveredImpressions(audienceAndDeliver.Sum(x => x.Delivery),
                                schedulesAggregate.IsEquivalized, bvsReportData.SpotLength, schedulesAggregate.PostType, schedulesAggregate.PostingBookId),
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
                    .Where(row => schedulesAggregate.AllowedForReport(row.Station,row.DisplayDaypart) && (row.DisplayDaypart != null || row.Status == 1))
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
            return bvsDetailList;
        }

        private string CombineBvsProgramNames(IEnumerable<string> names)
        {
            return string.Join(" / ", names);
        }

        private List<bvs_file_details> _ApplyNtiExclusions(List<EntityFrameworkMapping.Broadcast.bvs_file_details> inputBvsDetailList, SchedulePostType schedulePostType)
        {
            //do nothing if not NTI
            if (schedulePostType != SchedulePostType.NTI)
            {
                return inputBvsDetailList;
            }

            //exclude anything aired on mondays between 3am and 6am
            var ntiExclusionDay = DayOfWeek.Monday;
            var ntiExclusionStartTime = new TimeSpan(3, 0, 0);
            var ntiExclusionEndTime = new TimeSpan(5, 59, 59);

            var ntiOnlyList = new List<bvs_file_details>();
            foreach (var detail in inputBvsDetailList)
            {
                if (detail.date_aired.DayOfWeek == ntiExclusionDay &&
                    TimeSpan.FromSeconds(detail.time_aired) >= ntiExclusionStartTime &&
                    TimeSpan.FromSeconds(detail.time_aired) <= ntiExclusionEndTime)
                {
                    //skip
                }
                else
                {
                    ntiOnlyList.Add(detail);
                }
            }
            return ntiOnlyList;
        }

        private double _AdjustDeliveredImpressions(double rawDelivery, bool isEquivalized, int spotLength, SchedulePostType postType, int schedulePostingBook)
        {

            var adjustments = _RatingAdjustments.Where(a => a.MediaMonthId == schedulePostingBook).SingleOrDefault();
            var haveAdjustmentsForSchedule = (adjustments != null);
            double result;

            if (haveAdjustmentsForSchedule)
            {
                result = rawDelivery * (double) (1-adjustments.AnnualAdjustment/100);

                if (postType == SchedulePostType.NTI)
                {
                    result = result * (double) (1-adjustments.NtiAdjustment/100);
                }
            }
            else
            {
                result = rawDelivery;
            }

            double factor = 1;
            if (isEquivalized)
            {
                if(_SpotLengthMultipliers.ContainsKey(spotLength))
                {
                    factor = _SpotLengthMultipliers[spotLength];    
                }else
                {
                    throw new ApplicationException(
                            string.Format(
                                "Unknown spot length {0} found while calculating delivered impressions",
                                spotLength));
                }
                
                result = result * factor;
            }

            return result;
        }

        private Tuple<List<AudienceImpressionsAndDelivery>, double> GetOutOfScopeTotalDeliveryDetailsByAudienceId(bvs_file_details bfd, int audienceId)
        {
            var list = new List<AudienceImpressionsAndDelivery>();
            double delivery = 0;

            for (var j = 0; j < bfd.bvs_post_details.Count; j++)
            {
                var bpd = bfd.bvs_post_details.ElementAt(j);
                if (bpd.audience_id != audienceId)
                    continue;
                list.Add(new AudienceImpressionsAndDelivery { Impressions = null, Delivery = bpd.delivery, AudienceId = audienceId });
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
        public SpotDetailDto(IEnumerable<ScheduleAudience> scheduleAudiences, List<BvsPrePostReportData> reportData,SchedulesAggregate aggregate)
        {
            ImpressionsAndDelivery = new List<ImpressionAndDeliveryDto>();
            ReportData = reportData;
            foreach (var scheduleAudience in scheduleAudiences)
            {
                var audienceData = ReportData.SelectMany(row => row.AudienceImpressions);
                var deliveredImpressions = ReportData
                                            .Where(row => aggregate.AllowedForReport(row.Station,row.Date,row.AirTime))
                                            .SelectMany(row => row.AudienceImpressions)
                                            .Sum(x => x.Delivery);
                ImpressionsAndDelivery.Add(new ImpressionAndDeliveryDto
                {
                    AudienceId = scheduleAudience.AudienceId,
                    AudienceName = scheduleAudience.AudienceName,
                    OrderedImpressions = audienceData.Sum(x=>x.Impressions),
                    DeliveredImpressions = deliveredImpressions
                });
            }
        }

        public List<ImpressionAndDeliveryDto> ImpressionsAndDelivery { get; private set; }
        public List<BvsPrePostReportData> ReportData { get; set; }
    }
}