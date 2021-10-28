using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
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
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

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
                           spot_exceptions_recommended_plan_detail_id = recommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId,
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
                    .Where(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.program_air_time >= weekStartDate && spotExceptionsoutOfSpecDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_decisions)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.plan)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_lengths)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.daypart)
                    //the reason daypart1 is two foreign key referring to same daypart table
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.daypart1)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.audience)
                    //the reason audience1 is two foreign key referring to same audience table
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.audience1)
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
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_lengths)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.daypart)
                    //the reason daypart1 is two foreign key referring to same daypart table
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.daypart1)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.audience)
                    //the reason audience1 is two foreign key referring to same audience table
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.audience1)
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

                var spotExceptionsoutOfSpec =  _MapSpotExceptionsOutOfSpecToDto(spotExceptionsOutOfSpecsEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecsEntity.Station);
                return spotExceptionsoutOfSpec;

            });
        }
        private SpotExceptionsOutOfSpecsDto _MapSpotExceptionsOutOfSpecToDto(spot_exceptions_out_of_specs spotExceptionsOutOfSpecEntity, station stationEntity)
        {
            var spotExceptionsOutOfSpec = new SpotExceptionsOutOfSpecsDto
            {
                Id = spotExceptionsOutOfSpecEntity.id,
                ReasonCode = spotExceptionsOutOfSpecEntity.reason_code,
                ReasonCodeMessage = spotExceptionsOutOfSpecEntity.reason_code_message,
                EstimateId = spotExceptionsOutOfSpecEntity.estimate_id,
                IsciName = spotExceptionsOutOfSpecEntity.isci_name,
                RecommendedPlanId = spotExceptionsOutOfSpecEntity.recommended_plan_id,
                RecommendedPlanName = spotExceptionsOutOfSpecEntity.plan?.name,
                ProgramName = spotExceptionsOutOfSpecEntity.program_name,
                StationLegacyCallLetters = spotExceptionsOutOfSpecEntity.station_legacy_call_letters,
                Affiliate = stationEntity?.affiliation,
                Market = stationEntity?.market?.geography_name,             
                SpotLength = _MapSpotLengthToDto(spotExceptionsOutOfSpecEntity.spot_lengths),
                Audience = _MapAudienceToDto(spotExceptionsOutOfSpecEntity.audience),                
                Product = spotExceptionsOutOfSpecEntity.product,
                FlightStartDate = spotExceptionsOutOfSpecEntity.flight_start_date,
                FlightEndDate = spotExceptionsOutOfSpecEntity.flight_end_date,
                DaypartDetail = _MapDaypartToDto(spotExceptionsOutOfSpecEntity.daypart),
                ProgramDaypartDetail = _MapDaypartToDto(spotExceptionsOutOfSpecEntity.daypart1),
                ProgramFlightStartDate = spotExceptionsOutOfSpecEntity.program_flight_start_date,
                ProgramFlightEndDate = spotExceptionsOutOfSpecEntity.program_flight_end_date,
                ProgramAudience = _MapAudienceToDto(spotExceptionsOutOfSpecEntity.audience1),
                ProgramAirTime = spotExceptionsOutOfSpecEntity.program_air_time,
                IngestedBy = spotExceptionsOutOfSpecEntity.ingested_by,
                IngestedAt = spotExceptionsOutOfSpecEntity.ingested_at,
                SpotExceptionsOutOfSpecDecision = spotExceptionsOutOfSpecEntity.spot_exceptions_out_of_spec_decisions.Select(spotExceptionsOutOfSpecsDecisionDb => new SpotExceptionsOutOfSpecDecisionsDto
                {
                    Id = spotExceptionsOutOfSpecsDecisionDb.id,
                    SpotExceptionsOutOfSpecId = spotExceptionsOutOfSpecsDecisionDb.spot_exceptions_out_of_spec_id,
                    AcceptedAsInSpec = spotExceptionsOutOfSpecsDecisionDb.accepted_as_in_spec,
                    DecisionNotes= spotExceptionsOutOfSpecsDecisionDb.decision_notes,
                    UserName = spotExceptionsOutOfSpecsDecisionDb.username,
                    CreatedAt = spotExceptionsOutOfSpecsDecisionDb.created_at
                }).SingleOrDefault()              
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
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
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
