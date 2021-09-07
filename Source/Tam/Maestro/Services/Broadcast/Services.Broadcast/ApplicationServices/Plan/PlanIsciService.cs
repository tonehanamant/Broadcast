using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Helpers;
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
        List<IsciPlanResultDto> GetAvailableIsciPlans(IsciSearchDto isciPlanSearch);

        /// <summary>
        /// Save Isci mapping
        /// </summary>
        /// <param name="isciPlanProductMapping">The object which contains save parameters</param>
        /// /// <param name="createdBy">Created By</param>
        /// <returns>true or false</returns>
        bool SaveIsciMappings(IsciPlanProductMappingDto isciPlanProductMapping, string createdBy);
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
        /// <param name="featureToggleHelper"></param>
        /// <param name="configurationSettingsHelper"></param>
        public PlanIsciService(IDataRepositoryFactory dataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IDateTimeEngine dateTimeEngine,
            IAabEngine aabEngine, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
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
            List<IsciAdvertiserDto> isciAdvertiserListDto = new List<IsciAdvertiserDto>();
            var mediaMonthsDates = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(isciSearch.MediaMonth.Id);
            isciAdvertiserListDto = _PlanIsciRepository.GetAvailableIscis(mediaMonthsDates.StartDate, mediaMonthsDates.EndDate);
            if (isciAdvertiserListDto?.Any() ?? false)
            {
                if (isciSearch.UnmappedOnly)
                {
                    var isciAdvertiserListDtoWithoutPlan = isciAdvertiserListDto.Where(x => x.PlanIsci == 0).ToList();
                    isciAdvertiserListDto = isciAdvertiserListDtoWithoutPlan;
                }
                if (isciAdvertiserListDto?.Any() ?? false)
                {
                    var resultlamba = isciAdvertiserListDto.GroupBy(stu => new { stu.AdvertiserName, stu.Isci }).Select(x => x.LastOrDefault()).GroupBy(x => x.AdvertiserName).OrderBy(x => x.Key).ToList();

                    foreach (var group in resultlamba)
                    {
                        IsciListItemDto isciListItemDto = new IsciListItemDto();
                        isciListItemDto.AdvertiserName = group.Key;
                        foreach (var item in group)
                        {
                            IsciDto isciItemDto = new IsciDto();
                            isciItemDto.Id = item.Id;
                            isciItemDto.Isci = item.Isci;
                            isciItemDto.SpotLengthsString = $":{item.SpotLengthDuration}";
                            isciItemDto.ProductName = item.ProductName;
                            isciListItemDto.Iscis.Add(isciItemDto);
                        }
                        isciListDto.Add(isciListItemDto);
                    }
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

            if (isciSearch.UnmappedOnly)
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
        public List<IsciPlanResultDto> GetAvailableIsciPlans(IsciSearchDto isciPlanSearch)
        {
            var isciPlanResults = new List<IsciPlanResultDto>();
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";

            var searchedMediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(isciPlanSearch.MediaMonth.Id);
            var isciPlans = _PlanIsciRepository.GetAvailableIsciPlans(searchedMediaMonth.StartDate, searchedMediaMonth.EndDate);
            if (isciPlans?.Any() ?? false)
            {
                if (isciPlanSearch.UnmappedOnly)
                {
                    var IsciPlanDetailsDtoWithoutPlan = isciPlans.Where(x => x.Iscis.Count == 0).ToList();
                    isciPlans = IsciPlanDetailsDtoWithoutPlan;
                }
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
            }
            return isciPlanResults;
        }

        private void _SetIsciPlanAdvertiser(List<IsciPlanDetailDto> isciPlanSummaries)
        {
            var advertisers = _AabEngine.GetAdvertisers();
            isciPlanSummaries.ForEach(x => x.AdvertiserName = advertisers.SingleOrDefault(y => y.MasterId == x.AdvertiserMasterId)?.Name);
        }

        public bool SaveIsciMappings(IsciPlanProductMappingDto isciPlanProductMapping, string createdBy)
        {
            if (isciPlanProductMapping == null)
            {
                _LogWarning($"Called with null parameters.");
                return false;
            }
            var createdAt = _DateTimeEngine.GetCurrentMoment();
            var deletedAt = _DateTimeEngine.GetCurrentMoment();
            bool isSave = false;
            int isciProductMappingCount = 0;
            int isciPlanMappingCount = 0;
            int isciPlanMappingsDeletedCount = 0;
            if (isciPlanProductMapping.IsciProductMappings.Count > 0)
            {
                var resolvedIsciProductMappings = _RemoveDuplicateIsciProducts(isciPlanProductMapping.IsciProductMappings);
                if (resolvedIsciProductMappings.Any())
                {
                    isciProductMappingCount = _PlanIsciRepository.SaveIsciProductMappings(resolvedIsciProductMappings, createdBy, createdAt);
                }
            }
            _LogInfo($"{isciProductMappingCount } IsciProductMappings are stored into database");
            var isciPlanMappings = _RemoveDuplicateListItem(isciPlanProductMapping);
            _LogInfo($"{isciPlanMappingCount } IsciPlanMappings are stored into database");
            if (isciPlanProductMapping.IsciPlanMappingsDeleted.Count > 0)
            {
                isciPlanMappingsDeletedCount = _PlanIsciRepository.DeleteIsciPlanMappings(isciPlanProductMapping.IsciPlanMappingsDeleted, createdBy, deletedAt);
            }
            _LogInfo($"{isciPlanMappingsDeletedCount } IsciPlanMappings are deleted from database");
            if (isciPlanMappings.Count > 0)
            {
                isciPlanMappingCount = _PlanIsciRepository.SaveIsciPlanMappings(isciPlanMappings, createdBy, createdAt);
                
                if (isciPlanMappingCount > 0)
                {
                    isSave = true;
                }
                return isSave;
            }
            else
            {
                return isSave;
            }
        }

        private List<IsciProductMappingDto> _RemoveDuplicateIsciProducts(List<IsciProductMappingDto> isciProducts)
        {
            // remove duplicates within the list, keeping the first one
            var dedupped = new List<IsciProductMappingDto>();
            isciProducts.ForEach(p =>
                {
                    if (!dedupped.Any(d => d.Isci.Equals(p.Isci)))
                    {
                        dedupped.Add(p);
                    }
                });

            // if already in the db then ignore
            var iscis = dedupped.Select(s => s.Isci).ToList();
            var existing = _PlanIsciRepository.GetIsciProductMappings(iscis);
            if (!existing.Any())
            {
                return dedupped;
            }

            var dbDedupped = new List<IsciProductMappingDto>();
            var existingIscis = existing.Select(s => s.Isci).ToList();
            dedupped.ForEach(p =>
            {
                if (!existingIscis.Contains(p.Isci))
                {
                    dbDedupped.Add(p);
                }
            });
            return dbDedupped;
        }

        private List<IsciPlanMappingDto> _RemoveDuplicateListItem(IsciPlanProductMappingDto isciPlanProductMapping)
        {
            var isciPlanMappingList = isciPlanProductMapping.IsciPlanMappings.Select(item => new { item.PlanId, item.Isci }).Distinct();

            var isciPlanMappings = _PlanIsciRepository.GetPlanIscis();

            var isciPlanMappingUniqueList = isciPlanMappingList.Where(x => !isciPlanMappings.Any(y => y.PlanId == x.PlanId && y.Isci == x.Isci)).ToList();

            List<IsciPlanMappingDto> isciPlanMappingResult = isciPlanMappingUniqueList
                .Select(x => new IsciPlanMappingDto
                {
                    PlanId = x.PlanId,
                    Isci = x.Isci
                }).ToList();

            return isciPlanMappingResult;
        }
    }
}
