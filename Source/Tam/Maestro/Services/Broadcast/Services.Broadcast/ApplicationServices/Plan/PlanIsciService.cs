using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanIsciService : IApplicationService
    {
        /// <summary>
        /// list of Iscis based on search key.
        /// </summary>
        /// <param name="isciSearch">Isci search input</param>       
        List<IsciListItemDto> GetAvailableIscis(IsciSearchDto isciSearch);

        /// <summary>
        /// Gets media months
        /// </summary>
        /// <returns>List of MediaMonthDto object</returns>
        List<MediaMonthDto> GetMediaMonths();
    }
    /// <summary>
    /// Operations related to the PlanIsci domain.
    /// </summary>
    /// <seealso cref="IPlanIsciService" />
    public class PlanIsciService : BroadcastBaseClass, IPlanIsciService
    {

        private readonly IPlanIsciRepository _PlanIsciRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IDateTimeEngine _DateTimeEngine;
        public PlanIsciService(IDataRepositoryFactory dataRepositoryFactory, IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache, IDateTimeEngine dateTimeEngine)
        {
            _PlanIsciRepository = dataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DateTimeEngine = dateTimeEngine;
        }
        /// <inheritdoc />
        public List<IsciListItemDto> GetAvailableIscis(IsciSearchDto isciSearch)
        {

            List<IsciListItemDto> isciListDto = new List<IsciListItemDto>();
            IsciListItemDto isciListItemDto = new IsciListItemDto();
            IsciDto IsciItemDto = new IsciDto();
            var result = _PlanIsciRepository.GetAvailableIscis(isciSearch);
            var resultlamba = result.GroupBy(stu => stu.AdvertiserName).OrderBy(stu => stu.Key);
            foreach (var group in resultlamba)
            {
                isciListItemDto.AdvertiserName = group.Key;
                foreach (var item in group)
                {
                    IsciItemDto.Isci = item.Isci;
                    IsciItemDto.SpotLengthsString = Convert.ToString(item.SpotLengthsString);
                    IsciItemDto.ProductName = item.ProductName;
                    isciListItemDto.Iscis.Add(IsciItemDto);

                }
                isciListDto.Add(isciListItemDto);
            }
            return isciListDto;

        }

        /// <inheritdoc />
        public List<MediaMonthDto> GetMediaMonths()
        {
            var endDate = _DateTimeEngine.GetCurrentMoment();
            var startDate = endDate.AddMonths(-12);
            var mediaMonthsBetweenDatesInclusive = _MediaMonthAndWeekAggregateCache.GetMediaMonthsBetweenDatesInclusive(startDate, endDate);
            var last12MediaMonths = mediaMonthsBetweenDatesInclusive.OrderByDescending(x => x.EndDate).Take(12).Select(_ToMediaMonthDto).ToList();
            return last12MediaMonths;
        }

        private MediaMonthDto _ToMediaMonthDto(MediaMonth mediaMonth)
        {
            return new MediaMonthDto
            {
                Id = mediaMonth.Id,
                Year = mediaMonth.Year,
                Month = mediaMonth.Month
            };
        }
    }

}
