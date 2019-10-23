using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Services.Broadcast.Extensions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IPlanRepository : IDataRepository
    {
        /// <summary>
        /// Saves the new plan.
        /// </summary>
        /// <param name="planDto">The plan.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        void SaveNewPlan(PlanDto planDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Saves the plan.
        /// </summary>
        /// <param name="planDto">The plan dto.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        void SavePlan(PlanDto planDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanDto GetPlan(int planId);

        /// <summary>
        /// Gets the plans of a campaign excluding Canceled and Rejected.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        List<PlanDto> GetPlansForCampaign(int campaignId);
    }

    public class PlanRepository : BroadcastRepositoryBase, IPlanRepository
    {
        public PlanRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
        {
        }

        /// <inheritdoc/>
        public void SaveNewPlan(PlanDto planDto, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var newPlan = new plan();
                    _HydrateFromDto(newPlan, planDto, context, createdBy, createdDate);
                    
                    context.plans.Add(newPlan);
                    context.SaveChanges();

                    _UpdateLatestVersionId(newPlan, context);
                    
                    planDto.Id = newPlan.id;
                    planDto.VersionId = newPlan.latest_version_id;
                });
        }

        private void _UpdateLatestVersionId(plan newPlan, QueryHintBroadcastContext context)
        {
            newPlan.latest_version_id = newPlan.plan_versions.Max(x => x.id);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public void SavePlan(PlanDto planDto, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var plan = context.plans
                        .Include(p => p.plan_versions)
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
                        .Where(x=>x.id == planDto.Id)
                        .Single(p => p.id == planDto.Id, "Invalid plan id.");
                    _HydrateFromDto(plan, planDto, context, createdBy, createdDate);

                    context.SaveChanges();

                    _UpdateLatestVersionId(plan, context);

                    planDto.VersionId = plan.latest_version_id;
                });
        }

        /// <inheritdoc />
        public PlanDto GetPlan(int planId)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var markets = context.markets.ToList();
                    var entity = (from plan in context.plans
                                  join planVersion in context.plan_versions on plan.id equals planVersion.plan_id
                                  where plan.id == planId && plan.latest_version_id == planVersion.id
                                  select plan)
                        .Include(x => x.plan_versions)
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks.Select(y => y.media_weeks)))
                        .Single(s => s.id == planId, "Invalid plan id.");
                    return _MapToDto(entity, markets);
                });
        }
        
        /// <inheritdoc />
        public List<PlanDto> GetPlansForCampaign(int campaignId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var markets = context.markets.ToList();
                var entitiesRaw = (from plan in context.plans
                                  join planVersion in context.plan_versions on plan.id equals planVersion.plan_id
                                  where plan.campaign_id == campaignId && plan.latest_version_id == planVersion.id
                                  select plan)
                        .Include(x=>x.plan_versions)
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks.Select(y => y.media_weeks)))
                    .ToList();

                return entitiesRaw.Select(e => _MapToDto(e, markets)).ToList();
            });
        }

        private PlanDto _MapToDto(plan entity, List<market> markets)
        {
            var latestPlanVersion = entity.plan_versions.Where(x=>x.id == entity.latest_version_id).Single("There is no latest version available");
            var planSummary = latestPlanVersion.plan_version_summaries.Single(); 
            var dto = new PlanDto
            {
                Id = entity.id,
                CampaignId = entity.campaign_id,
                Name = entity.name,
                SpotLengthId = latestPlanVersion.spot_length_id,
                Equivalized = latestPlanVersion.equivalized,
                Status = EnumHelper.GetEnum<PlanStatusEnum>(latestPlanVersion.status),
                ProductId = entity.product_id,
                FlightStartDate = latestPlanVersion.flight_start_date,
                FlightEndDate = latestPlanVersion.flight_end_date,
                FlightNotes = latestPlanVersion.flight_notes,
                AudienceId = latestPlanVersion.target_audience_id,
                AudienceType = EnumHelper.GetEnum<AudienceTypeEnum>(latestPlanVersion.audience_type),
                HUTBookId = latestPlanVersion.hut_book_id,
                ShareBookId = latestPlanVersion.share_book_id,
                PostingType = EnumHelper.GetEnum<PostingTypeEnum>(latestPlanVersion.posting_type),
                FlightHiatusDays = latestPlanVersion.plan_version_flight_hiatus_days.Select(h => h.hiatus_day).ToList(),
                Budget = latestPlanVersion.budget,
                DeliveryImpressions = latestPlanVersion.target_impression,
                CPM = latestPlanVersion.target_cpm,
                DeliveryRatingPoints = latestPlanVersion.target_rating_points,
                CPP = latestPlanVersion.target_cpp,
                Currency = EnumHelper.GetEnum<PlanCurrenciesEnum>(latestPlanVersion.currency),
                GoalBreakdownType = EnumHelper.GetEnum<PlanGoalBreakdownTypeEnum>(latestPlanVersion.goal_breakdown_type),
                SecondaryAudiences = latestPlanVersion.plan_version_secondary_audiences.Select(_MapSecondaryAudiences).ToList(),
                Dayparts = latestPlanVersion.plan_version_dayparts.Select(_MapPlanDaypartDto).ToList(),
                CoverageGoalPercent = latestPlanVersion.coverage_goal_percent,
                AvailableMarkets = latestPlanVersion.plan_version_available_markets.Select(e => _MapAvailableMarketDto(e, markets)).ToList(),
                BlackoutMarkets = latestPlanVersion.plan_version_blackout_markets.Select(e => _MapBlackoutMarketDto(e, markets)).ToList(),
                WeeklyBreakdownWeeks = latestPlanVersion.plan_version_weeks.Select(_MapWeeklyBreakdownWeeks).ToList(),
                ModifiedBy = latestPlanVersion.modified_by,
                ModifiedDate = latestPlanVersion.modified_date.Value, //version have no modified date. we should acount for that in future stories
                Vpvh = latestPlanVersion.target_vpvh,
                Universe = latestPlanVersion.target_universe,
                HouseholdCPM = latestPlanVersion.hh_cpm,
                HouseholdCPP = latestPlanVersion.hh_cpp,
                HouseholdDeliveryImpressions = latestPlanVersion.hh_impressions,
                HouseholdRatingPoints = latestPlanVersion.hh_rating_points,
                HouseholdUniverse = latestPlanVersion.hh_universe,
                AvailableMarketsWithSovCount  = planSummary.available_market_with_sov_count,
                BlackoutMarketCount = planSummary.blackout_market_count,
                BlackoutMarketTotalUsCoveragePercent = planSummary.blackout_market_total_us_coverage_percent,
                IsDraft = latestPlanVersion.is_draft,
                VersionId = latestPlanVersion.id
            };
            return dto;
        }

        private WeeklyBreakdownWeek _MapWeeklyBreakdownWeeks(plan_version_weeks arg)
        {
            return new WeeklyBreakdownWeek
            {
                ActiveDays = arg.active_days_label,
                EndDate = arg.media_weeks.end_date,
                Impressions = arg.impressions,
                NumberOfActiveDays = arg.number_active_days,
                ShareOfVoice = arg.share_of_voice,
                StartDate = arg.media_weeks.start_date,
                MediaWeekId = arg.media_weeks.id
            };
        }

        private static PlanAudienceDto _MapSecondaryAudiences(plan_version_secondary_audiences x)
        {
            return new PlanAudienceDto
            {
                AudienceId = x.audience_id,
                Type = (AudienceTypeEnum)x.audience_type,
                Vpvh = x.vpvh,
                DeliveryRatingPoints = x.delivery_rating_points,
                DeliveryImpressions = x.delivery_impressions,
                CPM = x.cpm,
                CPP = (decimal?)x.cpp,
                Universe = x.universe
            };
        }

        private void _HydrateFromDto(plan entity, PlanDto planDto, QueryHintBroadcastContext context, string createdBy, DateTime createdDate)
        {
            entity.name = planDto.Name;
            entity.product_id = planDto.ProductId;
            entity.campaign_id = planDto.CampaignId;

            var planVersion = new plan_versions
            {
                spot_length_id = planDto.SpotLengthId,
                equivalized = planDto.Equivalized,
                status = (int)planDto.Status,
                flight_start_date = planDto.FlightStartDate.Value,
                flight_end_date = planDto.FlightEndDate.Value,
                flight_notes = planDto.FlightNotes,
                coverage_goal_percent = planDto.CoverageGoalPercent.Value,
                goal_breakdown_type = (int)planDto.GoalBreakdownType,
                created_by = createdBy, //when editing the draft, the created properties have to be removed in order to not overwrite the initial creator of the draft
                created_date = createdDate,
                modified_by = planDto.ModifiedBy,
                modified_date = planDto.ModifiedDate,
                target_vpvh = planDto.Vpvh,
                target_universe = planDto.Universe,
                hh_cpm = planDto.HouseholdCPM,
                hh_cpp = planDto.HouseholdCPP,
                hh_impressions = planDto.HouseholdDeliveryImpressions,
                hh_rating_points = planDto.HouseholdRatingPoints,
                hh_universe = planDto.HouseholdUniverse,
                is_draft = planDto.IsDraft
            };

            _HydratePlanAudienceInfo(planVersion, planDto);
            _HydratePlanBudget(planVersion, planDto);
            _HydratePlanFlightHiatus(planVersion, planDto, context);
            _HydrateDayparts(planVersion, planDto, context);
            _HydratePlanSecondaryAudiences(planVersion, planDto, context);
            _HydratePlanMarkets(planVersion, planDto, context);
            _HydrateWeeklyBreakdown(planVersion, planDto, context);

            entity.plan_versions.Add(planVersion);
        }

        private void _HydrateWeeklyBreakdown(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_weeks.RemoveRange(entity.plan_version_weeks);
            planDto.WeeklyBreakdownWeeks.ForEach(d =>
            {
                entity.plan_version_weeks.Add(new plan_version_weeks
                {
                    active_days_label = d.ActiveDays,
                    number_active_days = d.NumberOfActiveDays,
                    impressions = d.Impressions,
                    share_of_voice = d.ShareOfVoice,
                    media_week_id = d.MediaWeekId
                });
            });
        }

        private static void _HydratePlanAudienceInfo(plan_versions entity, PlanDto planDto)
        {
            entity.target_audience_id = planDto.AudienceId;
            entity.audience_type = (int)planDto.AudienceType;
            entity.hut_book_id = planDto.HUTBookId.Value;
            entity.share_book_id = planDto.ShareBookId;
            entity.posting_type = (int)planDto.PostingType;
        }

        private static void _HydratePlanBudget(plan_versions entity, PlanDto planDto)
        {
            entity.budget = planDto.Budget.Value;
            entity.target_impression = planDto.DeliveryImpressions.Value;
            entity.target_cpm = planDto.CPM.Value;
            entity.target_rating_points = planDto.DeliveryRatingPoints.Value;
            entity.target_cpp = planDto.CPP.Value;
            entity.currency = (int)planDto.Currency;
        }

        private void _HydratePlanFlightHiatus(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_flight_hiatus_days.RemoveRange(entity.plan_version_flight_hiatus_days);
            planDto.FlightHiatusDays.ForEach(d =>
            {
                entity.plan_version_flight_hiatus_days.Add(new plan_version_flight_hiatus_days { hiatus_day = d });
            });
        }

        private PlanDaypartDto _MapPlanDaypartDto(plan_version_dayparts entity)
        {
            var dto = new PlanDaypartDto
            {
                DaypartCodeId = entity.daypart_code_id,
                DaypartTypeId = EnumHelper.GetEnum<DaypartTypeEnum>(entity.daypart_type),
                StartTimeSeconds = entity.start_time_seconds,
                IsStartTimeModified = entity.is_start_time_modified,
                EndTimeSeconds = entity.end_time_seconds,
                IsEndTimeModified = entity.is_end_time_modified,
                WeightingGoalPercent = entity.weighting_goal_percent
            };
            return dto;
        }

        private void _HydrateDayparts(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_dayparts.RemoveRange(entity.plan_version_dayparts);
            planDto.Dayparts.ForEach(d => entity.plan_version_dayparts.Add(new plan_version_dayparts
            {
                daypart_code_id = d.DaypartCodeId,
                daypart_type = (int)d.DaypartTypeId,
                start_time_seconds = d.StartTimeSeconds,
                is_start_time_modified = d.IsStartTimeModified,
                end_time_seconds = d.EndTimeSeconds,
                is_end_time_modified = d.IsEndTimeModified,
                weighting_goal_percent = d.WeightingGoalPercent
            }));
        }

        private void _HydratePlanSecondaryAudiences(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_secondary_audiences.RemoveRange(entity.plan_version_secondary_audiences);
            planDto.SecondaryAudiences.ForEach(d =>
            {
                entity.plan_version_secondary_audiences.Add(new plan_version_secondary_audiences
                {
                    audience_id = d.AudienceId,
                    audience_type = (int)d.Type,
                    vpvh = d.Vpvh,
                    delivery_rating_points = d.DeliveryRatingPoints.Value,
                    delivery_impressions = d.DeliveryImpressions.Value,
                    cpm = d.CPM.Value,
                    cpp = (double)d.CPP.Value,
                    universe = d.Universe
                });
            });
        }

        private static void _HydratePlanMarkets(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_available_markets.RemoveRange(entity.plan_version_available_markets);
            context.plan_version_blackout_markets.RemoveRange(entity.plan_version_blackout_markets);

            planDto.AvailableMarkets.ForEach(m =>
            {
                entity.plan_version_available_markets.Add(new plan_version_available_markets
                {
                    market_code = m.MarketCode,
                    market_coverage_File_id = m.MarketCoverageFileId,
                    rank = m.Rank,
                    percentage_of_us = m.PercentageOfUS,
                    share_of_voice_percent = m.ShareOfVoicePercent
                }
                );
            });

            planDto.BlackoutMarkets.ForEach(m =>
            {
                entity.plan_version_blackout_markets.Add(new plan_version_blackout_markets()
                {
                    market_code = m.MarketCode,
                    market_coverage_file_id = m.MarketCoverageFileId,
                    rank = m.Rank,
                    percentage_of_us = m.PercentageOfUS,
                });
            });
        }

        private static PlanAvailableMarketDto _MapAvailableMarketDto(plan_version_available_markets entity, List<market> markets)
        {
            var marketName = markets.Where(s => s.market_code == entity.market_code).Select(s => s.geography_name)
                .Single($"More than one market found with code '{entity.market_code}'.");

            var dto = new PlanAvailableMarketDto
            {
                Id = entity.id,
                MarketCode = entity.market_code,
                Market = marketName,
                MarketCoverageFileId = entity.market_coverage_File_id,
                Rank = entity.rank,
                PercentageOfUS = entity.percentage_of_us,
                ShareOfVoicePercent = entity.share_of_voice_percent,
            };
            return dto;
        }

        private static PlanBlackoutMarketDto _MapBlackoutMarketDto(plan_version_blackout_markets entity, List<market> markets)
        {
            var marketName = markets.Where(s => s.market_code == entity.market_code).Select(s => s.geography_name)
                .Single($"More than one market found with code '{entity.market_code}'.");

            var dto = new PlanBlackoutMarketDto
            {
                Id = entity.id,
                MarketCode = entity.market_code,
                Market = marketName,
                MarketCoverageFileId = entity.market_coverage_file_id,
                Rank = entity.rank,
                PercentageOfUS = entity.percentage_of_us,
            };
            return dto;
        }
    }
}
