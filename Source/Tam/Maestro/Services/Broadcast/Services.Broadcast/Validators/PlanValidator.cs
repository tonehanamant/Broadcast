﻿using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Validators
{
    public interface IPlanValidator
    {
        /// <summary>
        /// Validates the new plan.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void ValidatePlan(PlanDto plan);

        /// <summary>
        /// Validates the weekly breakdown.
        /// </summary>
        /// <param name="request">WeeklyBreakdownRequest request object.</param>
        void ValidateWeeklyBreakdown(WeeklyBreakdownRequest request);
    }

    public class PlanValidator : IPlanValidator
    {
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly List<MediaMonth> _PostingBooks;
        private readonly ITrafficApiCache _TrafficApiCache;
        private readonly IPlanRepository _PlanRepository;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;

        const string INVALID_PLAN_NAME = "Invalid plan name";
        const string INVALID_SPOT_LENGTH = "Invalid spot length id {0}";
        const string INVALID_PRODUCT = "Invalid product";
        const string INVALID_SHARE_BOOK = "Invalid share book";
        const string INVALID_HUT_BOOK = "Invalid HUT book.";
        const string INVALID_FLIGHT_DATES = "Invalid flight dates.  The end date cannot be before the start date.";
        const string INVALID_FLIGHT_DAYS = "Invalid flight days. The plan should have at least one flight day";
        const string INVALID_FLIGHT_DATES_WITH_FLIGHT_DAYS = "Invalid flight dates. The flight cannot start or end, with non-flight days";
        const string INVALID_FLIGHT_DATE = "Invalid flight start/end date.";
        const string INVALID_FLIGHT_HIATUS_DAY =
            "Invalid flight hiatus day.  All days must be within the flight date range.";
        const string INVALID_FLIGHT_HIATUS_DAY_WITH_FLIGHT_DAYS = "Invalid flight hiatus day. Hiatus day cannot be a non-flight day.";
        const string INVALID_AUDIENCE = "Invalid audience";
        const string INVALID_AUDIENCE_DUPLICATE = "An audience cannot appear multiple times";
        const string INVALID_SHARE_HUT_BOOKS = "HUT Book must be prior to Share Book";
        const string INVALID_DAYPART_NUMBER = "There should be at least one daypart selected.";
        const string INVALID_DAYPART_TIMES = "Invalid daypart times.";
        const string INVALID_DAYPART_WEIGHTING_GOAL = "Invalid daypart weighting goal.";
        const string INVALID_COVERAGE_GOAL = "Invalid coverage goal value.";
        const string INVALID_TOTAL_MARKET_COVERAGE = "Invalid total market coverage.";
        const string INVALID_MARKET_SHARE_OF_VOICE = "Invalid share of voice for market.";
        const string INVALID_REQUEST = "Invalid request";
        public const string INVALID_IMPRESSIONS_COUNT = "The impressions count is different between the delivery and the weekly breakdown";
        public const string INVALID_SOV_COUNT = "The share of voice count is not equal to 100%";
        const string INVALID_FLIGHT_NOTES = "Flight notes cannot be longer than 1024 characters.";
        const string STOP_WORD_DETECTED = "Stop word detected in plan name";
        const string SUM_OF_DAYPART_WEIGHTINGS_EXCEEDS_LIMIT = "Sum of weighting is greater than 100%";
        const string INVALID_DAYPART_DUPLICATE_DAYPART = "Invalid dayparts.  Each daypart can be entered only once.";
        const string INVALID_BUDGET = "Invalid budget.";
        const string INVALID_CPM = "Invalid CPM.";
        const string INVALID_CPP = "Invalid CPP.";
        const string INVALID_DELIVERY_IMPRESSIONS = "Invalid Delivery Impressions.";
        const string INVALID_STATUS_TRANSITION_MESSAGE = "Invalid status, can't update a plan from status {0} to status {1}";
        const string INVALID_DRAFT_ON_NEW_PLAN = "Cannot create a new draft on a non existing plan";
        const string STOP_WORD = "eOm3wgvfm0dq4rI3srL2";

        const string SHOW_TYPE_CONTAIN_TYPE_IS_NOT_VALID = "Contain type of the show types restrictions is not valid";
        const string GENRE_CONTAIN_TYPE_IS_NOT_VALID = "Contain type of the genres restrictions is not valid";
        const string PROGRAM_CONTAIN_TYPE_IS_NOT_VALID = "Contain type of the program restrictions is not valid";
        const string AFFILIATE_CONTAIN_TYPE_IS_NOT_VALID = "Contain type of the affiliate restrictions is not valid";

        public PlanValidator(IBroadcastAudiencesCache broadcastAudiencesCache
            , IRatingForecastService ratingForecastService
            , ITrafficApiCache trafficApiCache
            , IDataRepositoryFactory broadcastDataRepositoryFactory
            , ICreativeLengthEngine creativeLengthEngine)
        {
            _AudienceCache = broadcastAudiencesCache;
            _TrafficApiCache = trafficApiCache;
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _CreativeLengthEngine = creativeLengthEngine;

            _PostingBooks = ratingForecastService.GetMediaMonthCrunchStatuses()
                .Where(a => a.Crunched == CrunchStatusEnum.Crunched)
                .Select(m => m.MediaMonth)
                .ToList();
        }

        public void ValidatePlan(PlanDto plan)
        {
            if (string.IsNullOrWhiteSpace(plan.Name) || plan.Name.Length > 255)
            {
                throw new Exception(INVALID_PLAN_NAME);
            }

            if ((plan.VersionId == 0 || plan.Id == 0) && plan.IsDraft == true)
            {
                throw new Exception(INVALID_DRAFT_ON_NEW_PLAN);
            }

            _CreativeLengthEngine.ValidateCreativeLengthsForPlanSave(plan.CreativeLengths);
            _ValidateProduct(plan);
            _ValidateFlightAndHiatus(plan);
            _ValidateDayparts(plan);
            _ValidatePrimaryAudience(plan);
            _ValidateSecondaryAudiences(plan.SecondaryAudiences, plan.AudienceId);
            _ValidateMarkets(plan);
            _ValidateWeeklyBreakdownWeeks(plan);
            _ValidateBudgetAndDelivery(plan);

            // PRI-14012 We'll use a stop word so QA can trigger an error 
            _ValidateStopWord(plan);
        }

        private void _ValidateStopWord(PlanDto plan)
        {
            if (plan.Name.Contains(STOP_WORD))
            {
                throw new Exception(STOP_WORD_DETECTED);
            }
        }

        ///<inheritdoc/>
        public void ValidateWeeklyBreakdown(WeeklyBreakdownRequest request)
        {
            if (request == null)
            {
                throw new Exception(INVALID_REQUEST);
            }

            if (request.FlightEndDate.Equals(DateTime.MinValue) || request.FlightStartDate.Equals(DateTime.MinValue))
            {
                throw new Exception(INVALID_FLIGHT_DATE);
            }

            if (request.FlightEndDate < request.FlightStartDate)
            {
                throw new Exception(INVALID_FLIGHT_DATES);
            }

            if (request.FlightDays == null || !request.FlightDays.Any())
            {
                throw new Exception(INVALID_FLIGHT_DAYS);
            }

            if (request.DeliveryType != PlanGoalBreakdownTypeEnum.EvenDelivery &&
                request.Weeks.Count(w => w.IsUpdated) > 1)
            {
                throw new Exception("More than one updated week found.");
            }

            if (request.TotalImpressions <= 0)
            {
                throw new Exception("Total impressions must be more than zero");
            }

            _ValidateWeeklyBreakdownDeliveryTypeAndWeeks(request.DeliveryType, request.Weeks);
        }

        private void _ValidateFlightAndHiatus(PlanDto plan)
        {
            if (!plan.FlightStartDate.HasValue || !plan.FlightEndDate.HasValue)
            {
                throw new Exception(INVALID_FLIGHT_DATE);
            }

            if (plan.FlightStartDate > plan.FlightEndDate)
            {
                throw new Exception(INVALID_FLIGHT_DATES);
            }

            if (plan.FlightDays == null || !plan.FlightDays.Any())
            {
                throw new Exception(INVALID_FLIGHT_DAYS);
            }

            if (plan.FlightHiatusDays?.Any() == true)
            {
                var hasInvalids =
                    plan.FlightHiatusDays.Any(h => h.Date < plan.FlightStartDate || h.Date > plan.FlightEndDate);
                if (hasInvalids)
                {
                    throw new Exception(INVALID_FLIGHT_HIATUS_DAY);
                }

                var hasInvalidHiatusDaysWithFlightDays =
                    plan.FlightHiatusDays
                    .Select(hiatus => (int)hiatus.GetBroadcastDayOfWeek()).Distinct()
                    .Any(hiatusDay => !plan.FlightDays.Contains(hiatusDay));
                if (hasInvalidHiatusDaysWithFlightDays)
                {
                    throw new Exception(INVALID_FLIGHT_HIATUS_DAY_WITH_FLIGHT_DAYS);
                }

            }

            if (!string.IsNullOrEmpty(plan.FlightNotes) && plan.FlightNotes.Length > 1024)
                throw new Exception(INVALID_FLIGHT_NOTES);

            var planFlightStartDateDayofWeek = (int)plan.FlightStartDate.Value.GetBroadcastDayOfWeek();
            var planFlightEndDateDayofWeek = (int)plan.FlightEndDate.Value.GetBroadcastDayOfWeek();
            if (!plan.FlightDays.Any(day => day == planFlightStartDateDayofWeek || day == planFlightEndDateDayofWeek))
            {
                throw new Exception(INVALID_FLIGHT_DATES_WITH_FLIGHT_DAYS);
            }
        }

        private void _ValidatePrimaryAudience(PlanDto plan)
        {
            if (!_AudienceCache.IsValidAudience(plan.AudienceId))
            {
                throw new Exception(INVALID_AUDIENCE);
            }

            if (!_PostingBooks.Any(x => x.Id == plan.ShareBookId))
            {
                throw new Exception(INVALID_SHARE_BOOK);
            }

            //if the hutbook is set but it's 0 or a value not available throw exception
            if (plan.HUTBookId.HasValue && (plan.HUTBookId <= 0 || !_PostingBooks.Any(x => x.Id == plan.HUTBookId)))
            {
                throw new Exception(INVALID_HUT_BOOK);
            }

            if (plan.HUTBookId.HasValue)
            {
                var shareBook = _PostingBooks.Single(x => x.Id == plan.ShareBookId);
                var hutBook = _PostingBooks.Single(x => x.Id == plan.HUTBookId);
                if (hutBook.StartDate > shareBook.StartDate)
                {
                    throw new Exception(INVALID_SHARE_HUT_BOOKS);
                }
            }
        }

        private void _ValidateSecondaryAudiences(List<PlanAudienceDto> secondaryAudiences, int primaryAudienceId)
        {
            var distinctAudiences = new List<int> { primaryAudienceId };
            foreach (var secondaryAudience in secondaryAudiences)
            {
                if (!_AudienceCache.IsValidAudience(secondaryAudience.AudienceId))
                    throw new Exception(INVALID_AUDIENCE);

                if (distinctAudiences.Contains(secondaryAudience.AudienceId))
                    throw new Exception(INVALID_AUDIENCE_DUPLICATE);

                distinctAudiences.Add(secondaryAudience.AudienceId);
            }
        }

        private void _ValidateDayparts(PlanDto plan)
        {
            const int daySecondsMin = 0;
            const int daySecondsMax = BroadcastConstants.OneDayInSeconds - 1;
            if (plan.Dayparts?.Any() != true)
            {
                throw new Exception(INVALID_DAYPART_NUMBER);
            }

            if (plan.Dayparts.GroupBy(d => d.DaypartCodeId).Any(g => g.Count() > 1))
            {
                throw new Exception(INVALID_DAYPART_DUPLICATE_DAYPART);
            }

            foreach (var daypart in plan.Dayparts)
            {
                if (daypart.StartTimeSeconds < daySecondsMin || daypart.StartTimeSeconds > daySecondsMax)
                {
                    throw new Exception(INVALID_DAYPART_TIMES);
                }

                if (daypart.EndTimeSeconds < daySecondsMin || daypart.EndTimeSeconds > daySecondsMax)
                {
                    throw new Exception(INVALID_DAYPART_TIMES);
                }

                const double minWeightingGoalPercent = 0.1;
                const double maxWeightingGoalPercent = 100.0;
                if (daypart.WeightingGoalPercent.HasValue &&
                    (daypart.WeightingGoalPercent.Value < minWeightingGoalPercent
                     || daypart.WeightingGoalPercent.Value > maxWeightingGoalPercent))
                {
                    throw new Exception(INVALID_DAYPART_WEIGHTING_GOAL);
                }

                _ValidatePlanDaypartRestrictions(daypart);
                _ValidatePlanDaypartAudienceVpvh(daypart);
            }

            var sumOfDaypartWeighting = plan.Dayparts.Aggregate(0d, (sumOfWeighting, dayPart) => sumOfWeighting + dayPart.WeightingGoalPercent.GetValueOrDefault());
            if (sumOfDaypartWeighting > 100)
            {
                throw new Exception(SUM_OF_DAYPART_WEIGHTINGS_EXCEEDS_LIMIT);
            }
        }

        private void _ValidatePlanDaypartAudienceVpvh(PlanDaypartDto planDaypartDto)
        {
            var vpvhForAudiences = planDaypartDto.VpvhForAudiences;

            foreach (var vpvhForAudience in vpvhForAudiences)
            {
                if (vpvhForAudience.Vpvh < 0)
                    throw new Exception("VPVH can not be less than zero");

                if (!EnumHelper.IsDefined(vpvhForAudience.VpvhType))
                    throw new Exception("Unknown VPVH type was discovered");

                if (vpvhForAudience.StartingPoint == default)
                    throw new Exception("StartingPoint is a required property");
            }
        }

        private void _ValidatePlanDaypartRestrictions(PlanDaypartDto planDaypartDto)
        {
            var restrictions = planDaypartDto.Restrictions;

            if (restrictions != null)
            {
                _ValidateShowTypeRestrictions(restrictions);
                _ValidateGenreRestrictions(restrictions);
                _ValidateProgramRestrictions(restrictions);
                _ValidateAffiliateRestrictions(restrictions);
            }
        }

        private void _ValidateShowTypeRestrictions(PlanDaypartDto.RestrictionsDto restrictions)
        {
            var showTypeRestrictions = restrictions.ShowTypeRestrictions;

            if (showTypeRestrictions != null && !EnumHelper.IsDefined(showTypeRestrictions.ContainType))
            {
                throw new Exception(SHOW_TYPE_CONTAIN_TYPE_IS_NOT_VALID);
            }
        }

        private void _ValidateGenreRestrictions(PlanDaypartDto.RestrictionsDto restrictions)
        {
            var genreRestrictions = restrictions.GenreRestrictions;

            if (genreRestrictions != null && !EnumHelper.IsDefined(genreRestrictions.ContainType))
            {
                throw new Exception(GENRE_CONTAIN_TYPE_IS_NOT_VALID);
            }
        }

        private void _ValidateProgramRestrictions(PlanDaypartDto.RestrictionsDto restrictions)
        {
            var programRestrictions = restrictions.ProgramRestrictions;

            if (programRestrictions != null && !EnumHelper.IsDefined(programRestrictions.ContainType))
            {
                throw new Exception(PROGRAM_CONTAIN_TYPE_IS_NOT_VALID);
            }
        }

        private void _ValidateAffiliateRestrictions(PlanDaypartDto.RestrictionsDto restrictions)
        {
            var affiliateRestrictions = restrictions.AffiliateRestrictions;

            if (affiliateRestrictions != null && !EnumHelper.IsDefined(affiliateRestrictions.ContainType))
            {
                throw new Exception(AFFILIATE_CONTAIN_TYPE_IS_NOT_VALID);
            }
        }

        private void _ValidateMarkets(PlanDto plan)
        {
            if (!plan.CoverageGoalPercent.HasValue
                || plan.CoverageGoalPercent.Value < 0.1
                || plan.CoverageGoalPercent.Value > 100.0)
            {
                throw new Exception(INVALID_COVERAGE_GOAL);
            }

            plan.AvailableMarkets.ForEach(m =>
                _ValidateOptionalPercentage(m.ShareOfVoicePercent, INVALID_MARKET_SHARE_OF_VOICE));

            var totalMarketCoverage = Math.Round(plan.AvailableMarkets.Sum(m => m.PercentageOfUS), 1);
            var coverageGoalRounded = Math.Round(plan.CoverageGoalPercent.Value, 1);

            if (totalMarketCoverage < coverageGoalRounded)
            {
                throw new Exception(INVALID_TOTAL_MARKET_COVERAGE);
            }
        }

        private void _ValidateOptionalPercentage(double? candidate, string errorMessage)
        {
            if (candidate.HasValue &&
                (candidate < 0.1 || candidate > 100.0))
            {
                throw new Exception(errorMessage);
            }
        }

        protected void _ValidateWeeklyBreakdownWeeks(PlanDto plan)
        {
            if (!plan.WeeklyBreakdownWeeks.Any())
            {
                return;
            }

            var roundedTargetImpressions = Math.Floor(plan.TargetImpressions.GetValueOrDefault());
            var roundedWeeklyImpressionsSum = Math.Floor(plan.WeeklyBreakdownWeeks.Select(x => x.WeeklyImpressions).Sum());
            if (roundedTargetImpressions.Equals(roundedWeeklyImpressionsSum) == false)
            {
                throw new Exception(INVALID_IMPRESSIONS_COUNT);
            }
            //We do not validate percentages or rating points since those are all derived from the impressions

            _ValidateWeeklyBreakdownDeliveryTypeAndWeeks(plan.GoalBreakdownType, plan.WeeklyBreakdownWeeks);
        }

        private void _ValidateWeeklyBreakdownDeliveryTypeAndWeeks(PlanGoalBreakdownTypeEnum deliveryType, List<WeeklyBreakdownWeek> weeklyBreakdown)
        {
            if (deliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                if (weeklyBreakdown.Any(x => !x.SpotLengthId.HasValue))
                    throw new Exception("For the chosen delivery type, each weekly breakdown row must have spot length associated with it");

                if (weeklyBreakdown.Any(x => !x.PercentageOfWeek.HasValue))
                    throw new Exception("For the chosen delivery type, each weekly breakdown row must have percentage of week set");

                var weeklyBreakdownHasSeveralRowsWithTheSameWeekAndSpotLength = weeklyBreakdown
                    .GroupBy(x => new { x.MediaWeekId, x.SpotLengthId })
                    .Select(x => x.Count())
                    .Any(x => x > 1);

                if (weeklyBreakdownHasSeveralRowsWithTheSameWeekAndSpotLength)
                    throw new Exception("For the chosen delivery type, each week and spot Length combination must be unique");
            }
        }

        private void _ValidateProduct(PlanDto plan)
        {
            try
            {
                _TrafficApiCache.GetProduct(plan.ProductId);
            }
            catch (Exception ex)
            {
                throw new Exception(INVALID_PRODUCT, ex);
            }
        }

        private void _ValidateBudgetAndDelivery(PlanDto plan)
        {
            if (!(plan.Budget.HasValue && plan.Budget.Value > 0m))
            {
                throw new Exception(INVALID_BUDGET);
            }

            if (!(plan.TargetCPM.HasValue && plan.TargetCPM.Value > 0m))
            {
                throw new Exception(INVALID_CPM);
            }

            if (!(plan.TargetCPP.HasValue && plan.TargetCPP.Value > 0m))
            {
                throw new Exception(INVALID_CPP);
            }

            if (!(plan.TargetImpressions.HasValue && plan.TargetImpressions.Value > 0d))
            {
                throw new Exception(INVALID_DELIVERY_IMPRESSIONS);
            }
        }
    }
}
