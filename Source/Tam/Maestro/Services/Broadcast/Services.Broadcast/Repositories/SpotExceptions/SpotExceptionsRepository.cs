using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsRepository : IDataRepository
    {
        bool AddSpotExceptionData(List<SpotExceptionsRecommendedPlansToDoDto> spotExceptionsRecommendedPlansToDo, List<SpotExceptionsRecommendedPlansDoneDto> spotExceptionsRecommendedPlansDone,
             List<SpotExceptionsOutOfSpecsToDoDto> spotExceptionsOutOfSpecsToDo, List<SpotExceptionsOutOfSpecsDoneDto> spotExceptionsOutOfSpecsDone);

  
        bool ClearSpotExceptionMockData();

  
        bool ClearSpotExceptionAllData();

        Task<bool> SyncOutOfSpecDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime);

        Task<bool> SyncRecommendedPlanDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime);

        Task<int> GetOutOfSpecDecisionQueuedCountAsync();

        Task<int> GetRecommendedPlanDecisionQueuedCountAsync();
    }

    /// <inheritdoc />
    public class SpotExceptionsRepository : BroadcastRepositoryBase, ISpotExceptionsRepository
    {
        public SpotExceptionsRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        public bool AddSpotExceptionData(List<SpotExceptionsRecommendedPlansToDoDto> spotExceptionsRecommendedPlansToDo, 
            List<SpotExceptionsRecommendedPlansDoneDto> spotExceptionsRecommendedPlansDone,
             List<SpotExceptionsOutOfSpecsToDoDto> spotExceptionsOutOfSpecsToDo, List<SpotExceptionsOutOfSpecsDoneDto> spotExceptionsOutOfSpecsDone)
        {
            var executionId = Guid.NewGuid();

            _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlansToDoToAdd = spotExceptionsRecommendedPlansToDo.Select(recommendedPlanToDo =>
                    new spot_exceptions_recommended_plans()
                    {
                        spot_unique_hash_external = recommendedPlanToDo.SpotUniqueHashExternal,
                        ambiguity_code = recommendedPlanToDo.AmbiguityCode,
                        execution_id_external = recommendedPlanToDo.ExecutionIdExternal,
                        estimate_id = recommendedPlanToDo.EstimateId,
                        inventory_source = recommendedPlanToDo.InventorySource,
                        house_isci = recommendedPlanToDo.HouseIsci,
                        client_isci = recommendedPlanToDo.ClientIsci,
                        spot_length_id = recommendedPlanToDo.SpotLengthId,
                        program_air_time = recommendedPlanToDo.ProgramAirTime,
                        station_legacy_call_letters = recommendedPlanToDo.StationLegacyCallLetters,
                        affiliate = recommendedPlanToDo.Affiliate,
                        market_code = recommendedPlanToDo.MarketCode,
                        market_rank = recommendedPlanToDo.MarketRank,
                        program_name = recommendedPlanToDo.ProgramName,
                        program_genre = recommendedPlanToDo.ProgramGenre,
                        ingested_by = recommendedPlanToDo.IngestedBy,
                        ingested_at = recommendedPlanToDo.IngestedAt,
                        ingested_media_week_id = recommendedPlanToDo.IngestedMediaWeekId,
                        spot_exceptions_recommended_plan_details = recommendedPlanToDo.SpotExceptionsRecommendedPlanDetailsToDo
                            .Select(recommendedPlanToDoDetails =>
                            {
                                var spotExceptionsRecommendedPlanToDoDetail = new spot_exceptions_recommended_plan_details()
                                {
                                    spot_exceptions_recommended_plan_id = recommendedPlanToDo.Id,
                                    recommended_plan_id = recommendedPlanToDoDetails.RecommendedPlanId,
                                    execution_trace_id = recommendedPlanToDoDetails.ExecutionTraceId,
                                    rate = recommendedPlanToDoDetails.Rate,
                                    audience_name = recommendedPlanToDoDetails.AudienceName,
                                    contracted_impressions = recommendedPlanToDoDetails.ContractedImpressions,
                                    delivered_impressions = recommendedPlanToDoDetails.DeliveredImpressions,
                                    is_recommended_plan = recommendedPlanToDoDetails.IsRecommendedPlan,
                                    plan_clearance_percentage = recommendedPlanToDoDetails.PlanClearancePercentage,
                                    daypart_code = recommendedPlanToDoDetails.DaypartCode,
                                    start_time = recommendedPlanToDoDetails.StartTime,
                                    end_time = recommendedPlanToDoDetails.EndTime,
                                    monday = recommendedPlanToDoDetails.Monday,
                                    tuesday = recommendedPlanToDoDetails.Tuesday,
                                    wednesday = recommendedPlanToDoDetails.Wednesday,
                                    thursday = recommendedPlanToDoDetails.Thursday,
                                    friday = recommendedPlanToDoDetails.Friday,
                                    saturday = recommendedPlanToDoDetails.Saturday,
                                    sunday = recommendedPlanToDoDetails.Sunday,
                                    spot_delivered_impressions = recommendedPlanToDoDetails.SpotDeliveredImpressions,
                                    plan_total_contracted_impressions = recommendedPlanToDoDetails.PlanTotalContractedImpressions,
                                    plan_total_delivered_impressions = recommendedPlanToDoDetails.PlanTotalDeliveredImpressions,
                                    ingested_by = recommendedPlanToDoDetails.IngestedBy,
                                    ingested_at = recommendedPlanToDoDetails.IngestedAt,
                                    ingested_media_week_id = recommendedPlanToDoDetails.IngestedMediaWeekId,
                                    spot_unique_hash_external = recommendedPlanToDo.SpotUniqueHashExternal,
                                    execution_id_external = recommendedPlanToDo.ExecutionIdExternal
                                };

                                return spotExceptionsRecommendedPlanToDoDetail;
                            }).ToList()
                    }).ToList();

                context.spot_exceptions_recommended_plans.AddRange(spotExceptionsRecommendedPlansToDoToAdd);

                var spotExceptionsRecommendedPlansDoneToAdd = spotExceptionsRecommendedPlansDone.Select(recommendedPlanDone =>
                    new spot_exceptions_recommended_plans_done()
                    {
                        spot_unique_hash_external = recommendedPlanDone.SpotUniqueHashExternal,
                        ambiguity_code = recommendedPlanDone.AmbiguityCode,
                        execution_id_external = recommendedPlanDone.ExecutionIdExternal,
                        estimate_id = recommendedPlanDone.EstimateId,
                        inventory_source = recommendedPlanDone.InventorySource,
                        house_isci = recommendedPlanDone.HouseIsci,
                        client_isci = recommendedPlanDone.ClientIsci,
                        spot_length_id = recommendedPlanDone.SpotLengthId,
                        program_air_time = recommendedPlanDone.ProgramAirTime,
                        station_legacy_call_letters = recommendedPlanDone.StationLegacyCallLetters,
                        affiliate = recommendedPlanDone.Affiliate,
                        market_code = recommendedPlanDone.MarketCode,
                        market_rank = recommendedPlanDone.MarketRank,
                        program_name = recommendedPlanDone.ProgramName,
                        program_genre = recommendedPlanDone.ProgramGenre,
                        ingested_by = recommendedPlanDone.IngestedBy,
                        ingested_at = recommendedPlanDone.IngestedAt,
                        ingested_media_week_id = recommendedPlanDone.IngestedMediaWeekId,
                        spot_exceptions_recommended_plan_details_done = recommendedPlanDone.SpotExceptionsRecommendedPlanDetailsDone
                            .Select(recommendedPlanDoneDetails =>
                            {
                                var spotExceptionsRecommendedPlanDetailDone = new spot_exceptions_recommended_plan_details_done()
                                {
                                    spot_exceptions_recommended_plan_done_id = recommendedPlanDone.Id,
                                    recommended_plan_id = recommendedPlanDoneDetails.RecommendedPlanId,
                                    execution_trace_id = recommendedPlanDoneDetails.ExecutionTraceId,
                                    rate = recommendedPlanDoneDetails.Rate,
                                    audience_name = recommendedPlanDoneDetails.AudienceName,
                                    contracted_impressions = recommendedPlanDoneDetails.ContractedImpressions,
                                    delivered_impressions = recommendedPlanDoneDetails.DeliveredImpressions,
                                    is_recommended_plan = recommendedPlanDoneDetails.IsRecommendedPlan,
                                    plan_clearance_percentage = recommendedPlanDoneDetails.PlanClearancePercentage,
                                    daypart_code = recommendedPlanDoneDetails.DaypartCode,
                                    start_time = recommendedPlanDoneDetails.StartTime,
                                    end_time = recommendedPlanDoneDetails.EndTime,
                                    monday = recommendedPlanDoneDetails.Monday,
                                    tuesday = recommendedPlanDoneDetails.Tuesday,
                                    wednesday = recommendedPlanDoneDetails.Wednesday,
                                    thursday = recommendedPlanDoneDetails.Thursday,
                                    friday = recommendedPlanDoneDetails.Friday,
                                    saturday = recommendedPlanDoneDetails.Saturday,
                                    sunday = recommendedPlanDoneDetails.Sunday,
                                    spot_delivered_impressions = recommendedPlanDoneDetails.SpotDeliveredImpressions,
                                    plan_total_contracted_impressions = recommendedPlanDoneDetails.PlanTotalContractedImpressions,
                                    plan_total_delivered_impressions = recommendedPlanDoneDetails.PlanTotalDeliveredImpressions,
                                    ingested_by = recommendedPlanDoneDetails.IngestedBy,
                                    ingested_at = recommendedPlanDoneDetails.IngestedAt,
                                    ingested_media_week_id = recommendedPlanDoneDetails.IngestedMediaWeekId,
                                    spot_unique_hash_external = recommendedPlanDone.SpotUniqueHashExternal,
                                    execution_id_external = recommendedPlanDone.ExecutionIdExternal
                                };

                                if (recommendedPlanDoneDetails.SpotExceptionsRecommendedPlanDoneDecisions != null)
                                {
                                    spotExceptionsRecommendedPlanDetailDone.spot_exceptions_recommended_plan_done_decisions =
                                        new List<spot_exceptions_recommended_plan_done_decisions>
                                        {
                                            new spot_exceptions_recommended_plan_done_decisions
                                            {
                                                decided_at = recommendedPlanDoneDetails.SpotExceptionsRecommendedPlanDoneDecisions.DecidedAt,
                                                decided_by = recommendedPlanDoneDetails.SpotExceptionsRecommendedPlanDoneDecisions.DecidedBy
                                            }
                                        };
                                }

                                return spotExceptionsRecommendedPlanDetailDone;
                            }).ToList()
                    }).ToList();

                context.spot_exceptions_recommended_plans_done.AddRange(spotExceptionsRecommendedPlansDoneToAdd);

                var spotExceptionsOutOfSpecsToDoToAdd = spotExceptionsOutOfSpecsToDo.Select(outOfSpecsToDo =>
                {
                    var spotExceptionsOutOfSpecToDo = new spot_exceptions_out_of_specs
                    {
                        reason_code_message = outOfSpecsToDo.ReasonCodeMessage,
                        estimate_id = outOfSpecsToDo.EstimateId,
                        isci_name = outOfSpecsToDo.IsciName,
                        recommended_plan_id = outOfSpecsToDo.RecommendedPlanId,
                        program_name = outOfSpecsToDo.ProgramName,
                        station_legacy_call_letters = outOfSpecsToDo.StationLegacyCallLetters,
                        spot_length_id = outOfSpecsToDo.SpotLengthId,
                        audience_id = outOfSpecsToDo.AudienceId,
                        program_network = outOfSpecsToDo.ProgramNetwork,
                        program_air_time = outOfSpecsToDo.ProgramAirTime,
                        reason_code_id = outOfSpecsToDo.SpotExceptionsOutOfSpecReasonCode.Id,
                        execution_id_external = executionId.ToString(),
                        impressions = outOfSpecsToDo.Impressions,
                        house_isci = outOfSpecsToDo.HouseIsci,
                        spot_unique_hash_external = outOfSpecsToDo.SpotUniqueHashExternal,
                        daypart_code = outOfSpecsToDo.DaypartCode,
                        genre_name = outOfSpecsToDo.GenreName,
                        market_code = outOfSpecsToDo.MarketCode,
                        market_rank = outOfSpecsToDo.MarketRank,
                        ingested_by = outOfSpecsToDo.IngestedBy,
                        ingested_at = outOfSpecsToDo.IngestedAt,
                        ingested_media_week_id = outOfSpecsToDo.IngestedMediaWeekId,
                        inventory_source_name = outOfSpecsToDo.InventorySourceName
                    };

                    return spotExceptionsOutOfSpecToDo;
                }).ToList();

                context.spot_exceptions_out_of_specs.AddRange(spotExceptionsOutOfSpecsToDoToAdd);

                var spotExceptionsOutOfSpecsDoneToAdd = spotExceptionsOutOfSpecsDone.Select(outOfSpecsDone =>
                {
                    var spotExceptionsOutOfSpecDone = new spot_exceptions_out_of_specs_done
                    {
                        reason_code_message = outOfSpecsDone.ReasonCodeMessage,
                        estimate_id = outOfSpecsDone.EstimateId,
                        isci_name = outOfSpecsDone.IsciName,
                        recommended_plan_id = outOfSpecsDone.RecommendedPlanId,
                        program_name = outOfSpecsDone.ProgramName,
                        station_legacy_call_letters = outOfSpecsDone.StationLegacyCallLetters,
                        spot_length_id = outOfSpecsDone.SpotLengthId,
                        audience_id = outOfSpecsDone.AudienceId,
                        program_network = outOfSpecsDone.ProgramNetwork,
                        program_air_time = outOfSpecsDone.ProgramAirTime,
                        reason_code_id = outOfSpecsDone.SpotExceptionsOutOfSpecReasonCode.Id,
                        execution_id_external = executionId.ToString(),
                        impressions = outOfSpecsDone.Impressions,
                        house_isci = outOfSpecsDone.HouseIsci,
                        spot_unique_hash_external = outOfSpecsDone.SpotUniqueHashExternal,
                        daypart_code = outOfSpecsDone.DaypartCode,
                        genre_name = outOfSpecsDone.GenreName,
                        market_code = outOfSpecsDone.MarketCode,
                        market_rank = outOfSpecsDone.MarketRank,
                        ingested_by = outOfSpecsDone.IngestedBy,
                        ingested_at = outOfSpecsDone.IngestedAt,
                        ingested_media_week_id = outOfSpecsDone.IngestedMediaWeekId,
                        inventory_source_name = outOfSpecsDone.InventorySourceName
                    };

                    if (outOfSpecsDone.SpotExceptionsOutOfSpecDoneDecision != null)
                    {
                        spotExceptionsOutOfSpecDone.spot_exceptions_out_of_spec_done_decisions = new List<spot_exceptions_out_of_spec_done_decisions>
                        {
                            new spot_exceptions_out_of_spec_done_decisions()
                            {
                                spot_exceptions_out_of_spec_done_id = outOfSpecsDone.SpotExceptionsOutOfSpecDoneDecision.SpotExceptionsOutOfSpecDoneId,
                                accepted_as_in_spec = outOfSpecsDone.SpotExceptionsOutOfSpecDoneDecision.AcceptedAsInSpec,
                                decision_notes = outOfSpecsDone.SpotExceptionsOutOfSpecDoneDecision.DecisionNotes,
                                decided_at = outOfSpecsDone.SpotExceptionsOutOfSpecDoneDecision.DecidedAt,
                                decided_by = outOfSpecsDone.SpotExceptionsOutOfSpecDoneDecision.DecidedBy
                            }
                        };
                    }

                    return spotExceptionsOutOfSpecDone;
                }).ToList();

                context.spot_exceptions_out_of_specs_done.AddRange(spotExceptionsOutOfSpecsDoneToAdd);

                context.SaveChanges();
            });
            return true;
        }

        public bool ClearSpotExceptionMockData()
        {
            return _InReadUncommitedTransaction(context =>
            {
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_details "
                                                    + "WHERE ingested_by = 'Mock Data'");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plans "
                                                    + "WHERE ingested_by = 'Mock Data';");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_done_decisions "
                                                    + "WHERE decided_by = 'Mock Data'");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_details_done "
                                                    + "WHERE ingested_by = 'Mock Data'");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plans "
                                                    + "WHERE ingested_by = 'Mock Data';");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs "
                                                    + "WHERE ingested_by = 'Mock Data';");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_spec_done_decisions "
                                                    + "WHERE decided_by = 'Mock Data'");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs_done "
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
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_done_decisions");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plan_details_done");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_recommended_plans_done");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_spec_decisions");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_spec_done_decisions");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_out_of_specs_done");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_unposted_no_plan");
                context.Database.ExecuteSqlCommand("DELETE FROM spot_exceptions_unposted_no_reel_roster");
                return true;
            });
        }

        /// <inheritdoc />
        public async Task<bool> SyncOutOfSpecDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsDecisionsEntities = context.spot_exceptions_out_of_spec_done_decisions
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
        public async Task<bool> SyncRecommendedPlanDecisionsAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest, DateTime dateTime)
        {
            return _InReadUncommitedTransaction(context =>
            {

                var spotExceptionsRecommandedDecisionsEntities = context.spot_exceptions_recommended_plan_done_decisions
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
        public async Task<int> GetOutOfSpecDecisionQueuedCountAsync()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var OutOfSpecDecisionCount = context.spot_exceptions_out_of_spec_done_decisions
                  .Where(x => x.synced_at == null)
                  .Count();

                return OutOfSpecDecisionCount;
            });
        }

        /// <inheritdoc />
        public async Task<int> GetRecommendedPlanDecisionQueuedCountAsync()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var recommandedPlansDecisionCount = context.spot_exceptions_recommended_plan_done_decisions
                  .Where(x => x.synced_at == null)
                  .Count();
                return recommandedPlansDecisionCount;
            });
        }
    }
}
