using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.PlanPricing;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
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
        /// <param name="versionId">Optional: version id. If nothing is passed, it will return the latest version</param>
        /// <returns></returns>
        PlanDto GetPlan(int planId, int? versionId = null);

        /// <summary>
        /// Gets the plans of a campaign excluding Canceled and Rejected.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        List<PlanDto> GetPlansForCampaign(int campaignId);

        /// <summary>
        /// Creates the or update draft.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        void CreateOrUpdateDraft(PlanDto plan, string createdBy, DateTime createdDate);

        /// <summary>
        /// Checks if a draft exist on the plan and returns the draft id
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>Dravt id</returns>
        int CheckIfDraftExists(int planId);

        /// <summary>
        /// Saves the model that ran in the pricing guide.
        /// </summary>
        /// <param name="planPricingRunModel">Plan pricing model data</param>
        void SavePricingRequest(PlanPricingApiRequestParametersDto planPricingRunModel);

        List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId);

        /// <summary>
        /// Gets the plan name by identifier.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>The name of the plan</returns>
        string GetPlanNameById(int planId);
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
                    var version = new plan_versions();
                    newPlan.plan_versions.Add(version);

                    _MapFromDto(planDto, context, newPlan, version);
                    _SetCreatedDate(version, createdBy, createdDate);

                    context.plans.Add(newPlan);
                    context.SaveChanges();

                    _UpdateLatestVersionId(newPlan, context);

                    planDto.Id = newPlan.id;
                    planDto.VersionId = newPlan.latest_version_id;
                });
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
                        .Where(x => x.id == planDto.Id)
                        .Single(p => p.id == planDto.Id, "Invalid plan id.");
                    var version = new plan_versions();
                    plan.plan_versions.Add(version);

                    _MapFromDto(planDto, context, plan, version);
                    _SetCreatedDate(version, createdBy, createdDate);

                    context.SaveChanges();

                    _UpdateLatestVersionId(plan, context);

                    planDto.VersionId = plan.latest_version_id;
                });
        }

        /// <inheritdoc/>
        public void CreateOrUpdateDraft(PlanDto planDto, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(
                   context =>
                   {
                       var plan = (from p in context.plans
                                   where p.id == planDto.Id
                                   select p)
                           .Include(p => p.plan_versions)
                           .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
                       .Single(x => x.id == planDto.Id, "Invalid plan id");

                       //there can be only 1 draft on a plan, so we're doing Single here
                       var draftVersion = plan.plan_versions.Where(x => x.is_draft == true).SingleOrDefault();
                       if (draftVersion == null)
                       {
                           //there is no draft on the plan, so we create a new version as the draft
                           draftVersion = new plan_versions();
                           plan.plan_versions.Add(draftVersion);
                           _SetCreatedDate(draftVersion, createdBy, createdDate);
                       }
                       else
                       {
                           draftVersion.modified_by = createdBy;
                           draftVersion.modified_date = createdDate;
                       }

                       _MapFromDto(planDto, context, plan, draftVersion);

                       context.SaveChanges();
                   });
        }

        /// <inheritdoc />
        public PlanDto GetPlan(int planId, int? versionId = null)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var markets = context.markets.ToList();
                    var entity = (from plan in context.plans
                                  where plan.id == planId
                                  select plan)
                        .Include(x => x.plan_versions)
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_show_type_restrictions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_show_type_restrictions.Select(r => r.show_types))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_genre_restrictions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_genre_restrictions.Select(r => r.genre))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks.Select(y => y.media_weeks)))
                        .Single(s => s.id == planId, "Invalid plan id.");
                    return _MapToDto(entity, markets, versionId);
                });
        }

        /// <inheritdoc />
        public string GetPlanNameById(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planName = (from plan in context.plans
                              where plan.id == planId
                              select plan.name)
                            .Single("Invalid plan id.");
                return planName;
            });
        }

        public int CheckIfDraftExists(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = (from plan in context.plans
                              where plan.id == planId
                              select plan)
                              .Include(x => x.plan_versions)
                    .Single(s => s.id == planId, "Invalid plan id.");
                return entity.plan_versions.Where(x => x.is_draft == true).Select(x => x.id).SingleOrDefault();
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
                        .Include(x => x.plan_versions)
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

        private void _UpdateLatestVersionId(plan newPlan, QueryHintBroadcastContext context)
        {
            newPlan.latest_version_id = newPlan.plan_versions.Max(x => x.id);
            context.SaveChanges();
        }

        private PlanDto _MapToDto(plan entity, List<market> markets, int? versionId = null)
        {
            var latestPlanVersion = versionId != null
                ? entity.plan_versions.Where(x => x.id == versionId).Single($"There is no version {versionId} available")
                : entity.plan_versions.Where(x => x.id == entity.latest_version_id).Single("There is no latest version available");

            //drafts don't have summary, so we're doing SingleOrDefault
            var planSummary = latestPlanVersion.plan_version_summaries.SingleOrDefault();
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
                ModifiedBy = latestPlanVersion.modified_by ?? latestPlanVersion.created_by,
                ModifiedDate = latestPlanVersion.modified_date ?? latestPlanVersion.created_date,
                Vpvh = latestPlanVersion.target_vpvh,
                Universe = latestPlanVersion.target_universe,
                HouseholdCPM = latestPlanVersion.hh_cpm,
                HouseholdCPP = latestPlanVersion.hh_cpp,
                HouseholdDeliveryImpressions = latestPlanVersion.hh_impressions,
                HouseholdRatingPoints = latestPlanVersion.hh_rating_points,
                HouseholdUniverse = latestPlanVersion.hh_universe,
                AvailableMarketsWithSovCount = planSummary?.available_market_with_sov_count ?? null,
                BlackoutMarketCount = planSummary?.blackout_market_count ?? null,
                BlackoutMarketTotalUsCoveragePercent = planSummary?.blackout_market_total_us_coverage_percent ?? null,
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

        private void _MapFromDto(PlanDto planDto, QueryHintBroadcastContext context, plan plan, plan_versions version)
        {
            plan.name = planDto.Name;
            plan.product_id = planDto.ProductId;
            plan.campaign_id = planDto.CampaignId;

            version.spot_length_id = planDto.SpotLengthId;
            version.equivalized = planDto.Equivalized;
            version.status = (int)planDto.Status;
            version.flight_start_date = planDto.FlightStartDate.Value;
            version.flight_end_date = planDto.FlightEndDate.Value;
            version.flight_notes = planDto.FlightNotes;
            version.coverage_goal_percent = planDto.CoverageGoalPercent.Value;
            version.goal_breakdown_type = (int)planDto.GoalBreakdownType;
            version.target_vpvh = planDto.Vpvh;
            version.target_universe = planDto.Universe;
            version.hh_cpm = planDto.HouseholdCPM;
            version.hh_cpp = planDto.HouseholdCPP;
            version.hh_impressions = planDto.HouseholdDeliveryImpressions;
            version.hh_rating_points = planDto.HouseholdRatingPoints;
            version.hh_universe = planDto.HouseholdUniverse;
            version.is_draft = planDto.IsDraft;

            _MapPlanAudienceInfo(version, planDto);
            _MapPlanBudget(version, planDto);
            _MapPlanFlightHiatus(version, planDto, context);
            _MapDayparts(version, planDto, context);
            _MapPlanSecondaryAudiences(version, planDto, context);
            _MapPlanMarkets(version, planDto, context);
            _MapWeeklyBreakdown(version, planDto, context);
        }

        private static void _SetCreatedDate(plan_versions version, string createdBy, DateTime createdDate)
        {
            version.created_by = createdBy; //when editing the draft; the created properties have to be removed in order to not overwrite the initial creator of the draft
            version.created_date = createdDate;
        }

        private static void _MapWeeklyBreakdown(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
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

        private static void _MapPlanAudienceInfo(plan_versions entity, PlanDto planDto)
        {
            entity.target_audience_id = planDto.AudienceId;
            entity.audience_type = (int)planDto.AudienceType;
            entity.hut_book_id = planDto.HUTBookId.Value;
            entity.share_book_id = planDto.ShareBookId;
            entity.posting_type = (int)planDto.PostingType;
        }

        private static void _MapPlanBudget(plan_versions entity, PlanDto planDto)
        {
            entity.budget = planDto.Budget.Value;
            entity.target_impression = planDto.DeliveryImpressions.Value;
            entity.target_cpm = planDto.CPM.Value;
            entity.target_rating_points = planDto.DeliveryRatingPoints.Value;
            entity.target_cpp = planDto.CPP.Value;
            entity.currency = (int)planDto.Currency;
        }

        private static void _MapPlanFlightHiatus(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_flight_hiatus_days.RemoveRange(entity.plan_version_flight_hiatus_days);
            planDto.FlightHiatusDays.ForEach(d =>
            {
                entity.plan_version_flight_hiatus_days.Add(new plan_version_flight_hiatus_days { hiatus_day = d });
            });
        }

        private static PlanDaypartDto _MapPlanDaypartDto(plan_version_dayparts entity)
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

            // if the contain type has ever been set
            if (entity.show_type_restrictions_contain_type.HasValue)
            {
                dto.Restrictions.ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                {
                    ContainType = (ContainTypeEnum)entity.show_type_restrictions_contain_type.Value,
                    ShowTypes = entity.plan_version_daypart_show_type_restrictions.Select(x => _MapToLookupDto(x.show_types)).ToList()
                };
            }

            // if the contain type has ever been set
            if (entity.genre_restrictions_contain_type.HasValue)
            {
                dto.Restrictions.GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                {
                    ContainType = (ContainTypeEnum)entity.genre_restrictions_contain_type.Value,
                    Genres = entity.plan_version_daypart_genre_restrictions.Select(x => _MapToLookupDto(x.genre)).ToList()
                };
            }

            return dto;
        }

        private static LookupDto _MapToLookupDto(show_types show_Type)
        {
            return new LookupDto()
            {
                Id = show_Type.id,
                Display = show_Type.name
            };
        }

        private static LookupDto _MapToLookupDto(genre genre)
        {
            return new LookupDto()
            {
                Id = genre.id,
                Display = genre.name
            };
        }

        private static void _MapDayparts(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_dayparts.RemoveRange(entity.plan_version_dayparts);

            foreach (var daypart in planDto.Dayparts)
            {
                var newDaypart = new plan_version_dayparts
                {
                    daypart_code_id = daypart.DaypartCodeId,
                    daypart_type = (int)daypart.DaypartTypeId,
                    start_time_seconds = daypart.StartTimeSeconds,
                    is_start_time_modified = daypart.IsStartTimeModified,
                    end_time_seconds = daypart.EndTimeSeconds,
                    is_end_time_modified = daypart.IsEndTimeModified,
                    weighting_goal_percent = daypart.WeightingGoalPercent
                };

                if (daypart.Restrictions != null)
                {
                    _HydrateShowTypeRestrictions(newDaypart, daypart.Restrictions.ShowTypeRestrictions);
                    _HydrateGenreRestrictions(newDaypart, daypart.Restrictions.GenreRestrictions);
                }
                
                entity.plan_version_dayparts.Add(newDaypart);
            }
        }

        private static void _HydrateShowTypeRestrictions(
            plan_version_dayparts daypart,
            PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto showTypeRestrictions)
        {
            if (showTypeRestrictions == null)
                return;

            daypart.show_type_restrictions_contain_type = (int)showTypeRestrictions.ContainType;

            if (!showTypeRestrictions.ShowTypes.IsEmpty())
            {
                foreach (var showTypeRestriction in showTypeRestrictions.ShowTypes)
                {
                    daypart.plan_version_daypart_show_type_restrictions.Add(new plan_version_daypart_show_type_restrictions
                    {
                        show_type_id = showTypeRestriction.Id
                    });
                }
            }
        }

        private static void _HydrateGenreRestrictions(
            plan_version_dayparts daypart,
            PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto genreRestrictions)
        {
            if (genreRestrictions == null)
                return;

            daypart.genre_restrictions_contain_type = (int)genreRestrictions.ContainType;

            if (!genreRestrictions.Genres.IsEmpty())
            {
                foreach (var genreRestriction in genreRestrictions.Genres)
                {
                    daypart.plan_version_daypart_genre_restrictions.Add(new plan_version_daypart_genre_restrictions
                    {
                        genre_id = genreRestriction.Id
                    });
                }
            }
        }

        private static void _MapPlanSecondaryAudiences(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
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

        private static void _MapPlanMarkets(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
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

        public void SavePricingRequest(PlanPricingApiRequestParametersDto planPricingRunModel)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var planPricingExecution = new plan_pricing_executions
                    {
                        plan_id = planPricingRunModel.PlanId,
                        min_cpm = planPricingRunModel.MinCpm,
                        max_cpm = planPricingRunModel.MaxCpm,
                        impressions_goal = planPricingRunModel.ImpressionsGoal,
                        budget_goal = planPricingRunModel.BudgetGoal,
                        cpm_goal = planPricingRunModel.CpmGoal,
                        proprietary_blend = planPricingRunModel.ProprietaryBlend,
                        competition_factor = planPricingRunModel.CompetitionFactor,
                        inflation_factor = planPricingRunModel.InflationFactor,
                        coverage_goal = planPricingRunModel.CoverageGoalPercent,
                        unit_caps_type = (int)planPricingRunModel.UnitCapType,
                        unit_caps = planPricingRunModel.UnitCaps
                    };

                    foreach (var market in planPricingRunModel.Markets)
                    {
                        planPricingExecution.plan_pricing_execution_markets.Add(new plan_pricing_execution_markets
                        {
                            market_code = (short)market.MarketId,
                            share_of_voice_percent = market.MarketShareOfVoice
                        });
                    }

                    context.plan_pricing_executions.Add(planPricingExecution);
                    context.SaveChanges();
                }
            );
        }

        public List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var executions = context.plan_pricing_executions.Where(p => p.plan_id == planId);

                return executions.Select(e => new PlanPricingApiRequestParametersDto
                {
                    PlanId = e.plan_id,
                    BudgetGoal = e.budget_goal,
                    CompetitionFactor = e.competition_factor,
                    CoverageGoalPercent = e.coverage_goal,
                    CpmGoal = e.cpm_goal,
                    ImpressionsGoal = e.impressions_goal,
                    InflationFactor = e.inflation_factor,
                    Markets = e.plan_pricing_execution_markets.Select(m => new PlanPricingMarketDto
                    {
                        MarketId = m.market_code,
                        MarketShareOfVoice = m.share_of_voice_percent
                    }).ToList(),
                    MaxCpm = e.max_cpm,
                    MinCpm = e.min_cpm,
                    ProprietaryBlend = e.proprietary_blend,
                    UnitCaps = e.unit_caps,
                    UnitCapType = (UnitCapEnum)e.unit_caps_type
                }).ToList();
            });
        }
    }
}