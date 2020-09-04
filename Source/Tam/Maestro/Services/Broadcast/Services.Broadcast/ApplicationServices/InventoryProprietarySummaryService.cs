using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Hangfire;
using Microsoft.VisualBasic;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;
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
		/// <param name="dto"></param>
		/// <returns></returns>
		InventoryProprietarySummaryResponse GetInventoryProprietarySummaries(InventoryProprietarySummaryRequest dto);
		//	void Test();
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
		
		private  const int SPOT_LENGTH_15 = 15;
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
			_LogInfo(
				$"Starting  job AggregateInventoryProprietarySummary. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");

            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceId);

            _LogInfo(
				$"running  job AggregateInventoryProprietarySummary to GetInventorySource. Inventory Source = '{inventorySource.Name}'");

            var quarters = _GetInventoryQuarters(startDate, endDate, DateTime.Now, inventorySourceId);

			foreach (var quarterDetail in quarters)
			{
				_LogInfo(
					$"Running  job AggregateInventoryProprietarySummary to GetInventoryProprietarySummary. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");

                var inventoryProprietarySummaries =
					_InventoryProprietarySummaryRepository.GetInventoryProprietaryQuarterSummaries(inventorySource,
						quarterDetail.StartDate, quarterDetail.EndDate);

				foreach (var summary in inventoryProprietarySummaries)
				{
					_LogInfo(
						$"Running  job AggregateInventoryProprietarySummary to GetInventorySummaryQuarter. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");

                    summary.Quarter = _GetInventorySummaryQuarter(quarterDetail);

                    _LogInfo(
						$"Running  job AggregateInventoryProprietarySummary to GetDataForInventoryProprietarySummaryAudiences. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");

                    summary.Audiences = _InventoryProprietarySummaryRepository.GetInventoryProprietarySummaryAudiences(
                            summary.ProprietaryDaypartProgramMappingId, quarterDetail.StartDate, quarterDetail.EndDate);

                    _LogInfo(
						$"Running  job AggregateInventoryProprietarySummary to GetMarketCodesForInventoryProprietarySummary. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");

                    var marketCodes =
						_InventoryProprietarySummaryRepository.GetMarketCodesForInventoryProprietarySummary(
                            summary.ProprietaryDaypartProgramMappingId, quarterDetail.StartDate, quarterDetail.EndDate);

                    var updatedMarketCodes = marketCodes.Select(m => Convert.ToInt32(m.Value)).ToList();

                    _LogInfo(
						$"Running  job AggregateInventoryProprietarySummary to GetLatestMarketCoverages. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");

                    var latestMarketCoverages =
						_MarketCoverageRepository.GetLatestMarketCoverages(updatedMarketCodes);

                    summary.Markets = new List<InventoryProprietarySummaryMarketDto>();

                    foreach (var marketCode in updatedMarketCodes)
					{
                        var marketDto = new InventoryProprietarySummaryMarketDto
                        {
                            MarketCoverage = latestMarketCoverages.MarketCoveragesByMarketCode[marketCode],
                            MarketCode = Convert.ToInt16(marketCode)
                        };
                        summary.Markets.Add(marketDto);
					}

					_LogInfo(
						$"Running  job AggregateInventoryProprietarySummary to start SaveInventoryProprietarySummary. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");

                    _InventoryProprietarySummaryRepository.SaveInventoryProprietarySummary(summary);

                    _LogInfo(
						$"Running  job AggregateInventoryProprietarySummary to  end SaveInventoryProprietarySummary. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");
				}
			}
		}

        private QuarterDto _GetInventorySummaryQuarter(QuarterDetailDto quarterDetail)
		{
			return new QuarterDto
			{
				Quarter = quarterDetail.Quarter,
				Year = quarterDetail.Year
			};
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

			if (string.IsNullOrEmpty(response.ValidationMessage))
			{
				response.ValidationMessage = "Plan flight falls in more than one quarter, please update to select Units";
                return response;
			}

            var firstQuarter = quarters.First();

            var summaryDaypartIds = _ConvertPlanDayPartIdsToInventoryDayPartIds(inventoryProprietarySummaryRequest, firstQuarter);

			var proprietarySummaries =
				_InventoryProprietarySummaryRepository.GetInventoryProprietarySummary(firstQuarter, summaryDaypartIds);
			
			var summaryAudienceIds = _AudienceRepository.GetRatingsAudiencesByMaestroAudience(new List<int> { inventoryProprietarySummaryRequest.AudienceId }).Select(am => am.rating_audience_id).Distinct().ToList();

			foreach (var proprietarySummary in proprietarySummaries)
			{
                proprietarySummary.ImpressionsTotal = GetImpressions(inventoryProprietarySummaryRequest, proprietarySummary.Id, summaryAudienceIds);
                proprietarySummary.Cpm = ProposalMath.CalculateCpm(proprietarySummary.UnitCost, proprietarySummary.ImpressionsTotal);
				var marketCoverage = _InventoryProprietarySummaryRepository.GetTotalMarketCoverageBySummaryId(proprietarySummary.Id);
                proprietarySummary.MarketCoverageTotal = marketCoverage;
            }

            return response;
		}

		private double GetImpressions(InventoryProprietarySummaryRequest inventoryProprietarySummaryRequest,int invPropSummaryId,  List<int> summaryAudienceIds)
		{
			double impressions = Math.Round(_InventoryProprietarySummaryRepository.GetTotalImpressionsBySummaryIdAndAudienceIds(invPropSummaryId, summaryAudienceIds));

			int spotLengthIdI5 = _SpotLengthMap.Where(s => s.Key.Equals(SPOT_LENGTH_15)).Select(s => s.Value).Single();
			int spotLengthId30 = _SpotLengthMap.Where(s => s.Key.Equals(SPOT_LENGTH_30)).Select(s => s.Value).Single();
			if (inventoryProprietarySummaryRequest.SpotLengthIds.Contains(spotLengthIdI5) && !inventoryProprietarySummaryRequest.SpotLengthIds.Contains(spotLengthId30))
			{
				impressions = impressions / 2;
			}
			return impressions;
		}


		protected string Validate(InventoryProprietarySummaryRequest inventoryProprietarySummaryRequest, List<QuarterDetailDto> quarterDetails)
		{
			string validationMessage = string.Empty;

			if (quarterDetails.Count() > 1)
			{
				validationMessage = "Plan flight falls in more than one quarter, please update to select Units.";
			}
			
			List<int> allowedSpotLengthIds = _SpotLengthMap.Where(s => s.Key.Equals(SPOT_LENGTH_15) || s.Key.Equals(SPOT_LENGTH_30))
				.Select(s => s.Value).ToList();
			if (!inventoryProprietarySummaryRequest.SpotLengthIds.Any(s => allowedSpotLengthIds.Contains(s)))
			{
				validationMessage += "Units only available for 15 and 30 spot lengths, please update to select Units.";
				
			}
			return validationMessage;

		}
		private HashSet<int> _ConvertPlanDayPartIdsToInventoryDayPartIds(InventoryProprietarySummaryRequest dto, QuarterDetailDto QuarterDetail)
		{
			// First Get all Daypart Ids from InventoryProprietary Summary Service based on quarter
			var inventoryDayPartIds = _InventoryProprietarySummaryRepository.GetDaypartIds(QuarterDetail);

			// Get DisplayDayPart info based on Inventory dayPart Ids
			var displayDayPartDictionary = DaypartCache.Instance.GetDisplayDayparts(inventoryDayPartIds);

			// Get DisplayDayPart list from dictionary, which is needed in Intersecting logic
			List<DisplayDaypart> inventoryDisplayDayParts = displayDayPartDictionary.Select(i => i.Value).ToList();

			//Get Intersecting daypart_id 
			HashSet<int> dayPartIdsFinalList = _GetIntersectingDayPartIds(dto, inventoryDisplayDayParts);

			return dayPartIdsFinalList;
		}

		private HashSet<int> _GetIntersectingDayPartIds(InventoryProprietarySummaryRequest dto, List<DisplayDaypart> inventoryDisplayDayParts)
		{
			HashSet<int> dayPartIdsFinalList = new HashSet<int>();
			
			foreach (var planDaypartReq in dto.PlanDaypartRequests)
			{
				var planDayPartId = _DaypartDefaultRepository.GetDayprtId(planDaypartReq.DefaultDayPartId);

				var planDisplayDaypart = DaypartCache.Instance.GetDisplayDaypart(planDayPartId);

				DisplayDaypart[] displayDayparts =
					DisplayDaypart.GetIntersectingDayparts(planDisplayDaypart, inventoryDisplayDayParts.ToArray());

				displayDayparts.ForEach(dd => dayPartIdsFinalList.Add(dd.Id));
				
			}

			return dayPartIdsFinalList;
		}
	}
}