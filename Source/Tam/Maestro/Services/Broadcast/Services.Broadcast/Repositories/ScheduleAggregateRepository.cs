﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Aggregates;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Repositories
{
    public interface IScheduleAggregateRepository : IDataRepository
    {
        SchedulesAggregate Find(int scheduleId, List<DisplayMediaWeek> mediaWeeks, int HouseHoldAudienceId);
    }

    public class ScheduleAggregateRepository : BroadcastRepositoryBase, IScheduleAggregateRepository
    {
        private readonly IStationProcessingEngine _StationProcessingEngine;

        public ScheduleAggregateRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper,
            IStationProcessingEngine stationProcessingEngine, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
            _StationProcessingEngine = stationProcessingEngine;
        }

        public SchedulesAggregate Find(int scheduleId, List<DisplayMediaWeek> mediaWeeks, int HouseHoldAudienceId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var schedule = context.schedules
                        .Include(s => s.schedule_restriction_dayparts)
                        .Include(s => s.schedule_iscis)
                        .Include("markets.stations")
                        .Include(s => s.schedule_audiences)
                        .Include(s => s.schedule_details)
                        .Include(s => s.schedule_iscis)
                        .Include(s => s.schedule_details.Select(sd => sd.schedule_detail_audiences))
                        .Include(s => s.schedule_details.Select(sd => sd.schedule_detail_weeks))
                        .Single(s => s.id == scheduleId);

                    var scheduleAudiences = schedule.schedule_audiences
                                                        .Select(sa => new ScheduleAudience
                                                            {
                                                                AudienceId = sa.audience_id,
                                                                Rank = sa.rank,
                                                                Population = sa.population
                                                            })
                                                        .OrderByDescending(a => a.AudienceId == HouseHoldAudienceId)
                                                        .ThenBy(a => a.Rank)
                                                        .ToList();

                    var scheduleDetails = schedule.schedule_details.ToList();

                    var scheduleIscis = schedule.schedule_iscis.ToList();

                    var scheduleDetailAudiences = schedule.schedule_details.SelectMany(sd => sd.schedule_detail_audiences).ToList();

                    var scheduleDetailWeeks = schedule.schedule_details.SelectMany(sd => sd.schedule_detail_weeks).ToList();

                    var bvsFileDetails = context.detection_file_details
                        .Include(f => f.detection_post_details)
                        .Where(bfd => bfd.estimate_id == schedule.estimate_id).ToList();

                    var bvsPostDetails = bvsFileDetails.SelectMany(fd => fd.detection_post_details).ToList();
                    var stationNames = scheduleDetails.Select(d => _StationProcessingEngine.StripStationSuffix(d.network)).Distinct().ToList();
                    var stationToAffiliateDict =
                        context.stations.Where(s => stationNames.Contains(s.legacy_call_letters))
                            .ToDictionary(k => k.legacy_call_letters, v => v.affiliation, StringComparer.OrdinalIgnoreCase);
                    
                    return new SchedulesAggregate(
                        schedule,
                        scheduleAudiences,
                        scheduleDetails,
                        scheduleIscis,
                        scheduleDetailAudiences,
                        scheduleDetailWeeks,
                        bvsFileDetails,
                        bvsPostDetails,
                        mediaWeeks,
                        (PostingTypeEnum)schedule.post_type,
                        (InventorySourceEnum)schedule.inventory_source,
                        schedule.equivalized,
                        schedule.start_date,
                        schedule.end_date,
                        stationToAffiliateDict,
                        _StationProcessingEngine);
                });
        }
    }
}
