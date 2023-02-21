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
        /// <summary>
        /// copy Isci mapping
        /// </summary>
        /// <param name="copyRequest">The object which contains save parameters</param>
        /// /// <param name="createdBy">Created By</param>
        /// <returns>true or false</returns>
        bool CopyIsciMappings(IsciPlanMappingsSaveRequestDto copyRequest, string createdBy);

        PlanIsciMappingsDetailsDto GetPlanIsciMappingsDetails(int planId);

        /// <summary>
        /// Search the Existing plan iscis based on the Plan
        /// </summary>
        /// <param name="planId">plan Id</param>
        /// <returns>List of Found Iscis</returns>
        SearchPlanIscisDto GetMappedIscis(int planId);

        /// <summary>
        /// Gets the Plan Iscis basis of the plan
        /// </summary>
        /// <param name="sourcePlanId">sourcePlanId</param>
        /// <returns>plan Iscis</returns>
        IsciTargetPlansDto GetTargetIsciPlans(int sourcePlanId);
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
        private readonly ISpotLengthRepository _SpotLengthRepository;
        protected Lazy<bool> _IsUnifiedCampaignEnabled;
        private readonly IStandardDaypartRepository _StandardDaypartRepository;
        const int defaultStartTime = 0;
        const int defaultEndTime = 86339; 
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
            _StandardDaypartRepository = dataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();

            _StandardDaypartService = standardDaypartService;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _DateTimeEngine = dateTimeEngine;
            _AabEngine = aabEngine;
            _PlanService = planService;
            _CampaignService = campaignService;
            _SpotLengthEngine = spotLengthEngine;
            _SpotLengthRepository = dataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _IsUnifiedCampaignEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_UNIFIED_CAMPAIGN));
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
            //This will always return single week since queryStartDate and queryStartDate will always contain single week
            var isciFilterWeek = BroadcastWeeksHelper.GetContainingWeeks(queryStartDate, queryStartDate).FirstOrDefault();
            var isciPlans = _PlanIsciRepository.GetAvailableIsciPlans(queryStartDate, queryEndDate);
            isciPlans = isciPlans.Where(x => x.Dayparts.Count != 0).ToList();
            List<int> outOfFlightPlans = new List<int>();
            isciPlans.ForEach(P =>
            {
                List<int> daypartDayIds = _GetDaypartDayIds(P.PlanDayparts);
                var week = BroadcastWeeksHelper.GetContainingWeeks(P.FlightStartDate, P.FlightEndDate).Where(x => x.WeekStartDate == queryStartDate && x.WeekEndDate == x.WeekEndDate).FirstOrDefault();
                List<int> planFlightDays = P.FlightDays.Where(x => x != null).Cast<int>().ToList();
                if (week != null)
                {
                    int WeekNo = CalculatorHelper.CalculateActiveDays(week.WeekStartDate, week.WeekEndDate, planFlightDays, P.FlightHiatusDays, daypartDayIds, out string activeDaysString);

                    // handle hiatus weeks
                    if (WeekNo < 1 && week.WeekStartDate == isciFilterWeek.WeekStartDate && week.WeekEndDate == isciFilterWeek.WeekEndDate)
                    {
                        outOfFlightPlans.Add(P.Id);
                    }
                }
            }
            );
            isciPlans = isciPlans.Where(P => !outOfFlightPlans.Contains(P.Id)).ToList();
            if (!_IsUnifiedCampaignEnabled.Value)
            {
                isciPlans = isciPlans.Where(x => x.UnifiedTacticLineId == null).ToList();
            }
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
                                    Iscis = isciPlanDetail.Iscis,
                                    UnifiedTacticLineId = isciPlanDetail.UnifiedTacticLineId,
                                    UnifiedCampaignLastSentAt = isciPlanDetail.UnifiedCampaignLastSentAt,
                                    UnifiedCampaignLastReceivedAt = isciPlanDetail.UnifiedCampaignLastReceivedAt
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
                    itemToAdd.FlightStartDate = flight.FlightStartDate.Value;
                    itemToAdd.FlightEndDate = flight.FlightEndDate.Value;
                    itemToAdd.SpotLengthId = m.SpotLengthId;
                    itemToAdd.Isci = m.Isci;
                    itemToAdd.StartTime = flight.StartTime;
                    itemToAdd.EndTime = flight.EndTime;
                    result.Add(itemToAdd);
                }
            });
            return result;
        }
        internal int _HandleCopyIsciPlanMapping(List<IsciPlanMappingDto> mappings, string createdBy, DateTime createdAt)
        {
            if (!mappings.Any())
            {
                return 0;
            }

            List<PlanIsciDto> toSave = _PoplateListItemMappingsOnCopy(mappings);
            toSave = _RemoveDuplicateFromPlanIscis(toSave);

            List<PlanIsciDto> toSaveFiltered = new List<PlanIsciDto>();
            foreach (var mapping in toSave)
            {

                int planIscisCount = 0;
                var planIsciList = _PlanIsciRepository.GetPlanIscis(mapping.PlanId);
                planIscisCount = planIscisCount + planIsciList.Count(x => x.Isci.ToLower() == mapping.Isci.ToLower()
                && (_IsDateSameTimeOverlap(x.FlightEndDate.Date, mapping.FlightStartDate.Date, x.EndTime, mapping.StartTime)
                || _IsDateSameTimeOverlap(x.FlightStartDate.Date, mapping.FlightEndDate.Date, x.StartTime, mapping.EndTime)
                || mapping.SpotLengthId != x.SpotLengthId)
                );
                planIscisCount = planIscisCount + planIsciList.Count(x => x.Isci.ToLower() == mapping.Isci.ToLower() &&
                x.FlightStartDate.Date == mapping.FlightStartDate.Date && x.FlightEndDate.Date == mapping.FlightEndDate.Date);
                DateTime flightEndDate = mapping.FlightEndDate.Date.AddDays(-1);
                planIscisCount = planIscisCount + planIsciList.Count(x => x.Isci.ToLower() == mapping.Isci.ToLower()
                && (_IsBewteenTwoDates(x.FlightStartDate.Date, x.FlightEndDate.Date, mapping.FlightStartDate.Date)
                || _IsBewteenTwoDates(x.FlightStartDate.Date, x.FlightEndDate.Date, flightEndDate)
                || mapping.SpotLengthId != x.SpotLengthId)
                );
                planIscisCount = planIscisCount + planIsciList.Count(x => x.Isci.ToLower() == mapping.Isci.ToLower()
                && (_IsBewteenTwoDates(mapping.FlightStartDate.Date, mapping.FlightEndDate.Date, x.FlightStartDate.Date)
                || _IsBewteenTwoDates(mapping.FlightStartDate.Date, mapping.FlightEndDate.Date, mapping.FlightEndDate.Date)
                || mapping.SpotLengthId != x.SpotLengthId)
                );
                if (planIscisCount == 0)
                {
                    toSaveFiltered.Add(mapping);
                }
            }
            var totalChangedCount = _PlanIsciRepository.SaveIsciPlanMappings(toSaveFiltered, createdBy, createdAt);
            return totalChangedCount;
        }

        private List<PlanIsciDto> _RemoveDuplicateFromPlanIscis(List<PlanIsciDto> planIscis)
        {
            var distinctPlanIsciList = planIscis
                .GroupBy(x => new { x.PlanId, x.Isci, x.FlightStartDate, x.FlightEndDate })
                .Select(x => x.First()).Distinct()
                .ToList();
            return distinctPlanIsciList;
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
                toModifyItem.FlightStartDate = Convert.ToDateTime(m.FlightStartDate);
                toModifyItem.FlightEndDate = Convert.ToDateTime(m.FlightEndDate);
                toModifyItem.SpotLengthId = isc.SpotLengthId;
                toModifyItem.StartTime = m.StartTime;
                toModifyItem.EndTime = m.EndTime;
                toModifyList.Add(toModifyItem);
            });
        });

            if (toModifyList.Any())
            {
                modifiedCount = _PlanIsciRepository.UpdateIsciPlanMappings(toModifyList, modifiedAt, modifiedBy);
            }
            return modifiedCount;
        }
        private DateTime _SetFlightStartDate(DateTime? startDate)
        {
            DateTime flightDate = (startDate < DateTime.Now) ? DateTime.Now : Convert.ToDateTime(startDate);
            return flightDate;
        }
        public bool SaveIsciMappings(IsciPlanMappingsSaveRequestDto saveRequest, string createdBy)
        {
            var createdAt = _DateTimeEngine.GetCurrentMoment();
            var deletedAt = _DateTimeEngine.GetCurrentMoment();
            var modifiedAt = _DateTimeEngine.GetCurrentMoment();

            var isciPlanMappingsDeletedCount = _HandleDeleteIsciPlanMapping(saveRequest.IsciPlanMappingsDeleted, createdBy, deletedAt);
            _LogInfo($"{isciPlanMappingsDeletedCount} IsciPlanMappings are deleted.");

            var addedCount = _HandleNewIsciPlanMapping(saveRequest.IsciPlanMappings, createdBy, createdAt);
            _LogInfo($"{addedCount} IsciPlanMappings were added.");

            var modifiedCount = _HandleModifiedIsciPlanMappings(saveRequest.IsciPlanMappingsModified, modifiedAt, createdBy);

            _LogInfo($"{modifiedCount} IsciPlanMappings were modified.");

            return true;
        }
        public bool CopyIsciMappings(IsciPlanMappingsSaveRequestDto copyRequest, string createdBy)
        {                     
            var createdAt = _DateTimeEngine.GetCurrentMoment();
            var deletedAt = _DateTimeEngine.GetCurrentMoment();
            var modifiedAt = _DateTimeEngine.GetCurrentMoment();
            var isciPlanMappingsDeletedCount = _HandleDeleteIsciPlanMapping(copyRequest.IsciPlanMappingsDeleted, createdBy, deletedAt);
            _LogInfo($"{isciPlanMappingsDeletedCount} IsciPlanMappings are deleted.");

            var addedCount = _HandleCopyIsciPlanMapping(copyRequest.IsciPlanMappings, createdBy, createdAt);
            _LogInfo($"{addedCount} IsciPlanMappings were added.");

            var modifiedCount = _HandleModifiedIsciPlanMappings(copyRequest.IsciPlanMappingsModified, modifiedAt, createdBy);

            _LogInfo($"{modifiedCount} IsciPlanMappings were modified.");
          
            return true;
        }

        /// <summary>
        /// populate the ISCI-Plan and flights collection for save.
        /// //If not in scope of the flight then don't copy the instructions, but still copy the iscis
        ///then pick flight start and end date from the plan
        /// </summary>
        private List<PlanIsciDto> _PoplateListItemMappingsOnCopy(List<IsciPlanMappingDto> mappings)
        {
            var result = new List<PlanIsciDto>();
            mappings.ForEach(m =>
            {
                PlanDto plan = new PlanDto();
                plan = _PlanService.GetPlan(m.PlanId);
                if (m.IsciPlanMappingFlights != null && m.IsciPlanMappingFlights.Count > 0)
                {
                    foreach (var flight in m.IsciPlanMappingFlights)
                    {
                        PlanIsciDto itemToAdd = new PlanIsciDto();
                        itemToAdd.PlanId = m.PlanId;
                        var validateIsciFlights = _ValidateFlights(flight.FlightStartDate, flight.FlightEndDate);
                        var validatePlanFlights = _ValidateFlights(plan.FlightStartDate, plan.FlightEndDate);
                        if (!validatePlanFlights)
                        {
                            continue;
                        }
                        if (!validateIsciFlights)
                        {
                            itemToAdd.FlightStartDate = _SetFlightStartDate(plan.FlightStartDate);
                            itemToAdd.FlightEndDate = Convert.ToDateTime(plan.FlightEndDate);
                        }
                        else
                        {

                            if (_IsBewteenTwoDates((DateTime)plan.FlightStartDate, (DateTime)plan.FlightEndDate, (DateTime)flight.FlightStartDate))
                            {
                                itemToAdd.FlightStartDate = _SetFlightStartDate(flight.FlightStartDate);                              
                            }
                            else
                            {
                                itemToAdd.FlightStartDate = _SetFlightStartDate(plan.FlightStartDate);                               
                            }
                            if (_IsBewteenTwoDates((DateTime)plan.FlightStartDate, (DateTime)plan.FlightEndDate, (DateTime)flight.FlightEndDate))
                            {
                                itemToAdd.FlightEndDate = flight.FlightEndDate.Value;                              
                            }
                            else
                            {
                                itemToAdd.FlightEndDate = plan.FlightEndDate.Value;
                            }

                        }
                        itemToAdd.SpotLengthId = m.SpotLengthId;
                        itemToAdd.Isci = m.Isci;
                        itemToAdd.StartTime = flight.StartTime;
                        itemToAdd.EndTime = flight.EndTime;
                        result.Add(itemToAdd);
                    }
                }
                else
                {
                    IsciPlanMappingFlightsDto flight = new IsciPlanMappingFlightsDto
                    {
                        FlightStartDate = plan.FlightStartDate,
                        FlightEndDate = plan.FlightEndDate
                    };
                    var validateisi = _ValidateFlights(plan.FlightStartDate, plan.FlightEndDate);
                    if (validateisi)
                    {
                        PlanIsciDto itemToAdd = new PlanIsciDto();
                        itemToAdd.PlanId = m.PlanId;
                        itemToAdd.FlightStartDate = _SetFlightStartDate(plan.FlightStartDate);
                        itemToAdd.FlightEndDate = Convert.ToDateTime(plan.FlightEndDate);
                        itemToAdd.SpotLengthId = m.SpotLengthId;
                        itemToAdd.Isci = m.Isci;
                        itemToAdd.StartTime = flight.StartTime;
                        itemToAdd.EndTime = flight.EndTime;
                        result.Add(itemToAdd);
                    }
                }
            });
            return result;
        }

        private bool _IsDateSameTimeOverlap(DateTime startDate, DateTime endDate, int? endDateTime, int? startDateTime)
        {
            startDateTime= startDateTime ?? defaultStartTime;
            endDateTime= endDateTime ?? defaultEndTime;
            int? timeDifference = startDateTime > endDateTime ? startDateTime - endDateTime : endDateTime - startDateTime;
            bool isOverlap = false;
            int result = DateTime.Compare(startDate, endDate);
            if (result == 0 && timeDifference < 1800)
            {
                isOverlap = true;
            }
            return isOverlap;
        }

        private bool _IsBewteenTwoDates(DateTime startDate, DateTime endDate, DateTime target)
        {
            bool isFallBetween = false;
            if (target.Ticks > startDate.Ticks && target.Ticks < endDate.Ticks)
            {
                isFallBetween = true;
            }
            return isFallBetween;
        }
        /// <summary>
        /// Validate the flights shoudn't be past dated
        /// If already exists then : ignore
        /// If in past then don't copy  (oriented from NOW)
        /// </summary>        
        private bool _ValidateFlights(DateTime? flightStartDate, DateTime? flightEndDate)
        {
            bool isValid = true;
            if ((flightStartDate < DateTime.Now && flightEndDate < DateTime.Now))
            {
                isValid = false;
            }
            return isValid;
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

            var isciMappings = mappedIscis.GroupBy(x => new { x.Isci, x.SpotLengthId, x.SpotLengthString, x.SpotLengthValueForSort }).OrderBy(y => y.Key.SpotLengthValueForSort).ThenBy(s => s.Key.Isci).ToList();

            var mappingsDetails = new PlanIsciMappingsDetailsDto
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                AdvertiserName = advertiserName,
                SpotLengthString = spotLengthsString,
                DaypartCode = daypartsString,
                DemoString = demoString,
                FlightStartDate = plan.FlightStartDate.Value.ToString("MM/dd/yyyy"),
                FlightEndDate = plan.FlightEndDate.Value.ToString("MM/dd/yyyy"),
                FlightString = flightString,
                IsciPlanMappings = isciMappings.Select(isciMappingsDetail => new IsciMappingDetailsDto
                {
                    Isci = isciMappingsDetail.Key.Isci,
                    SpotLengthString = isciMappingsDetail.Key.SpotLengthString,
                    SpotLengthId = isciMappingsDetail.Key.SpotLengthId,
                    IsciPlanMappingFlights = isciMappingsDetail.Select(mappingDetail => new MappingDetailsDto
                    {
                        PlanIsciMappingId = mappingDetail.PlanIsciMappingId,
                        FlightStartDate = mappingDetail.FlightStartDate.ToString("MM/dd/yyyy"),
                        FlightEndDate = mappingDetail.FlightEndDate.ToString("MM/dd/yyyy"),
                        FlightString = mappingDetail.FlightString,
                        StartTime = mappingDetail.StartTime,
                        EndTime = mappingDetail.EndTime
                    }).OrderBy(x => x.FlightStartDate).ToList()
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

            var result = planIscis.Select(i =>
            {
                // we are using .First() here as a reel isci should never have same isci with different spot id.
                var spotLengthString = _GetSpotLengthsString(i.SpotLengthId);
                int spotLengthValueForSort = _GetSpotLengthsValue(i.SpotLengthId);
                var flightString = _GetFlightString(i.FlightStartDate, i.FlightEndDate);
                var item = new PlanMappedIsciDetailsDto
                {
                    PlanIsciMappingId = i.Id,
                    Isci = i.Isci,
                    SpotLengthId = i.SpotLengthId,
                    SpotLengthString = spotLengthString,
                    FlightStartDate = i.FlightStartDate,
                    FlightEndDate = i.FlightEndDate,
                    FlightString = flightString,
                    StartTime = i.StartTime,
                    EndTime = i.EndTime,
                    SpotLengthValueForSort = spotLengthValueForSort
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

        private int _GetSpotLengthsValue(int spotLengthId)
        {
            int result = _SpotLengthEngine.GetSpotLengthValueById(spotLengthId);
            return result;
        }

        private List<int> _GetDaypartDayIds(List<PlanDaypartDto> planDayparts)
        {
            // the FE sends with at least 1 empty daypart...
            // look for valid dayparts for this calculation
            var validDayparts = planDayparts?.Where(d => d.DaypartCodeId > 0).ToList();
            if (validDayparts?.Any() != true)
            {
                return new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            }

            var planDefaultDaypartIds = planDayparts.Select(d => d.DaypartCodeId).ToList();
            var dayIds = _StandardDaypartRepository.GetDayIdsFromStandardDayparts(planDefaultDaypartIds);
            return dayIds;
        }

        public SearchPlanIscisDto GetMappedIscis(int planId)
        {
            var plan = _PlanService.GetPlan(planId);
            var campaign = _CampaignService.GetCampaignById(plan.CampaignId);
            var planIscis = _PlanIsciRepository.GetMappedIscis(campaign.AdvertiserMasterId);
            planIscis = _RemoveDuplicates(planIscis);
            var spotLengths = _SpotLengthRepository.GetSpotLengths().Select(x => new IsciSearchSpotLengthDto
            {
                Id = x.Id,
                Length = $":{x.Display}"
            }).ToList();
            var iscis = new SearchPlanIscisDto
            {
                Iscis = planIscis.Select(x =>
                {
                    return new SearchPlan
                    {
                        Isci = x.Isci,
                        SpotLengths = spotLengths
                    };
                }).ToList()
            };
            return iscis;
        }

        private List<SearchPlan> _RemoveDuplicates(List<SearchPlan> searchPlans)
        {
            var distinctIscis = searchPlans
                .GroupBy(x => x.Isci)
                .Select(x => x.First())
                .OrderBy(x => x.Isci)
                .ToList();
            return distinctIscis;
        }

        public IsciTargetPlansDto GetTargetIsciPlans(int sourcePlanId)
        {
            DateTime dateTime = DateTime.Today;
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";
            var result = new IsciTargetPlansDto();
            var plan = _PlanService.GetPlan(sourcePlanId);
            var campaign = _CampaignService.GetCampaignById(plan.CampaignId);
            var planIscis = _PlanIsciRepository.GetTargetIsciPlans(campaign.AdvertiserMasterId, sourcePlanId);
            foreach (var singlePlan in planIscis)
            {
                var isciplan = singlePlan.plan_versions.SingleOrDefault(x => x.flight_end_date >= dateTime && x.id == singlePlan.latest_version_id && x.is_draft == false);
                if (isciplan != null && isciplan.plan_version_dayparts.Count != 0)
                {
                    var dayparts = isciplan.plan_version_dayparts.Select(d => d.standard_dayparts.code).ToList();
                    var spotLengthString = _GetSpotLengthsString(isciplan.plan_version_creative_lengths.Select(y => y.spot_length_id).FirstOrDefault());
                    var flightString = $"{isciplan.flight_start_date.ToString(flightStartDateFormat)}-{isciplan.flight_end_date.ToString(flightEndDateFormat)}";
                    var targetPlan = new TargetPlans
                    {
                        Id = isciplan.plan_id,
                        SpotLengthString = spotLengthString,
                        DemoString = isciplan.audience.code,
                        Title = isciplan.plan.name,
                        DaypartsString = string.Join(", ", dayparts),
                        FlightString = flightString
                    };
                    result.Plans.Add(targetPlan);
                }
            }
            return result;
        }
    }
}
