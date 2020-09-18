using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryProprietarySummaryService : IApplicationService
    {
        [Queue("aggregateinventoryproprietarysummary")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void AggregateInventoryProprietarySummary(int inventorySourceId, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// </summary>
		/// <param name="inventoryProprietarySummaryRequest"></param>
        /// <returns></returns>
		InventoryProprietarySummaryResponse GetInventoryProprietarySummaries(InventoryProprietarySummaryRequest inventoryProprietarySummaryRequest);
        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <returns> Aggregated CPM, Impressions, Coverage and Percentage of Plan Impressions</returns>
        TotalInventoryProprietarySummaryResponse GetPlanProprietarySummaryAggregation(
            TotalInventoryProprietarySummaryRequest request);
    }

    public class InventoryProprietarySummaryService : BroadcastBaseClass, IInventoryProprietarySummaryService
    {
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryProprietarySummaryRepository _InventoryProprietarySummaryRepository;
        protected readonly IInventoryRepository InventoryRepository;
        protected readonly IStationRepository _StationRepository;
        protected readonly IMarketCoverageRepository _MarketCoverageRepository;
        protected readonly IDaypartDefaultRepository _DaypartDefaultRepository;
        protected readonly IBroadcastAudienceRepository _AudienceRepository;

        private const int SPOT_LENGTH_15 = 15;
        private const int SPOT_LENGTH_30 = 30;
        private readonly Dictionary<int, int> _SpotLengthMap;

        public InventoryProprietarySummaryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IQuarterCalculationEngine quarterCalculationEngine)
        {
            _QuarterCalculationEngine = quarterCalculationEngine;

            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();

            _InventoryProprietarySummaryRepository = broadcastDataRepositoryFactory
                .GetDataRepository<IInventoryProprietarySummaryRepository>();

            _MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _AudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _SpotLengthMap = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

        }

        public void AggregateInventoryProprietarySummary(int inventorySourceId, DateTime? startDate, DateTime? endDate)
        {
            _LogInfo($"Starting job AggregateInventoryProprietarySummary. " +
                     $"Inventory SourceId = '{inventorySourceId}'; " +
                     $"startDate = {startDate}; " +
                     $"endDate = {endDate}");

            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);

            _LogInfo($"running  job AggregateInventoryProprietarySummary to GetInventorySource. " +
                     $"Inventory Source = '{inventorySource.Name}'");

            var quarters = _GetInventoryQuarters(startDate, endDate, DateTime.Now, inventorySourceId);

            foreach (var quarterDetail in quarters)
            {
                _LogInfo($"Running job AggregateInventoryProprietarySummary to GetInventoryProprietaryQuarterSummaries. " +
                         $"Inventory SourceId = '{inventorySourceId}'; " +
                         $"startDate = {startDate}; " +
                         $"endDate = {endDate}");

                var inventoryProprietarySummaries = _InventoryProprietarySummaryRepository.GetInventoryProprietaryQuarterSummaries(
                    inventorySource,
                    quarterDetail.StartDate,
                    quarterDetail.EndDate);

                foreach (var summary in inventoryProprietarySummaries)
                {
                    summary.Quarter = new QuarterDto
                    {
                        Quarter = quarterDetail.Quarter,
                        Year = quarterDetail.Year
                    };

                    _LogInfo($"Running  job AggregateInventoryProprietarySummary to GetMarketCodesForInventoryProprietarySummary. " +
                         $"Inventory SourceId = '{inventorySourceId}'; " +
                         $"ProprietaryDaypartProgramMappingId = '{summary.ProprietaryDaypartProgramMappingId}'" +
                         $"startDate = {startDate}; " +
                         $"endDate = {endDate}");

                    summary.SummaryByStationByAudience = _InventoryProprietarySummaryRepository.GetInventoryProprietarySummaryByStationByAudience(
                        summary.ProprietaryDaypartProgramMappingId,
                        quarterDetail.StartDate,
                        quarterDetail.EndDate);

                    _LogInfo($"Running  job AggregateInventoryProprietarySummary to start SaveInventoryProprietarySummary. " +
                             $"Inventory SourceId = '{inventorySourceId}'; " +
                             $"startDate = {startDate}; " +
                             $"endDate = {endDate}");

                    _InventoryProprietarySummaryRepository.SaveInventoryProprietarySummary(summary);

                    _LogInfo($"Running  job AggregateInventoryProprietarySummary to  end SaveInventoryProprietarySummary. " +
                             $"Inventory SourceId = '{inventorySourceId}'; " +
                             $"startDate = {startDate}; " +
                             $"endDate = {endDate}");
                }
            }
        }

        private List<QuarterDetailDto> _GetInventoryQuarters(DateTime? startDate, DateTime? endDate,
            DateTime currentDate, int inventorySourceId)
        {
            if (startDate.HasValue && endDate.HasValue)
                return _QuarterCalculationEngine.GetAllQuartersBetweenDates(startDate.GetValueOrDefault(),
                    endDate.GetValueOrDefault());

            var dateRange = _GetInventorySourceOrCurrentQuarterDateRange(inventorySourceId, currentDate);

            return _QuarterCalculationEngine.GetAllQuartersBetweenDates(dateRange.Start.GetValueOrDefault(),
                dateRange.End.GetValueOrDefault());
        }

        private DateRange _GetInventorySourceOrCurrentQuarterDateRange(int inventorySourceId, DateTime currentDate)
        {
            var dateRange = _InventoryRepository.GetInventorySourceDateRange(inventorySourceId);

            if (dateRange.IsEmpty()) dateRange = _GetCurrentQuarterDateRange(currentDate);

            return dateRange;
        }

        private DateRange _GetCurrentQuarterDateRange(DateTime currentDate)
        {
            var datesTuple = _QuarterCalculationEngine.GetDatesForTimeframe(RatesTimeframe.THISQUARTER, currentDate);
            return new DateRange(datesTuple.Item1, datesTuple.Item2);
        }


        public InventoryProprietarySummaryResponse GetInventoryProprietarySummaries(
            InventoryProprietarySummaryRequest inventoryProprietarySummaryRequest)
        {
            var response = new InventoryProprietarySummaryResponse();
            var quarters =
                _QuarterCalculationEngine.GetAllQuartersBetweenDates(inventoryProprietarySummaryRequest.FlightStartDate, inventoryProprietarySummaryRequest.FlightEndDate);

            response.ValidationMessage = Validate(inventoryProprietarySummaryRequest, quarters);

            if (!string.IsNullOrEmpty(response.ValidationMessage))
            {
                return response;
            }

            var firstQuarter = quarters.First();
            var summaryDaypartDefaultIds = _ConvertPlanDefaultDaypartIdsToInventoryDefaultDaypartIds(inventoryProprietarySummaryRequest, firstQuarter);
            var proprietarySummaries = _InventoryProprietarySummaryRepository.GetInventoryProprietarySummary(firstQuarter, summaryDaypartDefaultIds);
            var marketCoverageByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var summaryAudienceIds = _AudienceRepository
                .GetRatingsAudiencesByMaestroAudience(new List<int> { inventoryProprietarySummaryRequest.AudienceId })
                .Select(am => am.rating_audience_id)
                .Distinct()
                .ToList();

            //This will give us total number of active weeks for selected plan quarter
            int activeWeekCount = inventoryProprietarySummaryRequest.WeeklyBreakdownWeeks
                .Where(w => w.NumberOfActiveDays > 0).Count();

            foreach (var proprietarySummary in proprietarySummaries)
            {
                proprietarySummary.ImpressionsTotal = _GetImpressions(proprietarySummary.Id, summaryAudienceIds);

                if (activeWeekCount > 1)
                {
                    proprietarySummary.ImpressionsTotal = activeWeekCount * proprietarySummary.ImpressionsTotal;
                    proprietarySummary.UnitCost = activeWeekCount * proprietarySummary.UnitCost;
                }
                if (_IsSpotLengthExist15Not30(inventoryProprietarySummaryRequest.SpotLengthIds))
                {
                    proprietarySummary.ImpressionsTotal /= 2;
                    proprietarySummary.UnitCost /= 2;
                }
                proprietarySummary.Cpm = ProposalMath.CalculateCpm(proprietarySummary.UnitCost, proprietarySummary.ImpressionsTotal);

                var marketCodes = _InventoryProprietarySummaryRepository.GetMarketCodesBySummaryIds(new List<int> { proprietarySummary.Id });
                var totalCoverage = marketCodes.Sum(x => marketCoverageByMarketCode[x]);

                proprietarySummary.MarketCoverageTotal = Math.Round(totalCoverage);

                proprietarySummary.NumberOfUnit = 1;
            }

            response.summaries = proprietarySummaries;

            return response;
        }
        protected bool _IsSpotLengthExist15Not30(List<int> spotLengthIds)
        {
            var spotLength15Not30 = false;
            int spotLengthIdI5 = _SpotLengthMap[SPOT_LENGTH_15];
            int spotLengthId30 = _SpotLengthMap[SPOT_LENGTH_30];

            if (spotLengthIds.Contains(spotLengthIdI5)
                && !spotLengthIds.Contains(spotLengthId30))
            {
                spotLength15Not30 = true;
            }
            return spotLength15Not30;

        }
        protected string Validate(InventoryProprietarySummaryRequest inventoryProprietarySummaryRequest
            , List<QuarterDetailDto> quarterDetails)
        {
            string validationMessage = null;

            if (quarterDetails.Count() > 1)
            {
                validationMessage = "Plan flight falls in more than one quarter, please update to select Units.";
            }

            var allowedSpotLengthIds = new List<int> { _SpotLengthMap[SPOT_LENGTH_15], _SpotLengthMap[SPOT_LENGTH_30] };

            if (!inventoryProprietarySummaryRequest.SpotLengthIds.Any(s => allowedSpotLengthIds.Contains(s)))
            {
                validationMessage += "Units only available for 15 and 30 spot lengths, please update to select Units.";

            }

            return validationMessage;

        }

        private List<int> _ConvertPlanDefaultDaypartIdsToInventoryDefaultDaypartIds(InventoryProprietarySummaryRequest inventoryProprietarySummaryRequest
            , QuarterDetailDto QuarterDetail)
        {
            // First Get all default_daypart_ids  from InventoryProprietary Summary Service based on quarter
            var daypartDefaultIds = _InventoryProprietarySummaryRepository.GetDayPartDefaultIds(QuarterDetail);

            var daypartIds = _DaypartDefaultRepository.GetDayPartIds(daypartDefaultIds);
            // Get DisplayDayPart info based on Inventory dayPart Ids
            var displayDaypartDictionary = DaypartCache.Instance.GetDisplayDayparts(daypartIds);

            // Get DisplayDayPart list from dictionary, which is needed in Intersecting logic
            List<DisplayDaypart> inventoryDisplayDayparts = displayDaypartDictionary.Select(i => i.Value).ToList();

            //Get Intersecting daypart_id 
            List<int> daypartIdsFinalList = _GetIntersectingDaypartIds(inventoryProprietarySummaryRequest, inventoryDisplayDayparts).ToList();

            var summaryDaypartDefaultIds = _DaypartDefaultRepository.GetDaypartDefaultIds(daypartIdsFinalList);
            return summaryDaypartDefaultIds;
        }

        private HashSet<int> _GetIntersectingDaypartIds(InventoryProprietarySummaryRequest inventoryProprietarySummaryRequest
            , List<DisplayDaypart> inventoryDisplayDayparts)
        {
            HashSet<int> daypartIdsFinalList = new HashSet<int>();
            var planDefaultDaypartIds = inventoryProprietarySummaryRequest.PlanDaypartRequests.Select(d => d.DefaultDayPartId).ToList();
            var planDaypartIds = _DaypartDefaultRepository.GetDayPartIds(planDefaultDaypartIds);

            foreach (var planDaypartId in planDaypartIds)
            {
                var planDisplayDaypart = DaypartCache.Instance.GetDisplayDaypart(planDaypartId);

                DisplayDaypart[] displayDayparts =
                    DisplayDaypart.GetIntersectingDayparts(planDisplayDaypart, inventoryDisplayDayparts.ToArray());

                displayDayparts.ForEach(dd => daypartIdsFinalList.Add(dd.Id));

            }

            return daypartIdsFinalList;
        }
        public TotalInventoryProprietarySummaryResponse GetPlanProprietarySummaryAggregation(
            TotalInventoryProprietarySummaryRequest request)
        {
            var response = new TotalInventoryProprietarySummaryResponse();
            var summaryList = new List<InventoryProprietarySummary>();
            var summaryIds = request.InventoryProprietarySummaryIds;
            int activeWeekCount = request.WeeklyBreakdownWeeks.Where(w => w.NumberOfActiveDays > 0).ToList().Count;
            var summaryAudienceIds = _AudienceRepository
                .GetRatingsAudiencesByMaestroAudience(new List<int> { request.PlanPrimaryAudienceId })
                .Select(am => am.rating_audience_id).Distinct().ToList();

			foreach (var summaryId in summaryIds)
			{
				var summary = new InventoryProprietarySummary
				{
					UnitCost = _GetUnitCost(summaryId.Id),
					ImpressionsTotal = _GetImpressions(summaryId.Id, summaryAudienceIds)
				};

                if (activeWeekCount > 1)
                {
                    summary.ImpressionsTotal = activeWeekCount * summary.ImpressionsTotal;
                    summary.UnitCost = activeWeekCount * summary.UnitCost;
                }

				if (_IsSpotLengthExist15Not30(request.SpotLengthIds))
				{
					summary.ImpressionsTotal /= 2;
					summary.UnitCost /= 2;
				}
				summary.Cpm = ProposalMath.CalculateCpm(summary.UnitCost, summary.ImpressionsTotal);
				summary.NumberOfUnit = summaryId.NumberOfUnit;

                summaryList.Add(summary);
            }

            var totalImpressions = summaryList.Select(i => i.ImpressionsTotal).Sum();

			response.Impressions = totalImpressions;
			response.MarketCoverage = GetTotalMarketCoverage(summaryIds.Select(s => s.Id).ToList());
			response.Cpm = _GetTotalCpm(summaryList);

			response.PercentageOfPlanImpressions =
			_GetPercentageOfPlanImpressions(request.PlanGoalImpressions, response.Impressions);
			response.NumberOfUnits = summaryList.Sum(s => s.NumberOfUnit);

            return response;
        }

        private double _GetPercentageOfPlanImpressions(double planGoalImpressions, double totalImpressions)
        {
			// BP-1462 Plan Goal Impressions are not raw numbers. FE is sending planGoalImpressions/1000 while proprietary impressions are raw numbers.
			var convertedPlanGoalImpressions = planGoalImpressions * 1000;
			var percentageOfPlanImpressions =
				Math.Round(GeneralMath.ConvertFractionToPercentage(totalImpressions / convertedPlanGoalImpressions));
			return percentageOfPlanImpressions;
        }

        private decimal _GetUnitCost(int id)
        {
            var unitCost = _InventoryProprietarySummaryRepository.GetProprietarySummaryUnitCost(id);

            return unitCost ?? 0;
        }

        private decimal _GetTotalCpm(List<InventoryProprietarySummary> inventoryProprietarySummaries)
        {
            var totalCost = 0m;
            var totalImpressions = 0d;

            foreach (var summary in inventoryProprietarySummaries)
            {
                totalCost += summary.UnitCost;
                totalImpressions += summary.ImpressionsTotal;
            }

            return ProposalMath.CalculateCpm(totalCost, totalImpressions);
        }

        private double GetTotalMarketCoverage(List<int> summaryIds)
        {
            var marketCodes = _InventoryProprietarySummaryRepository.GetMarketCodesBySummaryIds(summaryIds);
            var marketCoverageByMarketCode = _MarketCoverageRepository.GetLatestMarketCoverages().MarketCoveragesByMarketCode;
            var totalCoverage = marketCodes.Sum(x => marketCoverageByMarketCode[x]);

            return Math.Round(totalCoverage);
        }

        private double _GetImpressions(int invPropSummaryId
            , List<int> summaryAudienceIds)
        {
            double impressions = _InventoryProprietarySummaryRepository.GetTotalImpressionsBySummaryIdAndAudienceIds(invPropSummaryId, summaryAudienceIds);

            return impressions;
        }
    }
}