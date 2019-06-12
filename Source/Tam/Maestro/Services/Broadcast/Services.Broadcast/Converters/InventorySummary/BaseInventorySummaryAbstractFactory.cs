using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.InventorySummary
{
    public abstract class BaseInventorySummaryAbstractFactory
    {
        protected readonly IInventoryRepository InventoryRepository;
        protected readonly IInventorySummaryRepository InventorySummaryRepository;

        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IProgramRepository _ProgramRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public BaseInventorySummaryAbstractFactory(IInventoryRepository inventoryRepository,
                                                   IInventorySummaryRepository inventorySummaryRepository,
                                                   IQuarterCalculationEngine quarterCalculationEngine,
                                                   IProgramRepository programRepository,
                                                   IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            InventoryRepository = inventoryRepository;
            InventorySummaryRepository = inventorySummaryRepository;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _ProgramRepository = programRepository;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public abstract InventorySummaryDto CreateInventorySummary(InventorySource inventorySource,
                                                                   int householdAudienceId,
                                                                   QuarterDetailDto quarterDetail,
                                                                   List<InventorySummaryManifestDto> manifests);

        protected Tuple<QuarterDetailDto, QuarterDetailDto> GetQuartersForInventoryAvailable(List<StationInventoryManifestWeek> weeks)
        {
            if (weeks.Any())
            {
                var ratesAvailableFrom = weeks.Min(x => x.StartDate);
                var ratesAvailableTo = weeks.Max(x => x.EndDate);
                var ratesAvailableFromQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableFrom);
                var ratesAvailableToQuarterDetail = _QuarterCalculationEngine.GetQuarterRangeByDate(ratesAvailableTo);

                return new Tuple<QuarterDetailDto, QuarterDetailDto>(ratesAvailableFromQuarterDetail, ratesAvailableToQuarterDetail);
            }

            return new Tuple<QuarterDetailDto, QuarterDetailDto>(null, null);
        }      

        protected double? GetHouseholdImpressions(List<InventorySummaryManifestDto> inventorySummaryManifests, int householdAudienceId)
        {
            var manifestIds = inventorySummaryManifests.Select(x => x.ManifestId).ToList();

            return InventorySummaryRepository.GetInventorySummaryHouseholdImpressions(manifestIds, householdAudienceId);
        }

        protected List<InventorySummaryBookDto> GetInventoryPostingBooks(List<InventorySummaryManifestFileDto> inventorySummaryManifestFileDtos)
        {
            return inventorySummaryManifestFileDtos.
                        Where(x => x.ShareProjectionBookId.HasValue).
                        GroupBy(x => new { x.ShareProjectionBookId, x.HutProjectionBookId }).
                        Select(x => new InventorySummaryBookDto
                        {
                            ShareProjectionBookId = x.Key.ShareProjectionBookId,
                            HutProjectionBookId = x.Key.HutProjectionBookId
                        }).
                        ToList();
        }

        protected List<InventorySummaryManifestFileDto> GetInventorySummaryManifestFiles(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            var fileIds = inventorySummaryManifests.Where(x => x.FileId != null).Select(x => (int)x.FileId).Distinct().ToList();

            return InventorySummaryRepository.GetInventorySummaryManifestFileDtos(fileIds);
        }

        protected int GetTotalMarkets(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            return inventorySummaryManifests.Where(x => x.MarketCode.HasValue).GroupBy(x => x.MarketCode).Count();
        }

        protected int GetTotalStations(List<InventorySummaryManifestDto> inventorySummaryManifests)
        {
            return inventorySummaryManifests.Where(x => x.StationId.HasValue).GroupBy(x => x.StationId).Count();
        }

        protected bool GetIsInventoryUpdating(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Any(x => x.JobStatus == InventoryFileRatingsProcessingStatus.Queued ||
                                                          x.JobStatus == InventoryFileRatingsProcessingStatus.Processing);
        }

        protected DateTime? GetLastJobCompletedDate(List<InventorySummaryManifestFileDto> inventorySummaryManifestFiles)
        {
            return inventorySummaryManifestFiles.Max(x => x.JobCompletedDate);
        }

        protected void RemoveWeeksNotInQuarter(List<StationInventoryManifest> manifests, QuarterDetailDto quarterDetail)
        {
            foreach (var manifest in manifests)
            {
                manifest.ManifestWeeks = manifest.ManifestWeeks.Where(x => x.StartDate <= quarterDetail.EndDate && x.EndDate >= quarterDetail.StartDate).ToList();
            }
        }

        protected int GetTotalPrograms(List<InventorySummaryManifestDto> manifests)
        {
            var manifestIds = manifests.Select(x => x.ManifestId).ToList();
            return _ProgramRepository.GetUniqueProgramNamesByManifests(manifestIds)
                                     .Distinct(StringComparer.OrdinalIgnoreCase)
                                     .Count();
        }

        protected void CalculateHouseHoldImpressionsAndCPM(
            IEnumerable<StationInventoryManifest> manifests, 
            int householdAudienceId, 
            out double? impressionsResult, 
            out decimal? cpmResult)
        {
            impressionsResult = null;
            cpmResult = null;

            manifests = manifests.Where(x => _ManifestHasProvidedHHImpressionsAndCpm(x, householdAudienceId) ||
                                             _ManifestHasCalculatedHHImpressionsAndSpotCast(x, householdAudienceId));

            if (!manifests.Any())
                return;

            double impressionsTotal = 0;
            decimal cpmTotal = 0;

            var manifestsGroupedByMediaWeek = manifests
                .SelectMany(x => x.ManifestWeeks, (manifest, week) => new { manifest, mediaWeekId = week.MediaWeek.Id })
                .GroupBy(x => x.mediaWeekId);

            foreach (var grouping in manifestsGroupedByMediaWeek)
            {
                var weekManifests = grouping.Select(x => x.manifest).ToList();
                double weekImpressionsTotal = 0;
                decimal weekCpmTotal = 0;

                foreach (var manifest in weekManifests)
                {
                    double hhImpressions;
                    decimal hhCPM;
                    var hhAudience = manifest.ManifestAudiencesReferences.SingleOrDefault(x => x.Audience.Id == householdAudienceId && x.Impressions.HasValue);

                    if (hhAudience == null)
                    {
                        // Use calculated data if it isn`t provided. 
                        hhAudience = manifest.ManifestAudiences.Single(x => x.Audience.Id == householdAudienceId);
                        hhImpressions = hhAudience.Impressions.Value;
                        var spotCost = manifest.ManifestRates.First(r => r.SpotLengthId == manifest.SpotLengthId).SpotCost;
                        hhCPM = ProposalMath.CalculateCpm(spotCost, hhImpressions);
                    }
                    else
                    {
                        // Use provided data
                        hhImpressions = hhAudience.Impressions.Value;
                        hhCPM = hhAudience.CPM.Value;
                    }

                    weekImpressionsTotal += hhImpressions;
                    weekCpmTotal += hhCPM;
                }

                var weekImpressionsAverage = weekImpressionsTotal / weekManifests.Count;
                var weekCPMAverage = weekCpmTotal / weekManifests.Count;

                impressionsTotal += weekImpressionsAverage;
                cpmTotal += weekCPMAverage;
            }

            impressionsResult = impressionsTotal;
            cpmResult = cpmTotal / manifestsGroupedByMediaWeek.Count();
        }

        private bool _ManifestHasProvidedHHImpressionsAndCpm(StationInventoryManifest manifest, int householdAudienceId)
        {
            return manifest.ManifestAudiencesReferences.Any(x => x.Audience.Id == householdAudienceId && x.Impressions.HasValue && x.CPM.HasValue);
        }

        private bool _ManifestHasCalculatedHHImpressionsAndSpotCast(StationInventoryManifest manifest, int householdAudienceId)
        {
            return manifest.ManifestAudiences.Any(x => x.Audience.Id == householdAudienceId && x.Impressions.HasValue) &&
                   manifest.ManifestRates.Any(x => x.SpotLengthId == manifest.SpotLengthId);
        }

        protected bool HasInventoryGapsForDateRange(IEnumerable<StationInventoryManifestWeek> manifestWeeks, Tuple<QuarterDetailDto, QuarterDetailDto> inventoryDateRangeTuple)
        {
            var dateRange = DateRange.ConvertToDateRange(inventoryDateRangeTuple);

            // no inventory - no gaps
            if (dateRange.IsEmpty())
            {
                return false;
            }

            var manifestMediaWeeks = manifestWeeks.Select(x => x.MediaWeek.Id).Distinct();
            var mediaWeeksForDateRange = _MediaMonthAndWeekAggregateCache.GetMediaWeeksIntersecting(dateRange.Start.Value, dateRange.End.Value).Select(x => x.Id);

            return mediaWeeksForDateRange.Except(manifestMediaWeeks).Any();
        }
    }
}
