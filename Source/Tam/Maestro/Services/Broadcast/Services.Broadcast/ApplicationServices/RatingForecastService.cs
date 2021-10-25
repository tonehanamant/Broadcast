using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

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
    }

    public class RatingForecastService : IRatingForecastService
    {
        private readonly IMediaMonthCrunchCache _MediaMonthCrunchCache;

        internal static string IdenticalHutAndShareMessage = "Hut and Share Media Month {0} cannot be identical";
        internal static string HutMediaMonthMustBeEarlierLessThanThanShareMediaMonth = "Hut Media Month {0} must be earlier (less than) than Share Media Month {1}";
        internal static string NoSelectedDaysMessage = "The following programs have dayparts with no selected days:\n {0}";
        internal static string DuplicateProgramsMessage = "The following programs are duplicates:\n {0} ";

        public RatingForecastService(IMediaMonthCrunchCache mediaMonthCrunchCache)
        {
            _MediaMonthCrunchCache = mediaMonthCrunchCache;
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
    }
}
