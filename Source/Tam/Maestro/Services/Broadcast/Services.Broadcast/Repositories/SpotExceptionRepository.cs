﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Microsoft.EntityFrameworkCore.Internal;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProgramMapping;
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
        /// Clear mocked data from spot exceptions tables.
        /// </summary>   
        bool ClearSpotExceptionMockData();

        /// <summary>
        /// Clear all data from spot exceptions tables.
        /// </summary>   
        bool ClearSpotExceptionAllData();

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
        /// <returns>IsSpotExceptionsRecommendedPlanDecisionSaved True if spot exception recommended plan decision saves successfully otherwise false</returns>
        bool SaveSpotExceptionsRecommendedPlanDecision(SpotExceptionsRecommendedPlanDecisionDto spotExceptionsRecommendedPlanDecision);

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
        /// Saves the Out of spec decision spots.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecDecisionsPostsRequest"></param>
        /// <param name="userName"></param>
        /// <param name="createdAt"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Save the Out of spec Decision 
        /// </summary>
        /// <param name="spotExceptionSaveDecisionsPlansRequest">The SpotExceptionsOutOfSpecDecisions Request</param>
        /// <param name="userName">User Name</param>
        /// <param name="createdAt">Created At Time</param>
        /// <returns>true or false</returns>
        bool SaveSpotExceptionsOutOfSpecsDecisionsPlans(SpotExceptionSaveDecisionsPlansRequestDto spotExceptionSaveDecisionsPlansRequest, string userName, DateTime createdAt);

        /// <summary>
        /// Sync Decision Data Assigning the SyncedAt  and SyncedBy Indicators.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">User Name </param>
        /// <param name="dateTime">Current Date time </param>
        /// <returns>Return true when decision data is synced or else returning false</returns>
        bool SyncOutOfSpecDecision(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime);

        /// <summary>
        /// Get the Queued Decision Count
        /// </summary>
        /// <returns>Count of Decision Data</returns>
        int GetDecisionQueuedCount();

        /// <summary>
        /// Sync Recommande Plan Decision Data Assigning the SyncedAt  and SyncedBy Indicators.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">User Name</param>
        /// <param name="dateTime">Current Date Time</param>
        /// <returns>true or false</returns>
        bool SyncRecommandedPlanDecision(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime);

        /// <summary>
        /// Get The Recommanded Decision Count
        /// </summary>
        /// <returns>Count of Recommanded plan Decision Data</returns>
        int GetRecommandedPlanDecisionQueuedCount();

        /// <summary>
        /// Gets the all Recommended plans Spots
        /// </summary>
        /// <param name="planId">Plan Id</param>
        /// <param name="weekStartDate">Week Start Date</param>
        /// <param name="weekEndDate">Week End Date</param>
        /// <returns></returns>
        List<SpotExceptionsRecommendedPlansDto> GetSpotExceptionRecommendedPlanSpots(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Update is recommended plan for the spot exceptions recommended plan details
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanDetail">The spot exceptoins recommended plan detail parameter</param>
        /// <returns>True if is recommended plan of pot exceptions recommended plan details updates successfully otherwise false</returns>
        bool UpdateSpotExceptionsRecommendedPlanDetails(SpotExceptionsRecommendedPlanSaveDto spotExceptionsRecommendedPlanDetail);

        /// <summary>
        /// Gets the recommended plan advertiser m ids per week.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<Guid> GetRecommendedPlanAdvertiserMasterIdsPerWeek(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the spot exceptions recommended plan stations.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<string> GetSpotExceptionsRecommendedPlanStations(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the name of the market.
        /// </summary>
        /// <param name="marketCode">The market code.</param>
        /// <returns>
        /// </returns>
        string GetMarketName(int marketCode);

        /// <summary>
        /// Gets the program Genres List.
        /// </summary>
        /// <param name="programSearchString">The program Search String.</param>
        /// <returns>
        /// </returns>
        List<ProgramNameDto> FindProgramFromPrograms(string programSearchString);

        /// <summary>
        /// Gets the program Genres List from SpotExceptionDecisions.
        /// </summary>
        /// <param name="programSearchString">The program Search String.</param>
        /// <returns>
        /// </returns>
        List<ProgramNameDto> FindProgramFromSpotExceptionDecisions(string programSearchString);
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
                var spotExceptionsRecommendedPlansToAdd = spotExceptionsRecommendedPlans.Select(recommendedPlan =>
                    new spot_exceptions_recommended_plans()
                    {
                        spot_unique_hash_external = recommendedPlan.SpotUniqueHashExternal,
                        ambiguity_code = recommendedPlan.AmbiguityCode,
                        execution_id_external = recommendedPlan.ExecutionIdExternal,
                        estimate_id = recommendedPlan.EstimateId,
                        inventory_source = recommendedPlan.InventorySource,
                        house_isci = recommendedPlan.HouseIsci,
                        client_isci = recommendedPlan.ClientIsci,
                        spot_length_id = recommendedPlan.SpotLengthId,
                        program_air_time = recommendedPlan.ProgramAirTime,
                        station_legacy_call_letters = recommendedPlan.StationLegacyCallLetters,
                        affiliate = recommendedPlan.Affiliate,
                        market_code = recommendedPlan.MarketCode,
                        market_rank = recommendedPlan.MarketRank,
                        program_name = recommendedPlan.ProgramName,
                        program_genre = recommendedPlan.ProgramGenre,
                        ingested_by = recommendedPlan.IngestedBy,
                        ingested_at = recommendedPlan.IngestedAt,
                        created_by = recommendedPlan.CreatedBy,
                        created_at = recommendedPlan.CreatedAt,
                        modified_by = recommendedPlan.ModifiedBy,
                        modified_at = recommendedPlan.ModifiedAt,
                        spot_exceptions_recommended_plan_details = recommendedPlan.SpotExceptionsRecommendedPlanDetails
                            .Select(recommendedPlanDetails =>
                            {
                                var spotExceptionsRecommendedPlanDetail = new spot_exceptions_recommended_plan_details()
                                {
                                    recommended_plan_id = recommendedPlanDetails.RecommendedPlanId,
                                    execution_trace_id = recommendedPlanDetails.ExecutionTraceId,
                                    rate = recommendedPlanDetails.Rate,
                                    audience_name = recommendedPlanDetails.AudienceName,
                                    contracted_impressions = recommendedPlanDetails.ContractedImpressions,
                                    delivered_impressions = recommendedPlanDetails.DeliveredImpressions,
                                    is_recommended_plan = recommendedPlanDetails.IsRecommendedPlan,
                                    plan_clearance_percentage = recommendedPlanDetails.PlanClearancePercentage,
                                    daypart_code = recommendedPlanDetails.DaypartCode,
                                    start_time = recommendedPlanDetails.StartTime,
                                    end_time = recommendedPlanDetails.EndTime,
                                    monday = recommendedPlanDetails.Monday,
                                    tuesday = recommendedPlanDetails.Tuesday,
                                    wednesday = recommendedPlanDetails.Wednesday,
                                    thursday = recommendedPlanDetails.Thursday,
                                    friday = recommendedPlanDetails.Friday,
                                    saturday = recommendedPlanDetails.Saturday,
                                    sunday = recommendedPlanDetails.Sunday,
                                    plan_spot_unique_hash_external = recommendedPlan.SpotUniqueHashExternal,
                                    plan_execution_id_external = recommendedPlan.ExecutionIdExternal,
                                    spot_delivered_impression = recommendedPlanDetails.SpotDeliveredImpressions,
                                    plan_total_contracted_impressions = recommendedPlanDetails.PlanTotalContractedImpressions,
                                    plan_total_delivered_impressions = recommendedPlanDetails.PlanTotalDeliveredImpressions
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
                                                    .UserName,
                                                accepted_as_in_spec = recommendedPlanDetails.SpotExceptionsRecommendedPlanDecision.AcceptedAsInSpec
                                            }
                                        };
                                }

                                ;
                                return spotExceptionsRecommendedPlanDetail;
                            }).ToList()
                    }).ToList();

                context.spot_exceptions_recommended_plans.AddRange(spotExceptionsRecommendedPlansToAdd);

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
                        impressions = outOfSpecs.Impressions,
                        house_isci = outOfSpecs.HouseIsci,
                        spot_unique_hash_external = outOfSpecs.SpotUniqueHashExternal,
                        daypart_code = outOfSpecs.DaypartCode,
                        genre_name = outOfSpecs.GenreName,
                        market_code = outOfSpecs.MarketCode,
                        market_rank = outOfSpecs.MarketRank,
                        ingested_by = outOfSpecs.IngestedBy,
                        ingested_at = outOfSpecs.IngestedAt,
                        created_by = outOfSpecs.CreatedBy,
                        created_at = outOfSpecs.CreatedAt,
                        modified_by = outOfSpecs.ModifiedBy,
                        modified_at = outOfSpecs.ModifiedAt,
                        inventory_source_name = outOfSpecs.InventorySourceName
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

        public bool ClearSpotExceptionMockData()
        {
            return _InReadUncommitedTransaction(context =>
            {
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_decision "
                                                    + "WHERE spot_exceptions_recommended_plan_detail_id IN "
                                                    + "(SELECT id FROM spot_exceptions_recommended_plan_details dts "
                                                    + "WHERE dts.spot_exceptions_recommended_plan_id IN "
                                                    + "(SELECT id FROM spot_exceptions_recommended_plans p "
                                                    + "WHERE p.ingested_by = 'Mock Data'));");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_details "
                                                    + "WHERE spot_exceptions_recommended_plan_id IN "
                                                    + "(SELECT id FROM spot_exceptions_recommended_plans p "
                                                    + "WHERE p.ingested_by = 'Mock Data');");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plans "
                                                    + "WHERE ingested_by = 'Mock Data';");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_spec_decisions "
                                                    + "WHERE spot_exceptions_out_of_spec_id IN "
                                                    + "(SELECT id FROM spot_exceptions_out_of_specs p "
                                                    + "WHERE p.ingested_by = 'Mock Data');");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs "
                                                    + "WHERE ingested_by = 'Mock Data';");
                return true;
            });
        }

        public bool ClearSpotExceptionAllData()
        {
            return _InReadUncommitedTransaction(context =>
            {
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_decision");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_details");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plans");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_spec_decisions");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_unposted_no_plan");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_unposted_no_reel_roster");
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
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan.plan_versions.Select(x => x.plan_version_dayparts))
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_lengths)
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
                DaypartCode = spotExceptionsOutOfSpecEntity.daypart_code,
                GenreName = spotExceptionsOutOfSpecEntity.genre_name,
                Affiliate = stationEntity?.affiliation,
                Market = stationEntity?.market?.geography_name,
                SpotLength = _MapSpotLengthToDto(spotExceptionsOutOfSpecEntity.spot_lengths),
                Audience = _MapAudienceToDto(spotExceptionsOutOfSpecEntity.audience),
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
                    SyncedAt = spotExceptionsOutOfSpecsDecisionDb.synced_at,
                    ProgramName = spotExceptionsOutOfSpecsDecisionDb.program_name,
                    GenreName = spotExceptionsOutOfSpecsDecisionDb?.genre_name,
                    DaypartCode = spotExceptionsOutOfSpecsDecisionDb.daypart_code
                }).SingleOrDefault(),
                SpotExceptionsOutOfSpecReasonCode = _MapSpotExceptionsOutOfSpecReasonCodeToDto(spotExceptionsOutOfSpecEntity.spot_exceptions_out_of_spec_reason_codes),
                MarketCode = spotExceptionsOutOfSpecEntity.market_code,
                MarketRank = spotExceptionsOutOfSpecEntity.market_rank,
                Comments = spotExceptionsOutOfSpecEntity.comment,
                InventorySourceName = spotExceptionsOutOfSpecEntity.inventory_source_name
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
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.spot_exceptions_recommended_plan_decision))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_lengths)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsRecommendedPlans = spotExceptionsRecommendedPlanEntities.Select(spotExceptionsRecommendedPlanEntity => _MapSpotExceptionsRecommendedPlanToDto(spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan)).ToList();
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
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_lengths)
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

                var spotExceptionsRecommendedPlan = _MapSpotExceptionsRecommendedPlanToDto(spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan);
                return spotExceptionsRecommendedPlan;
            });
        }

        private SpotExceptionsRecommendedPlansDto _MapSpotExceptionsRecommendedPlanToDto(spot_exceptions_recommended_plans spotExceptionsRecommendedPlanEntity)
        {
            var spotExceptionsRecommendedPlan = new SpotExceptionsRecommendedPlansDto
            {
                Id = spotExceptionsRecommendedPlanEntity.id,
                SpotUniqueHashExternal = spotExceptionsRecommendedPlanEntity.spot_unique_hash_external,
                AmbiguityCode = spotExceptionsRecommendedPlanEntity.ambiguity_code,
                ExecutionIdExternal = spotExceptionsRecommendedPlanEntity.execution_id_external,
                EstimateId = spotExceptionsRecommendedPlanEntity.estimate_id,
                InventorySource = spotExceptionsRecommendedPlanEntity.inventory_source,
                HouseIsci = spotExceptionsRecommendedPlanEntity.house_isci,
                ClientIsci = spotExceptionsRecommendedPlanEntity.client_isci,
                SpotLengthId = spotExceptionsRecommendedPlanEntity.spot_length_id,
                ProgramAirTime = spotExceptionsRecommendedPlanEntity.program_air_time,
                StationLegacyCallLetters = spotExceptionsRecommendedPlanEntity.station_legacy_call_letters,
                Affiliate = spotExceptionsRecommendedPlanEntity.affiliate,
                MarketCode = spotExceptionsRecommendedPlanEntity.market_code,
                MarketRank = spotExceptionsRecommendedPlanEntity.market_rank,
                ProgramName = spotExceptionsRecommendedPlanEntity.program_name,
                ProgramGenre = spotExceptionsRecommendedPlanEntity.program_genre,
                IngestedBy = spotExceptionsRecommendedPlanEntity.ingested_by,
                IngestedAt = spotExceptionsRecommendedPlanEntity.ingested_at,
                SpotLength = _MapSpotLengthToDto(spotExceptionsRecommendedPlanEntity.spot_lengths),
                SpotExceptionsRecommendedPlanDetails = spotExceptionsRecommendedPlanEntity.spot_exceptions_recommended_plan_details.Select(spotExceptionsRecommendedPlanDetailDb =>
                {
                    var recommendedPlan = spotExceptionsRecommendedPlanDetailDb.plan;
                    var recommendedPlanVersion = recommendedPlan.plan_versions.Single(planVersion => planVersion.id == recommendedPlan.latest_version_id);
                    var spotExceptionsRecommendedPlanDetail = new SpotExceptionsRecommendedPlanDetailsDto
                    {
                        Id = spotExceptionsRecommendedPlanDetailDb.id,
                        SpotExceptionsRecommendedPlanId = spotExceptionsRecommendedPlanDetailDb.spot_exceptions_recommended_plan_id,
                        RecommendedPlanId = spotExceptionsRecommendedPlanDetailDb.recommended_plan_id,
                        ExecutionTraceId = spotExceptionsRecommendedPlanDetailDb.execution_trace_id,
                        Rate = spotExceptionsRecommendedPlanDetailDb.rate,
                        AudienceName = spotExceptionsRecommendedPlanDetailDb.audience_name,
                        ContractedImpressions = spotExceptionsRecommendedPlanDetailDb.contracted_impressions,
                        DeliveredImpressions = spotExceptionsRecommendedPlanDetailDb.delivered_impressions,
                        IsRecommendedPlan = spotExceptionsRecommendedPlanDetailDb.is_recommended_plan,
                        PlanClearancePercentage = spotExceptionsRecommendedPlanDetailDb.plan_clearance_percentage,
                        DaypartCode = spotExceptionsRecommendedPlanDetailDb.daypart_code,
                        StartTime = spotExceptionsRecommendedPlanDetailDb.start_time,
                        EndTime = spotExceptionsRecommendedPlanDetailDb.end_time,
                        Monday = spotExceptionsRecommendedPlanDetailDb.monday,
                        Tuesday = spotExceptionsRecommendedPlanDetailDb.tuesday,
                        Wednesday = spotExceptionsRecommendedPlanDetailDb.wednesday,
                        Thursday = spotExceptionsRecommendedPlanDetailDb.thursday,
                        Friday = spotExceptionsRecommendedPlanDetailDb.friday,
                        Saturday = spotExceptionsRecommendedPlanDetailDb.saturday,
                        Sunday = spotExceptionsRecommendedPlanDetailDb.sunday,
                        SpotDeliveredImpressions = spotExceptionsRecommendedPlanDetailDb.spot_delivered_impression,
                        PlanTotalContractedImpressions = spotExceptionsRecommendedPlanDetailDb.plan_total_contracted_impressions,
                        PlanTotalDeliveredImpressions = spotExceptionsRecommendedPlanDetailDb.plan_total_delivered_impressions,
                        RecommendedPlanDetail = new RecommendedPlanDetailDto
                        {
                            Id = spotExceptionsRecommendedPlanDetailDb.recommended_plan_id,
                            Name = recommendedPlan.name,
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AdvertiserMasterId = recommendedPlan.campaign.advertiser_master_id,
                            ProductMasterId = recommendedPlan.product_master_id
                        },
                        SpotExceptionsRecommendedPlanDecision = spotExceptionsRecommendedPlanDetailDb.spot_exceptions_recommended_plan_decision.Select(spotExceptionsRecommendedPlanDecisionDb => new SpotExceptionsRecommendedPlanDecisionDto
                        {
                            Id = spotExceptionsRecommendedPlanDecisionDb.id,
                            SpotExceptionsRecommendedPlanDetailId = spotExceptionsRecommendedPlanDecisionDb.spot_exceptions_recommended_plan_detail_id,
                            UserName = spotExceptionsRecommendedPlanDecisionDb.username,
                            CreatedAt = spotExceptionsRecommendedPlanDecisionDb.created_at,
                            SyncedBy = spotExceptionsRecommendedPlanDecisionDb.synced_by,
                            SyncedAt = spotExceptionsRecommendedPlanDecisionDb.synced_at,
                            AcceptedAsInSpec = spotExceptionsRecommendedPlanDecisionDb.accepted_as_in_spec
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

        /// <summary>
        /// Saves spot exception recommended plan decision
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanDecision">The spot exceptions recommended plan decision parameters</param>
        /// <returns>IsSpotExceptionsRecommendedPlanDecisionSaved True if spot exception recommended plan decision saves successfully otherwise false</returns>
        public bool SaveSpotExceptionsRecommendedPlanDecision(SpotExceptionsRecommendedPlanDecisionDto spotExceptionsRecommendedPlanDecision)
        {
            return _InReadUncommitedTransaction(context =>
            {
                bool isSpotExceptionsRecommendedPlanDecisionSaved = false;
                var spotExceptionsRecommendedPlanDetails = context.spot_exceptions_recommended_plan_details
                .Where(x => x.spot_exceptions_recommended_plan_id == spotExceptionsRecommendedPlanDecision.SpotExceptionsId).ToList();
                if (spotExceptionsRecommendedPlanDetails != null && spotExceptionsRecommendedPlanDetails.Count() > 0)
                {
                    spotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId = spotExceptionsRecommendedPlanDetails
                       .FirstOrDefault(x => x.spot_exceptions_recommended_plan_id == spotExceptionsRecommendedPlanDecision.SpotExceptionsId
                       && x.recommended_plan_id == spotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanId).id;
                    List<int> planDetailIds = spotExceptionsRecommendedPlanDetails.Select(x => x.id).ToList();
                    var existSpotExceptionsRecommendedPlanDecision = context.spot_exceptions_recommended_plan_decision.
                    Where(x => planDetailIds.Contains(x.spot_exceptions_recommended_plan_detail_id)).FirstOrDefault();
                    if (existSpotExceptionsRecommendedPlanDecision == null)
                    {
                        context.spot_exceptions_recommended_plan_decision.Add(new spot_exceptions_recommended_plan_decision
                        {
                            spot_exceptions_recommended_plan_detail_id = spotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId,
                            username = spotExceptionsRecommendedPlanDecision.UserName,
                            created_at = spotExceptionsRecommendedPlanDecision.CreatedAt,
                            accepted_as_in_spec = spotExceptionsRecommendedPlanDecision.AcceptedAsInSpec,
                            synced_by = spotExceptionsRecommendedPlanDecision.SyncedBy,
                            synced_at = spotExceptionsRecommendedPlanDecision.SyncedAt
                        });
                    }
                    else
                    {
                        existSpotExceptionsRecommendedPlanDecision.spot_exceptions_recommended_plan_detail_id = spotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId;
                        existSpotExceptionsRecommendedPlanDecision.username = spotExceptionsRecommendedPlanDecision.UserName;
                        existSpotExceptionsRecommendedPlanDecision.created_at = spotExceptionsRecommendedPlanDecision.CreatedAt;
                        existSpotExceptionsRecommendedPlanDecision.accepted_as_in_spec = spotExceptionsRecommendedPlanDecision.AcceptedAsInSpec;
                        existSpotExceptionsRecommendedPlanDecision.synced_at = spotExceptionsRecommendedPlanDecision.SyncedAt;
                        existSpotExceptionsRecommendedPlanDecision.synced_by = spotExceptionsRecommendedPlanDecision.SyncedBy;
                    }
                    isSpotExceptionsRecommendedPlanDecisionSaved = context.SaveChanges() > 0;
                }
                return isSpotExceptionsRecommendedPlanDecisionSaved;
            });
        }

        /// <inheritdoc />
        public int AddSpotExceptionsRecommendedPlans(List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlansToAdd = spotExceptionsRecommendedPlans.Select(recommendedPlan =>
                    new spot_exceptions_recommended_plans()
                    {
                        spot_unique_hash_external = recommendedPlan.SpotUniqueHashExternal,
                        ambiguity_code = recommendedPlan.AmbiguityCode,
                        execution_id_external = recommendedPlan.ExecutionIdExternal,
                        estimate_id = recommendedPlan.EstimateId,
                        inventory_source = recommendedPlan.InventorySource,
                        house_isci = recommendedPlan.HouseIsci,
                        client_isci = recommendedPlan.ClientIsci,
                        spot_length_id = recommendedPlan.SpotLengthId,
                        program_air_time = recommendedPlan.ProgramAirTime,
                        station_legacy_call_letters = recommendedPlan.StationLegacyCallLetters,
                        affiliate = recommendedPlan.Affiliate,
                        market_code = recommendedPlan.MarketCode,
                        market_rank = recommendedPlan.MarketRank,
                        program_name = recommendedPlan.ProgramName,
                        program_genre = recommendedPlan.ProgramGenre,
                        ingested_by = recommendedPlan.IngestedBy,
                        ingested_at = recommendedPlan.IngestedAt,
                        created_by = recommendedPlan.CreatedBy,
                        created_at = recommendedPlan.CreatedAt,
                        modified_by = recommendedPlan.ModifiedBy,
                        modified_at = recommendedPlan.ModifiedAt,
                        spot_exceptions_recommended_plan_details = recommendedPlan.SpotExceptionsRecommendedPlanDetails
                            .Select(recommendedPlanDetails =>
                            {
                                var spotExceptionsRecommendedPlanDetail = new spot_exceptions_recommended_plan_details()
                                {
                                    recommended_plan_id = recommendedPlanDetails.RecommendedPlanId,
                                    execution_trace_id = recommendedPlanDetails.ExecutionTraceId,
                                    rate = recommendedPlanDetails.Rate,
                                    audience_name = recommendedPlanDetails.AudienceName,
                                    contracted_impressions = recommendedPlanDetails.ContractedImpressions,
                                    delivered_impressions = recommendedPlanDetails.DeliveredImpressions,
                                    is_recommended_plan = recommendedPlanDetails.IsRecommendedPlan,
                                    plan_clearance_percentage = recommendedPlanDetails.PlanClearancePercentage,
                                    daypart_code = recommendedPlanDetails.DaypartCode,
                                    start_time = recommendedPlanDetails.StartTime,
                                    end_time = recommendedPlanDetails.EndTime,
                                    monday = recommendedPlanDetails.Monday,
                                    tuesday = recommendedPlanDetails.Tuesday,
                                    wednesday = recommendedPlanDetails.Wednesday,
                                    thursday = recommendedPlanDetails.Thursday,
                                    friday = recommendedPlanDetails.Friday,
                                    saturday = recommendedPlanDetails.Saturday,
                                    sunday = recommendedPlanDetails.Sunday,
                                    plan_spot_unique_hash_external = recommendedPlan.SpotUniqueHashExternal,
                                    plan_execution_id_external = recommendedPlan.ExecutionIdExternal
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
                                                    .UserName,
                                                accepted_as_in_spec = recommendedPlanDetails.SpotExceptionsRecommendedPlanDecision.AcceptedAsInSpec
                                            }
                                        };
                                };
                                return spotExceptionsRecommendedPlanDetail;
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
                    inventory_source_name = outOfSpecs.InventorySourceName
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
                    .Select(x => new SpotExceptionUnpostedNoReelRosterDto
                    {
                        HouseIsci = x.house_isci,
                        Count = x.count,
                        ProgramAirTime = x.program_air_time,
                        EstimateId = x.estimate_id
                    }).ToList();
                return spotExceptionsUnpostedNoReelRosterEntities;
            });
        }

        public bool SaveSpotExceptionsOutOfSpecsDecisionsPlans(
           SpotExceptionSaveDecisionsPlansRequestDto spotExceptionSaveDecisionsPlansRequest,
            string userName, DateTime createdAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                foreach (var spotExceptionsOutOfSpecId in spotExceptionSaveDecisionsPlansRequest.Decisions)
                {
                    var alreadyRecordExists = context.spot_exceptions_out_of_spec_decisions.SingleOrDefault(x =>
                        x.spot_exceptions_out_of_spec_id == spotExceptionsOutOfSpecId.Id);
                    var outOfSpecId = context.spot_exceptions_out_of_specs.SingleOrDefault(x =>
                        x.id == spotExceptionsOutOfSpecId.Id);
                    if (!(string.IsNullOrEmpty(spotExceptionsOutOfSpecId.ProgramName) && string.IsNullOrEmpty(spotExceptionsOutOfSpecId.GenreName) && string.IsNullOrEmpty(spotExceptionsOutOfSpecId.DaypartCode)))
                    {
                        if (alreadyRecordExists == null)
                        {
                            context.spot_exceptions_out_of_spec_decisions.Add(new spot_exceptions_out_of_spec_decisions
                            {
                                spot_exceptions_out_of_spec_id = spotExceptionsOutOfSpecId.Id,
                                accepted_as_in_spec = spotExceptionsOutOfSpecId.AcceptAsInSpec,
                                decision_notes = spotExceptionsOutOfSpecId.AcceptAsInSpec ? "In" : "Out",
                                username = userName,
                                created_at = createdAt,
                                program_name = spotExceptionsOutOfSpecId.ProgramName,
                                genre_name = spotExceptionsOutOfSpecId.GenreName,
                                daypart_code = spotExceptionsOutOfSpecId.DaypartCode
                            });
                        }
                        else
                        {
                            alreadyRecordExists.accepted_as_in_spec = spotExceptionsOutOfSpecId.AcceptAsInSpec;
                            alreadyRecordExists.username = userName;
                            alreadyRecordExists.created_at = createdAt;
                            alreadyRecordExists.decision_notes = spotExceptionsOutOfSpecId.AcceptAsInSpec ? "In" : "Out";
                            alreadyRecordExists.synced_at = null;
                            alreadyRecordExists.synced_by = null;
                            alreadyRecordExists.program_name = spotExceptionsOutOfSpecId.ProgramName;
                            alreadyRecordExists.genre_name = spotExceptionsOutOfSpecId.GenreName;
                            alreadyRecordExists.daypart_code = spotExceptionsOutOfSpecId.DaypartCode;
                        }
                    }
                    else if (spotExceptionsOutOfSpecId.Comments == null)
                    {
                        if (alreadyRecordExists == null)
                        {
                            context.spot_exceptions_out_of_spec_decisions.Add(new spot_exceptions_out_of_spec_decisions
                            {
                                spot_exceptions_out_of_spec_id = spotExceptionsOutOfSpecId.Id,
                                accepted_as_in_spec = spotExceptionsOutOfSpecId.AcceptAsInSpec,
                                decision_notes = spotExceptionsOutOfSpecId.AcceptAsInSpec ? "In" : "Out",
                                username = userName,
                                created_at = createdAt
                            });
                        }
                        else
                        {
                            alreadyRecordExists.accepted_as_in_spec = spotExceptionsOutOfSpecId.AcceptAsInSpec;
                            alreadyRecordExists.username = userName;
                            alreadyRecordExists.created_at = createdAt;
                            alreadyRecordExists.decision_notes = spotExceptionsOutOfSpecId.AcceptAsInSpec ? "In" : "Out";
                            alreadyRecordExists.synced_at = null;
                            alreadyRecordExists.synced_by = null;
                        }
                    }
                    else
                    {
                        outOfSpecId.comment = spotExceptionsOutOfSpecId.Comments;
                    }
                }

                bool isSpotExceptionsOutOfSpecDecisionSaved = false;
                int recordCount = 0;
                recordCount = context.SaveChanges();
                if (recordCount > 0)
                {
                    isSpotExceptionsOutOfSpecDecisionSaved = true;
                }

                return isSpotExceptionsOutOfSpecDecisionSaved;
            });
        }

        /// <inheritdoc />
        public bool SyncOutOfSpecDecision(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsDecisionsEntities = context.spot_exceptions_out_of_spec_decisions
                    .Where(spotExceptionsDecisionDb => spotExceptionsDecisionDb.synced_at == null).ToList();

                if (spotExceptionsDecisionsEntities?.Any() ?? false)
                {
                    spotExceptionsDecisionsEntities.ForEach(x => { x.synced_by = triggerDecisionSyncRequest.UserName; x.synced_at = dateTime; });
                }

                bool isSpotExceptionsOutOfSpecDecisionSynced = false;
                int recordCount = context.SaveChanges();
                isSpotExceptionsOutOfSpecDecisionSynced = recordCount > 0;
                return isSpotExceptionsOutOfSpecDecisionSynced;
            });
        }

        /// <inheritdoc />
        public int GetDecisionQueuedCount()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var OutOfSpecDecisionCount = context.spot_exceptions_out_of_spec_decisions
                  .Where(x => x.synced_at == null)
                  .Count();
                return OutOfSpecDecisionCount;
            });
        }

        /// <inheritdoc />
        public bool SyncRecommandedPlanDecision(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime)
        {
            return _InReadUncommitedTransaction(context =>
            {

                var spotExceptionsRecommandedDecisionsEntities = context.spot_exceptions_recommended_plan_decision
                    .Where(spotExceptionsDecisionDb => spotExceptionsDecisionDb.synced_at == null).ToList();

                if (spotExceptionsRecommandedDecisionsEntities?.Any() ?? false)
                {
                    spotExceptionsRecommandedDecisionsEntities.ForEach(x => { x.synced_by = triggerDecisionSyncRequest.UserName; x.synced_at = dateTime; });
                }

                bool isSpotExceptionsRecommandedPlanDecisionSynced = false;
                int recordCount = context.SaveChanges();
                isSpotExceptionsRecommandedPlanDecisionSynced = recordCount > 0;
                return isSpotExceptionsRecommandedPlanDecisionSynced;
            });
        }

        /// <inheritdoc />
        public int GetRecommandedPlanDecisionQueuedCount()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var recommandedPlansDecisionCount = context.spot_exceptions_recommended_plan_decision
                  .Where(x => x.synced_at == null)
                  .Count();
                return recommandedPlansDecisionCount;
            });
        }

        private SpotExceptionsOutOfSpecGenreDto _MapToDto(genre genre)
        {
            return new SpotExceptionsOutOfSpecGenreDto
            {
                GenreName = genre.name,
                Id = genre.id
            };
        }

        /// <inheritdoc />
        public List<SpotExceptionsRecommendedPlansDto> GetSpotExceptionRecommendedPlanSpots(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanEntities = context.spot_exceptions_recommended_plans
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details).Where(p => p.spot_exceptions_recommended_plan_details.Any(x => x.recommended_plan_id == planId))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.spot_exceptions_recommended_plan_decision))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_lengths)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsRecommendedPlans = spotExceptionsRecommendedPlanEntities.Select(spotExceptionsRecommendedPlanEntity => _MapSpotExceptionsRecommendedPlanToDto(spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan)).ToList();
                return spotExceptionsRecommendedPlans;
            });
        }

        /// <inheritdoc />
        public bool UpdateSpotExceptionsRecommendedPlanDetails(SpotExceptionsRecommendedPlanSaveDto spotExceptionsRecommendedPlanDetail)
        {
            return _InReadUncommitedTransaction(context =>
            {

                var spotExceptionsRecommendedPlanDetailList = context.spot_exceptions_recommended_plan_details.Where(x => x.spot_exceptions_recommended_plan_id == spotExceptionsRecommendedPlanDetail.Id).ToList();
                if (spotExceptionsRecommendedPlanDetailList != null)
                {
                    foreach (var spotExceptionsRecommendedPlan in spotExceptionsRecommendedPlanDetailList)
                    {
                        if(spotExceptionsRecommendedPlan.spot_exceptions_recommended_plan_id == spotExceptionsRecommendedPlanDetail.Id && spotExceptionsRecommendedPlan.recommended_plan_id == spotExceptionsRecommendedPlanDetail.SelectedPlanId)
                        {
                            spotExceptionsRecommendedPlan.is_recommended_plan = true;
                        }
                        else
                        {
                            spotExceptionsRecommendedPlan.is_recommended_plan = false;
                        }
                    }
                }
                bool isSpotExceptionsRecommendedPlanDetailUpdated = context.SaveChanges() > 0;
                return isSpotExceptionsRecommendedPlanDetailUpdated;
            });
        }

        /// <inheritdoc />
        public List<Guid> GetRecommendedPlanAdvertiserMasterIdsPerWeek(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {

                var recommendedPlanAdvertiserMasterIdsPerWeek = context.spot_exceptions_recommended_plans
                                                .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                                                .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details)
                                                .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                                                .Select(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.campaign).Select(p => p.advertiser_master_id ?? default).FirstOrDefault()).ToList();
                return recommendedPlanAdvertiserMasterIdsPerWeek;
            });
        }

        /// <inheritdoc />
        public List<string> GetSpotExceptionsRecommendedPlanStations(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var recommendedPlanStations = context.spot_exceptions_recommended_plans
                                            .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                                            .Select(rps => rps.station_legacy_call_letters ?? "Unknown")
                                            .ToList();
                return recommendedPlanStations;
            });
        }

        /// <inheritdoc />
        public string GetMarketName(int marketCode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var marketName = context.markets
                                .Where(m => m.market_code == marketCode)
                                .Select(m => m.geography_name)
                                .Single();
                return marketName;
            });
        }
        /// <inheritdoc />
        public List<ProgramNameDto> FindProgramFromPrograms(string programSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.programs
                        .Where(p => p.name.ToLower().Contains(programSearchString.ToLower()))
                        .OrderBy(p => p.name)
                        .Select(
                            p => new ProgramNameDto
                            {
                                OfficialProgramName = p.name,
                                GenreId = p.genre_id
                            }).ToList();
                });
        }
        /// <inheritdoc />
        public List<ProgramNameDto> FindProgramFromSpotExceptionDecisions(string programSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.spot_exceptions_out_of_spec_decisions
                        .Where(p => p.program_name.ToLower().Contains(programSearchString.ToLower()))
                        .OrderBy(p => p.program_name)
                        .Select(
                            p => new ProgramNameDto
                            {
                                OfficialProgramName = p.program_name,
                                GenreId = context.genres.FirstOrDefault(x => x.name == p.genre_name).id
                            }).ToList();
                });
        }
    }
}
