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
using Services.Broadcast.Extensions;
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
        /// <param name="isciPlanProductMapping">The object which contains save parameters</param>
        /// /// <param name="createdBy">Created By</param>
        /// <returns>true or false</returns>
        bool SaveIsciMappings(IsciPlanProductMappingDto isciPlanProductMapping, string createdBy);

        IsciPlanMappingDetailsDto GetPlanIsciMappingsDetails(int planId);
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

        private Lazy<bool> _EnablePlanIsciByWeek;

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

            _StandardDaypartService = standardDaypartService;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DateTimeEngine = dateTimeEngine;
            _AabEngine = aabEngine;
            _PlanService = planService;
            _CampaignService = campaignService;
            _SpotLengthEngine = spotLengthEngine;

            _EnablePlanIsciByWeek = new Lazy<bool>(_GetEnablePlanIsciByWeek);
        }

        private bool _GetEnablePlanIsciByWeek()
        {
            var result = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PLAN_ISCI_BY_WEEK);
            return result;
        }

        /// <inheritdoc />
        public List<IsciListItemDto> GetAvailableIscis(IsciSearchDto isciSearch) {
            var isciListDto = new List<IsciListItemDto>();
            var isciAdvertiserListDto = new List<IsciAdvertiserDto>();

            DateTime queryStartDate;
            DateTime queryEndDate;

            if (_EnablePlanIsciByWeek.Value)
            {
                if (!isciSearch.WeekStartDate.HasValue || !isciSearch.WeekEndDate.HasValue)
                {
                    throw new InvalidOperationException("WeekStartDate and WeekEndDate are both required.");
                }

                queryStartDate = isciSearch.WeekStartDate.Value;
                queryEndDate = isciSearch.WeekEndDate.Value;
            }
            else
            {
                var mediaMonthsDates = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(isciSearch.MediaMonth.Id);
                queryStartDate = mediaMonthsDates.StartDate;
                queryEndDate = mediaMonthsDates.EndDate;
            }
                
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

            if (_EnablePlanIsciByWeek.Value)
            {
                if (!isciPlanSearch.WeekStartDate.HasValue || !isciPlanSearch.WeekEndDate.HasValue)
                {
                    throw new InvalidOperationException("WeekStartDate and WeekEndDate are both required.");
                }

                queryStartDate = isciPlanSearch.WeekStartDate.Value;
                queryEndDate = isciPlanSearch.WeekEndDate.Value;
            }
            else
            {
                var mediaMonthsDates = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(isciPlanSearch.MediaMonth.Id);
                queryStartDate = mediaMonthsDates.StartDate;
                queryEndDate = mediaMonthsDates.EndDate;
            }
            
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

        public IsciPlanMappingDetailsDto GetPlanIsciMappingsDetails(int planId)
        {
            var plan = _PlanService.GetPlan(planId);
            var campaign = _CampaignService.GetCampaignById(plan.CampaignId);
            var productName = _GetProductName(campaign.AdvertiserMasterId.Value, plan.ProductMasterId.Value);
            var daypartsString = _GetDaypartCodesString(plan.Dayparts);
            var spotLengthsString = _GetSpotLengthsString(plan.CreativeLengths);
            var demoString = _GetAudienceString(plan.AudienceId);
            var flightString = _GetFlightString(plan.FlightStartDate.Value, plan.FlightEndDate.Value);

            var mappedIscis = _GetIsciPlanMappingIsciDetailsDto(planId, plan.FlightStartDate.Value, plan.FlightEndDate.Value);

            var mappingsDetails = new IsciPlanMappingDetailsDto
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
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

        private List<IsciPlanMappingIsciDetailsDto> _GetIsciPlanMappingIsciDetailsDto(int planId, DateTime planStartDate, DateTime planEndDate)
        {
            var planIscis = _PlanIsciRepository.GetPlanIscis(planId);
            var iscis = planIscis.Select(s => s.Isci).ToList();
            var isciDetails = _PlanIsciRepository.GetIsciDetails(iscis);
            var mappedIsciDetails = _ResolvePlanMappedIscis(planStartDate, planEndDate, isciDetails);

            mappedIsciDetails.ForEach(s =>
            {
                s.SpotLengthString = _GetSpotLengthsString(s.SpotLengthId);
                s.FlightString = _GetFlightString(s.FlightStartDate, s.FlightEndDate);
            });

            return mappedIsciDetails;
        }

        internal List<IsciPlanMappingIsciDetailsDto> _ResolvePlanMappedIscis(DateTime planStartDate, DateTime planEndDate, List<IsciPlanMappingIsciDetailsDto> iscis)
        {
            var result = new List<IsciPlanMappingIsciDetailsDto>();

            var unknowns = new List<IsciPlanMappingIsciDetailsDto>();
            foreach (var isci in iscis)
            {
                var overlap = _GetOverlappingDateRange(new DateRange(planStartDate, planEndDate), 
                    new DateRange(isci.ActiveStartDate, isci.ActiveEndDate));

                // it's mapped, but doesn't overlap
                if (overlap.IsEmpty())
                {
                    unknowns.Add(isci);
                    continue;
                }
                else
                {
                    isci.FlightStartDate = overlap.Start.Value;
                    isci.FlightEndDate = overlap.End.Value;
                }
                
                result.Add(isci);
            }

            foreach (var isci in unknowns)
            {
                // only add it once
                if (result.Select(s => s.Isci).ToList().Contains(isci.Isci))
                {
                    continue;
                }
                // set it per the plan's dimensions
                isci.FlightStartDate = planStartDate;
                isci.FlightEndDate = planEndDate;
                result.Add(isci);
            }

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
