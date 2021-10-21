using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        ///  <param name="spotExceptionsRecommendedPlanDetails">Mock data collection for ExceptionsRecommendedPlanDetails table.</param>
        ///  <param name="spotExceptionsRecommendedPlanDecision">Mock data collection for ExceptionsRecommendedPlanDecision table.</param>
        ///  <param name="spotExceptionsOutOfSpecs">Mock data collection for ExceptionsOutOfSpecs table.</param>
        ///  <param name="spotExceptionsOutOfSpecDecisions">Mock data collection for ExceptionsOutOfSpecDecisions table.</param>
        bool AddSpotExceptionData(List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans
             , List<SpotExceptionsRecommendedPlanDetailsDto> spotExceptionsRecommendedPlanDetails
             , List<SpotExceptionsRecommendedPlanDecisionDto> spotExceptionsRecommendedPlanDecision,
             List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs,
             List<SpotExceptionsOutOfSpecDecisionsDto> spotExceptionsOutOfSpecDecisions);
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
        /// Gets spot exceptions recommended plans
        /// </summary>
        /// <param name="weekStartDate">The week start date</param>
        /// <param name="weekEndDate">The week end date</param>
        /// <returns>The spot exceptions recommended plans</returns>
        List<SpotExceptionsRecommendedPlansDto> GetSpotExceptionsRecommendedPlans(DateTime weekStartDate, DateTime weekEndDate);

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
    }

    public class SpotExceptionRepository : BroadcastRepositoryBase, ISpotExceptionRepository
    {
        public SpotExceptionRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public bool AddSpotExceptionData(List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans
            , List<SpotExceptionsRecommendedPlanDetailsDto> spotExceptionsRecommendedPlanDetails
            , List<SpotExceptionsRecommendedPlanDecisionDto> spotExceptionsRecommendedPlanDecision,
            List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs,
            List<SpotExceptionsOutOfSpecDecisionsDto> spotExceptionsOutOfSpecDecisions)
        {
            _InReadUncommitedTransaction(context =>
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
                   spot_exceptions_recommended_plan_details = spotExceptionsRecommendedPlanDetails.Select(recommendedPlanDetails => new spot_exceptions_recommended_plan_details()
                   {
                       spot_exceptions_recommended_plan_id = recommendedPlanDetails.SpotExceptionsRecommendedPlanId,
                       recommended_plan_id = recommendedPlanDetails.RecommendedPlanId,
                       metric_percent = recommendedPlanDetails.MetricPercent,
                       is_recommended_plan = recommendedPlanDetails.IsRecommendedPlan,
                       spot_exceptions_recommended_plan_decision = spotExceptionsRecommendedPlanDecision.Select(recommendedPlanDecision => new spot_exceptions_recommended_plan_decision()
                       {
                           spot_exceptions_recommended_plan_detail_id = recommendedPlanDecision.SelectedDetailsId,
                           created_at = recommendedPlanDecision.CreatedAt,
                           username = recommendedPlanDecision.UserName
                       }).ToList()
                   }).ToList()
               }).ToList();
               context.spot_exceptions_recommended_plans.AddRange(spotExceptionsRecommendedPlansToAdd);

               var spotExceptionsOutOfSpecsToAdd = spotExceptionsOutOfSpecs.Select(outOfSpecs => new spot_exceptions_out_of_specs()
               {
                   reason_code = outOfSpecs.ReasonCode,
                   reason_code_message = outOfSpecs.ReasonCodeMessage,
                   estimate_id = outOfSpecs.EstimateId,
                   isci_name = outOfSpecs.IsciName,
                   recommended_plan_id = outOfSpecs.RecommendedPlanId,
                   program_name = outOfSpecs.ProgramName,
                   station_legacy_call_letters = outOfSpecs.StationLegacyCallLetters,
                   spot_length_id = outOfSpecs.SpotLengthId,
                   audience_id = outOfSpecs.AudienceId,
                   product = outOfSpecs.Product,
                   flight_start_date = outOfSpecs.FlightStartDate,
                   flight_end_date = outOfSpecs.FlightEndDate,
                   program_flight_start_date = outOfSpecs.ProgramFlightStartDate,
                   program_flight_end_date = outOfSpecs.ProgramFlightEndDate,
                   program_network = outOfSpecs.ProgramNetwork,
                   program_audience_id = outOfSpecs.ProgramAudienceId,
                   program_air_time = outOfSpecs.ProgramAirTime,
                   program_daypart_id = outOfSpecs.ProgramDaypartId,
                   ingested_by = outOfSpecs.IngestedBy,
                   ingested_at = outOfSpecs.IngestedAt,
                   spot_exceptions_out_of_spec_decisions = spotExceptionsOutOfSpecDecisions.Select(outOfSpecsDecision => new spot_exceptions_out_of_spec_decisions()
                   {
                       spot_exceptions_out_of_spec_id = outOfSpecsDecision.SpotExceptionsOutOfSpecId,
                       accepted_as_in_spec = outOfSpecsDecision.AcceptedAsInSpec,
                       decision_notes = outOfSpecsDecision.DecisionNotes,
                       created_at = outOfSpecsDecision.CreatedAt,
                       username = outOfSpecsDecision.UserName
                   }).ToList()
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
                    .Where(spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.program_air_time >= weekStartDate && spotExceptionsoutOfSpec.program_air_time <= weekEndDate)
                    .Include(spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.spot_exceptions_out_of_spec_decisions)
                    .Include(spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.plan)
                    .Include(spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.spot_lengths)
                    .Include(spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.daypart)
                    //the reason daypart1 is two foreign key referring to same daypart table
                    .Include(spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.daypart1)
                    .Include(spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.audience)
                    //the reason audience1 is two foreign key referring to same audience table
                    .Include(spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.audience1)
                    .GroupJoin(
                        context.stations
                        .Include(station => station.market),
                        spotExceptionsoutOfSpec => spotExceptionsoutOfSpec.station_legacy_call_letters,
                        station => station.legacy_call_letters,
                        (spotExceptionsoutOfSpec, station) => new { SpotExceptionsoutOfSpec = spotExceptionsoutOfSpec, Station = station.FirstOrDefault() })
                    .ToList();

                var spotExceptionsOutOfSpecs = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity =>
                {
                    var spotExceptionsOutSpecDecison = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.spot_exceptions_out_of_spec_decisions.FirstOrDefault();

                    var spotExceptionsOutOfSpec = new SpotExceptionsOutOfSpecsDto
                    {
                        Id = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.id,
                        ReasonCode = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.reason_code,
                        ReasonCodeMessage = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.reason_code_message,
                        EstimateId = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.estimate_id,
                        IsciName = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.isci_name,
                        RecommendedPlanId = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.recommended_plan_id,
                        RecommendedPlanName = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.plan?.name,
                        ProgramName = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.program_name,
                        StationLegacyCallLetters = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.station_legacy_call_letters,
                        Affiliate = spotExceptionsOutOfSpecEntity.Station?.affiliation,
                        Market = spotExceptionsOutOfSpecEntity.Station?.market?.geography_name,
                        SpotLengthId = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.spot_length_id,
                        SpotLengthString = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.spot_lengths.length.ToString(),
                        AudienceId = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.program_audience_id,                                         
                        Product = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.product,
                        FlightStartDate = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.flight_start_date,
                        FlightEndDate = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.flight_end_date,                
                        DaypartId = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.daypart_id,
                        ProgramDaypartId = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.program_daypart_id,
                        DaypartCode = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.daypart1.code,
                        ProgramFlightStartDate = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.program_flight_start_date,
                        ProgramFlightEndDate = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.program_flight_end_date,
                        ProgramAudienceId = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.program_audience_id,
                        AudienceName = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.audience1.name,
                        ProgramAirTime = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.program_air_time,
                        IngestedBy = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.ingested_by,
                        IngestedAt = spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.ingested_at,
                        SpotExceptionsOutOfSpecId = spotExceptionsOutSpecDecison?.id,                        
                    };
                    return spotExceptionsOutOfSpec;
                }).ToList();

                return spotExceptionsOutOfSpecs;
            });
        }

        /// <inheritdoc />
        public List<SpotExceptionsRecommendedPlansDto> GetSpotExceptionsRecommendedPlans(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanEntities = context.spot_exceptions_recommended_plans
                    .Where(spotExceptionsRecommendedPlan => spotExceptionsRecommendedPlan.program_air_time >= weekStartDate && spotExceptionsRecommendedPlan.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlan => spotExceptionsRecommendedPlan.spot_exceptions_recommended_plan_details)
                    .Include(spotExceptionsRecommendedPlan => spotExceptionsRecommendedPlan.spot_exceptions_recommended_plan_details.Select(y => y.spot_exceptions_recommended_plan_decision))
                    .Include(spotExceptionsRecommendedPlan => spotExceptionsRecommendedPlan.plan)
                    .Include(spotExceptionsRecommendedPlan => spotExceptionsRecommendedPlan.spot_lengths)
                    .Include(spotExceptionsRecommendedPlan => spotExceptionsRecommendedPlan.daypart)
                    .Include(spotExceptionsRecommendedPlan => spotExceptionsRecommendedPlan.audience)
                    .GroupJoin(
                        context.stations
                        .Include(station => station.market),
                        spotExceptionsRecommendedPlan => spotExceptionsRecommendedPlan.station_legacy_call_letters,
                        station => station.legacy_call_letters,
                        (spotExceptionsRecommendedPlan, station) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlan, Station = station.FirstOrDefault() })
                    .ToList();

                var spotExceptionsRecommendedPlans = spotExceptionsRecommendedPlanEntities.Select(spotExceptionsRecommendedPlanEntity =>
                {
                    var spotExceptionsRecommendedPlanDetails = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.spot_exceptions_recommended_plan_details.FirstOrDefault(x => x.is_recommended_plan);
                    var spotExceptionsRecommendedPlanDecision = spotExceptionsRecommendedPlanDetails?.spot_exceptions_recommended_plan_decision.FirstOrDefault(x => x.spot_exceptions_recommended_plan_detail_id == spotExceptionsRecommendedPlanDetails.id);

                    var spotExceptionsRecommendedPlan = new SpotExceptionsRecommendedPlansDto
                    {
                        Id = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.id,
                        EstimateId = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.estimate_id,
                        IsciName = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.isci_name,
                        RecommendedPlanId = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.recommended_plan_id,
                        RecommendedPlanName = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.plan?.name,
                        ProgramName = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.program_name,
                        ProgramAirTime = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.program_air_time,
                        StationLegacyCallLetters = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.station_legacy_call_letters,
                        Affiliate = spotExceptionsRecommendedPlanEntity.Station?.affiliation,
                        Market = spotExceptionsRecommendedPlanEntity.Station?.market?.geography_name,
                        Cost = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.cost,
                        Impressions = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.impressions,
                        SpotLengthId = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.spot_length_id,
                        SpotLengthString = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.spot_lengths.length.ToString(),
                        AudienceId = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.audience_id,
                        AudienceName = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.audience.name,
                        Product = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.product,
                        FlightStartDate = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.flight_start_date,
                        FlightEndDate = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.flight_end_date,
                        DaypartId = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.daypart_id,
                        IngestedAt = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.ingested_at,
                        IngestedBy = spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan.ingested_by,
                        SpotExceptionsRecommendedPlanDecisionId = spotExceptionsRecommendedPlanDecision?.id
                    };
                    return spotExceptionsRecommendedPlan;
                }).ToList();

                return spotExceptionsRecommendedPlans;
            });
        }

        /// <inheritdoc />
        public int AddSpotExceptionsRecommendedPlans(List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans)
        {
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
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsToAdd = spotExceptionsOutOfSpecs.Select(outOfSpecs => new spot_exceptions_out_of_specs()
                {
                    reason_code = outOfSpecs.ReasonCode,
                    reason_code_message = outOfSpecs.ReasonCodeMessage,
                    estimate_id = outOfSpecs.EstimateId,
                    isci_name = outOfSpecs.IsciName,
                    recommended_plan_id = outOfSpecs.RecommendedPlanId,
                    program_name = outOfSpecs.ProgramName,
                    station_legacy_call_letters = outOfSpecs.StationLegacyCallLetters,
                    spot_length_id = outOfSpecs.SpotLengthId,
                    audience_id = outOfSpecs.AudienceId,
                    product = outOfSpecs.Product,
                    flight_start_date = outOfSpecs.FlightStartDate,
                    flight_end_date = outOfSpecs.FlightEndDate,
                    program_flight_start_date = outOfSpecs.ProgramFlightStartDate,
                    program_flight_end_date = outOfSpecs.ProgramFlightEndDate,
                    program_network = outOfSpecs.ProgramNetwork,
                    program_audience_id = outOfSpecs.ProgramAudienceId,
                    program_air_time = outOfSpecs.ProgramAirTime,
                    program_daypart_id = outOfSpecs.ProgramDaypartId,
                    ingested_by = outOfSpecs.IngestedBy,
                    ingested_at = outOfSpecs.IngestedAt,    
                }).ToList();
                context.spot_exceptions_out_of_specs.AddRange(spotExceptionsOutOfSpecsToAdd);
                context.SaveChanges();

                var addedCount = spotExceptionsOutOfSpecs.Count();
                return addedCount;
            });
        }

        /// <inheritdoc />
        public bool SaveSpotExceptionsOutOfSpecsDecisions(SpotExceptionsOutOfSpecDecisionsPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest, string userName, DateTime createdAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                bool isSpotExceptionsOutOfSpecDecisionSaved = false;
                int recordCount = 0;
                var alreadyRecordExists = context.spot_exceptions_out_of_spec_decisions.SingleOrDefault(x => x.spot_exceptions_out_of_spec_id == spotExceptionsOutOfSpecDecisionsPostsRequest.Id);
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
                    alreadyRecordExists.accepted_as_in_spec = spotExceptionsOutOfSpecDecisionsPostsRequest.AcceptAsInSpec;
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
    }
}
