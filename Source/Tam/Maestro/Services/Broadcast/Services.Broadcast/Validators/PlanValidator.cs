using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

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
        const string INVALID_HUT_BOOK = "Invalid HUT boook. The HUT book must be prior to share book";
        const string INVALID_FLIGHT_DATES = "Invalid flight dates.  The end date cannot be before the start date.";
        const string INVALID_FLIGHT_HIATUS_DAY = "Invalid flight hiatus day.  All days must be within the flight date range.";
        const string INVALID_AUDIENCE = "Invalid audience";
        const string INVALID_SHARE_HUT_BOOKS = "HUT Book must be prior to Share Book";
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
            if(plan.ProductId <= 0)
            {
                throw new Exception(INVALID_PRODUCT);
            }

            _ValidateFlightAndHiatusDates(plan);
            _ValidateAudiences(plan);
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
        
        private void _ValidateAudiences(PlanDto plan)
        {   
            if (!_AudienceCache.IsValidAudience(plan.AudienceId))
            {
                throw new Exception(INVALID_AUDIENCE);
            }
            if (!_PostingBooks.Any(x=>x.Id == plan.ShareBookId)){
                throw new Exception(INVALID_SHARE_BOOK);
            }

            //if the hutbook is set but it's 0 or a value not available throw exception
            if (plan.HUTBookId.HasValue && (plan.HUTBookId <= 0 || !_PostingBooks.Any(x => x.Id == plan.HUTBookId))){
                throw new Exception(INVALID_HUT_BOOK);
            }

            if (plan.HUTBookId.HasValue)
            {
                var shareBook = _PostingBooks.Single(x => x.Id == plan.ShareBookId);
                var hutBook = _PostingBooks.Single(x => x.Id == plan.HUTBookId);
                if(hutBook.StartDate > shareBook.StartDate)
                {
                    throw new Exception(INVALID_SHARE_HUT_BOOKS);
                }
            }
        }

        #endregion // #region Helpers
    }
}
