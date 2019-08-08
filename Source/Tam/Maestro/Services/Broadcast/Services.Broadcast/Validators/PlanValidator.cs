using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
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
    }

    public class PlanValidator : IPlanValidator
    {
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly List<MediaMonth> _PostingBooks;

        #region Error Messages
        const string INVALID_PLAN_NAME = "Invalid plan name";
        const string INVALID_SPOT_LENGTH = "Invalid spot length";
        const string INVALID_PRODUCT = "Invalid product";
        const string INVALID_SHARE_BOOK = "Invalid share book";
        const string INVALID_HUT_BOOK = "Invalid HUT boook.";
        const string INVALID_FLIGHT_DATES = "Invalid flight dates.  The end date cannot be before the start date.";
        const string INVALID_FLIGHT_HIATUS_DAY = "Invalid flight hiatus day.  All days must be within the flight date range.";
        const string INVALID_AUDIENCE = "Invalid audience";
        const string INVALID_AUDIENCE_DUPLICATE = "An audience cannot appear multiple times";
        const string INVALID_SHARE_HUT_BOOKS = "HUT Book must be prior to Share Book";
        const string INVALID_DAYPART_TIMES = "Invalid daypart times.";
        const string INVALID_DAYPART_WEIGHTING_GOAL = "Invalid daypart weighting goal.";
        private const string INVALID_COVERAGE_GOAL = "Invalid coverage goal value.";
        private const string INVALID_MARKET_SHARE_OF_VOICE = "Invalid share of voice for market.";

        #endregion

        public PlanValidator(ISpotLengthEngine spotLengthEngine
            , IBroadcastAudiencesCache broadcastAudiencesCache
            , IRatingForecastService ratingForecastService)
        {
            _SpotLengthEngine = spotLengthEngine;
            _AudienceCache = broadcastAudiencesCache;

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
            if (!_SpotLengthEngine.SpotLengthIdExists(plan.SpotLengthId))
            {
                throw new Exception(INVALID_SPOT_LENGTH);
            }
            if (plan.ProductId <= 0)
            {
                throw new Exception(INVALID_PRODUCT);
            }

            _ValidateFlightAndHiatusDates(plan);
            _ValidateDayparts(plan);
            _ValidatePrimaryAudience(plan);
            _ValidateSecondaryAudiences(plan.SecondaryAudiences, plan.AudienceId);
            _ValidateOptionalPercentage(plan.CoverageGoalPercent, INVALID_COVERAGE_GOAL);
            _ValidateMarkets(plan);
        }

        #region Helpers

        private void _ValidateFlightAndHiatusDates(PlanDto plan)
        {
            if (plan.FlightStartDate.HasValue == false || plan.FlightEndDate.HasValue == false)
            {
                return;
            }

            if (plan.FlightStartDate > plan.FlightEndDate)
            {
                throw new Exception(INVALID_FLIGHT_DATES);
            }

            if (plan.FlightHiatusDays?.Any() == true)
            {
                var hasInvalids =
                    plan.FlightHiatusDays.Any(h => h.Date < plan.FlightStartDate || h.Date > plan.FlightEndDate);
                if (hasInvalids)
                {
                    throw new Exception(INVALID_FLIGHT_HIATUS_DAY);
                }
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
            secondaryAudiences.Select(x => x.AudienceId)
                .ForEach(audienceId =>
                {
                    if (!_AudienceCache.IsValidAudience(audienceId))
                    {
                        throw new Exception(INVALID_AUDIENCE);
                    }
                    if (distinctAudiences.Contains(audienceId))
                    {
                        throw new Exception(INVALID_AUDIENCE_DUPLICATE);
                    }
                    distinctAudiences.Add(audienceId);
                });
        }

        private void _ValidateDayparts(PlanDto plan)
        {
            const int daySecondsMin = 0;
            const int daySecondsMax = 86400;
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
            }
        }

        private void _ValidateMarkets(PlanDto plan)
        {
            plan.AvailableMarkets.ForEach(m => _ValidateOptionalPercentage(m.ShareOfVoicePercent, INVALID_MARKET_SHARE_OF_VOICE));
        }

        private void _ValidateOptionalPercentage(double? candidate, string errorMessage)
        {
            if (candidate.HasValue &&
                (candidate < 0.1 || candidate > 100.0))
            {
                throw new Exception(errorMessage);
            }
        }

        #endregion // #region Helpers
    }
}
