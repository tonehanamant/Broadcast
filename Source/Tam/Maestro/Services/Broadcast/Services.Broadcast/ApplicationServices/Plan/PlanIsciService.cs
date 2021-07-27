﻿using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Repositories;
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

        /// <summary>
        /// A data mock of the return from GetAvailableIscis
        /// </summary>
        /// <param name="isciSearch">The isci search.</param>
        List<IsciListItemDto> GetAvailableIscisMock(IsciSearchDto isciSearch);

        /// <summary>
        /// Gets the available plans for Isci mapping
        /// </summary>
        /// <param name="isciPlanSearch">The object which contains search parameters</param>
        /// <returns>List of IsciPlanResultDto object</returns>
        List<IsciPlanResultDto> GetAvailableIsciPlans(IsciPlanSearchDto isciPlanSearch);
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
        private readonly IAabEngine _AabEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanIsciService"/> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        /// <param name="mediaMonthAndWeekAggregateCache">The media month and week aggregate cache.</param>
        /// <param name="dateTimeEngine">The date time engine.</param>
        /// <param name="aabEngine">The Aab engine.</param>
        public PlanIsciService(IDataRepositoryFactory dataRepositoryFactory, 
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache, 
            IDateTimeEngine dateTimeEngine,
            IAabEngine aabEngine)
        {
            _PlanIsciRepository = dataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DateTimeEngine = dateTimeEngine;
            _AabEngine = aabEngine;
        }

        /// <inheritdoc />
        public List<IsciListItemDto> GetAvailableIscis(IsciSearchDto isciSearch)
        {
            List<IsciListItemDto> isciListDto = new List<IsciListItemDto>();
            var mediaMonthsDates = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(isciSearch.MediaMonth.Id);
            var result = _PlanIsciRepository.GetAvailableIscis(mediaMonthsDates.StartDate, mediaMonthsDates.EndDate);
            if (result?.Any() ?? false)
            {
                var resultlamba = result.GroupBy(stu => stu.AdvertiserName).OrderBy(stu => stu.Key);
                foreach (var group in resultlamba)
                {
                    IsciListItemDto isciListItemDto = new IsciListItemDto();
                    isciListItemDto.AdvertiserName = group.Key;
                    foreach (var item in group)
                    {
                        isciListItemDto.Iscis = new List<IsciDto>();
                        IsciDto isciItemDto = new IsciDto();
                        isciItemDto.Isci = item.Isci;
                        isciItemDto.SpotLengthsString = $":{item.SpotLengthDuration}";
                        isciItemDto.ProductName = item.ProductName;
                        isciListItemDto.Iscis.Add(isciItemDto);
                    }
                    isciListDto.Add(isciListItemDto);
                }
            }
            return isciListDto;
        }

        /// <inheritdoc />
        public List<IsciListItemDto> GetAvailableIscisMock(IsciSearchDto isciSearch)
        {
            var isciListDtos = new List<IsciListItemDto>
            {
                new IsciListItemDto
                {
                    AdvertiserName = "Advertiser1",
                    Iscis = new List<IsciDto>
                    {
                        new IsciDto
                        {
                            Id = 1,
                            Isci = "ABC123",
                            SpotLengthsString = ":15",
                            ProductName = "Product123"
                        }
                    }
                },
                // One advertiser with multiple iscis
                new IsciListItemDto
                {
                    AdvertiserName = "Advertiser2",
                    Iscis = new List<IsciDto>
                    {
                        new IsciDto
                        {
                            Id = 2,
                            Isci = "BC123",
                            SpotLengthsString = ":15",
                            ProductName = "Product124"
                        },
                        new IsciDto
                        {
                            Id = 3,
                            Isci = "BC126",
                            SpotLengthsString = ":30",
                            ProductName = "Product125"
                        }
                    }
                },
                // same Isci, different advertisers
                new IsciListItemDto
                {
                    AdvertiserName = "Advertiser3",
                    Iscis = new List<IsciDto>
                    {
                        new IsciDto
                        {
                            Id = 1,
                            Isci = "ABC123",
                            SpotLengthsString = ":15",
                            ProductName = "Product123"
                        }
                    }
                },
                // With and Without Products
                new IsciListItemDto
                {
                    AdvertiserName = "Advertiser4",
                    Iscis = new List<IsciDto>
                    {
                        new IsciDto
                        {
                            Id = 4,
                            Isci = "DTO5323",
                            SpotLengthsString = ":30",
                            ProductName = "Product666"
                        },
                        new IsciDto
                        {
                            Id = 5,
                            Isci = "DTO6868",
                            SpotLengthsString = ":15",
                            ProductName = null
                        },
                        new IsciDto
                        {
                            Id = 6,
                            Isci = "DTO8868",
                            SpotLengthsString = ":30",
                            ProductName = null
                        },
                        new IsciDto
                        {
                            Id = 7,
                            Isci = "DFR9865",
                            SpotLengthsString = ":30",
                            ProductName = null
                        }
                    }
                },
            };

            if (isciSearch.WithoutPlansOnly)
            {
                isciListDtos.ForEach(s =>
                {
                    var withoutProduct = s.Iscis.Where(l => string.IsNullOrWhiteSpace(l.ProductName)).ToList();
                    s.Iscis = withoutProduct;
                });
                var stillHasIscis = isciListDtos.Where(s => s.Iscis.Any()).ToList();
                isciListDtos = stillHasIscis;
            }

            return isciListDtos;
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

        /// <inheritdoc />
        public List<IsciPlanResultDto> GetAvailableIsciPlans(IsciPlanSearchDto isciPlanSearch)
        {
            var isciPlanResults = new List<IsciPlanResultDto>();
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";

            var searchedMediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(isciPlanSearch.MediaMonth.Id);
            var isciPlans = _PlanIsciRepository.GetAvailableIsciPlans(searchedMediaMonth.StartDate, searchedMediaMonth.EndDate);
            if (isciPlans?.Any() ?? false)
            {
                _SetIsciPlanAdvertiser(isciPlans);

                var isciPlansGroupedByAdvertiser = isciPlans.GroupBy(x => x.AdvertiserName).OrderBy(x => x.Key);
                foreach (var isciPlanItem in isciPlansGroupedByAdvertiser)
                {
                    var isciPlanResult = new IsciPlanResultDto()
                    {
                        AdvertiserName = isciPlanItem.Key,
                        IsciPlans = isciPlanItem.OrderBy(x => x.FlightStartDate).Select(isciPlanDetail =>
                        {
                            var isciPlan = new IsciPlanDto()
                            {
                                Id = isciPlanDetail.Id,
                                SpotLengthsString = string.Join(", ", isciPlanDetail.SpotLengthValues.Select(x => $":{x}")),
                                DemoString = isciPlanDetail.AudienceCode,
                                Title = isciPlanDetail.Title,
                                DaypartsString = string.Join(", ", isciPlanDetail.Dayparts),
                                ProductName = isciPlanDetail.ProductName,
                                FlightString = $"{isciPlanDetail.FlightStartDate.ToString(flightStartDateFormat)}-{isciPlanDetail.FlightEndDate.ToString(flightEndDateFormat)}",
                                Iscis = isciPlanDetail.Iscis
                            };
                            return isciPlan;
                        }).ToList()
                    };
                    isciPlanResults.Add(isciPlanResult);
                }
            }
            return isciPlanResults;
        }

        private void _SetIsciPlanAdvertiser(List<IsciPlanDetailDto> isciPlanSummaries)
        {
            var advertisers = _AabEngine.GetAdvertisers();
            isciPlanSummaries.ForEach(x => x.AdvertiserName = advertisers.SingleOrDefault(y => y.MasterId == x.AdvertiserMasterId)?.Name);
        }        
    }
}
