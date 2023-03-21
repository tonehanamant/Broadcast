using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters.InventorySummary;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventorySummaryService : IApplicationService
    {
        List<InventorySource> GetInventorySources();
        InventoryQuartersDto GetInventoryQuarters(int inventorySourceId, int standardDaypartId);
        InventoryQuartersDto GetInventoryQuarters(DateTime currentDate);
        List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate);
        List<InventorySummaryDto> GetInventorySummariesWithCache(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate);
        List<StandardDaypartDto> GetStandardDayparts(int inventorySourceId);
        List<string> GetInventoryUnits(int inventorySourceId, int standardDaypartId, DateTime startDate, DateTime endDate);
        List<LookupDto> GetInventorySourceTypes();

        /// <summary>
        /// Aggregates all the information based on the list of inventory source ids
        /// </summary>
        /// <param name="inventorySourceIds">List of inventory source ids</param>
        [Queue("inventorysummaryaggregation")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void AggregateInventorySummaryData(List<int> inventorySourceIds);

        /// <summary>
        /// Aggregates all the information based on the list of inventory source ids
        /// </summary>
        /// <param name="inventorySourceIds">List of inventory source ids</param>
        /// <param name="endDate">Inventory end date</param>
        /// <param name="startDate">Inventory start date</param>
        [Queue("inventorysummaryaggregation")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void AggregateInventorySummaryData(List<int> inventorySourceIds, DateTime? startDate, DateTime? endDate);

        void QueueAggregateInventorySummaryDataJob(int inventorySourceId);
    }

    public class InventorySummaryService : BroadcastBaseClass, IInventorySummaryService
    {
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventorySummaryRepository _InventorySummaryRepository;
        private readonly IProgramRepository _ProgramRepository;
        private readonly IStandardDaypartRepository _StandardDaypartRepository;
        private readonly List<InventorySourceTypeEnum> SummariesSourceTypes = new List<InventorySourceTypeEnum>
        {
            InventorySourceTypeEnum.Barter,
            InventorySourceTypeEnum.OpenMarket,
            InventorySourceTypeEnum.ProprietaryOAndO,
            InventorySourceTypeEnum.Syndication,
            InventorySourceTypeEnum.Diginet
        };
        private readonly IMarketCoverageCache _MarketCoverageCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IInventoryGapCalculationEngine _InventoryGapCalculationEngine;
        private readonly IInventoryLogoRepository _InventoryLogoRepository;
        private readonly IInventorySummaryCache _InventorySummaryCache;
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IInventoryManagementApiClient _InventoryManagementApiClient;
        protected Lazy<bool> _IsInventoryServiceMigrationEnabled;
        public InventorySummaryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
                                       IQuarterCalculationEngine quarterCalculationEngine,
                                       IBroadcastAudiencesCache audiencesCache,
                                       IMarketCoverageCache marketCoverageCache,
                                       IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                                       IInventoryGapCalculationEngine inventoryGapCalculationEngine,
                                       IInventorySummaryCache inventorySummaryCache,
                                       IBackgroundJobClient backgroundJobClient, IInventoryManagementApiClient inventoryManagementApiClient,
                                       IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _QuarterCalculationEngine = quarterCalculationEngine;
            _AudiencesCache = audiencesCache;
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>(); ;
            _InventorySummaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventorySummaryRepository>();
            _ProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramRepository>();
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _MarketCoverageCache = marketCoverageCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _InventoryGapCalculationEngine = inventoryGapCalculationEngine;
            _InventoryLogoRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryLogoRepository>();
            _InventorySummaryCache = inventorySummaryCache;
            _BackgroundJobClient = backgroundJobClient;
            _InventoryManagementApiClient = inventoryManagementApiClient;
            _IsInventoryServiceMigrationEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION));            
        }

        public List<InventorySource> GetInventorySources()
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryManagementApiClient.GetInventorySources();
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                return _InventoryRepository.GetInventorySources();
            }            
        }

        public List<LookupDto> GetInventorySourceTypes()
        {
            if(_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryManagementApiClient.GetInventorySourceTypes();
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                return EnumExtensions.ToLookupDtoList<InventorySourceTypeEnum>()
               .OrderBy(i => i.Display).ToList();
            }
           
        }

        public InventoryQuartersDto GetInventoryQuarters(DateTime currentDate)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryManagementApiClient.GetInventoryQuarters();
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                return new InventoryQuartersDto
                {
                    Quarters = _GetInventorySummaryQuarters(currentDate),
                    DefaultQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate)
                };
            }
                
        }

        public InventoryQuartersDto GetInventoryQuarters(int inventorySourceId, int standardDaypartId)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {                
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryManagementApiClient.GetInventoryQuarters(inventorySourceId, standardDaypartId);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                var weeks = _InventoryRepository.GetStationInventoryManifestWeeks(inventorySourceId, standardDaypartId);
                var mediaMonthIds = weeks.Select(x => x.MediaWeek.MediaMonthId).Distinct();
                var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsByIds(mediaMonthIds);
                var quarters = mediaMonths
                    .GroupBy(x => new { x.Quarter, x.Year }) // take unique quarters
                    .Select(x => _QuarterCalculationEngine.GetQuarterDetail(x.Key.Quarter, x.Key.Year))
                    .ToList();

                return new InventoryQuartersDto { Quarters = quarters };
            }
        }

        public List<InventorySummaryDto> GetInventorySummaries(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate)
        {
            var inventorySummaryDtos = new List<InventorySummaryDto>();

            if (inventorySummaryFilterDto.InventorySourceId != null)
            {
                inventorySummaryDtos.AddRange(_LoadInventorySummariesForSource(inventorySummaryFilterDto, currentDate));
            }
            else
            {
                inventorySummaryDtos.AddRange(_LoadInventorySummaryForQuarter(inventorySummaryFilterDto));
            }

            if (inventorySummaryDtos.Any(x => x.InventorySourceId != 0))   //if there is summary data in the result set
            {
                _SetHasLogo(inventorySummaryDtos);
                _SetHasInventoryForSource(inventorySummaryDtos, inventorySummaryFilterDto.LatestInventoryUpdatesBySourceId);
            }

            return inventorySummaryDtos;
        }

        public List<InventorySummaryDto> GetInventorySummariesWithCache(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _LoadInventorySummaries(inventorySummaryFilterDto);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                List<InventorySummaryDto> GetInventorySummariesFunc() => GetInventorySummaries(inventorySummaryFilterDto, currentDate);
                inventorySummaryFilterDto.LatestInventoryUpdatesBySourceId = _InventorySummaryRepository.GetLatestSummaryUpdatesBySource();
                return _InventorySummaryCache.GetOrCreate(inventorySummaryFilterDto, GetInventorySummariesFunc);
            }
        }
        private List<InventorySummaryDto> _LoadInventorySummaries(InventorySummaryFilterDto inventorySummaryFilterDto)
        {
            List<InventorySummaryDto> inventorySummaries = new List<InventorySummaryDto>();
            List<InventorySummaryApiResponse> summaryApisResponses = _InventoryManagementApiClient.GetInventorySummaries(inventorySummaryFilterDto);
            List<InventorySource> inventorySources = _InventoryManagementApiClient.GetInventorySources();
            foreach (var items in summaryApisResponses)
            {
                var inventorySource = inventorySources.FirstOrDefault(x => x.Id == items.InventorySourceId);
                if (inventorySource.InventoryType == InventorySourceTypeEnum.OpenMarket)
                {
                    inventorySummaries.Add(_LoadOpenMarketInventorySummary(items));
                }
                else if(inventorySource.InventoryType == InventorySourceTypeEnum.Barter)
                {
                    inventorySummaries.Add(_LoadBaterInventorySummary(items));
                }
                else if (inventorySource.InventoryType == InventorySourceTypeEnum.ProprietaryOAndO)
                {
                    inventorySummaries.Add(_LoadProprietaryInventorySummary(items));
                }
                else if (inventorySource.InventoryType == InventorySourceTypeEnum.Syndication)
                {
                    inventorySummaries.Add(_LoadSyndicationInventorySummary(items));
                }
                else if(inventorySource.InventoryType == InventorySourceTypeEnum.Diginet)
                {
                    inventorySummaries.Add(_LoadDiginetInventorySummary(items));
                }

            }
            return inventorySummaries;
        }
        private InventorySummaryDto _LoadOpenMarketInventorySummary(InventorySummaryApiResponse openMarketData)
        {
            var result = new OpenMarketInventorySummaryDto
            {
                Quarter = openMarketData.Quarter,
                InventorySourceId = openMarketData.InventorySourceId,
                InventorySourceName = openMarketData.InventorySourceName,
                Details = null // OpenMarket does not have details
            };
            result.HasInventoryGaps = openMarketData.InventoryGaps.Any();
            result.InventoryGaps = openMarketData.InventoryGaps;
            result.LastUpdatedDate = openMarketData.LastUpdatedDate;
            result.RatesAvailableFromQuarter = openMarketData.RatesAvailableFromQuarter;
            result.RatesAvailableToQuarter = openMarketData.RatesAvailableToQuarter;
            result.HutBook = openMarketData.HutBook;
            result.ShareBook = openMarketData.ShareBook;
            result.HouseholdImpressions = openMarketData.HouseholdImpressions; 
            result.TotalMarkets = openMarketData.TotalMarkets;
            result.TotalStations = openMarketData.TotalStations;
            result.TotalPrograms = openMarketData.TotalPrograms;
            result.HasRatesAvailableForQuarter = openMarketData.HasRatesAvailableForQuarter;
            result.HasLogo = openMarketData.HasLogo;
            result.HasInventoryForSource = openMarketData.HasInventoryForSource;
            return result;
        }
        private InventorySummaryDto _LoadBaterInventorySummary(InventorySummaryApiResponse barterData)
        {
            var result = new BarterInventorySummaryDto
            {                   
                InventorySourceId= barterData.InventorySourceId,
                InventorySourceName = barterData.InventorySourceName,
                Quarter = barterData.Quarter
            };
            result.HasRatesAvailableForQuarter = barterData.HasRatesAvailableForQuarter;
            result.TotalMarkets = barterData.TotalMarkets;
            result.TotalStations = barterData.TotalStations;
            result.HouseholdImpressions = barterData.HouseholdImpressions;
            result.LastUpdatedDate = barterData.LastUpdatedDate;
            result.IsUpdating = barterData.IsUpdating;
            result.RatesAvailableFromQuarter = barterData.RatesAvailableFromQuarter;
            result.RatesAvailableToQuarter = barterData.RatesAvailableToQuarter;
            result.HasInventoryGaps = barterData.InventoryGaps.Any();
            result.HutBook = barterData.HutBook;
            result.ShareBook = barterData.ShareBook;
            result.InventoryGaps = barterData.InventoryGaps;
            result.HasLogo = barterData.HasLogo;
            result.HasInventoryForSource = barterData.HasInventoryForSource;
           
            result.TotalUnits = barterData.TotalUnits;
            result.TotalDaypartCodes = barterData.TotalDaypartCodes;
            result.Details = barterData.Details.Select(x => new BarterInventorySummaryDto.Detail
            {
                CPM = x.CPM,
                TotalUnits = x.TotalUnits,
                Daypart = x.Daypart,
                HouseholdImpressions = x.HouseholdImpressions,
                TotalCoverage = x.TotalCoverage,
                TotalMarkets = x.TotalMarkets
            }).ToList();

            return result;
        }
        private InventorySummaryDto _LoadProprietaryInventorySummary(InventorySummaryApiResponse proprietaryData)
        {
            var result = new ProprietaryOAndOInventorySummaryDto
            {
                InventorySourceId = proprietaryData.InventorySourceId,
                InventorySourceName = proprietaryData.InventorySourceName,
                Quarter = proprietaryData.Quarter
            };
            result.HasRatesAvailableForQuarter = proprietaryData.HasRatesAvailableForQuarter;
            result.TotalMarkets = proprietaryData.TotalMarkets;
            result.TotalStations = proprietaryData.TotalStations;
            result.HouseholdImpressions = proprietaryData.HouseholdImpressions;
            result.LastUpdatedDate = proprietaryData.LastUpdatedDate;
            result.IsUpdating = proprietaryData.IsUpdating;
            result.RatesAvailableFromQuarter = proprietaryData.RatesAvailableFromQuarter;
            result.RatesAvailableToQuarter = proprietaryData.RatesAvailableToQuarter;
            result.HasInventoryGaps = proprietaryData.InventoryGaps.Any();
            result.HutBook = proprietaryData.HutBook;
            result.ShareBook = proprietaryData.ShareBook;
            result.InventoryGaps = proprietaryData.InventoryGaps;
            result.HasLogo = proprietaryData.HasLogo;
            result.HasInventoryForSource = proprietaryData.HasInventoryForSource;

            result.TotalPrograms = proprietaryData.TotalPrograms;
            result.TotalDaypartCodes = proprietaryData.TotalDaypartCodes;
            result.Details = proprietaryData.Details.Select(x => new ProprietaryOAndOInventorySummaryDto.Detail
            {
                CPM = x.CPM,
                Daypart = x.Daypart,
                HouseholdImpressions = x.HouseholdImpressions,
                TotalCoverage = x.TotalCoverage,
                TotalMarkets = x.TotalMarkets,
                TotalPrograms= x.TotalPrograms,
                MinSpotsPerWeek = x.MinSpotsPerWeek,
                MaxSpotsPerWeek = x.MaxSpotsPerWeek
           }).ToList();

            return result;
        }
        private InventorySummaryDto _LoadSyndicationInventorySummary(InventorySummaryApiResponse syndicationData)
        {
            var result = new SyndicationInventorySummaryDto
            {
                InventorySourceId = syndicationData.InventorySourceId,
                InventorySourceName = syndicationData.InventorySourceName,
                Quarter = syndicationData.Quarter,
                Details = null // Syndication does not have details
            };
            result.HasRatesAvailableForQuarter = syndicationData.HasRatesAvailableForQuarter;
            result.TotalMarkets = syndicationData.TotalMarkets;
            result.TotalStations = syndicationData.TotalStations;
            result.HouseholdImpressions = syndicationData.HouseholdImpressions;
            result.LastUpdatedDate = syndicationData.LastUpdatedDate;
            result.IsUpdating = syndicationData.IsUpdating;
            result.RatesAvailableFromQuarter = syndicationData.RatesAvailableFromQuarter;
            result.RatesAvailableToQuarter = syndicationData.RatesAvailableToQuarter;
            result.HasInventoryGaps = syndicationData.InventoryGaps.Any();
            result.HutBook = syndicationData.HutBook;
            result.ShareBook = syndicationData.ShareBook;
            result.InventoryGaps = syndicationData.InventoryGaps;
            result.HasLogo = syndicationData.HasLogo;
            result.HasInventoryForSource = syndicationData.HasInventoryForSource;
            result.TotalPrograms = syndicationData.TotalPrograms;
            return result;
        }
        private InventorySummaryDto _LoadDiginetInventorySummary(InventorySummaryApiResponse diginetData)
        {
            var result = new DiginetInventorySummaryDto
            {
                InventorySourceId = diginetData.InventorySourceId,
                InventorySourceName = diginetData.InventorySourceName,
                Quarter = diginetData.Quarter
            };
            result.HasRatesAvailableForQuarter = diginetData.HasRatesAvailableForQuarter;
            result.TotalMarkets = diginetData.TotalMarkets;
            result.TotalStations = diginetData.TotalStations;
            result.HouseholdImpressions = diginetData.HouseholdImpressions;
            result.LastUpdatedDate = diginetData.LastUpdatedDate;
            result.IsUpdating = diginetData.IsUpdating;
            result.RatesAvailableFromQuarter = diginetData.RatesAvailableFromQuarter;
            result.RatesAvailableToQuarter = diginetData.RatesAvailableToQuarter;
            result.HasInventoryGaps = diginetData.InventoryGaps.Any();
            result.HutBook = diginetData.HutBook;
            result.ShareBook = diginetData.ShareBook;
            result.InventoryGaps = diginetData.InventoryGaps;
            result.HasLogo = diginetData.HasLogo;
            result.HasInventoryForSource = diginetData.HasInventoryForSource;

            result.CPM = diginetData.CPM;
            result.TotalDaypartCodes = diginetData.TotalDaypartCodes;
            result.Details = diginetData.Details.Select(x => new DiginetInventorySummaryDto.Detail
            {
                CPM = x.CPM,
                Daypart = x.Daypart,
                HouseholdImpressions = x.HouseholdImpressions
            }).ToList();


            return result;
        }

        private IEnumerable<InventorySummaryDto> _LoadInventorySummaryForQuarter(InventorySummaryFilterDto inventorySummaryFilterDto)
        {
            var quarter = inventorySummaryFilterDto.Quarter.Quarter;
            var year = inventorySummaryFilterDto.Quarter.Year;
            var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(quarter, year);
            var inventorySources = GetInventorySources();

            foreach (var inventorySource in inventorySources)
            {
                if (_ShouldFilterBySourceType(inventorySummaryFilterDto, inventorySource))
                {
                    continue;
                }

                var data = _InventorySummaryRepository.GetInventorySummaryDataForSources(inventorySource, quarterDetail.Quarter, quarterDetail.Year);

                if (_ShouldFilterOutDataByStandardDaypart(data, inventorySummaryFilterDto))
                {
                    continue;
                }

                yield return _LoadInventorySummary(inventorySource, data, quarterDetail);
            }
        }

       

        private IEnumerable<InventorySummaryDto> _LoadInventorySummariesForSource(InventorySummaryFilterDto inventorySummaryFilterDto, DateTime currentDate)
        {
            var inventorySourceId = inventorySummaryFilterDto.InventorySourceId.Value;
            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);

            var inventorySourceDateRange = _GetInventorySourceOrCurrentQuarterDateRange(inventorySourceId, currentDate);
            var allQuartersBetweenDates =
                _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventorySourceDateRange.Start.Value,
                                                                     inventorySourceDateRange.End.Value);

            foreach (var quarterDetail in allQuartersBetweenDates)
            {
                var data = _InventorySummaryRepository.GetInventorySummaryDataForSources(inventorySource, quarterDetail.Quarter, quarterDetail.Year);

                if (_ShouldFilterOutDataByStandardDaypart(data, inventorySummaryFilterDto))
                {
                    continue;
                }

                yield return _LoadInventorySummary(inventorySource, data, quarterDetail);
            }
        }

        private List<InventoryQuarterSummary> _CreateInventorySummariesForSource(InventorySource inventorySource,
            BaseInventorySummaryAbstractFactory inventorySummaryFactory,
            int householdAudienceId, DateTime currentDate)
        {
            var result = new List<InventoryQuarterSummary>();
            var sw = Stopwatch.StartNew();
            var quarters = _GetInventoryQuarters(null, null, currentDate, inventorySource.Id);
            sw.Stop();
            Debug.WriteLine($"Got inventory quarters in {sw.Elapsed}");

            InventoryAvailability inventoryAvailability;
            try
            {
                inventoryAvailability = inventorySummaryFactory.GetInventoryAvailabilityBySource(inventorySource);
            }
            catch (Exception e)
            {
                var msg = $"Inventory not found for source '{inventorySource.Name}'.";
                Debug.WriteLine(msg);
                _LogError(msg, e);
                return result;
            }

            var standardDayparts = _StandardDaypartRepository.GetAllStandardDayparts();
            foreach (var quarterDetail in quarters)
            {
                InventoryQuarterSummary summaryData;
                sw.Restart();
                if ((InventorySourceEnum)inventorySource.Id == InventorySourceEnum.OpenMarket)
                {
                    summaryData = inventorySummaryFactory.CreateInventorySummary
                        (inventorySource, householdAudienceId, quarterDetail,
                        standardDayparts, inventoryAvailability);
                }
                else
                {
                    var manifests = _GetInventorySummaryManifests(inventorySource, quarterDetail);
                    summaryData = inventorySummaryFactory.CreateInventorySummary
                    (inventorySource, householdAudienceId, quarterDetail, manifests,
                    standardDayparts, inventoryAvailability);
                }
                sw.Stop();
                Debug.WriteLine($"Created inventory summary in {sw.Elapsed}");

                // add summary only if there is some inventory during the quarter
                if (summaryData.TotalPrograms > 0 || summaryData.TotalStations > 0 || summaryData.Details?.Any() == true)
                {
                    result.Add(summaryData);
                }
            }

            return result;
        }

        private List<InventoryQuarterSummary> _CreateInventorySummariesForSource(InventorySource inventorySource,
            BaseInventorySummaryAbstractFactory inventorySummaryFactory,
            int householdAudienceId, List<QuarterDetailDto> quarters)
        {
            var result = new List<InventoryQuarterSummary>();
            var sw = Stopwatch.StartNew();

            InventoryAvailability inventoryAvailability;
            try
            {
                inventoryAvailability = inventorySummaryFactory.GetInventoryAvailabilityBySource(inventorySource);
            }
            catch (Exception e)
            {
                var msg = $"Inventory not found for source '{inventorySource.Name}'.";
                Debug.WriteLine(msg);
                _LogError(msg, e);
                return result;
            }

            var standardDayparts = _StandardDaypartRepository.GetAllStandardDayparts();
            foreach (var quarterDetail in quarters)
            {
                InventoryQuarterSummary summaryData;
                sw.Restart();
                if ((InventorySourceEnum)inventorySource.Id == InventorySourceEnum.OpenMarket)
                {
                    summaryData = inventorySummaryFactory.CreateInventorySummary
                        (inventorySource, householdAudienceId, quarterDetail,
                        standardDayparts, inventoryAvailability);
                }
                else
                {
                    var manifests = _GetInventorySummaryManifests(inventorySource, quarterDetail);
                    summaryData = inventorySummaryFactory.CreateInventorySummary
                    (inventorySource, householdAudienceId, quarterDetail, manifests,
                    standardDayparts, inventoryAvailability);
                }
                sw.Stop();
                Debug.WriteLine($"Created inventory summary in {sw.Elapsed}");

                // add summary only if there is some inventory during the quarter
                if (summaryData.TotalPrograms > 0 || summaryData.TotalStations > 0 || summaryData.Details?.Any() == true)
                {
                    result.Add(summaryData);
                }
            }

            return result;
        }

        private void _SetHasLogo(List<InventorySummaryDto> inventorySummaryDtos)
        {
            var inventorySources = inventorySummaryDtos.Select(x => x.InventorySourceId).Distinct();
            var inventorySourcesWithLogo = _InventoryLogoRepository.GetInventorySourcesWithLogo(inventorySources);

            foreach (var inventorySummaryDto in inventorySummaryDtos)
            {
                inventorySummaryDto.HasLogo = inventorySourcesWithLogo.Contains(inventorySummaryDto.InventorySourceId);
            }
        }

        private void _SetHasInventoryForSource(List<InventorySummaryDto> inventorySummaryDtos, Dictionary<int, DateTime?> latestInventoryUpdatesBySourceId)
        {
            foreach (var inventorySummaryDto in inventorySummaryDtos)
            {
                if (latestInventoryUpdatesBySourceId.TryGetValue(inventorySummaryDto.InventorySourceId, out var latestInventoryUpdateDate) &&
                    latestInventoryUpdateDate.HasValue)
                {
                    inventorySummaryDto.HasInventoryForSource = true;
                }
                else
                {
                    inventorySummaryDto.HasInventoryForSource = false;
                }
            }
        }

        private bool _ShouldFilterOutDataByStandardDaypart(InventoryQuarterSummary data, InventorySummaryFilterDto inventorySummaryFilterDto)
        {
            var standardDaypartId = inventorySummaryFilterDto.StandardDaypartId;

            if (standardDaypartId.HasValue)
            {
                // don't add empty objects
                if (!data.HasInventorySourceSummary || !data.HasInventorySourceSummaryQuarterDetails)
                {
                    return true;
                }

                var standardDaypartIds = data.Details.Select(m => m.StandardDaypartId).Distinct();

                return !standardDaypartIds.Contains(standardDaypartId.Value);
            }

            return false;
        }

        private bool _ShouldFilterBySourceType(InventorySummaryFilterDto inventorySummaryFilterDto, InventorySource inventorySource)
        {
            return inventorySummaryFilterDto.InventorySourceType.HasValue &&
                   inventorySource.InventoryType != inventorySummaryFilterDto.InventorySourceType;
        }

        private List<InventorySummaryManifestDto> _GetInventorySummaryManifests(InventorySource inventorySource, QuarterDetailDto quarterDetail)
        {
            var @switch = new Dictionary<InventorySourceTypeEnum, Func<List<InventorySummaryManifestDto>>> {
                { InventorySourceTypeEnum.Barter,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForBarterSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
                { InventorySourceTypeEnum.OpenMarket,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForOpenMarketSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
                { InventorySourceTypeEnum.ProprietaryOAndO,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForProprietaryOAndOSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
                { InventorySourceTypeEnum.Syndication,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForSyndicationSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
                { InventorySourceTypeEnum.Diginet,
                    () => _InventorySummaryRepository.GetInventorySummaryManifestsForDiginetSources(inventorySource, quarterDetail.StartDate, quarterDetail.EndDate) },
            };

            var result = @switch[inventorySource.InventoryType]();

            return result.Any() ? result : new List<InventorySummaryManifestDto>();
        }

        private InventorySummaryDto _LoadInventorySummary(InventorySource inventorySource, InventoryQuarterSummary data, QuarterDetailDto quarterDetail)
        {
            var inventorySummaryFactory = _GetInventorySummaryFactory(inventorySource.InventoryType);

            if (data.HasInventorySourceSummary)
            {
                foreach (var gap in data.InventoryGaps)
                {
                    gap.Quarter = _QuarterCalculationEngine.GetQuarterDetail(gap.Quarter.Quarter, gap.Quarter.Year);
                }
            }

            return inventorySummaryFactory.LoadInventorySummary(inventorySource, data, quarterDetail);
        }

        private BaseInventorySummaryAbstractFactory _GetInventorySummaryFactory(InventorySourceTypeEnum inventoryType)
        {
            var @switch = new Dictionary<InventorySourceTypeEnum, BaseInventorySummaryAbstractFactory>
            {
                {
                InventorySourceTypeEnum.Barter, new BarterInventorySummaryFactory(_InventoryRepository,
                                                                            _InventorySummaryRepository,
                                                                            _QuarterCalculationEngine,
                                                                            _ProgramRepository,
                                                                            _MediaMonthAndWeekAggregateCache,
                                                                            _MarketCoverageCache,
                                                                            _InventoryGapCalculationEngine)
                },
                {
                    InventorySourceTypeEnum.OpenMarket, new OpenMarketSummaryFactory(_InventoryRepository,
                                                                       _InventorySummaryRepository,
                                                                       _QuarterCalculationEngine,
                                                                       _ProgramRepository,
                                                                       _MediaMonthAndWeekAggregateCache,
                                                                       _MarketCoverageCache,
                                                                       _InventoryGapCalculationEngine)
                },
                {
                    InventorySourceTypeEnum.ProprietaryOAndO, new ProprietaryOAndOSummaryFactory(_InventoryRepository,
                                                                             _InventorySummaryRepository,
                                                                             _QuarterCalculationEngine,
                                                                             _ProgramRepository,
                                                                             _MediaMonthAndWeekAggregateCache,
                                                                             _MarketCoverageCache,
                                                                             _InventoryGapCalculationEngine)
                },
                {
                     InventorySourceTypeEnum.Syndication, new SyndicationSummaryFactory(_InventoryRepository,
                                                                        _InventorySummaryRepository,
                                                                        _QuarterCalculationEngine,
                                                                        _ProgramRepository,
                                                                        _MediaMonthAndWeekAggregateCache,
                                                                        _MarketCoverageCache,
                                                                        _InventoryGapCalculationEngine)
                },
                {
                    InventorySourceTypeEnum.Diginet, new DiginetSummaryFactory(_InventoryRepository,
                                                                    _InventorySummaryRepository,
                                                                    _QuarterCalculationEngine,
                                                                    _ProgramRepository,
                                                                    _MediaMonthAndWeekAggregateCache,
                                                                    _MarketCoverageCache,
                                                                    _InventoryGapCalculationEngine)
                }
            };

            return @switch[inventoryType];
        }

        private List<QuarterDetailDto> _GetInventorySummaryQuarters(DateTime currentDate)
        {
            var dateRange = _InventoryRepository.GetAllInventoriesDateRange();

            if (dateRange.IsEmpty())
            {
                dateRange = _GetCurrentQuarterDateRange(currentDate);
            }

            return _QuarterCalculationEngine.GetAllQuartersBetweenDates(dateRange.Start.Value, dateRange.End.Value);
        }

        private List<QuarterDetailDto> _GetInventoryQuarters(DateTime? startDate, DateTime? endDate, DateTime currentDate, int inventorySourceId)
        {
            if (startDate.HasValue && endDate.HasValue)
                return _QuarterCalculationEngine.GetAllQuartersBetweenDates(startDate.GetValueOrDefault(), endDate.GetValueOrDefault());

            var dateRange = _GetInventorySourceOrCurrentQuarterDateRange(inventorySourceId, currentDate);

            return _QuarterCalculationEngine.GetAllQuartersBetweenDates(dateRange.Start.GetValueOrDefault(), dateRange.End.GetValueOrDefault());
        }

        private DateRange _GetInventorySourceOrCurrentQuarterDateRange(int inventorySourceId, DateTime currentDate)
        {
            var dateRange = _InventoryRepository.GetInventorySourceDateRange(inventorySourceId);

            if (dateRange.IsEmpty())
            {
                dateRange = _GetCurrentQuarterDateRange(currentDate);
            }

            return dateRange;
        }

        private DateRange _GetCurrentQuarterDateRange(DateTime currentDate)
        {
            var datesTuple = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, currentDate);
            return new DateRange(datesTuple.Item1, datesTuple.Item2);
        }

        public List<StandardDaypartDto> GetStandardDayparts(int inventorySourceId)
        {
            if(_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryManagementApiClient.GetStandardDayparts(inventorySourceId);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                return  _StandardDaypartRepository.GetStandardDaypartsByInventorySource(inventorySourceId);
            }
        }

        public List<string> GetInventoryUnits(int inventorySourceId, int standardDaypartId, DateTime startDate, DateTime endDate)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    var result = _InventoryManagementApiClient.GetInventoryUnits(inventorySourceId, standardDaypartId, startDate, endDate);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            else
            {
                if (startDate > endDate)
                {
                    return new List<string>();
                }
                var groups = _InventoryRepository.GetInventoryGroups(inventorySourceId, standardDaypartId, startDate, endDate);

                return groups.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
        }

        public void QueueAggregateInventorySummaryDataJob(int inventorySourceId)
        {
            _BackgroundJobClient.Enqueue<IInventorySummaryService>(x => x.AggregateInventorySummaryData(new List<int> { inventorySourceId }));
        }

        /// <inheritdoc />
        public void AggregateInventorySummaryData(List<int> inventorySourceIds)
        {
            var houseHoldAudienceId = _AudiencesCache.GetDefaultAudience().Id;

            foreach (var inventorySourceId in inventorySourceIds)
            {
                var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
                var inventorySummaryFactory = _GetInventorySummaryFactory(inventorySource.InventoryType);
                var summaryData = _CreateInventorySummariesForSource(inventorySource, inventorySummaryFactory, houseHoldAudienceId, DateTime.Now);

                _InventorySummaryRepository.SaveInventoryAggregatedData(summaryData, inventorySourceId, null);
            }
        }

        /// <inheritdoc />
        public void AggregateInventorySummaryData(List<int> inventorySourceIds, DateTime? startDate, DateTime? endDate)
        {
            var houseHoldAudienceId = _AudiencesCache.GetDefaultAudience().Id;

            foreach (var inventorySourceId in inventorySourceIds)
            {
                var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);
                var inventorySummaryFactory = _GetInventorySummaryFactory(inventorySource.InventoryType);
                var quarters = _GetInventoryQuarters(startDate, endDate, DateTime.Now, inventorySourceId);
                var summaryData = _CreateInventorySummariesForSource(inventorySource, inventorySummaryFactory, houseHoldAudienceId, quarters);

                _InventorySummaryRepository.SaveInventoryAggregatedData(summaryData, inventorySourceId, quarters);
            }
        }
    }
}