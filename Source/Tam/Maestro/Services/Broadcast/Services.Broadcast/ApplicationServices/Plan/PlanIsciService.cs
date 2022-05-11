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

        private int _HandleDeleteIsciPlanMapping(List<int> toDelete, string deletedBy, DateTime deletedAt)
        {
            if (!toDelete.Any())
            {
                return 0;
            }

            var deletedCount = _PlanIsciRepository.DeleteIsciPlanMappings(toDelete, deletedBy, deletedAt);
            return deletedCount;
        }

        private int _HandleNewIsciPlanMapping(List<IsciPlanMappingDto> mappings, string createdBy, DateTime createdAt)
        {
            if (!mappings.Any())
            {
                return 0;
            }
                  
            List<PlanIsciDto> toSave = _PoplateListItemMappings(mappings);
            var totalChangedCount = _PlanIsciRepository.SaveIsciPlanMappings(toSave, createdBy, createdAt);            
            return totalChangedCount;
        }

        internal int _HandleModifiedIsciPlanMappings(List<IsciPlanModifiedMappingDto> modified, DateTime modifiedAt, string modifiedBy)
        {
            var modifiedCount = 0;
            var toModifyList = new List<PlanIsciDto>();

            if (!modified.Any())
            {
                return modifiedCount;
            }

                modified.ForEach(m =>
            {                
                var isci = _PlanIsciRepository.GetPlanIscisByMappingId(m.PlanIsciMappingId);
                isci.ForEach(isc =>
                {
                    var toModifyItem = new PlanIsciDto();
                    toModifyItem.Id = m.PlanIsciMappingId;
                    toModifyItem.PlanId = isc.PlanId;
                    toModifyItem.Isci = isc.Isci;
                    toModifyItem.FlightStartDate = m.FlightStartDate;
                    toModifyItem.FlightEndDate = m.FlightEndDate;
                    toModifyItem.SpotLengthId = isc.SpotLengthId;
                    toModifyList.Add(toModifyItem);
                });              
            });           

            if (toModifyList.Any())
            {
                modifiedCount = _PlanIsciRepository.UpdateIsciPlanMappings(toModifyList, modifiedAt, modifiedBy);
            }           
            return modifiedCount;
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

            return true;
        }

        /// <summary>
        /// populate the ISCI-Plan and flights collection for save.
        /// </summary>
        private List<PlanIsciDto> _PoplateListItemMappings(List<IsciPlanMappingDto> mappings)
        {
            var result = new List<PlanIsciDto>();
            mappings.ForEach(m =>
            {
                foreach (var flight in m.IsciPlanMappingFlights)
                {
                    PlanIsciDto itemToAdd = new PlanIsciDto();
                    itemToAdd.PlanId = m.PlanId;
                    itemToAdd.FlightStartDate = flight.FlightStartDate;
                    itemToAdd.FlightEndDate = flight.FlightEndDate;
                    itemToAdd.SpotLengthId = m.SpotLengthId;
                    itemToAdd.Isci = m.Isci;
                    result.Add(itemToAdd);
                }
            });
            return result;
        }
        public PlanIsciMappingsDetailsDto GetPlanIsciMappingsDetails(int planId)
        {
            var plan = _PlanService.GetPlan(planId);
            var campaign = _CampaignService.GetCampaignById(plan.CampaignId);
            var advertiserName = _GetAdvertiserName(campaign.AdvertiserMasterId.Value);
            var daypartsString = _GetDaypartCodesString(plan.Dayparts);
            var spotLengthsString = _GetSpotLengthsString(plan.CreativeLengths);
            var demoString = _GetAudienceString(plan.AudienceId);
            var flightString = _GetFlightString(plan.FlightStartDate.Value, plan.FlightEndDate.Value);

            var mappedIscis = _GetIsciPlanMappingIsciDetailsDto(planId, plan.FlightStartDate.Value, plan.FlightEndDate.Value);

            var isciMappings = mappedIscis.GroupBy(x => new { x.Isci, x.SpotLengthId, x.SpotLengthString }).ToList();
            var mappingsDetails = new PlanIsciMappingsDetailsDto
                {
                    PlanId = plan.Id,
                    PlanName = plan.Name,
                    AdvertiserName = advertiserName,
                    SpotLengthString = spotLengthsString,
                    DaypartCode = daypartsString,
                    DemoString = demoString,
                    FlightStartDate = plan.FlightStartDate.Value,
                    FlightEndDate = plan.FlightEndDate.Value,
                    FlightString = flightString,
                    PlanIsciMappings = isciMappings.Select(isciMappingsDetail => new IsciMappingDetailsDto
                    {
                        Isci = isciMappingsDetail.Key.Isci,
                        SpotLengthString = isciMappingsDetail.Key.SpotLengthString,
                        SpotLengthId = isciMappingsDetail.Key.SpotLengthId,
                        PlanIsciMappingsFlights = isciMappingsDetail.Select(mappingDetail => new MappingDetailsDto
                        {
                            PlanIsciMappingId = mappingDetail.PlanIsciMappingId,
                            FlightStartDate = mappingDetail.FlightStartDate,
                            FlightEndDate = mappingDetail.FlightEndDate,
                            FlightString = mappingDetail.FlightString,
                            StartTime = null,
                            EndTime = null
                        }).ToList()
                    }).ToList(),
                };
            return mappingsDetails;
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
                var item = new PlanMappedIsciDetailsDto
                {
                    PlanIsciMappingId = i.Id,
                    Isci = i.Isci,
                    SpotLengthId = spotLengthId,
                    SpotLengthString = spotLengthString,
                    FlightStartDate = i.FlightStartDate,
                    FlightEndDate = i.FlightEndDate,
                    FlightString = flightString
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
            var audience = _AudienceRepository.GetAudiencesByIds(new List<int> { audienceId }).First();
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
