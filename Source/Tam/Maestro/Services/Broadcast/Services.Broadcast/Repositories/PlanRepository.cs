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
        /// <returns>Id of the new plan</returns>
        int SaveNewPlan(PlanDto planDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Saves the plan.
        /// </summary>
        /// <param name="planDto">The plan.</param>
        void SavePlan(PlanDto planDto);

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanDto GetPlan(int planId);

        /// <summary>
        /// Gets the plan status.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanStatusEnum GetPlanStatus(int planId);

        /// <summary>
        /// Gets the campaign plans.
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
        public int SaveNewPlan(PlanDto planDto, string createdBy, DateTime createdDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var newPlan = new plan
                    {
                        created_by = createdBy,
                        created_date = createdDate,
                    };
                    _HydrateFromDto(newPlan, planDto, context);

                    context.plans.Add(newPlan);
                    context.SaveChanges();
                    return newPlan.id;
                });
        }

        /// <inheritdoc />
        public void SavePlan(PlanDto planDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var plan = context.plans
                        .Include(p => p.plan_flight_hiatus)
                        .Include(p => p.plan_dayparts)
                        .Single(p => p.id == planDto.Id, "Invalid plan id.");
                    _HydrateFromDto(plan, planDto, context);

                    context.SaveChanges();
                });
        }

        /// <inheritdoc />
        public PlanDto GetPlan(int planId)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var markets = context.markets.ToList();
                    var entity = context.plans
                        .Include(p => p.plan_flight_hiatus)
                        .Include(p => p.plan_secondary_audiences)
                        .Include(p => p.plan_dayparts)
                        .Include(p => p.plan_available_markets)
                        .Include(p => p.plan_blackout_markets)
                        .Include(p => p.plan_weeks)
                        .Include(p => p.plan_weeks.Select(x=>x.media_weeks))
                        .Single(s => s.id == planId, "Invalid plan id.");
                    return _MapToDto(entity, markets);
                });
        }

        /// <inheritdoc />
        public PlanStatusEnum GetPlanStatus(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planStatus = context.plans
                    .Where(x=> x.id == planId)
                    .Select(x => x.status)
                    .Single("Invalid plan id.");
                return EnumHelper.GetEnum<PlanStatusEnum>(planStatus);
            });
        }

        /// <inheritdoc />
        public List<PlanDto> GetPlansForCampaign(int campaignId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var markets = context.markets.ToList();
                var entitiesRaw = context.plans
                    .Include(p => p.plan_flight_hiatus)
                    .Include(p => p.plan_secondary_audiences)
                    .Include(p => p.plan_dayparts)
                    .Include(p => p.plan_available_markets)
                    .Include(p => p.plan_blackout_markets)
                    .Include(p => p.plan_weeks)
                    .Include(p => p.plan_weeks.Select(x => x.media_weeks))
                    .Where(p => p.campaign_id == campaignId)
                    .ToList();

                return entitiesRaw.Select(e => _MapToDto(e, markets)).ToList();
            });
        }

        private PlanDto _MapToDto(plan entity, List<market> markets)
        {
            var dto = new PlanDto
            {
                Id = entity.id,
                CampaignId = entity.campaign_id,
                Name = entity.name,
                SpotLengthId = entity.spot_length_id,
                Equivalized = entity.equivalized,
                Status = EnumHelper.GetEnum<PlanStatusEnum>(entity.status),
                ProductId = entity.product_id,
                FlightStartDate = entity.flight_start_date,
                FlightEndDate = entity.flight_end_date,
                FlightNotes = entity.flight_notes,
                AudienceId = entity.audience_id,
                AudienceType = EnumHelper.GetEnum<AudienceTypeEnum>(entity.audience_type),
                HUTBookId = entity.hut_book_id,
                ShareBookId = entity.share_book_id,
                PostingType = EnumHelper.GetEnum<PostingTypeEnum>(entity.posting_type),
                FlightHiatusDays = entity.plan_flight_hiatus.Select(h => h.hiatus_day).ToList(),
                Budget = entity.budget,
                DeliveryImpressions = entity.delivery_impressions,
                CPM = entity.cpm,
                DeliveryRatingPoints = entity.delivery_rating_points,
                CPP = entity.cpp,
                Currency = EnumHelper.GetEnum<PlanCurrenciesEnum>(entity.currency),
                GoalBreakdownType = EnumHelper.GetEnum<PlanGoalBreakdownTypeEnum>(entity.goal_breakdown_type),
                SecondaryAudiences = entity.plan_secondary_audiences.Select(_MapSecondaryAudiences).ToList(),
                Dayparts = entity.plan_dayparts.Select(_MapPlanDaypartDto).ToList(),
                CoverageGoalPercent = entity.coverage_goal_percent,
                AvailableMarkets = entity.plan_available_markets.Select(e => _MapAvailableMarketDto(e, markets)).ToList(),
                BlackoutMarkets = entity.plan_blackout_markets.Select(e => _MapBlackoutMarketDto(e, markets)).ToList(),
                WeeklyBreakdownWeeks = entity.plan_weeks.Select(_MapWeeklyBreakdownWeeks).ToList(),
                ModifiedBy = entity.modified_by,
                ModifiedDate = entity.modified_date,
                Vpvh = entity.vpvh,
                Universe = entity.universe,
                HouseholdCPM = entity.household_cpm,
                HouseholdCPP = entity.household_cpp,
                HouseholdDeliveryImpressions = entity.household_delivery_impressions,
                HouseholdRatingPoints = entity.household_rating_points,
                HouseholdUniverse = entity.household_universe
            };
            return dto;
        }

        private WeeklyBreakdownWeek _MapWeeklyBreakdownWeeks(plan_weeks arg)
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

        private static PlanAudienceDto _MapSecondaryAudiences(plan_secondary_audiences x)
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

        /// <summary>
        /// Populates the editable fields from the dto to the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="planDto">The plan dto.</param>
        /// <param name="context">The context.</param>
        private void _HydrateFromDto(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            entity.name = planDto.Name;
            entity.product_id = planDto.ProductId;
            entity.spot_length_id = planDto.SpotLengthId;
            entity.equivalized = planDto.Equivalized;
            entity.status = (int)planDto.Status;
            entity.campaign_id = planDto.CampaignId;

            entity.flight_start_date = planDto.FlightStartDate.Value;
            entity.flight_end_date = planDto.FlightEndDate.Value;
            entity.flight_notes = planDto.FlightNotes;

            entity.coverage_goal_percent = planDto.CoverageGoalPercent.Value;
            entity.goal_breakdown_type = (int)planDto.GoalBreakdownType;

            entity.modified_by = planDto.ModifiedBy;
            entity.modified_date = planDto.ModifiedDate;

            entity.vpvh = planDto.Vpvh;

            entity.universe = planDto.Universe;
            entity.household_cpm = planDto.HouseholdCPM;
            entity.household_cpp = planDto.HouseholdCPP;
            entity.household_delivery_impressions = planDto.HouseholdDeliveryImpressions;
            entity.household_rating_points = planDto.HouseholdRatingPoints;
            entity.household_universe = planDto.HouseholdUniverse;

            _HydratePlanAudienceInfo(entity, planDto);
            _HydratePlanBudget(entity, planDto);
            _HydratePlanFlightHiatus(entity, planDto, context);
            _HydrateDayparts(entity, planDto, context);
            _HydratePlanSecondaryAudiences(entity, planDto, context);
            _HydratePlanMarkets(entity, planDto, context);
            _HydrateWeeklyBreakdown(entity, planDto, context);
        }

        private void _HydrateWeeklyBreakdown(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_weeks.RemoveRange(entity.plan_weeks);
            planDto.WeeklyBreakdownWeeks.ForEach(d =>
            {
                entity.plan_weeks.Add(new plan_weeks
                {
                    active_days_label = d.ActiveDays,
                    number_active_days = d.NumberOfActiveDays,
                    impressions = d.Impressions,
                    share_of_voice = d.ShareOfVoice,
                    media_week_id = d.MediaWeekId
                });
            });
        }

        private static void _HydratePlanAudienceInfo(plan entity, PlanDto planDto)
        {
            entity.audience_id = planDto.AudienceId;
            entity.audience_type = (int)planDto.AudienceType;
            entity.hut_book_id = planDto.HUTBookId.Value;
            entity.share_book_id = planDto.ShareBookId;
            entity.posting_type = (int)planDto.PostingType;
        }

        private static void _HydratePlanBudget(plan entity, PlanDto planDto)
        {
            entity.budget = planDto.Budget.Value;
            entity.delivery_impressions = planDto.DeliveryImpressions.Value;
            entity.cpm = planDto.CPM.Value;
            entity.delivery_rating_points = planDto.DeliveryRatingPoints.Value;
            entity.cpp = planDto.CPP.Value;
            entity.currency = (int)planDto.Currency;
        }

        private void _HydratePlanFlightHiatus(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_flight_hiatus.RemoveRange(entity.plan_flight_hiatus);
            planDto.FlightHiatusDays.ForEach(d =>
            {
                entity.plan_flight_hiatus.Add(new plan_flight_hiatus { hiatus_day = d });
            });
        }

        private PlanDaypartDto _MapPlanDaypartDto(plan_dayparts entity)
        {
            var dto = new PlanDaypartDto
            {
                DaypartCodeId = entity.daypart_code_id,
                DaypartTypeId = EnumHelper.GetEnum<DaypartTypeEnum>(entity.daypart_type),
                StartTimeSeconds = entity.start_time_seconds,
                EndTimeSeconds = entity.end_time_seconds,
                WeightingGoalPercent = entity.weighting_goal_percent
            };
            return dto;
        }

        private void _HydrateDayparts(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_dayparts.RemoveRange(entity.plan_dayparts);
            planDto.Dayparts.ForEach(d => entity.plan_dayparts.Add(new plan_dayparts
            {
                daypart_code_id = d.DaypartCodeId,
                daypart_type = (int)d.DaypartTypeId,
                start_time_seconds = d.StartTimeSeconds,
                end_time_seconds = d.EndTimeSeconds,
                weighting_goal_percent = d.WeightingGoalPercent
            }));
        }
        
        private void _HydratePlanSecondaryAudiences(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_secondary_audiences.RemoveRange(entity.plan_secondary_audiences);
            planDto.SecondaryAudiences.ForEach(d =>
            {
                entity.plan_secondary_audiences.Add(new plan_secondary_audiences
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

        private static void _HydratePlanMarkets(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_available_markets.RemoveRange(entity.plan_available_markets);
            context.plan_blackout_markets.RemoveRange(entity.plan_blackout_markets);

            planDto.AvailableMarkets.ForEach(m =>
            {
                entity.plan_available_markets.Add(new plan_available_markets
                {
                    market_code = m.MarketCode,
                    market_coverage_file_id = m.MarketCoverageFileId,
                    rank = m.Rank,
                    percentage_of_us = m.PercentageOfUS,
                    share_of_voice_percent = m.ShareOfVoicePercent
                }
                );
            });

            planDto.BlackoutMarkets.ForEach(m =>
            {
                entity.plan_blackout_markets.Add(new plan_blackout_markets()
                {
                    market_code = m.MarketCode,
                    market_coverage_file_id = m.MarketCoverageFileId,
                    rank = m.Rank,
                    percentage_of_us = m.PercentageOfUS,
                });
            });
        }

        private static PlanAvailableMarketDto _MapAvailableMarketDto(plan_available_markets entity, List<market> markets)
        {
            var marketName = markets.Where(s => s.market_code == entity.market_code).Select(s => s.geography_name)
                .Single($"More than one market found with code '{entity.market_code}'.");

            var dto = new PlanAvailableMarketDto
            {
                Id = entity.id,
                MarketCode = entity.market_code,
                Market = marketName,
                MarketCoverageFileId = entity.market_coverage_file_id,
                Rank = entity.rank,
                PercentageOfUS = entity.percentage_of_us,
                ShareOfVoicePercent = entity.share_of_voice_percent,
            };
            return dto;
        }

        private static PlanBlackoutMarketDto _MapBlackoutMarketDto(plan_blackout_markets entity, List<market> markets)
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
