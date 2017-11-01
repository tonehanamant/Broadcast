using System;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Aggregates;
using Services.Broadcast.Entities;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;
using Image = System.Drawing.Image;

namespace Services.Broadcast.ApplicationServices
{
    public interface ISchedulesReportService : IApplicationService
    {
        ScheduleReportDto GenerateScheduleReportDto(int scheduleId);
        ReportOutput GenerateScheduleReport(int scheduleId);

        ScheduleReportDto GenerateClientReportDto(int scheduleId);
        ReportOutput GenerateClientReport(int scheduleId);

        ScheduleReportDto Generate3rdPartyProviderReportDto(int scheduleId);
        ReportOutput Generate3rdPartyProviderReport(int scheduleId);
    }
    /// <summary>
    /// This class is recreated for each report invoked
    /// </summary>
    public class SchedulesReportService : ISchedulesReportService
    {
        private readonly IScheduleReportDtoFactoryService _ScheduleReportDtoFactoryService;
        private readonly IScheduleAggregateFactoryService _ScheduleFactoryService;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        private readonly Lazy<Image> _LogoImage;

        public SchedulesReportService(
            IScheduleReportDtoFactoryService scheduleReportDtoFactoryService,
            IScheduleAggregateFactoryService scheduleFactoryService,
            IDataRepositoryFactory brodcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            ISMSClient smsClient)
        {
            _ScheduleReportDtoFactoryService = scheduleReportDtoFactoryService;
            _ScheduleFactoryService = scheduleFactoryService;
            _BroadcastDataRepositoryFactory = brodcastDataRepositoryFactory;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;

            _LogoImage = new Lazy<Image>(() => Image.FromStream(new MemoryStream(smsClient.GetLogoImage(CMWImageEnums.CMW_CADENT_LOGO).ImageData)));
        }

        public ScheduleReportDto GenerateScheduleReportDto(int scheduleId)
        {
            var schedulesAggregate = _ScheduleFactoryService.GetScheduleAggregate(scheduleId);
            var scheduleReportDto = _ScheduleReportDtoFactoryService.GenereteScheduleReportData(schedulesAggregate, ScheduleReportType.Schedule);
            return scheduleReportDto;
        }

        public ReportOutput GenerateScheduleReport(int scheduleId)
        {
            var scheduleReportDto = GenerateScheduleReportDto(scheduleId);

            var excelGenerator = new BvsExcelReportGenerator(scheduleReportDto, _LogoImage.Value);
            var result = excelGenerator.GetScheduleReport();

            return result;
        }


        public ScheduleReportDto GenerateClientReportDto(int scheduleId)
        {
            var schedulesAggregate = _ScheduleFactoryService.GetScheduleAggregate(scheduleId);
            var scheduleReportDto = _ScheduleReportDtoFactoryService.GenereteScheduleReportData(schedulesAggregate, ScheduleReportType.Client);

            _FindAndCombineRelatedSchedules(scheduleId, schedulesAggregate, scheduleReportDto);

            return scheduleReportDto;
        }
        public ReportOutput GenerateClientReport(int scheduleId)
        {
            var scheduleReportDto = GenerateClientReportDto(scheduleId);

            var excelGenerator = new BvsExcelReportGenerator(scheduleReportDto, _LogoImage.Value);
            var result = excelGenerator.GetClientReport();

            return result;
        }

        public ScheduleReportDto Generate3rdPartyProviderReportDto(int scheduleId)
        {
            var schedulesAggregate = _ScheduleFactoryService.GetScheduleAggregate(scheduleId);
            var scheduleReportDto = _ScheduleReportDtoFactoryService.GenereteScheduleReportData(schedulesAggregate, ScheduleReportType.ThirdPartyProvider);
            return scheduleReportDto;
        }
        public ReportOutput Generate3rdPartyProviderReport(int scheduleId)
        {
            var scheduleReportDto = Generate3rdPartyProviderReportDto(scheduleId);
            var excelGenerator = new BvsExcelReportGenerator(scheduleReportDto, _LogoImage.Value);
            var result = excelGenerator.GetProviderReport();

            return result;
        }

        private void _FindAndCombineRelatedSchedules(int scheduleId, SchedulesAggregate schedulesAggregate, ScheduleReportDto scheduleReportDto)
        {
            var scheduleIscsi =
                _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>().GetScheduleIscis(scheduleId);

            var relatedScheduleIds =
                _BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>()
                    .GetScheduleIdsByIscis(scheduleIscsi).Except(
                        new List<int>
                        {
                            scheduleId
                        }).ToList();

            var relatedScheduleReportDtoList = _GetListOfRelatedScheduleAggregates(
                relatedScheduleIds,
                schedulesAggregate,
                scheduleIscsi);

            var scheduleAudiences = schedulesAggregate.GetScheduleAudiences().ToList();

            _CombineRelatedAdvertiserDataForClientReport(
                scheduleReportDto,
                relatedScheduleReportDtoList,
                scheduleAudiences);
            _CombineRelatedStationSummaryDataForClientReport(
                scheduleReportDto,
                relatedScheduleReportDtoList,
                scheduleAudiences);
            _CombineRelatedWeeklyInSpecDataForClientReport(
                scheduleReportDto,
                relatedScheduleReportDtoList,
                scheduleAudiences,
                schedulesAggregate);
            _CombineRelatedPrePostDataForClientReport(
                scheduleReportDto,
                relatedScheduleReportDtoList);
            _CombineDeliveryBySource(
                scheduleReportDto,
                relatedScheduleReportDtoList);

        }

        internal List<ScheduleReportDto> _GetListOfRelatedScheduleAggregates(
            List<int> relatedScheduleIds,
            SchedulesAggregate schedulesAggregate,
            List<string> scheduleIscsi)
        {
            var relatedScheduleReportDtoList = new List<ScheduleReportDto>();
            //var scehduleAudienceIds = schedulesAggregate.GetScheduleAudiences().Select(sa => sa.AudienceId).ToList();

            foreach (var relatedScheduleId in relatedScheduleIds)
            {
                var relatedScheduleAggregate = _ScheduleFactoryService.GetScheduleAggregate(relatedScheduleId);
                relatedScheduleAggregate.PostType = schedulesAggregate.PostType;
                relatedScheduleAggregate.OverrideScheduleRestrictions(schedulesAggregate.Schedule.markets.ToList(), schedulesAggregate.Schedule.schedule_restriction_dayparts);
                relatedScheduleAggregate.SetScheduleAudiences(schedulesAggregate.GetScheduleAudiences().ToList());

                relatedScheduleAggregate.ApplyBvsDetailsFilter(
                    scheduleIscsi,
                    schedulesAggregate.StartDate,
                    schedulesAggregate.EndDate,
                    new List<TrackingStatus> { TrackingStatus.InSpec });

                var relatedScheduleReportDto =
                    _ScheduleReportDtoFactoryService.GenereteScheduleReportData(relatedScheduleAggregate, ScheduleReportType.Client);

                relatedScheduleReportDtoList.Add(relatedScheduleReportDto);
            }
            return relatedScheduleReportDtoList;
        }

        private static void _CombineDeliveryBySource(ScheduleReportDto scheduleReportDto, List<ScheduleReportDto> relatedScheduleReportDtoList)
        {
            var deliveryBySource = new List<SpotsAndImpressionsDeliveryBySource>();
            var possibleAudienceIds =
                relatedScheduleReportDtoList.SelectMany(r => r.SpotsAndImpressionsBySource)
                    .SelectMany(r => r.AudienceImpressions.Keys).Distinct().ToList();

            // Need to make sure possible audiences are added to related schedules SpotsAndImpressionsBySource audience impressions
            var uniqueAudienceIds =
                scheduleReportDto.StationSummaryData.ScheduleAudiences.Where(
                    sa => !possibleAudienceIds.Contains(sa.AudienceId)).Select(sa => sa.AudienceId).ToList();

            uniqueAudienceIds.ForEach(audienceId =>
                {
                    relatedScheduleReportDtoList.SelectMany(r => r.SpotsAndImpressionsBySource).ForEach(sis =>
                    {
                        sis.AudienceImpressions.Add(audienceId, scheduleReportDto.AdvertiserData.ImpressionsAndDelivey.Where(id => id.AudienceId == audienceId).Sum(id => id.DeliveredImpressions));
                    });
                });
            // add sources excluding blank sources
            deliveryBySource.AddRange(scheduleReportDto.SpotsAndImpressionsBySource.Where(si => si.Source != InventorySourceEnum.Blank));
            foreach (var reportDto in relatedScheduleReportDtoList)
            {
                deliveryBySource.AddRange(reportDto.SpotsAndImpressionsBySource.Where(si => si.Source != InventorySourceEnum.Blank));
            }

            var combinedDeliveryBySource =
                deliveryBySource.GroupBy(g => g.Source).Select(
                    g => new SpotsAndImpressionsDeliveryBySource
                    {
                        Source = g.Key,
                        Spots = g.Sum(d => d.Spots),
                        AudienceImpressions =
                            g.SelectMany(a => a.AudienceImpressions)
                                .GroupBy(a => a.Key)
                                .ToDictionary(a => a.Key, i => i.Sum(d => d.Value))
                    }).ToList();
            scheduleReportDto.SpotsAndImpressionsBySource = combinedDeliveryBySource;
        }

        private static void _CombineRelatedPrePostDataForClientReport(
            ScheduleReportDto scheduleReportDto,
            IEnumerable<ScheduleReportDto> relatedScheduleReportDtoList)
        {
            var prePostData = scheduleReportDto.SpotDetailData.ReportData;

            //SchedulesAggregate.CombineImpressionsAndDelivery(scheduleReportDto.SpotDetailData.ImpressionsAndDelivery, relatedScheduleReportDtoList.SelectMany(r => r.SpotDetailData.ImpressionsAndDelivery).ToList());

            foreach (var relatedScheduleDto in relatedScheduleReportDtoList)
            {
                prePostData.AddRange(relatedScheduleDto.SpotDetailData.ReportData);
            }

            foreach (var impressionsAndDelivery in scheduleReportDto.SpotDetailData.ImpressionsAndDelivery)
            {
                impressionsAndDelivery.OrderedImpressions =
                    prePostData.SelectMany(r => r.AudienceImpressions)
                        .Where(ai => ai.AudienceId == impressionsAndDelivery.AudienceId)
                        .Sum(x => x.Impressions);

                impressionsAndDelivery.DeliveredImpressions =
                    prePostData.SelectMany(r => r.AudienceImpressions)
                        .Where(ai => ai.AudienceId == impressionsAndDelivery.AudienceId)
                        .Sum(x => x.Delivery);
            }
        }

        private static void _CombineRelatedStationSummaryDataForClientReport(
            ScheduleReportDto scheduleReportDto,
            IEnumerable<ScheduleReportDto> relatedScheduleReportDtoList,
            List<ScheduleAudience> scheduleAudiences)
        {
            var stationSummaryData = scheduleReportDto.StationSummaryData;
            var summaryData = stationSummaryData.ReportData;
            foreach (var relatedScheduleAggregate in relatedScheduleReportDtoList)
            {
                summaryData.AddRange(relatedScheduleAggregate.StationSummaryData.ReportData);
            }

            var consolidatedData = summaryData.GroupBy(
                g => new
                {
                    g.Rank,
                    g.Market,
                    g.Station,
                    g.Affiliate,
                    g.SpotLength,
                    g.Status,
                    g.SpecStatus
                }).Select(
                    g => g.Key.Status == (int)TrackingStatus.InSpec ? new BvsReportData
                    {
                        Rank = g.Key.Rank,
                        Market = g.Key.Market,
                        Station = g.Key.Station,
                        Affiliate = g.Key.Affiliate,
                        SpotLength = g.Key.SpotLength,
                        Status = g.Key.Status,
                        SpecStatus = g.Key.SpecStatus,
                        OrderedSpots = g.Sum(d => d.OrderedSpots),
                        DeliveredSpots = g.Sum(d => d.DeliveredSpots),
                        SpotClearance =
                                g.Sum(d => d.OrderedSpots) == 0
                                    ? 0
                                    : (double)g.Sum(d => d.DeliveredSpots) / g.Sum(d => d.OrderedSpots),
                        AudienceImpressions = _AddUpAudienceImpressions(
                            g.ToList(),
                            scheduleAudiences)

                    } : new BvsReportOutOfSpecData
                    {
                        Rank = g.Key.Rank,
                        Market = g.Key.Market,
                        Station = g.Key.Station,
                        Affiliate = g.Key.Affiliate,
                        SpotLength = g.Key.SpotLength,
                        Status = g.Key.Status,
                        SpecStatus = g.Key.SpecStatus,
                        OrderedSpots = g.Sum(d => d.OrderedSpots),
                        DeliveredSpots = g.Sum(d => d.DeliveredSpots),
                        SpotClearance =
                                g.Sum(d => d.OrderedSpots) == 0
                                    ? 0
                                    : (double)g.Sum(d => d.DeliveredSpots) / g.Sum(d => d.OrderedSpots),
                        OutOfSpecSpots = g.Sum(d => d.OutOfSpecSpots),
                        AudienceImpressions = _AddUpAudienceImpressions(
                            g.ToList(),
                            scheduleAudiences)


                    }).ToList();
            stationSummaryData.ReportData = consolidatedData;
            _UpdateTotalsPerAudience(stationSummaryData.ImpressionsAndDelivery, consolidatedData, true);

        }


        /// <summary>
        /// for blank report there will be no weekly data, so we need to create some fake placeholders for the weekly data that can be
        /// combined with related schedules.
        /// </summary>
        private List<WeeklyImpressionAndDeliveryDto> GeneratePseudoWeeklyData(SchedulesAggregate scheduleAggregate)
        {
            var dateRover = scheduleAggregate.StartDate;
            var weeklyData = new List<WeeklyImpressionAndDeliveryDto>();

            while (dateRover < scheduleAggregate.EndDate)
            {
                var mediaWeekID = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(dateRover).Id;
                dateRover = dateRover.AddDays(6);
                var weekData = new WeeklyImpressionAndDeliveryDto
                {
                    Week = _MediaMonthAndWeekAggregateCache.FindMediaWeekLookup(mediaWeekID)
                };
                weeklyData.Add(weekData);
            }

            return weeklyData;
        }

        private void _CombineRelatedWeeklyInSpecDataForClientReport(
            ScheduleReportDto scheduleReportDto,
            List<ScheduleReportDto> relatedScheduleReportDtoList,
            List<ScheduleAudience> scheduleAudiences,
            SchedulesAggregate scheduleAggregate)
        {
            var weeklyData = scheduleReportDto.WeeklyData.ReportDataByWeek;
            if (scheduleAggregate.Schedule.estimate_id == null)
            {
                weeklyData = GeneratePseudoWeeklyData(scheduleAggregate);
                scheduleReportDto.WeeklyData.ReportDataByWeek = weeklyData;
            }

            foreach (var weeklyImpressionAndDeliveryDto in weeklyData) //for each weekly data set
            {
                foreach (var relatedScheduleReportDto in relatedScheduleReportDtoList)
                {
                    var relatedInSpecWeeklyData = relatedScheduleReportDto.WeeklyData.ReportDataByWeek.Where(
                        w => w.Week.Id == weeklyImpressionAndDeliveryDto.Week.Id)
                        .SelectMany(rd => rd.GetInSpec());
                    weeklyImpressionAndDeliveryDto.ReportData.AddRange(relatedInSpecWeeklyData);
                }

                var iadAudienceIds = weeklyImpressionAndDeliveryDto
                                        .ImpressionsAndDelivery
                                        .Select(id => id.AudienceId)
                                        .ToList();
                foreach (var scheduleAudience in scheduleAudiences.Where(sa => !iadAudienceIds.Contains(sa.AudienceId)))
                {
                    weeklyImpressionAndDeliveryDto.ImpressionsAndDelivery.Add(new ImpressionAndDeliveryDto
                    {
                        AudienceId = scheduleAudience.AudienceId,
                        AudienceName = scheduleAudience.AudienceName
                    });
                }

                var groupedWeeklyRecords = weeklyImpressionAndDeliveryDto.ReportData.GroupBy(
                    g => new
                    {
                        g.Rank,
                        g.Market,
                        g.Station,
                        g.Affiliate,
                        g.SpotLength,
                        g.ProgramName,
                        g.Isci,
                        g.BvsDate,
                        g.BroadcastDate,
                        g.TimeAired,
                        g.DisplayDaypart,
                        g.Status,
                        g.SpecStatus,
                        g.SpotCost
                    }).Select(
                        g => g.Key.Status == (int)TrackingStatus.InSpec ?
                            new BvsReportData
                            {
                                Rank = g.Key.Rank,
                                Market = g.Key.Market,
                                Station = g.Key.Station,
                                Affiliate = g.Key.Affiliate,
                                SpotLength = g.Key.SpotLength,
                                ProgramName = g.Key.ProgramName,
                                DisplayDaypart = g.Key.DisplayDaypart,
                                Status = g.Key.Status,
                                SpecStatus = g.Key.SpecStatus,
                                Cost = g.Sum(r => r.Cost),
                                SpotCost = g.Key.SpotCost,
                                OrderedSpots = g.Sum(r => r.OrderedSpots),
                                DeliveredSpots = g.Sum(r => r.DeliveredSpots),
                                SpotClearance =
                                g.Sum(a => a.OrderedSpots) == 0
                                    ? 0
                                    : (double)g.Sum(a => a.DeliveredSpots) / g.Sum(a => a.OrderedSpots),
                                AudienceImpressions = _AddUpAudienceImpressions(
                                g.ToList(),
                                scheduleAudiences)
                            } :
                        new BvsReportOutOfSpecData
                        {
                            Rank = g.Key.Rank,
                            Market = g.Key.Market,
                            Station = g.Key.Station,
                            Affiliate = g.Key.Affiliate,
                            SpotLength = g.Key.SpotLength,
                            ProgramName = g.Key.ProgramName,
                            Isci = g.Key.Isci,
                            BvsDate = g.Key.BvsDate,
                            BroadcastDate = g.Key.BroadcastDate,
                            TimeAired = g.Key.TimeAired,
                            DisplayDaypart = g.Key.DisplayDaypart,
                            Status = g.Key.Status,
                            SpecStatus = g.Key.SpecStatus,
                            Cost = g.Sum(r => r.Cost),
                            OrderedSpots = g.Sum(r => r.OrderedSpots),
                            DeliveredSpots = g.Sum(r => r.DeliveredSpots),
                            SpotClearance =
                                g.Sum(a => a.OrderedSpots) == 0
                                    ? 0
                                    : (double)g.Sum(a => a.DeliveredSpots) / g.Sum(a => a.OrderedSpots),
                            AudienceImpressions = _AddUpAudienceImpressions(
                                g.ToList(),
                                scheduleAudiences)
                        }).ToList();

                weeklyImpressionAndDeliveryDto.ReportData = groupedWeeklyRecords;
                _UpdateTotalsPerAudience(weeklyImpressionAndDeliveryDto.ImpressionsAndDelivery, groupedWeeklyRecords, true);
            }
        }

        private static void _UpdateTotalsPerAudience(
            IEnumerable<ImpressionAndDeliveryDto> impressionAndDeliveryList,
            List<BvsReportData> detailedRecords,
            bool separateOutOfSpec)
        {
            foreach (var impressionsAndDelivery in impressionAndDeliveryList)
            {
                impressionsAndDelivery.OrderedImpressions =
                    detailedRecords.SelectMany(r => r.AudienceImpressions)
                        .Where(ai => ai.AudienceId == impressionsAndDelivery.AudienceId)
                        .Sum(x => x.Impressions);
                if (separateOutOfSpec)
                {
                    impressionsAndDelivery.DeliveredImpressions =
                        detailedRecords.Where(d => d.Status == (int)TrackingStatus.InSpec).SelectMany(r => r.AudienceImpressions)
                            .Where(ai => ai.AudienceId == impressionsAndDelivery.AudienceId)
                            .Sum(x => x.Delivery);
                    impressionsAndDelivery.OutOfSpecDeliveredImpressions =
                        detailedRecords.Where(d => d.Status != (int)TrackingStatus.InSpec)
                            .SelectMany(r => r.AudienceImpressions)
                            .Where(ai => ai.AudienceId == impressionsAndDelivery.AudienceId)
                            .Sum(x => x.Delivery);
                }
                else
                {
                    impressionsAndDelivery.DeliveredImpressions =
                        detailedRecords.SelectMany(r => r.AudienceImpressions)
                            .Where(ai => ai.AudienceId == impressionsAndDelivery.AudienceId)
                            .Sum(x => x.Delivery);
                }
            }
        }

        private static void _CombineRelatedAdvertiserDataForClientReport(
            ScheduleReportDto scheduleReportDto,
            IEnumerable<ScheduleReportDto> relatedScheduleReportDtoList,
            List<ScheduleAudience> scheduleAudiences)
        {
            var advertisersData = scheduleReportDto.AdvertiserData;
            var reportRows = new List<BvsReportData>();
            reportRows.AddRange(advertisersData.ReportData);
            foreach (var relatedScheduleReportDto in relatedScheduleReportDtoList)
            {
                reportRows.AddRange(relatedScheduleReportDto.AdvertiserData.ReportData);
            }

            var combinedRows = reportRows.GroupBy(
                g => new
                {
                    g.Rank,
                    g.Market,
                    g.Station,
                    g.Affiliate,
                    g.ProgramName,
                    g.DisplayDaypart,
                    g.SpotLength,
                    g.SpotCost
                }).Select(
                    g => new BvsReportData
                    {
                        Rank = g.Key.Rank,
                        Affiliate = g.Key.Affiliate,
                        DeliveredSpots = g.Sum(a => a.DeliveredSpots),
                        DisplayDaypart = g.Key.DisplayDaypart,
                        Market = g.Key.Market,
                        Station = g.Key.Station,
                        ProgramName = g.Key.ProgramName,
                        SpotLength = g.Key.SpotLength,
                        Cost = g.Sum(a => a.Cost),
                        SpotCost = g.Key.SpotCost,
                        OrderedSpots = g.Sum(a => a.OrderedSpots),
                        SpotClearance = g.Sum(a => a.OrderedSpots) == 0 ? 0 : (double)g.Sum(a => a.DeliveredSpots) / g.Sum(a => a.OrderedSpots),
                        AudienceImpressions = _AddUpAudienceImpressions(
                            g.ToList(),
                            scheduleAudiences)
                    }).ToList();
            _UpdateTotalsPerAudience(advertisersData.ImpressionsAndDelivey, combinedRows, false);
            advertisersData.ReportData = combinedRows;
        }

        private static List<AudienceImpressionsAndDelivery> _AddUpAudienceImpressions(
            List<BvsReportData> rows,
            IEnumerable<ScheduleAudience> scheduleAudiences)
        {
            var result = scheduleAudiences.Select(
                a => new AudienceImpressionsAndDelivery
                {
                    AudienceId = a.AudienceId,
                    Impressions =
                        rows.SelectMany(
                            r =>
                                r.AudienceImpressions.Where(i => i.AudienceId == a.AudienceId)
                                    .Select(x => x.Impressions)).Sum(),
                    Delivery = rows.SelectMany(
                            r =>
                                r.AudienceImpressions.Where(i => i.AudienceId == a.AudienceId)
                                    .Select(x => x.Delivery)).Sum()
                }).ToList();

            return result;
        }
    }
}