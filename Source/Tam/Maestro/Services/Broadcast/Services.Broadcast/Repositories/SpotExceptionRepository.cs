using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
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
                   spot_length_id = recommendedPlan.SpotLenthId,
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
                   spot_length_id = outOfSpecs.SpotLenthId,
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
                       spot_exceptions_out_of_spec_id = outOfSpecsDecision.SpotExceptionsRecommendedPlanId,
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
    }
}
