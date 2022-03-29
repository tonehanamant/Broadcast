using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface IRatingForecastService : IApplicationService
    {
        List<MediaMonthCrunchStatus> GetMediaMonthCrunchStatuses();
        void ClearMediaMonthCrunchCache();

        /// <summary>
        /// Returns posting books
        /// </summary>
        /// <returns>List of LookupDto objects containing posting books</returns>
        List<LookupDto> GetPostingBooks();

        RatingsForecastResponseDto ForecastRatings(RatingsForecastRequestDto request);
    }

    public class RatingForecastService : IRatingForecastService
    {
        private readonly IMediaMonthCrunchCache _MediaMonthCrunchCache;
        private readonly IRatingForecastRepository _RatingForecastRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;

        internal static string IdenticalHutAndShareMessage = "Hut and Share Media Month {0} cannot be identical";
        internal static string HutMediaMonthMustBeEarlierLessThanThanShareMediaMonth = "Hut Media Month {0} must be earlier (less than) than Share Media Month {1}";
        internal static string NoSelectedDaysMessage = "The following programs have dayparts with no selected days:\n {0}";
        internal static string DuplicateProgramsMessage = "The following programs are duplicates:\n {0} ";

        public RatingForecastService(IMediaMonthCrunchCache mediaMonthCrunchCache, IDataRepositoryFactory dataRepositoryFactory)
        {
            _MediaMonthCrunchCache = mediaMonthCrunchCache;
            _RatingForecastRepository = dataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            _BroadcastAudienceRepository = dataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
        }

        public void ClearMediaMonthCrunchCache()
        {
            MediaMonthCrunchCache.Instance.ClearMediaMonthCache();
        }
        public List<MediaMonthCrunchStatus> GetMediaMonthCrunchStatuses()
        {
            return _MediaMonthCrunchCache.GetMediaMonthCrunchStatuses();
        }

        /// <summary>
        /// Returns posting books
        /// </summary>
        /// <returns>List of LookupDto objects containing posting books</returns>
        public List<LookupDto> GetPostingBooks()
        {
            return _MediaMonthCrunchCache.GetMediaMonthCrunchStatuses()
                .Where(m => m.Crunched == CrunchStatusEnum.Crunched).Select(m => new LookupDto(m.MediaMonth.Id, m.MediaMonth.MediaMonthX))
                .ToList();
        }

        public RatingsForecastResponseDto ForecastRatings(RatingsForecastRequestDto request)
        {

            var audiencesMappings = _BroadcastAudienceRepository
                    .GetRatingsAudiencesByMaestroAudience(new List<int> { request.AudiencId }).ToList();

            var audiences = audiencesMappings.Select(s => s.rating_audience_id).ToList();
            var stationDayparts = new List<ManifestDetailDaypart>
            { 
                new ManifestDetailDaypart
                {
                    Id = 1,
                    LegacyCallLetters = request.StationLegacyCallLetters,
                    DisplayDaypart = new DisplayDaypart
                    {
                        StartTime = request.StartTimeSeconds,
                        EndTime = request.EndTimeSeconds,
                        Monday = request.Monday,
                        Tuesday = request.Tuesday,
                        Wednesday = request.Wednesday,
                        Thursday = request.Thursday,
                        Friday = request.Friday,
                        Saturday = request.Saturday,
                        Sunday = request.Sunday,
                    }
                }
            };

            var projectedResult = _RatingForecastRepository.GetImpressionsDaypart(request.HutMediaMonth, 
                request.ShareMediaMonth, audiences, stationDayparts, request.PlaybackType);

            var totalImpressions = projectedResult.Impressions.Sum(s => s.Impressions);

            var response = new RatingsForecastResponseDto
            {
                ProjectedImpressions = totalImpressions
            };
            return response;
        }
    }
}
