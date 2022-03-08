using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Microsoft.EntityFrameworkCore.Internal;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface ISpotExceptionRepository : IDataRepository
    {
        /// <summary>
        /// Add mock data to spot exceptions tables.
        /// </summary>   
        ///  <param name="spotExceptionsRecommendedPlans">Mock data collection for ExceptionsRecommendedPlans table.</param>
        ///  <param name="spotExceptionsOutOfSpecs">Mock data collection for ExceptionsOutOfSpecs table.</param>
        bool AddSpotExceptionData(List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans,
             List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs);
        /// <summary>
        /// Clear data from spot exceptions tables.
        /// </summary>   
        bool ClearSpotExceptionData();
        /// <summary>
        /// Gets the available outofspecsPosts within week start and end date
        /// </summary>
        /// <param name="weekStartDate">The media week start date</param>
        /// <param name="weekEndDate">The media week end date</param>
        /// <returns>List of SpotExceptionsOutOfSpecsDto object</returns>
        List<SpotExceptionsOutOfSpecsDto> GetSpotExceptionsOutOfSpecPosts(DateTime weekStartDate, DateTime weekEndDate);
        /// <summary>
        /// Gets spot exceptions out of spec by id
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecId">The spot exceptions out of spec id</param>
        /// <returns>The spot exceptions out of specs</returns>
        SpotExceptionsOutOfSpecsDto GetSpotExceptionsOutOfSpecById(int spotExceptionsOutOfSpecId);

        /// <summary>
        /// Gets spot exceptions recommended plans
        /// </summary>
        /// <param name="weekStartDate">The week start date</param>
        /// <param name="weekEndDate">The week end date</param>
        /// <returns>The spot exceptions recommended plans</returns>
        List<SpotExceptionsRecommendedPlansDto> GetSpotExceptionsRecommendedPlans(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets spot exceptions recommended plan by id
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanId">The spot exceptions recommended plan id</param>
        /// <returns>The spot exceptions recommended plan</returns>
        SpotExceptionsRecommendedPlansDto GetSpotExceptionsRecommendedPlanById(int spotExceptionsRecommendedPlanId);

        /// <summary>
        /// Saves spot exception recommended plan decision
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanDecision">The spot exceptions recommended plan decision parameters</param>
        /// <returns>True if spot exception recommended plan decision saves successfully otherwise false</returns>
        bool SaveSpotExceptionsRecommendedPlanDecision(SpotExceptionsRecommendedPlanDecisionDto spotExceptionsRecommendedPlanDecision);

        /// <summary>
        /// Updates recommended plan of spot exceptions recommended plan
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanDetail">The spot exceptoins recommended plan detail parameter</param>
        /// <returns>True if recommended plan of spot exception recommended plan updates successfully otherwise false</returns>
        bool UpdateRecommendedPlanOfSpotExceptionsRecommendedPlan(SpotExceptionsRecommendedPlanDetailsDto spotExceptionsRecommendedPlanDetail);

        /// <summary>
        /// Adds spot exceptions recommended plans
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlans">The list of spot exceptions recommended plans to be inserted</param>
        /// <returns>Total number of inserted spot exceptions recommended plans</returns>
        int AddSpotExceptionsRecommendedPlans(List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans);

        /// <summary>
        /// Adds spot exceptions OutOfSpecPosts
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecs">The list of spot exceptions OutofSpecsPosts to be inserted</param>
        /// <returns>Total number of inserted spot exceptions OutofSpecs</returns> 
        int AddOutOfSpecs(List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs);

        /// <summary>
        /// <param name="userName">The User Name</param>
        /// <param name="createdAt">Created at Date</param>
        /// <returns>true or false</returns>
        bool SaveSpotExceptionsOutOfSpecsDecisions(SpotExceptionsOutOfSpecDecisionsPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest, string userName, DateTime createdAt);

        /// <summary>
        /// Gets spot exceptions out of spec reason codes
        /// </summary>
        /// <returns>The spot exceptions out of spec reason codes</returns>
        List<SpotExceptionsOutOfSpecReasonCodeDto> GetSpotExceptionsOutOfSpecReasonCodes();
        
        /// <summary>
        /// Gets Spot Exceptions No plan Unposted Plans
        /// </summary>
        /// <param name="weekStartDate">Start Date </param>
        /// <param name="weekEndDate">End Date </param>
        /// <returns>Unposted No plan Spot Exceptions</returns>
        List<SpotExceptionUnpostedNoPlanDto> GetSpotExceptionUnpostedNoPlan(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets Spot Exceptions Unposted No Reels Roster Plans
        /// </summary>
        /// <param name="weekStartDate">Start week</param>
        /// <param name="weekEndDate">End Week </param>
        /// <returns>Unposted No Reel Roster Spot Exceptions</returns>
        List<SpotExceptionUnpostedNoReelRosterDto> GetSpotExceptionUnpostedNoReelRoster(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the spot exceptions out of spec spots for a plan for a daterange.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        List<SpotExceptionsOutOfSpecsDto> GetSpotExceptionsOutOfSpecPlanSpots(int planId, DateTime startDate, DateTime endDate);
    }

    public class SpotExceptionRepository : BroadcastRepositoryBase, ISpotExceptionRepository
    {
        public SpotExceptionRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public bool AddSpotExceptionData(List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans,
            List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs)
        {
            var executionId = Guid.NewGuid();

            _InReadUncommitedTransaction(context =>
            {
                var uniqueExternalIdRp = 1;

                var spotExceptionsRecommendedPlansToAdd = spotExceptionsRecommendedPlans.Select(recommendedPlan =>
                    new spot_exceptions_recommended_plans()
                    {
                        estimate_id = recommendedPlan.EstimateId,
                        isci_name = recommendedPlan.IsciName,
                        recommended_plan_id = recommendedPlan.RecommendedPlanId,
                        program_name = recommendedPlan.ProgramName,
                        program_air_time = recommendedPlan.ProgramAirTime,
                        advertiser_name = recommendedPlan.AdvertiserName,
                        station_legacy_call_letters = recommendedPlan.StationLegacyCallLetters,
                        cost = recommendedPlan.Cost,
                        impressions = recommendedPlan.Impressions,
                        spot_length_id = recommendedPlan.SpotLengthId,
                        audience_id = recommendedPlan.AudienceId,
                        product = recommendedPlan.Product,
                        flight_start_date = recommendedPlan.FlightStartDate,
                        flight_end_date = recommendedPlan.FlightEndDate,
                        daypart_id = recommendedPlan.DaypartId,
                        ingested_by = recommendedPlan.IngestedBy,
                        ingested_at = recommendedPlan.IngestedAt,
                        unique_id_external = ++uniqueExternalIdRp,
                        execution_id_external = executionId.ToString(),
                        spot_exceptions_recommended_plan_details = recommendedPlan.SpotExceptionsRecommendedPlanDetails
                            .Select(recommendedPlanDetails =>
                            {
                                var spotExceptionsRecommendedPlanDetail = new spot_exceptions_recommended_plan_details()
                                {

                                    recommended_plan_id = recommendedPlanDetails.RecommendedPlanId,
                                    metric_percent = recommendedPlanDetails.MetricPercent,
                                    is_recommended_plan = recommendedPlanDetails.IsRecommendedPlan
                                };
                                if (recommendedPlanDetails.SpotExceptionsRecommendedPlanDecision != null)
                                {
                                    spotExceptionsRecommendedPlanDetail.spot_exceptions_recommended_plan_decision =
                                        new List<spot_exceptions_recommended_plan_decision>
                                        {
                                            new spot_exceptions_recommended_plan_decision
                                            {
                                                created_at = recommendedPlanDetails
                                                    .SpotExceptionsRecommendedPlanDecision.CreatedAt,
                                                username = recommendedPlanDetails.SpotExceptionsRecommendedPlanDecision
                                                    .UserName
                                            }
                                        };
                                }

                                ;
                                return spotExceptionsRecommendedPlanDetail;
                            }).ToList()
                    }).ToList();

                context.spot_exceptions_recommended_plans.AddRange(spotExceptionsRecommendedPlansToAdd);

                var uniqueExternalIdOos = 1;
                var spotExceptionsOutOfSpecsToAdd = spotExceptionsOutOfSpecs.Select(outOfSpecs =>
                {
                    var spotExceptionsOutOfSpec = new spot_exceptions_out_of_specs
                    {
                        reason_code_message = outOfSpecs.ReasonCodeMessage,
                        estimate_id = outOfSpecs.EstimateId,
                        isci_name = outOfSpecs.IsciName,
                        recommended_plan_id = outOfSpecs.RecommendedPlanId,
                        program_name = outOfSpecs.ProgramName,
                        station_legacy_call_letters = outOfSpecs.StationLegacyCallLetters,
                        spot_length_id = outOfSpecs.SpotLengthId,
                        audience_id = outOfSpecs.AudienceId,
                        program_network = outOfSpecs.ProgramNetwork,
                        program_air_time = outOfSpecs.ProgramAirTime,                        
                        reason_code_id = outOfSpecs.SpotExceptionsOutOfSpecReasonCode.Id,
                        execution_id_external = executionId.ToString(),
                        impressions= outOfSpecs.Impressions,
                        house_isci = outOfSpecs.HouseIsci,
                        program_genre_id = outOfSpecs.ProgramGenre?.Id,
                        spot_unique_hash_external = outOfSpecs.SpotUniqueHashExternal,
                        daypart_id = outOfSpecs.DaypartId,
                        market_code = outOfSpecs.MarketCode,
                        market_rank = outOfSpecs.MarketRank,
                        ingested_by = outOfSpecs.IngestedBy,
                        ingested_at = outOfSpecs.IngestedAt,
                        created_by = outOfSpecs.CreatedBy,
                        created_at = outOfSpecs.CreatedAt,
                        modified_by = outOfSpecs.ModifiedBy,
                        modified_at = outOfSpecs.ModifiedAt
                    };
                    if (outOfSpecs.SpotExceptionsOutOfSpecDecision != null)
                    {
                        spotExceptionsOutOfSpec.spot_exceptions_out_of_spec_decisions =
                            new List<spot_exceptions_out_of_spec_decisions>
                            {
                                new spot_exceptions_out_of_spec_decisions()
                                {
                                    spot_exceptions_out_of_spec_id = outOfSpecs.SpotExceptionsOutOfSpecDecision
                                        .SpotExceptionsOutOfSpecId,
                                    accepted_as_in_spec = outOfSpecs.SpotExceptionsOutOfSpecDecision.AcceptedAsInSpec,
                                    decision_notes = outOfSpecs.SpotExceptionsOutOfSpecDecision.DecisionNotes,
                                    created_at = outOfSpecs.SpotExceptionsOutOfSpecDecision.CreatedAt,
                                    username = outOfSpecs.SpotExceptionsOutOfSpecDecision.UserName
                                }
                            };
                    }

                    return spotExceptionsOutOfSpec;
                }).ToList();
                context.spot_exceptions_out_of_specs.AddRange(spotExceptionsOutOfSpecsToAdd);
                context.SaveChanges();
            });
            return true;
        }

        public bool ClearSpotExceptionData()
        {
            return _InReadUncommitedTransaction(context =>
            {
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_decision");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_details");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plans");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_spec_decisions");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs");
                return true;
            });
        }

        public List<SpotExceptionsOutOfSpecsDto> GetSpotExceptionsOutOfSpecPosts(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsEntities = context.spot_exceptions_out_of_specs
                    .Where(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.program_air_time >= weekStartDate && spotExceptionsoutOfSpecDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_decisions)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan.campaign)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan.plan_versions)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_lengths)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.daypart)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.genre)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.audience)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_reason_codes)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsoutOfSpecDb, stationDb) => new { SpotExceptionsoutOfSpec = spotExceptionsoutOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsoutOfSpecPosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapSpotExceptionsOutOfSpecToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecEntity.Station)).ToList();
                return spotExceptionsoutOfSpecPosts;

            });
        }

        public List<SpotExceptionsOutOfSpecsDto> GetSpotExceptionsOutOfSpecPlanSpots(int planId, DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsEntities = context.spot_exceptions_out_of_specs
                    .Where(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.recommended_plan_id == planId &&
                            spotExceptionsoutOfSpecDb.program_air_time >= startDate && spotExceptionsoutOfSpecDb.program_air_time <= endDate)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_decisions)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan.campaign)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan.plan_versions)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_lengths)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.daypart)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.genre)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.audience)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_reason_codes)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsoutOfSpecDb, stationDb) => new { SpotExceptionsoutOfSpec = spotExceptionsoutOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsoutOfSpecPosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapSpotExceptionsOutOfSpecToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecEntity.Station)).ToList();
                return spotExceptionsoutOfSpecPosts;

            });
        }

        public SpotExceptionsOutOfSpecsDto GetSpotExceptionsOutOfSpecById(int spotExceptionsOutOfSpecId)
        {

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsEntity = context.spot_exceptions_out_of_specs
                    .Where(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.id == spotExceptionsOutOfSpecId)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_decisions)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan.plan_versions)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan.campaign)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_lengths)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.daypart)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.genre)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.audience)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_reason_codes)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsoutOfSpecDb, stationDb) => new { SpotExceptionsoutOfSpec = spotExceptionsoutOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .SingleOrDefault();

                if (spotExceptionsOutOfSpecsEntity == null)
                {
                    return null;
                }

                var spotExceptionsoutOfSpec = _MapSpotExceptionsOutOfSpecToDto(spotExceptionsOutOfSpecsEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecsEntity.Station);
                return spotExceptionsoutOfSpec;

            });
        }
        private SpotExceptionsOutOfSpecsDto _MapSpotExceptionsOutOfSpecToDto(spot_exceptions_out_of_specs spotExceptionsOutOfSpecEntity, station stationEntity)
        {
            var planVersion = spotExceptionsOutOfSpecEntity.plan?.plan_versions.First(v => v.id == spotExceptionsOutOfSpecEntity.plan.latest_version_id);

            var spotExceptionsOutOfSpec = new SpotExceptionsOutOfSpecsDto
            {
                Id = spotExceptionsOutOfSpecEntity.id,
                SpotUniqueHashExternal = spotExceptionsOutOfSpecEntity.spot_unique_hash_external,
                ReasonCodeMessage = spotExceptionsOutOfSpecEntity.reason_code_message,
                EstimateId = spotExceptionsOutOfSpecEntity.estimate_id,
                IsciName = spotExceptionsOutOfSpecEntity.isci_name,
                HouseIsci = spotExceptionsOutOfSpecEntity.house_isci,
                RecommendedPlanId = spotExceptionsOutOfSpecEntity.recommended_plan_id,
                RecommendedPlanName = spotExceptionsOutOfSpecEntity.plan?.name,
                ProgramName = spotExceptionsOutOfSpecEntity.program_name,
                StationLegacyCallLetters = spotExceptionsOutOfSpecEntity.station_legacy_call_letters,
                Affiliate = stationEntity?.affiliation,
                Market = stationEntity?.market?.geography_name,
                SpotLength = _MapSpotLengthToDto(spotExceptionsOutOfSpecEntity.spot_lengths),
                Audience = _MapAudienceToDto(spotExceptionsOutOfSpecEntity.audience),
                DaypartDetail = _MapDaypartToDto(spotExceptionsOutOfSpecEntity.daypart),
                ProgramAirTime = spotExceptionsOutOfSpecEntity.program_air_time,
                IngestedBy = spotExceptionsOutOfSpecEntity.ingested_by,
                IngestedAt = spotExceptionsOutOfSpecEntity.ingested_at,
                CreatedBy = spotExceptionsOutOfSpecEntity.created_by,
                CreatedAt = spotExceptionsOutOfSpecEntity.created_at,
                ModifiedBy = spotExceptionsOutOfSpecEntity.modified_by,
                ModifiedAt = spotExceptionsOutOfSpecEntity.modified_at,
                Impressions = spotExceptionsOutOfSpecEntity.impressions,
                PlanId = spotExceptionsOutOfSpecEntity.recommended_plan_id ?? 0,
                FlightStartDate = planVersion?.flight_start_date,
                FlightEndDate = planVersion?.flight_end_date,
                AdvertiserMasterId = spotExceptionsOutOfSpecEntity.plan?.campaign.advertiser_master_id,
                Product = null,                
                SpotExceptionsOutOfSpecDecision = spotExceptionsOutOfSpecEntity.spot_exceptions_out_of_spec_decisions.Select(spotExceptionsOutOfSpecsDecisionDb => new SpotExceptionsOutOfSpecDecisionsDto
                {
                    Id = spotExceptionsOutOfSpecsDecisionDb.id,
                    SpotExceptionsOutOfSpecId = spotExceptionsOutOfSpecsDecisionDb.spot_exceptions_out_of_spec_id,
                    AcceptedAsInSpec = spotExceptionsOutOfSpecsDecisionDb.accepted_as_in_spec,
                    DecisionNotes = spotExceptionsOutOfSpecsDecisionDb.decision_notes,
                    UserName = spotExceptionsOutOfSpecsDecisionDb.username,
                    CreatedAt = spotExceptionsOutOfSpecsDecisionDb.created_at,
                    SyncedBy = spotExceptionsOutOfSpecsDecisionDb.synced_by,
                    SyncedAt = spotExceptionsOutOfSpecsDecisionDb.synced_at
                }).SingleOrDefault(),
                SpotExceptionsOutOfSpecReasonCode = _MapSpotExceptionsOutOfSpecReasonCodeToDto(spotExceptionsOutOfSpecEntity.spot_exceptions_out_of_spec_reason_codes),
                MarketCode = spotExceptionsOutOfSpecEntity.market_code,
                MarketRank = spotExceptionsOutOfSpecEntity.market_rank,
                ProgramGenre = new Genre
                { 
                    Id = spotExceptionsOutOfSpecEntity.genre.id,
                    Name = spotExceptionsOutOfSpecEntity.genre.name,
                    ProgramSourceId = spotExceptionsOutOfSpecEntity.genre.program_source_id
                }
             };
            return spotExceptionsOutOfSpec;
        }

        /// <inheritdoc />
        public List<SpotExceptionsRecommendedPlansDto> GetSpotExceptionsRecommendedPlans(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanEntities = context.spot_exceptions_recommended_plans
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.spot_exceptions_recommended_plan_decision))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.plan)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_lengths)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.daypart)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.audience)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsRecommendedPlans = spotExceptionsRecommendedPlanEntities.Select(spotExceptionsRecommendedPlanEntity => _MapSpotExceptionsRecommendedPlanToDto(spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan, spotExceptionsRecommendedPlanEntity.Station)).ToList();
                return spotExceptionsRecommendedPlans;
            });
        }

        /// <inheritdoc />
        public SpotExceptionsRecommendedPlansDto GetSpotExceptionsRecommendedPlanById(int spotExceptionsRecommendedPlanId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanEntity = context.spot_exceptions_recommended_plans
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.id == spotExceptionsRecommendedPlanId)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.spot_exceptions_recommended_plan_decision))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.plan)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_lengths)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.daypart)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.audience)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanDb, Station = stationDb.FirstOrDefault() })
                    .SingleOrDefault();

                if (spotExceptionsRecommendedPlanEntity == null)
                {
                    return null;
                }

                var spotExceptionsRecommendedPlan = _MapSpotExceptionsRecommendedPlanToDto(spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan, spotExceptionsRecommendedPlanEntity.Station);
                return spotExceptionsRecommendedPlan;
            });
        }

        private SpotExceptionsRecommendedPlansDto _MapSpotExceptionsRecommendedPlanToDto(spot_exceptions_recommended_plans spotExceptionsRecommendedPlanEntity, station stationEntity)
        {
            var spotExceptionsRecommendedPlan = new SpotExceptionsRecommendedPlansDto
            {
                Id = spotExceptionsRecommendedPlanEntity.id,
                EstimateId = spotExceptionsRecommendedPlanEntity.estimate_id,
                IsciName = spotExceptionsRecommendedPlanEntity.isci_name,
                RecommendedPlanId = spotExceptionsRecommendedPlanEntity.recommended_plan_id,
                RecommendedPlanName = spotExceptionsRecommendedPlanEntity.plan?.name,
                ProgramName = spotExceptionsRecommendedPlanEntity.program_name,
                AdvertiserName= spotExceptionsRecommendedPlanEntity.advertiser_name,
                ProgramAirTime = spotExceptionsRecommendedPlanEntity.program_air_time,
                StationLegacyCallLetters = spotExceptionsRecommendedPlanEntity.station_legacy_call_letters,
                Affiliate = stationEntity?.affiliation,
                Market = stationEntity?.market?.geography_name,
                Cost = spotExceptionsRecommendedPlanEntity.cost,
                Impressions = spotExceptionsRecommendedPlanEntity.impressions,
                SpotLength = _MapSpotLengthToDto(spotExceptionsRecommendedPlanEntity.spot_lengths),
                Audience = _MapAudienceToDto(spotExceptionsRecommendedPlanEntity.audience),
                Product = spotExceptionsRecommendedPlanEntity.product,
                FlightStartDate = spotExceptionsRecommendedPlanEntity.flight_start_date,
                FlightEndDate = spotExceptionsRecommendedPlanEntity.flight_end_date,
                DaypartDetail = _MapDaypartToDto(spotExceptionsRecommendedPlanEntity.daypart),
                IngestedAt = spotExceptionsRecommendedPlanEntity.ingested_at,
                IngestedBy = spotExceptionsRecommendedPlanEntity.ingested_by,
                SpotExceptionsRecommendedPlanDetails = spotExceptionsRecommendedPlanEntity.spot_exceptions_recommended_plan_details.Select(spotExceptionsRecommendedPlanDetailDb =>
                {
                    var recommendedPlan = spotExceptionsRecommendedPlanDetailDb.plan;
                    var recommendedPlanVersion = recommendedPlan.plan_versions.Single(planVersion => planVersion.id == recommendedPlan.latest_version_id);
                    var spotExceptionsRecommendedPlanDetail = new SpotExceptionsRecommendedPlanDetailsDto
                    {
                        Id = spotExceptionsRecommendedPlanDetailDb.id,
                        SpotExceptionsRecommendedPlanId = spotExceptionsRecommendedPlanDetailDb.spot_exceptions_recommended_plan_id,
                        RecommendedPlanDetail = new RecommendedPlanDetailDto
                        {
                            Id = spotExceptionsRecommendedPlanDetailDb.recommended_plan_id,
                            Name = recommendedPlan.name,
                            FlightStartDate = recommendedPlanVersion.flight_start_date ?? DateTime.MinValue, // Once requirement is clear about plan's flight_start_date and flight_end_date in case of goals by daypart feature we need to change this DateTime.MinValue value
                            FlightEndDate = recommendedPlanVersion.flight_end_date ?? DateTime.MinValue, // Once requirement is clear about plan's flight_start_date and flight_end_date in case of goals by daypart feature we need to change this DateTime.MinValue value
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList()
                        },
                        MetricPercent = spotExceptionsRecommendedPlanDetailDb.metric_percent,
                        IsRecommendedPlan = spotExceptionsRecommendedPlanDetailDb.is_recommended_plan,
                        SpotExceptionsRecommendedPlanDecision = spotExceptionsRecommendedPlanDetailDb.spot_exceptions_recommended_plan_decision.Select(spotExceptionsRecommendedPlanDecisionDb => new SpotExceptionsRecommendedPlanDecisionDto
                        {
                            Id = spotExceptionsRecommendedPlanDecisionDb.id,
                            SpotExceptionsRecommendedPlanDetailId = spotExceptionsRecommendedPlanDecisionDb.spot_exceptions_recommended_plan_detail_id,
                            UserName = spotExceptionsRecommendedPlanDecisionDb.username,
                            CreatedAt = spotExceptionsRecommendedPlanDecisionDb.created_at
                        }).SingleOrDefault()
                    };
                    return spotExceptionsRecommendedPlanDetail;
                }).ToList()
            };
            return spotExceptionsRecommendedPlan;
        }

        private SpotLengthDto _MapSpotLengthToDto(spot_lengths spotLengthEntity)
        {
            if (spotLengthEntity == null)
            {
                return null;
            }

            var spotLength = new SpotLengthDto
            {
                Id = spotLengthEntity.id,
                Length = spotLengthEntity.length
            };
            return spotLength;
        }

        private AudienceDto _MapAudienceToDto(audience audienceEntity)
        {
            if (audienceEntity == null)
            {
                return null;
            }

            var audience = new AudienceDto
            {
                Id = audienceEntity.id,
                Code = audienceEntity.code,
                Name = audienceEntity.name
            };
            return audience;
        }

        private DaypartDetailDto _MapDaypartToDto(daypart daypartEntity)
        {
            if (daypartEntity == null)
            {
                return null;
            }

            var daypart = new DaypartDetailDto
            {
                Id = daypartEntity.id,
                Code = daypartEntity.code,
                Name = daypartEntity.name,
                DaypartText = daypartEntity.daypart_text
            };
            return daypart;
        }

        private SpotExceptionsOutOfSpecReasonCodeDto _MapSpotExceptionsOutOfSpecReasonCodeToDto(spot_exceptions_out_of_spec_reason_codes spotExceptionsOutOfSpecReasonCodesEntity)
        { 
            if (spotExceptionsOutOfSpecReasonCodesEntity == null)
            {
                return null;
            }

            var spotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
            {
                Id = spotExceptionsOutOfSpecReasonCodesEntity.id,
                ReasonCode = spotExceptionsOutOfSpecReasonCodesEntity.reason_code,
                Reason = spotExceptionsOutOfSpecReasonCodesEntity.reason,
                Label = spotExceptionsOutOfSpecReasonCodesEntity.label
            };
            return spotExceptionsOutOfSpecReasonCode;
        }

        /// <inheritdoc />
        public bool SaveSpotExceptionsRecommendedPlanDecision(SpotExceptionsRecommendedPlanDecisionDto spotExceptionsRecommendedPlanDecision)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var existSpotExceptionsRecommendedPlanDecision = context.spot_exceptions_recommended_plan_decision
                .SingleOrDefault(x => x.spot_exceptions_recommended_plan_details.spot_exceptions_recommended_plan_id == spotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanId);
                if (existSpotExceptionsRecommendedPlanDecision == null)
                {
                    context.spot_exceptions_recommended_plan_decision.Add(new spot_exceptions_recommended_plan_decision
                    {
                        spot_exceptions_recommended_plan_detail_id = spotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId,
                        username = spotExceptionsRecommendedPlanDecision.UserName,
                        created_at = spotExceptionsRecommendedPlanDecision.CreatedAt
                    });
                }
                else
                {
                    existSpotExceptionsRecommendedPlanDecision.spot_exceptions_recommended_plan_detail_id = spotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId;
                    existSpotExceptionsRecommendedPlanDecision.username = spotExceptionsRecommendedPlanDecision.UserName;
                    existSpotExceptionsRecommendedPlanDecision.created_at = spotExceptionsRecommendedPlanDecision.CreatedAt;
                }
                bool isSpotExceptionsRecommendedPlanDecisionSaved = context.SaveChanges() > 0;
                return isSpotExceptionsRecommendedPlanDecisionSaved;
            });
        }

        /// <inheritdoc />
        public bool UpdateRecommendedPlanOfSpotExceptionsRecommendedPlan(SpotExceptionsRecommendedPlanDetailsDto spotExceptionsRecommendedPlanDetail)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanEntity = context.spot_exceptions_recommended_plans
                .Include(x => x.spot_exceptions_recommended_plan_details)
                .SingleOrDefault(x => x.id == spotExceptionsRecommendedPlanDetail.SpotExceptionsRecommendedPlanId);
                if (spotExceptionsRecommendedPlanEntity == null)
                {
                    return false;
                }

                var spotExceptionsRecommendedPlanDetailEntity = spotExceptionsRecommendedPlanEntity.spot_exceptions_recommended_plan_details
                .SingleOrDefault(x => x.id == spotExceptionsRecommendedPlanDetail.Id);
                if (spotExceptionsRecommendedPlanDetailEntity == null)
                {
                    return false;
                }

                if (spotExceptionsRecommendedPlanEntity.recommended_plan_id == spotExceptionsRecommendedPlanDetailEntity.recommended_plan_id)
                { 
                    return true;
                }

                spotExceptionsRecommendedPlanEntity.recommended_plan_id = spotExceptionsRecommendedPlanDetailEntity.recommended_plan_id;

                bool isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated = context.SaveChanges() > 0;
                return isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated;
            });
        }

        /// <inheritdoc />
        public int AddSpotExceptionsRecommendedPlans(List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans)
        {
            // these are dummy entries until we plug in the real ingest ETL.
            var uniqueExternalId = 1;
            var executionId = Guid.NewGuid();

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlansToAdd = spotExceptionsRecommendedPlans.Select(recommendedPlan => new spot_exceptions_recommended_plans()
                {
                    estimate_id = recommendedPlan.EstimateId,
                    isci_name = recommendedPlan.IsciName,
                    recommended_plan_id = recommendedPlan.RecommendedPlanId,
                    program_name = recommendedPlan.ProgramName,
                    program_air_time = recommendedPlan.ProgramAirTime,
                    station_legacy_call_letters = recommendedPlan.StationLegacyCallLetters,
                    cost = recommendedPlan.Cost,
                    impressions = recommendedPlan.Impressions,
                    spot_length_id = recommendedPlan.SpotLengthId,
                    audience_id = recommendedPlan.AudienceId,
                    product = recommendedPlan.Product,
                    flight_start_date = recommendedPlan.FlightStartDate,
                    flight_end_date = recommendedPlan.FlightEndDate,
                    daypart_id = recommendedPlan.DaypartId,
                    ingested_by = recommendedPlan.IngestedBy,
                    ingested_at = recommendedPlan.IngestedAt,
                    unique_id_external = ++uniqueExternalId,
                    execution_id_external = executionId.ToString(),
                    spot_exceptions_recommended_plan_details = recommendedPlan.SpotExceptionsRecommendedPlanDetails.Select(recommendedPlanDetails => new spot_exceptions_recommended_plan_details()
                    {
                        recommended_plan_id = recommendedPlanDetails.RecommendedPlanId,
                        metric_percent = recommendedPlanDetails.MetricPercent,
                        is_recommended_plan = recommendedPlanDetails.IsRecommendedPlan
                    }).ToList()
                }).ToList();
                context.spot_exceptions_recommended_plans.AddRange(spotExceptionsRecommendedPlansToAdd);
                context.SaveChanges();

                var addedCount = spotExceptionsRecommendedPlans.Count();
                return addedCount;
            });
        }
        public int AddOutOfSpecs(List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs)
        {
            // these are dummy entries until we plug in the real ingest ETL.
            var uniqueExternalId = 1;
            var executionId = Guid.NewGuid();

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsToAdd = spotExceptionsOutOfSpecs.Select(outOfSpecs => new spot_exceptions_out_of_specs()
                {
                    reason_code_id = outOfSpecs.SpotExceptionsOutOfSpecReasonCode.Id,
                    reason_code_message = outOfSpecs.ReasonCodeMessage,
                    estimate_id = outOfSpecs.EstimateId,
                    isci_name = outOfSpecs.IsciName,
                    recommended_plan_id = outOfSpecs.RecommendedPlanId,
                    program_name = outOfSpecs.ProgramName,
                    station_legacy_call_letters = outOfSpecs.StationLegacyCallLetters,
                    spot_length_id = outOfSpecs.SpotLengthId,
                    audience_id = outOfSpecs.AudienceId,
                    program_network = outOfSpecs.ProgramNetwork,
                    program_air_time = outOfSpecs.ProgramAirTime,
                    ingested_by = outOfSpecs.IngestedBy,
                    ingested_at = outOfSpecs.IngestedAt,
                    created_by = outOfSpecs.CreatedBy,
                    created_at = outOfSpecs.CreatedAt,
                    modified_by = outOfSpecs.ModifiedBy,
                    modified_at = outOfSpecs.ModifiedAt,
                    execution_id_external = executionId.ToString(),
                    spot_unique_hash_external = outOfSpecs.SpotUniqueHashExternal,
                    house_isci = outOfSpecs.HouseIsci,
                    program_genre_id = outOfSpecs.ProgramGenre.Id
                }).ToList();
                context.spot_exceptions_out_of_specs.AddRange(spotExceptionsOutOfSpecsToAdd);
                context.SaveChanges();

                var addedCount = spotExceptionsOutOfSpecs.Count();
                return addedCount;
            });
        }

        /// <inheritdoc />
        public bool SaveSpotExceptionsOutOfSpecsDecisions(
            SpotExceptionsOutOfSpecDecisionsPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest,
            string userName, DateTime createdAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                bool isSpotExceptionsOutOfSpecDecisionSaved = false;
                int recordCount = 0;
                var alreadyRecordExists = context.spot_exceptions_out_of_spec_decisions.SingleOrDefault(x =>
                    x.spot_exceptions_out_of_spec_id == spotExceptionsOutOfSpecDecisionsPostsRequest.Id);
                if (alreadyRecordExists == null)
                {
                    context.spot_exceptions_out_of_spec_decisions.Add(new spot_exceptions_out_of_spec_decisions
                    {
                        spot_exceptions_out_of_spec_id = spotExceptionsOutOfSpecDecisionsPostsRequest.Id,
                        accepted_as_in_spec = spotExceptionsOutOfSpecDecisionsPostsRequest.AcceptAsInSpec,
                        decision_notes = spotExceptionsOutOfSpecDecisionsPostsRequest.DecisionNotes,
                        username = userName,
                        created_at = createdAt
                    });
                }
                else
                {
                    alreadyRecordExists.accepted_as_in_spec =
                        spotExceptionsOutOfSpecDecisionsPostsRequest.AcceptAsInSpec;
                    alreadyRecordExists.decision_notes = spotExceptionsOutOfSpecDecisionsPostsRequest.DecisionNotes;
                    alreadyRecordExists.username = userName;
                    alreadyRecordExists.created_at = createdAt;
                }

                recordCount = context.SaveChanges();
                if (recordCount > 0)
                {
                    isSpotExceptionsOutOfSpecDecisionSaved = true;
                }

                return isSpotExceptionsOutOfSpecDecisionSaved;
            });
        }

        /// <inheritdoc />
        public List<SpotExceptionsOutOfSpecReasonCodeDto> GetSpotExceptionsOutOfSpecReasonCodes()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecReasonCodesEntities = context.spot_exceptions_out_of_spec_reason_codes.ToList();

                var spotExceptionsOutOfSpecReasonCodes = spotExceptionsOutOfSpecReasonCodesEntities.Select(spotExceptionsOutOfSpecReasonCodesEntity => new SpotExceptionsOutOfSpecReasonCodeDto
                {
                    Id = spotExceptionsOutOfSpecReasonCodesEntity.id,
                    ReasonCode = spotExceptionsOutOfSpecReasonCodesEntity.reason_code,
                    Reason = spotExceptionsOutOfSpecReasonCodesEntity.reason,
                    Label = spotExceptionsOutOfSpecReasonCodesEntity.label
                }).ToList();
                return spotExceptionsOutOfSpecReasonCodes;
            });
        }

        public List<SpotExceptionUnpostedNoPlanDto> GetSpotExceptionUnpostedNoPlan(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsUnpostedNoPlanEntities = context.spot_exceptions_unposted_no_plan
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                    .Select(x => new SpotExceptionUnpostedNoPlanDto
                    {
                        HouseIsci = x.house_isci,
                        ClientIsci = x.client_isci,
                        ClientSpotLengthId = x.client_spot_length_id,
                        Count = x.count,
                        ProgramAirTime = x.program_air_time,
                        EstimateID = x.estimate_id
                    }).ToList();
                return spotExceptionsUnpostedNoPlanEntities;
            });
        }

        public List<SpotExceptionUnpostedNoReelRosterDto> GetSpotExceptionUnpostedNoReelRoster(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsUnpostedNoReelRosterEntities = context.spot_exceptions_unposted_no_reel_roster
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                    .Select(x => new SpotExceptionUnpostedNoReelRosterDto {
                        HouseIsci = x.house_isci,
                        Count = x.count,
                        ProgramAirTime = x.program_air_time,
                        EstimateId = x.estimate_id
                    }).ToList();
                return spotExceptionsUnpostedNoReelRosterEntities;
            });
        }
    }
}
