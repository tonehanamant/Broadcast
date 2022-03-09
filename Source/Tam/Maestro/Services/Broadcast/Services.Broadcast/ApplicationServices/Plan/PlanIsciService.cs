using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
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

        /// <summary>
        /// Gets the available plans for Isci mapping
        /// </summary>
        /// <param name="isciPlanSearch">The object which contains search parameters</param>
        /// <returns>List of IsciPlanResultDto object</returns>
        List<IsciPlanResultDto> GetAvailableIsciPlans(IsciSearchDto isciPlanSearch);

        /// <summary>
        /// Save Isci mapping
        /// </summary>
        /// <param name="saveRequest">The object which contains save parameters</param>
        /// /// <param name="createdBy">Created By</param>
        /// <returns>true or false</returns>
        bool SaveIsciMappings(IsciPlanMappingsSaveRequestDto saveRequest, string createdBy);

        PlanIsciMappingsDetailsDto GetPlanIsciMappingsDetails(int planId);
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
        private readonly IPlanService _PlanService;
        private readonly ICampaignService _CampaignService;
        private readonly IStandardDaypartService _StandardDaypartService;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IAudienceRepository _AudienceRepository;
        private readonly IReelIsciRepository _ReelIsciRepository;
        private readonly IReelIsciProductRepository _ReelIsciProductRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanIsciService"/> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        /// <param name="mediaMonthAndWeekAggregateCache">The media month and week aggregate cache.</param>
        /// <param name="planService">The plan service.</param>
        /// <param name="campaignService">The campaign service.</param>
        /// <param name="standardDaypartService">The standard daypart service.</param>
        /// <param name="spotLengthEngine">The spot length engine.</param>
        /// <param name="dateTimeEngine">The date time engine.</param>
        /// <param name="aabEngine">The aab engine.</param>
        /// <param name="featureToggleHelper">The feature toggle helper.</param>
        /// <param name="configurationSettingsHelper">The configuration settings helper.</param>
        public PlanIsciService(IDataRepositoryFactory dataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IPlanService planService,
            ICampaignService campaignService,
            IStandardDaypartService standardDaypartService,
            ISpotLengthEngine spotLengthEngine,
            IDateTimeEngine dateTimeEngine,
            IAabEngine aabEngine, IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _PlanIsciRepository = dataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            _AudienceRepository = dataRepositoryFactory.GetDataRepository<IAudienceRepository>();
            _ReelIsciRepository = dataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            _ReelIsciProductRepository = dataRepositoryFactory.GetDataRepository<IReelIsciProductRepository>();

            _StandardDaypartService = standardDaypartService;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DateTimeEngine = dateTimeEngine;
            _AabEngine = aabEngine;
            _PlanService = planService;
            _CampaignService = campaignService;
            _SpotLengthEngine = spotLengthEngine;
        }

        /// <inheritdoc />
        public List<IsciListItemDto> GetAvailableIscis(IsciSearchDto isciSearch)
        {
            var isciListDto = new List<IsciListItemDto>();
            var isciAdvertiserListDto = new List<IsciAdvertiserDto>();

            DateTime queryStartDate;
            DateTime queryEndDate;

            if (!isciSearch.WeekStartDate.HasValue || !isciSearch.WeekEndDate.HasValue)
            {
                throw new InvalidOperationException("WeekStartDate and WeekEndDate are both required.");
            }

            queryStartDate = isciSearch.WeekStartDate.Value;
            queryEndDate = isciSearch.WeekEndDate.Value;

            isciAdvertiserListDto = _PlanIsciRepository.GetAvailableIscis(queryStartDate, queryEndDate);
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

            DateTime queryStartDate;
            DateTime queryEndDate;

            if (!isciPlanSearch.WeekStartDate.HasValue || !isciPlanSearch.WeekEndDate.HasValue)
            {
                throw new InvalidOperationException("WeekStartDate and WeekEndDate are both required.");
            }

            queryStartDate = isciPlanSearch.WeekStartDate.Value;
            queryEndDate = isciPlanSearch.WeekEndDate.Value;

            var isciPlans = _PlanIsciRepository.GetAvailableIsciPlans(queryStartDate, queryEndDate);
            if (isciPlans?.Any() ?? false)
            {
                if (isciPlanSearch.UnmappedOnly)
                {
                    var isciPlanDetailsDtoWithoutPlan = isciPlans.Where(x => x.Iscis.Count == 0).ToList();
                    isciPlans = isciPlanDetailsDtoWithoutPlan;
                }
                if (isciPlans?.Any() ?? false)
                {
                    _SetIsciPlanAdvertiser(isciPlans);
                    _SetIsciPlanAdvertiserProduct(isciPlans);

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
            isciPlanSummaries.ForEach(x =>
                x.AdvertiserName = advertisers.SingleOrDefault(y => y.MasterId == x.AdvertiserMasterId)?.Name);
        }

        private void _SetIsciPlanAdvertiserProduct(List<IsciPlanDetailDto> isciPlanSummaries)
        {
            isciPlanSummaries.ForEach(item =>
            {
                var product = _AabEngine.GetAdvertiserProduct(item.AdvertiserMasterId.Value, item.ProductMasterId.Value);

                item.ProductName = product.Name;
            });
        }

        private int _HandleSaveIsciProduct(List<IsciProductMappingDto> mappings, string createdBy, DateTime createdAt)
        {
            if (!mappings.Any())
            {
                return 0;
            }

            var dedupedMappings = _RemoveDuplicateIsciProducts(mappings);
            if (!dedupedMappings.Any())
            {
                return 0;
            }

            var savedCount = _ReelIsciProductRepository.SaveIsciProductMappings(dedupedMappings, createdBy, createdAt);
            return savedCount;
        }

        private int _HandleDeleteIsciPlanMapping(List<int> toDelete, string deletedBy, DateTime deletedAt)
        {
            if (!toDelete.Any())
            {
                return 0;
            }

            var deletedCount = _PlanIsciRepository.DeleteIsciPlanMappings(toDelete, deletedBy, deletedAt);
            return deletedCount;
        }

        private int _CleanupOrphanedIsciProductMappings(List<int> candidateIds)
        {
            if (!candidateIds.Any())
            {
                return 0;
            }

            var isciPlanCounts = _PlanIsciRepository.GetIsciPlanMappingCounts(candidateIds);
            var toCleanup = isciPlanCounts.Where(i => i.MappedPlanCount == 0).Select(s => s.Isci).ToList();
            if (!toCleanup.Any())
            {
                return 0;
            }

            var cleanedCount = _ReelIsciProductRepository.DeleteIsciProductMapping(toCleanup);
            return cleanedCount;
        }

        private int _HandleNewIsciPlanMapping(List<IsciPlanMappingDto> mappings, string createdBy, DateTime createdAt)
        {
            if (!mappings.Any())
            {
                return 0;
            }

            var dedupedListMappings = _RemoveDuplicateListItemMappings(mappings);
            var flightedMappings = _PopulateIsciPlanMappingFlights(dedupedListMappings);
            var toSaveMappings = _RemoveDuplicateDatabaseMappings(flightedMappings);
            var separatedMappings = _SeparateSoftDeletedIsciMappings(toSaveMappings);

            var unDeletedCount = 0;
            if (separatedMappings.ToUnDeleteIds.Any())
            {
                unDeletedCount = _PlanIsciRepository.UnDeleteIsciPlanMappings(separatedMappings.ToUnDeleteIds);
            }

            var savedCount = 0;
            if (separatedMappings.ToSave.Any())
            {
                savedCount = _PlanIsciRepository.SaveIsciPlanMappings(separatedMappings.ToSave, createdBy, createdAt);
            }

            var totalChangedCount = unDeletedCount + savedCount;
            return totalChangedCount;
        }

        internal IsciPlanMappingModifiedCountsDto _HandleModifiedIsciPlanMappings(List<IsciPlanModifiedMappingDto> modified, DateTime modifiedAt, string modifiedBy)
        {
            var duplicateCount = 0;
            var noDuplicateCount = 0;
            var noDuplicateList = new List<PlanIsciDto>();
            var duplicateList = new List<PlanIsciDto>();
            var result = new IsciPlanMappingModifiedCountsDto();

            if (!modified.Any())
            {
                return result;
            }

            var potentialDuplicates = _PlanIsciRepository.GetPlanIsciDuplicates(modified);


            modified.ForEach(m =>
            {
                if (!potentialDuplicates.Any())
                {
                    var isci = _PlanIsciRepository.GetPlanIscisByMappingId(m.PlanIsciMappingId);
                    noDuplicateList = isci.Select(p => new PlanIsciDto
                    {
                        Id = m.PlanIsciMappingId,
                        PlanId = p.PlanId,
                        Isci = p.Isci,
                        FlightStartDate = m.FlightStartDate,
                        FlightEndDate = m.FlightEndDate
                    }).ToList();
                }
                else
                {
                    noDuplicateList = potentialDuplicates
                        .Where(p => p.DeletedAt != null)
                        .Select(p => new PlanIsciDto
                        {
                            Id = m.PlanIsciMappingId,
                            PlanId = p.PlanId,
                            Isci = p.Isci,
                            FlightStartDate = m.FlightStartDate,
                            FlightEndDate = m.FlightEndDate
                        }).ToList();

                    duplicateList = potentialDuplicates
                        .Where(p => p.DeletedAt == null)
                        .Select(p => new PlanIsciDto
                        {
                            Id = p.Id,
                            PlanId = p.PlanId,
                            Isci = p.Isci,
                            FlightStartDate = m.FlightStartDate,
                            FlightEndDate = m.FlightEndDate
                        }).ToList();
                }
            });

            if (noDuplicateList.Any())
            {
                noDuplicateCount = _PlanIsciRepository.UpdateIsciPlanMappings(noDuplicateList, modifiedAt, modifiedBy);
            }

            if (duplicateList.Any())
            {
                duplicateCount = _PlanIsciRepository.UpdateIsciPlanMappings(duplicateList, modifiedAt, modifiedBy);
            }

            result = new IsciPlanMappingModifiedCountsDto
            {
                TotalChangedCount = noDuplicateCount + duplicateCount,
                NoDuplicateCount = noDuplicateCount,
                DuplicateCount = duplicateCount
            };

            return result;
        }

        public bool SaveIsciMappings(IsciPlanMappingsSaveRequestDto saveRequest, string createdBy)
        {
            var createdAt = _DateTimeEngine.GetCurrentMoment();
            var deletedAt = _DateTimeEngine.GetCurrentMoment();
            var modifiedAt = _DateTimeEngine.GetCurrentMoment();

            var isciPlanMappingsDeletedCount = _HandleDeleteIsciPlanMapping(saveRequest.IsciPlanMappingsDeleted, createdBy, deletedAt);
            _LogInfo($"{isciPlanMappingsDeletedCount } IsciPlanMappings are deleted.");

            var addedCount = _HandleNewIsciPlanMapping(saveRequest.IsciPlanMappings, createdBy, createdAt);
            _LogInfo($"{addedCount} IsciPlanMappings were added.");

            var modifiedCount = _HandleModifiedIsciPlanMappings(saveRequest.IsciPlanMappingsModified, modifiedAt, createdBy);
            _LogInfo($"{modifiedCount} IsciPlanMappings were modified.");

            var isciProductMappingCount = _HandleSaveIsciProduct(saveRequest.IsciProductMappings, createdBy, createdAt);
            _LogInfo($"{isciProductMappingCount } IsciProductMappings were saved.");

            var orphanedProductMappingCount = _CleanupOrphanedIsciProductMappings(saveRequest.IsciPlanMappingsDeleted);
            _LogInfo($"{orphanedProductMappingCount } OrphanedProductMappingCount were cleaned up.");

            return true;
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
            var existing = _ReelIsciProductRepository.GetIsciProductMappings(iscis);
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

        /// <summary>
        /// Removes duplicate entries from the given list.
        /// </summary>
        private List<IsciPlanMappingDto> _RemoveDuplicateListItemMappings(List<IsciPlanMappingDto> mappings)
        {
            var result = new List<IsciPlanMappingDto>();
            mappings.ForEach(m =>
            {
                if (!result.Any(s => s.PlanId.Equals(m.PlanId) && s.Isci.Equals(m.Isci)))
                {
                    result.Add(m);
                }
            });
            return result;
        }

        private SeparatedMappings _SeparateSoftDeletedIsciMappings(List<PlanIsciDto> flightedMappings)
        {
            var result = new SeparatedMappings();
            var planIds = flightedMappings.Select(m => m.PlanId).Distinct().ToList();
            var softDeletedIscis = _PlanIsciRepository.GetDeletedPlanIscis(planIds);

            foreach (var candidate in softDeletedIscis)
            {
                foreach (var mapping in flightedMappings)
                {
                    if (mapping.Equals(candidate))
                    {
                        result.ToUnDeleteIds.Add(candidate.Id);
                        // set the id to indicate we can soft delete it
                        mapping.Id = candidate.Id;
                    }
                }
            }

            result.ToSave = flightedMappings;
            result.ToSave.RemoveAll(s => s.Id > 0);
            return result;
        }

        private class SeparatedMappings
        {
            public List<PlanIsciDto> ToSave { get; set; } = new List<PlanIsciDto>();
            public List<int> ToUnDeleteIds { get; set; } = new List<int>();
        }

        /// <summary>
        /// Removes the list items if already exist in teh database.
        /// </summary>
        private List<PlanIsciDto> _RemoveDuplicateDatabaseMappings(List<PlanIsciDto> mappings)
        {
            var planIds = mappings.Select(s => s.PlanId).Distinct().ToList();
            var planIscis = _PlanIsciRepository.GetPlanIscis(planIds);

            var result = new List<PlanIsciDto>();

            mappings.ForEach(m =>
            {
                if (!planIscis.Any(s =>
                    s.PlanId.Equals(m.PlanId) &&
                    s.Isci.Equals(m.Isci) &&
                    _GetOverlappingDateRange(new DateRange(m.FlightStartDate, m.FlightEndDate)
                        ,new DateRange(s.FlightStartDate, s.FlightEndDate)) != null
                    ))
                {
                    result.Add(m);
                }
            });
            return result;
        }

        /// <summary>
        /// Populates the mappings flight dates.  Creates multiple entries if needed.
        /// </summary>
        private List<PlanIsciDto> _PopulateIsciPlanMappingFlights(List<IsciPlanMappingDto> isciPlanMappings)
        {
            var result = new List<PlanIsciDto>();

            var iscis = isciPlanMappings.Select(s => s.Isci).Distinct().ToList();
            var allReelIscis = _ReelIsciRepository.GetReelIscis(iscis);

            foreach (var mapping in isciPlanMappings)
            {
                var plan = _PlanService.GetPlan(mapping.PlanId);
                var reelIsciDetails = allReelIscis.Where(s => s.Isci.Equals(mapping.Isci)).ToList();

                var fullMappings = _PopulateIsciPlanMappingFlightsForMapping(plan.Id, plan.FlightStartDate.Value, plan.FlightEndDate.Value, reelIsciDetails);

                result.AddRange(fullMappings);
            }

            return result;
        }

        internal List<PlanIsciDto> _PopulateIsciPlanMappingFlightsForMapping(int planId, DateTime planFlightStartDate, DateTime planFlightEndDate,
            List<ReelIsciDto> reelIsciDetails)
        {
            var result = new List<PlanIsciDto>();

            foreach (var reelIsci in reelIsciDetails)
            {
                var overlap = _GetOverlappingDateRange(new DateRange(planFlightStartDate, planFlightEndDate),
                    new DateRange(reelIsci.ActiveStartDate, reelIsci.ActiveEndDate));

                if (overlap.IsEmpty())
                {
                    continue;
                }

                result.Add(new PlanIsciDto
                {
                    PlanId = planId,
                    Isci = reelIsci.Isci,
                    FlightStartDate = overlap.Start.Value,
                    FlightEndDate = overlap.End.Value
                });
            }

            return result;
        }

        public PlanIsciMappingsDetailsDto GetPlanIsciMappingsDetails(int planId)
        {
            var plan = _PlanService.GetPlan(planId);
            var campaign = _CampaignService.GetCampaignById(plan.CampaignId);
            var advertiserName = _GetAdvertiserName(campaign.AdvertiserMasterId.Value);
            var productName = _GetProductName(campaign.AdvertiserMasterId.Value, plan.ProductMasterId.Value);
            var daypartsString = _GetDaypartCodesString(plan.Dayparts);
            var spotLengthsString = _GetSpotLengthsString(plan.CreativeLengths);
            var demoString = _GetAudienceString(plan.AudienceId);
            var flightString = _GetFlightString(plan.FlightStartDate.Value, plan.FlightEndDate.Value);

            var mappedIscis = _GetIsciPlanMappingIsciDetailsDto(planId, plan.FlightStartDate.Value, plan.FlightEndDate.Value);

            var mappingsDetails = new PlanIsciMappingsDetailsDto
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                AdvertiserName = advertiserName,
                ProductName = productName,
                SpotLengthString = spotLengthsString,
                DaypartCode = daypartsString,
                DemoString = demoString,
                FlightStartDate = plan.FlightStartDate.Value,
                FlightEndDate = plan.FlightEndDate.Value,
                FlightString = flightString,
                MappedIscis = mappedIscis
            };
            return mappingsDetails;
        }

        private string _GetProductName(Guid advertiserMasterId, Guid productMasterId)
        {
            var product = _AabEngine.GetAdvertiserProduct(advertiserMasterId, productMasterId);
            return product.Name;
        }

        private string _GetAdvertiserName(Guid advertiserMasterId)
        {
            var advertiser = _AabEngine.GetAdvertiser(advertiserMasterId);
            return advertiser.Name;
        }

        private List<PlanMappedIsciDetailsDto> _GetIsciPlanMappingIsciDetailsDto(int planId, DateTime planStartDate, DateTime planEndDate)
        {
            var planIscis = _PlanIsciRepository.GetPlanIscis(planId);
            var iscis = planIscis.Select(s => s.Isci).ToList();
            var isciSpotLengths = _PlanIsciRepository.GetIsciSpotLengths(iscis);

            var result = planIscis.Select(i =>
            {
                // we are using .First() here as a reel isci should never have same isci with different spot id.
                var spotLengthId = isciSpotLengths.Where(s => s.Isci.Equals(i.Isci)).Select(s => s.SpotLengthId).First();
                var spotLengthString = _GetSpotLengthsString(spotLengthId);
                var flightString = _GetFlightString(i.FlightStartDate, i.FlightEndDate);
                var availabilityString = _GetFlightString(i.ActiveStartDate, i.ActiveEndDate);
                var item = new PlanMappedIsciDetailsDto
                {
                    PlanIsciMappingId = i.Id,
                    Isci = i.Isci,
                    SpotLengthId = spotLengthId,
                    SpotLengthString = spotLengthString,
                    FlightStartDate = i.FlightStartDate,
                    FlightEndDate = i.FlightEndDate,
                    FlightString = flightString,
                    AvailabilityStartDate = i.ActiveStartDate,
                    AvailabilityEndDate = i.ActiveEndDate,
                    AvailabilityString = availabilityString
                };
                return item;
            })
            .ToList();
            return result;
        }

        internal static DateRange _GetOverlappingDateRange(DateRange planFlightDateRange, DateRange isciActiveDateRange)
        {
            var result = new DateRange();

            if (isciActiveDateRange.Start > planFlightDateRange.End ||
                isciActiveDateRange.End < planFlightDateRange.Start)
            {
                return result;
            }

            result.Start = planFlightDateRange.Start >= isciActiveDateRange.Start
                ? planFlightDateRange.Start
                : isciActiveDateRange.Start;

            result.End = planFlightDateRange.End <= isciActiveDateRange.End
                ? planFlightDateRange.End
                : isciActiveDateRange.End;

            return result;
        }

        internal string _GetFlightString(DateTime startDate, DateTime endDate)
        {
            const string fullFormat = "MM/dd/yyyy";
            const string shortFormat = "MM/dd";
            var startDateString = startDate.Year == endDate.Year
                ? startDate.ToString(shortFormat)
                : startDate.ToString(fullFormat);
            var endDateString = endDate.ToString(fullFormat);

            var result = $"{startDateString} - {endDateString}";
            return result;
        }

        private string _GetAudienceString(int audienceId)
        {
            var audience = _AudienceRepository.GetAudiencesByIds(new List<int> {audienceId}).First();
            return audience.Display;
        }

        private string _GetDaypartCodesString(List<PlanDaypartDto> planDayparts)
        {
            var planDaypartIds = planDayparts.Select(d => d.DaypartCodeId).ToList();

            var allStandardDayparts = _StandardDaypartService.GetAllStandardDayparts();
            var daypartCodes = allStandardDayparts
                .Where(d => planDaypartIds.Contains(d.Id))
                .Select(d => d.Code)
                .ToList();

            var result = string.Join(",", daypartCodes);
            return result;
        }

        private string _GetSpotLengthsString(List<CreativeLength> spotLengths)
        {
            var formattedLengths = spotLengths
                .Select(s => _GetSpotLengthsString(s.SpotLengthId))
                .ToList();

            var result = string.Join(",", formattedLengths);
            return result;
        }

        private string _GetSpotLengthsString(int spotLengthId)
        {
            var result = $":{_SpotLengthEngine.GetSpotLengthValueById(spotLengthId)}";
            return result;
        }
    }
}
