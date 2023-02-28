using Common.Services.Repositories;
using Common.Services.Extensions;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Exceptions;
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
        /// Validates the plan draft.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void ValidatePlanDraft(PlanDto plan);

        /// <summary>
        /// Validates an ADU Type Plan.
        /// </summary>
        void ValidateAduPlan(PlanDto plan);
      
        /// <summary>
        /// Validates the weekly breakdown.
        /// </summary>
        /// <param name="request">WeeklyBreakdownRequest request object.</param>
        void ValidateWeeklyBreakdown(WeeklyBreakdownRequest request);

        /// <summary>
        /// Validates the impressions per unit.
        /// </summary>
        /// <param name="impressionsPerUnit">The impressions per unit.</param>
        /// <param name="totalImpressions">The total impressions.</param>
        void ValidateImpressionsPerUnit(double impressionsPerUnit, double totalImpressions);


        /// <summary>
        /// Validates the plan in pricing.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void ValidatePlanForPricing(PlanDto plan);

        /// <summary>
        /// Validates the plan for buying.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void ValidatePlanForBuying(PlanDto plan);

        /// <summary>
        /// Validates the plan does not cross quarters.
        /// For use within the Pricing context.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void ValidatePlanNotCrossQuartersForPricing(PlanDto plan);

        /// <summary>
        /// Validates the plan does not cross quarters.
        /// For use within the Buying context.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void ValidatePlanNotCrossQuartersForBuying(PlanDto plan);

        /// <summary>
        /// Validates the weekly breakdown item weights are 100% each.
        /// </summary>
        /// <param name="dayparts">The dayparts.</param>
        /// <param name="creativeLengths">The creative lengths.</param>
        /// <param name="deliveryType">Type of the delivery.</param>
        void ValidateWeeklyBreakdownItemWeights(List<StandardDaypartWeightingGoal> dayparts, List<CreativeLength> creativeLengths, PlanGoalBreakdownTypeEnum deliveryType);        
    }

    public class PlanValidator : IPlanValidator
    {
        private readonly IBroadcastAudiencesCache _AudienceCache;
        private readonly List<MediaMonth> _PostingBooks;
        private readonly ICreativeLengthEngine _CreativeLengthEngine;

        private readonly ICampaignRepository _CampaignRepository;
        private readonly IAabEngine _AabEngine;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IPlanMarketSovCalculator _PlanMarketSovCalculator;
        private readonly IRatingForecastService _RatingForecastService;

        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;

        const string INVALID_PLAN_NAME = "Invalid plan name.";
        const string INVALID_PRODUCT = "Invalid product";
        const string INVALID_SHARE_BOOK = "Invalid share book";
        const string INVALID_HUT_BOOK = "Invalid HUT book.";
        const string INVALID_FLIGHT_DATES = "Invalid flight dates.  The end date cannot be before the start date.";
        const string INVALID_FLIGHT_DAYS = "Invalid flight days. The plan should have at least one flight day.";
        const string INVALID_FLIGHT_DATES_WITH_FLIGHT_DAYS = "Invalid flight dates. The flight cannot start or end, with non-flight days";
        const string INVALID_FLIGHT_DATE = "Invalid flight start/end date.";
        const string INVALID_FLIGHT_HIATUS_DAY =
            "Invalid flight hiatus day.  All days must be within the flight date range.";
        const string INVALID_FLIGHT_HIATUS_DAY_WITH_FLIGHT_DAYS = "Invalid flight hiatus day. Hiatus day cannot be a non-flight day.";
        const string INVALID_AUDIENCE = "Invalid audience.";
        const string INVALID_AUDIENCE_DUPLICATE = "An audience cannot appear multiple times.";
        const string INVALID_SHARE_HUT_BOOKS = "HUT Book must be prior to Share Book";
        const string INVALID_DAYPART_NUMBER = "There should be at least one daypart selected.";
        const string INVALID_DAYPART_TIMES = "Invalid daypart times.";
        const string INVALID_DAYPART_WEIGHTING_GOAL = "Invalid daypart weighting goal.";
        const string INVALID_COVERAGE_GOAL = "Invalid coverage goal value.";
        const string INVALID_TOTAL_MARKET_COVERAGE = "Invalid total market coverage.";
        const string INVALID_MARKET_SHARE_OF_VOICE = "Invalid share of voice for market.";
        private const string INVALID_TOTAL_MARKET_SHARE_OF_VOICE = "Invalid total market share of voice.";
        const string INVALID_REQUEST = "Invalid request";
        public const string INVALID_IMPRESSIONS_COUNT = "The impressions count is different between the delivery and the weekly breakdown";
        public const string INVALID_SOV_COUNT = "The share of voice count is not equal to 100%";
        const string INVALID_FLIGHT_NOTES = "Flight notes cannot be longer than 1024 characters.";
        const string INVALID_INTERNAL_FLIGHT_NOTES = "Internal flight notes cannot be longer than 1024 characters.";
        const string STOP_WORD_DETECTED = "Stop word detected in plan name";
        const string SUM_OF_DAYPART_WEIGHTINGS_EXCEEDS_LIMIT = "Sum of weighting is greater than 100%";
        const string INVALID_DAYPART_DUPLICATE_DAYPART = "Invalid dayparts.  Each daypart can be entered only once.";
        const string INVALID_BUDGET = "Invalid budget.";
        const string INVALID_CPM = "Invalid CPM.";
        const string INVALID_CPP = "Invalid CPP.";
        const string INVALID_DELIVERY_IMPRESSIONS = "Invalid Delivery Impressions.";
        const string INVALID_IMPRESSIONS_PER_UNIT = "Impressions per Unit must be less or equal to delivery impressions.";
        const string INVALID_DRAFT_ON_NEW_PLAN = "Cannot create a new draft on a non existing plan.";
        const string STOP_WORD = "eOm3wgvfm0dq4rI3srL2";

        const string SHOW_TYPE_CONTAIN_TYPE_IS_NOT_VALID = "Contain type of the show types restrictions is not valid";
        const string GENRE_CONTAIN_TYPE_IS_NOT_VALID = "Contain type of the genres restrictions is not valid";
        const string PROGRAM_CONTAIN_TYPE_IS_NOT_VALID = "Contain type of the program restrictions is not valid";
        const string AFFILIATE_CONTAIN_TYPE_IS_NOT_VALID = "Contain type of the affiliate restrictions is not valid";

        const string INVALID_CUSTOM_DAYPART_NAME = "Invalid daypart name";
        const string INVALID_DAYPART_WEIGHT_TOTAL = "Sum Weight of all Dayparts must equal 100%.";

        const string INVALID_FLIGHT_CROSS_QUARTERS_PRICING = "Cross-Quarter flighting is invalid for Pricing.";
        const string INVALID_FLIGHT_CROSS_QUARTERS_BUYING = "Cross-Quarter flighting is invalid for Buying.";

        const string ADU_PLAN_INVALID_ADUS = "Invalid ADUs for this type of Plan.  Expecting total ADUs greater than 0.";

        public PlanValidator(IBroadcastAudiencesCache broadcastAudiencesCache
            , IRatingForecastService ratingForecastService
            , IDataRepositoryFactory broadcastDataRepositoryFactory
            , ICreativeLengthEngine creativeLengthEngine
            , IAabEngine aabEngine
            , IFeatureToggleHelper featureToggleHelper
            , IPlanMarketSovCalculator planMarketSovCalculator
            , IQuarterCalculationEngine quarterCalculationEngine
            )
        {
            _RatingForecastService = ratingForecastService;
            _AudienceCache = broadcastAudiencesCache;
            _CreativeLengthEngine = creativeLengthEngine;
            _PostingBooks = ratingForecastService.GetMediaMonthCrunchStatuses()
                .Where(a => a.Crunched == CrunchStatusEnum.Crunched)
                .Select(m => m.MediaMonth)
                .ToList();

            _CampaignRepository = broadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();

            _AabEngine = aabEngine;
            _FeatureToggleHelper = featureToggleHelper;
            _PlanMarketSovCalculator = planMarketSovCalculator;
            _QuarterCalculationEngine = quarterCalculationEngine;
        }

        /// <inheritdoc/>
        public void ValidatePlan(PlanDto plan)
        {
            if (string.IsNullOrWhiteSpace(plan.Name) || plan.Name.Length > 255)
            {
                throw new PlanValidationException(INVALID_PLAN_NAME);
            }

            if ((plan.VersionId == 0 || plan.Id == 0) && plan.IsDraft == true)
            {
                throw new PlanValidationException(INVALID_DRAFT_ON_NEW_PLAN);
            }

            _ValidateCreativeLengths(plan.CreativeLengths);
            _ValidateProduct(plan);
            _ValidateFlightAndHiatus(plan);
            _ValidateCustomDayparts(plan);
            _ValidateDayparts(plan);            
            _ValidatePrimaryAudience(plan);
            _ValidateSecondaryAudiences(plan.SecondaryAudiences, plan.AudienceId);
            _ValidateMarkets(plan);
            _ValidateWeeklyBreakdownWeeks(plan);
            _ValidateBudgetAndDelivery(plan);
            ValidateImpressionsPerUnit(plan.ImpressionsPerUnit.Value, plan.TargetImpressions.Value);

            // PRI-14012 We'll use a stop word so QA can trigger an error 
            _ValidateStopWord(plan);
        }

        /// <inheritdoc/>
        public void ValidatePlanDraft(PlanDto plan)
        {
            if (string.IsNullOrWhiteSpace(plan.Name) || plan.Name.Length > 255)
            {
                throw new PlanValidationException(INVALID_PLAN_NAME);
            }

            _ValidateFlightAndHiatus(plan);
        }        

        /// <inheritdoc/>
        public void ValidatePlanForPricing(PlanDto plan)
        {
            _ValidateFlightAndHiatus(plan);
            _ValidateDayparts(plan);
            _ValidatePrimaryAudience(plan);
            //_ValidateWeeklyBreakdownWeeks(plan);
            _ValidateMarkets(plan);
        }

        /// <inheritdoc/>
        public void ValidatePlanForBuying(PlanDto plan)
        {
            _ValidateFlightAndHiatus(plan);
            _ValidateDayparts(plan);
            _ValidatePrimaryAudience(plan);
            //_ValidateWeeklyBreakdownWeeks(plan);
            _ValidateMarkets(plan);
        }

        /// <inheritdoc/>
        public void ValidateAduPlan(PlanDto plan)
        {
            if (string.IsNullOrWhiteSpace(plan.Name) || plan.Name.Length > 255)
            {
                throw new PlanValidationException(INVALID_PLAN_NAME);
            }

            if ((plan.VersionId == 0 || plan.Id == 0) && plan.IsDraft == true)
            {
                throw new PlanValidationException(INVALID_DRAFT_ON_NEW_PLAN);
            }

            _ValidateCreativeLengths(plan.CreativeLengths);
            _ValidateProduct(plan);
            _ValidateFlightAndHiatus(plan);
            _ValidateCustomDayparts(plan);
            _ValidateDayparts(plan);
            _ValidatePrimaryAudience(plan);
            _ValidateSecondaryAudiences(plan.SecondaryAudiences, plan.AudienceId);
            _ValidateMarkets(plan);

            _ValidateWeeklyBreakdownWeeksForAduPlan(plan);

            // PRI-14012 We'll use a stop word so QA can trigger an error 
            _ValidateStopWord(plan);
        }

        private void _ValidateWeeklyBreakdownWeeksForAduPlan(PlanDto plan)
        {
            // There has to be some ADUs in any Week.
            var totalAdus = plan.WeeklyBreakdownWeeks.Sum(s => s.WeeklyAdu);
            if (totalAdus < 1)
            {
                throw new PlanValidationException(ADU_PLAN_INVALID_ADUS);
            }
        }

        /// <inheritdoc/>
        public void ValidatePlanNotCrossQuartersForPricing(PlanDto plan)
        {
            if (!plan.FlightStartDate.HasValue || !plan.FlightEndDate.HasValue)
            {
                return;
            }

            var quarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(plan.FlightStartDate.Value, plan.FlightEndDate.Value);
            if (quarters.Count > 1)
            {
                throw new PlanValidationException(INVALID_FLIGHT_CROSS_QUARTERS_PRICING);
            }
        }

        /// <inheritdoc/>
        public void ValidatePlanNotCrossQuartersForBuying(PlanDto plan)
        {
            if (!plan.FlightStartDate.HasValue || !plan.FlightEndDate.HasValue)
            {
                return;
            }

            var quarters = _QuarterCalculationEngine.GetAllQuartersBetweenDates(plan.FlightStartDate.Value, plan.FlightEndDate.Value);
            if (quarters.Count > 1)
            {
                throw new PlanValidationException(INVALID_FLIGHT_CROSS_QUARTERS_BUYING);
            }
        }

        private void _ValidateStopWord(PlanDto plan)
        {
            if (plan.Name.Contains(STOP_WORD))
            {
                throw new PlanValidationException(STOP_WORD_DETECTED);
            }
        }

        ///<inheritdoc/>
        public void ValidateWeeklyBreakdown(WeeklyBreakdownRequest request)
        {
            if (request == null)
            {
                throw new PlanValidationException(INVALID_REQUEST);
            }

            if (request.FlightEndDate.Equals(DateTime.MinValue) || request.FlightStartDate.Equals(DateTime.MinValue))
            {
                throw new PlanValidationException(INVALID_FLIGHT_DATE);
            }

            if (request.FlightEndDate < request.FlightStartDate)
            {
                throw new PlanValidationException(INVALID_FLIGHT_DATES);
            }

            if (request.FlightDays == null || !request.FlightDays.Any())
            {
                throw new PlanValidationException(INVALID_FLIGHT_DAYS);
            }

            var isClearAll = request.Weeks.All(x => x.IsUpdated || x.IsLocked);

            if (request.DeliveryType != PlanGoalBreakdownTypeEnum.EvenDelivery &&
                request.Weeks.Count(w => w.IsUpdated) > 1 && !isClearAll)
            {
                throw new PlanValidationException("More than one updated week found.");
            }

            if (request.TotalImpressions <= 0 && !isClearAll  && !request.IsAduOnly)
            {
                throw new PlanValidationException("Total impressions must be more than zero");
            }

            _ValidateWeeklyBreakdownDeliveryTypeAndWeeks(request.DeliveryType, request.Weeks, request.Dayparts);
        }

        private void _ValidateFlightAndHiatus(PlanDto plan)
        {
            if (!plan.FlightStartDate.HasValue || !plan.FlightEndDate.HasValue)
            {
                throw new PlanValidationException(INVALID_FLIGHT_DATE);
            }

            if (plan.FlightStartDate > plan.FlightEndDate)
            {
                throw new PlanValidationException(INVALID_FLIGHT_DATES);
            }

            if (plan.FlightDays == null || !plan.FlightDays.Any())
            {
                throw new PlanValidationException(INVALID_FLIGHT_DAYS);
            }

            if (plan.FlightHiatusDays?.Any() == true)
            {
                var hasInvalids =
                    plan.FlightHiatusDays.Any(h => h.Date < plan.FlightStartDate || h.Date > plan.FlightEndDate);
                if (hasInvalids)
                {
                    throw new PlanValidationException(INVALID_FLIGHT_HIATUS_DAY);
                }

                var hasInvalidHiatusDaysWithFlightDays =
                    plan.FlightHiatusDays
                    .Select(hiatus => (int)hiatus.GetBroadcastDayOfWeek()).Distinct()
                    .Any(hiatusDay => !plan.FlightDays.Contains(hiatusDay));
                if (hasInvalidHiatusDaysWithFlightDays)
                {
                    throw new PlanValidationException(INVALID_FLIGHT_HIATUS_DAY_WITH_FLIGHT_DAYS);
                }

            }

            if (!string.IsNullOrEmpty(plan.FlightNotes) && plan.FlightNotes.Length > 1024)
                throw new PlanValidationException(INVALID_FLIGHT_NOTES);

            if (!string.IsNullOrEmpty(plan.FlightNotesInternal) && plan.FlightNotesInternal.Length > 1024)
                throw new PlanValidationException(INVALID_INTERNAL_FLIGHT_NOTES);

            var planFlightStartDateDayofWeek = (int)plan.FlightStartDate.Value.GetBroadcastDayOfWeek();
            var planFlightEndDateDayofWeek = (int)plan.FlightEndDate.Value.GetBroadcastDayOfWeek();
            if (!plan.FlightDays.Any(day => day == planFlightStartDateDayofWeek || day == planFlightEndDateDayofWeek))
            {
                throw new PlanValidationException(INVALID_FLIGHT_DATES_WITH_FLIGHT_DAYS);
            }
        }

        private void _ValidatePrimaryAudience(PlanDto plan)
        {
            if (!_AudienceCache.IsValidAudience(plan.AudienceId))
            {
                throw new PlanValidationException(INVALID_AUDIENCE);
            }

            if (!_PostingBooks.Any(x => x.Id == plan.ShareBookId))
            {
                throw new PlanValidationException(INVALID_SHARE_BOOK);
            }

            //if the hutbook is set but it's 0 or a value not available throw exception
            if (plan.HUTBookId.HasValue && (plan.HUTBookId <= 0 || !_PostingBooks.Any(x => x.Id == plan.HUTBookId)))
            {
                throw new PlanValidationException(INVALID_HUT_BOOK);
            }

            if (plan.HUTBookId.HasValue)
            {
                var shareBook = _PostingBooks.Single(x => x.Id == plan.ShareBookId);
                var hutBook = _PostingBooks.Single(x => x.Id == plan.HUTBookId);
                if (hutBook.StartDate > shareBook.StartDate)
                {
                    throw new PlanValidationException(INVALID_SHARE_HUT_BOOKS);
                }
            }
        }

        private void _ValidateSecondaryAudiences(List<PlanAudienceDto> secondaryAudiences, int? primaryAudienceId)
        {
            var distinctAudiences = new List<int?> { primaryAudienceId };
            foreach (var secondaryAudience in secondaryAudiences)
            {
                if (!_AudienceCache.IsValidAudience(secondaryAudience.AudienceId))
                    throw new PlanValidationException(INVALID_AUDIENCE);

                if (distinctAudiences.Contains(secondaryAudience.AudienceId))
                    throw new PlanValidationException(INVALID_AUDIENCE_DUPLICATE);

                distinctAudiences.Add(secondaryAudience.AudienceId);
            }
        }

        private void _ValidateCustomDayparts(PlanDto plan)
        {
            if (plan.Dayparts?.Any() != true)
            {
                throw new PlanValidationException(INVALID_DAYPART_NUMBER);
            }

            if (plan.Dayparts.Where(daypart => EnumHelper.IsCustomDaypart(daypart.DaypartTypeId.GetDescriptionAttribute())).Any(n => string.IsNullOrWhiteSpace(n.CustomName)))
            {
                throw new PlanValidationException(INVALID_CUSTOM_DAYPART_NAME);
            }

            if (plan.Dayparts.Where(daypart => EnumHelper.IsCustomDaypart(daypart.DaypartTypeId.GetDescriptionAttribute())).GroupBy(d => new { d.DaypartOrganizationId, d.CustomName }).Any(g => g.Count() > 1))
            {
                throw new PlanValidationException(INVALID_DAYPART_DUPLICATE_DAYPART);
            }
        }

        private void _ValidateDayparts(PlanDto plan)
        {
            const int daySecondsMin = 0;
            const int daySecondsMax = BroadcastConstants.OneDayInSeconds - 1;
            if (plan.Dayparts?.Any() != true)
            {
                throw new PlanValidationException(INVALID_DAYPART_NUMBER);
            }

            if (plan.Dayparts.Where(daypart => !EnumHelper.IsCustomDaypart(daypart.DaypartTypeId.GetDescriptionAttribute())).GroupBy(d => d.DaypartCodeId).Any(g => g.Count() > 1))
            {
                throw new PlanValidationException(INVALID_DAYPART_DUPLICATE_DAYPART);
            }

            foreach (var daypart in plan.Dayparts)
            {
                if (daypart.StartTimeSeconds < daySecondsMin || daypart.StartTimeSeconds > daySecondsMax)
                {
                    throw new PlanValidationException(INVALID_DAYPART_TIMES);
                }

                if (daypart.EndTimeSeconds < daySecondsMin || daypart.EndTimeSeconds > daySecondsMax)
                {
                    throw new PlanValidationException(INVALID_DAYPART_TIMES);
                }

                const double minWeightingGoalPercent = 0.1;
                const double maxWeightingGoalPercent = 100.0;
                if (daypart.WeightingGoalPercent.HasValue &&
                    (daypart.WeightingGoalPercent.Value < minWeightingGoalPercent
                     || daypart.WeightingGoalPercent.Value > maxWeightingGoalPercent))
                {
                    throw new PlanValidationException(INVALID_DAYPART_WEIGHTING_GOAL);
                }

                if ((daypart.WeekdaysWeighting.HasValue && !daypart.WeekendWeighting.HasValue) ||
                    (!daypart.WeekdaysWeighting.HasValue && daypart.WeekendWeighting.HasValue))
                {
                    throw new PlanValidationException("Weekdays weighting and weekend weighting must be either both set or both must be nulls");
                }

                if (daypart.WeekdaysWeighting.HasValue &&
                    daypart.WeekendWeighting.HasValue &&
                    (daypart.WeekdaysWeighting.Value + daypart.WeekendWeighting.Value) != 100)
                {
                    throw new PlanValidationException("Weekdays weighting and weekend weighting must sum up to 100");
                }

                _ValidatePlanDaypartRestrictions(daypart);
                _ValidatePlanDaypartAudienceVpvh(daypart);
            }

            var sumOfDaypartWeighting = plan.Dayparts.Aggregate(0d, (sumOfWeighting, dayPart) => sumOfWeighting + dayPart.WeightingGoalPercent.GetValueOrDefault());
            if (sumOfDaypartWeighting > 100)
            {
                throw new PlanValidationException(SUM_OF_DAYPART_WEIGHTINGS_EXCEEDS_LIMIT);
            }
        }

        private void _ValidatePlanDaypartAudienceVpvh(PlanDaypartDto planDaypartDto)
        {
            var vpvhForAudiences = planDaypartDto.VpvhForAudiences;

            foreach (var vpvhForAudience in vpvhForAudiences)
            {
                if (vpvhForAudience.Vpvh < 0)
                    throw new PlanValidationException("VPVH can not be less than zero");

                if (!EnumHelper.IsDefined(vpvhForAudience.VpvhType))
                    throw new PlanValidationException("Unknown VPVH type was discovered");

                if (vpvhForAudience.StartingPoint == default)
                    throw new PlanValidationException("StartingPoint is a required property");

                if (vpvhForAudience.VpvhType == VpvhTypeEnum.Custom)
                {
                    if (vpvhForAudience.Vpvh < 0.001 || vpvhForAudience.Vpvh > 10)
                    {
                        throw new PlanValidationException("VPVH must be between 0.001 and 10");
                    }
                }
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
                throw new PlanValidationException(SHOW_TYPE_CONTAIN_TYPE_IS_NOT_VALID);
            }
        }

        private void _ValidateGenreRestrictions(PlanDaypartDto.RestrictionsDto restrictions)
        {
            var genreRestrictions = restrictions.GenreRestrictions;

            if (genreRestrictions != null && !EnumHelper.IsDefined(genreRestrictions.ContainType))
            {
                throw new PlanValidationException(GENRE_CONTAIN_TYPE_IS_NOT_VALID);
            }
        }

        private void _ValidateProgramRestrictions(PlanDaypartDto.RestrictionsDto restrictions)
        {
            var programRestrictions = restrictions.ProgramRestrictions;

            if (programRestrictions != null && !EnumHelper.IsDefined(programRestrictions.ContainType))
            {
                throw new PlanValidationException(PROGRAM_CONTAIN_TYPE_IS_NOT_VALID);
            }
        }

        private void _ValidateAffiliateRestrictions(PlanDaypartDto.RestrictionsDto restrictions)
        {
            var affiliateRestrictions = restrictions.AffiliateRestrictions;

            if (affiliateRestrictions != null && !EnumHelper.IsDefined(affiliateRestrictions.ContainType))
            {
                throw new PlanValidationException(AFFILIATE_CONTAIN_TYPE_IS_NOT_VALID);
            }
        }

        private void _ValidateMarkets(PlanDto plan)
        {
            if (!plan.CoverageGoalPercent.HasValue
                || plan.CoverageGoalPercent.Value <= 0
                || plan.CoverageGoalPercent.Value > 100.0)
            {
                throw new PlanValidationException(INVALID_COVERAGE_GOAL);
            }

            plan.AvailableMarkets.ForEach(m =>
                _ValidateOptionalPercentage(m.ShareOfVoicePercent, INVALID_MARKET_SHARE_OF_VOICE));

            var totalMarketCoverage = Math.Round(plan.AvailableMarkets.Sum(m => m.PercentageOfUS), 1);
            var coverageGoalRounded = Math.Round(plan.CoverageGoalPercent.Value, 1);

            if (totalMarketCoverage < coverageGoalRounded)
            {
                throw new PlanValidationException(INVALID_TOTAL_MARKET_COVERAGE);
            }
                const double maxTotalMarketSov = 100.01;
                var marketsSovTotalExceedsThreshold = _PlanMarketSovCalculator.DoesMarketSovTotalExceedThreshold(plan.AvailableMarkets, maxTotalMarketSov);
                if (marketsSovTotalExceedsThreshold)
                {
                    throw new PlanValidationException(INVALID_TOTAL_MARKET_SHARE_OF_VOICE);
                }
        }

        private void _ValidateOptionalPercentage(double? candidate, string errorMessage)
        {
            if (candidate.HasValue &&
                (candidate <= 0 || candidate > 100.0))
            {
                throw new PlanValidationException(errorMessage);
            }
        }

        internal void _ValidateWeeklyBreakdownWeeks(PlanDto plan)
        {
            if (!plan.WeeklyBreakdownWeeks.Any())
            {
                return;
            }

            var roundedTargetImpressions = Math.Floor(plan.TargetImpressions.GetValueOrDefault());
            var roundedWeeklyImpressionsSum = Math.Floor(plan.WeeklyBreakdownWeeks.Select(x => x.WeeklyImpressions).Sum());
            if (roundedTargetImpressions.Equals(roundedWeeklyImpressionsSum) == false)
            {
                throw new PlanValidationException(INVALID_IMPRESSIONS_COUNT);
            }
            //We do not validate percentages or rating points since those are all derived from the impressions

            _ValidateWeeklyBreakdownDeliveryTypeAndWeeks(plan.GoalBreakdownType, plan.WeeklyBreakdownWeeks, plan.Dayparts);
        }

        private void _ValidateWeeklyBreakdownDeliveryTypeAndWeeks(PlanGoalBreakdownTypeEnum deliveryType, List<WeeklyBreakdownWeek> weeklyBreakdown, List<PlanDaypartDto> dayparts)
        {
            if (deliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                if (dayparts?.Any() != true)
                    throw new PlanValidationException("For the chosen delivery type, dayparts are required");

                if (weeklyBreakdown.Any(x => !x.DaypartCodeId.HasValue))
                    throw new PlanValidationException("For the chosen delivery type, each weekly breakdown row must have daypart associated with it");

                var weeklyBreakdownHasSeveralRowsWithTheSameWeekAndDaypart = weeklyBreakdown
                   .GroupBy(x => new { x.MediaWeekId, x.DaypartCodeId })
                   .Select(x => x.Count())
                   .Any(x => x > 1);

                if (weeklyBreakdownHasSeveralRowsWithTheSameWeekAndDaypart && weeklyBreakdown.Any(x => !x.DaypartCodeId.HasValue) && weeklyBreakdown.Any(x => !x.SpotLengthId.HasValue))
                    throw new PlanValidationException("For the chosen delivery type, each week and daypart combination must be unique");
            }
            else if (deliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                if (weeklyBreakdown.Any(x => !x.SpotLengthId.HasValue) && weeklyBreakdown.Any(x => !x.SpotLengthDuration.HasValue))
                    throw new PlanValidationException("For the chosen delivery type, each weekly breakdown row must have spot length associated with it");

                if (weeklyBreakdown.Any(x => !x.PercentageOfWeek.HasValue))
                    throw new PlanValidationException("For the chosen delivery type, each weekly breakdown row must have percentage of week set");

                var weeklyBreakdownHasSeveralRowsWithTheSameWeekAndSpotLength = weeklyBreakdown
                    .GroupBy(x => new { x.MediaWeekId, x.SpotLengthId })
                    .Select(x => x.Count())
                    .Any(x => x > 1);

                if (weeklyBreakdownHasSeveralRowsWithTheSameWeekAndSpotLength && weeklyBreakdown.Any(x => !x.DaypartCodeId.HasValue) && weeklyBreakdown.Any(x => !x.SpotLengthId.HasValue))
                    throw new PlanValidationException("For the chosen delivery type, each week and spot Length combination must be unique");
            }
        }

        private void _ValidateProduct(PlanDto plan)
        {
            try
            {
                var campaign = _CampaignRepository.GetCampaign(plan.CampaignId);
                _AabEngine.GetAdvertiserProduct(campaign.AdvertiserMasterId.Value, plan.ProductMasterId.Value);
            }
            catch (Exception ex)
            {
                throw new PlanValidationException(INVALID_PRODUCT, ex);
            }
        }

        private void _ValidateBudgetAndDelivery(PlanDto plan)
        {
            if (!(plan.Budget.HasValue && plan.Budget.Value > 0m))
            {
                throw new PlanValidationException(INVALID_BUDGET);
            }

            if (!(plan.TargetCPM.HasValue && plan.TargetCPM.Value > 0m))
            {
                throw new PlanValidationException(INVALID_CPM);
            }

            if (!(plan.TargetCPP.HasValue && plan.TargetCPP.Value > 0m))
            {
                throw new PlanValidationException(INVALID_CPP);
            }

            if (!(plan.TargetImpressions.HasValue && plan.TargetImpressions.Value > 0d))
            {
                throw new PlanValidationException(INVALID_DELIVERY_IMPRESSIONS);
            }
        }

        public void ValidateImpressionsPerUnit(double impressionsPerUnit, double totalImpressions)
        {
            if (impressionsPerUnit <= 0 || impressionsPerUnit > totalImpressions)
            {
                throw new PlanValidationException(INVALID_IMPRESSIONS_PER_UNIT);
            }
        }

        private void _ValidateCreativeLengths(List<CreativeLength> creativeLengths)
        {
            try
            {
                _CreativeLengthEngine.ValidateCreativeLengthsForPlanSave(creativeLengths);
            }
            catch (Exception ex)
            {
                throw new PlanValidationException(ex.Message, ex);
            }
        }

        public void ValidateWeeklyBreakdownItemWeights(List<StandardDaypartWeightingGoal> dayparts, List<CreativeLength> creativeLengths, PlanGoalBreakdownTypeEnum deliveryType)
        {
            if (deliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByAdLength)
            {
                _CreativeLengthEngine.ValidateCreativeLengthsForPlanSave(creativeLengths);
            }
            else if (deliveryType == PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart)
            {
                var totalDaypartWeights = dayparts.Sum(d => d.WeightingGoalPercent);
                if (totalDaypartWeights != 100)
                {
                    throw new ApplicationException(INVALID_DAYPART_WEIGHT_TOTAL);
                }
            }
        }
    }
}
