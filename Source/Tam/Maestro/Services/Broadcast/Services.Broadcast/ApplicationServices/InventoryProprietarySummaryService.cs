using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Repositories;
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
		public InventoryProprietarySummaryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
			IQuarterCalculationEngine quarterCalculationEngine )
		{
			_QuarterCalculationEngine = quarterCalculationEngine;

			_InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

			_InventoryProprietarySummaryRepository = broadcastDataRepositoryFactory
				.GetDataRepository<IInventoryProprietarySummaryRepository>();

			_MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
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
				var quarterHeaderSummary =
					_InventoryProprietarySummaryRepository.GetDataForInventoryProprietarySummary(inventorySource,
						quarterDetail.StartDate, quarterDetail.EndDate);

				if (IsValidQuarterSummary(quarterHeaderSummary))
					foreach (var detail in quarterHeaderSummary)
					{
						_LogInfo(
							$"Running  job AggregateInventoryProprietarySummary to GetInventorySummaryQuarter. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");
						detail.Quarter = GetInventorySummaryQuarter(quarterDetail);
						_LogInfo(
							$"Running  job AggregateInventoryProprietarySummary to GetDataForInventoryProprietarySummaryAudiences. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");
						detail.Audiences =
							_InventoryProprietarySummaryRepository.GetDataForInventoryProprietarySummaryAudiences(
								inventorySource, quarterDetail.StartDate, quarterDetail.EndDate);
						_LogInfo(
							$"Running  job AggregateInventoryProprietarySummary to GetMarketCodesForInventoryProprietarySummary. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");
						var marketCodes =
							_InventoryProprietarySummaryRepository.GetMarketCodesForInventoryProprietarySummary(
								inventorySource, quarterDetail.StartDate, quarterDetail.EndDate);
						var updatedMarketCodes = marketCodes.Select(m => Convert.ToInt32(m.Value)).ToList();
						_LogInfo(
							$"Running  job AggregateInventoryProprietarySummary to GetLatestMarketCoverages. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");
						var latestMarketCoverages =
							_MarketCoverageRepository.GetLatestMarketCoverages(updatedMarketCodes);
						detail.Markets = new List<InventoryProprietarySummaryMarketDto>();
						foreach (var marketCode in updatedMarketCodes)
						{
							var marketDto = new InventoryProprietarySummaryMarketDto();
							marketDto.MarketCoverage = latestMarketCoverages.MarketCoveragesByMarketCode[marketCode];
							marketDto.MarketCode = Convert.ToInt16(marketCode);
							detail.Markets.Add(marketDto);
						}

						_LogInfo(
							$"Running  job AggregateInventoryProprietarySummary to start SaveInventoryProprietarySummary. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");
						_InventoryProprietarySummaryRepository.SaveInventoryProprietarySummary(detail);
						_LogInfo(
							$"Running  job AggregateInventoryProprietarySummary to  end SaveInventoryProprietarySummary. Inventory SourceId = '{inventorySourceId}'; startDate = {startDate}; endDate = {endDate}");
					}
			}
		}

		protected QuarterDto GetInventorySummaryQuarter(QuarterDetailDto quarterDetail)
		{
			return new QuarterDto
			{
				Quarter = quarterDetail.Quarter,
				Year = quarterDetail.Year
			};
		}

		public bool IsValidQuarterSummary(List<InventoryProprietaryQuarterSummaryDto> quarterHeaderSummary)
		{
			_LogInfo("Running  job AggregateInventoryProprietarySummary to ValidateQuarterSummary. ");
			var groupCount = quarterHeaderSummary.GroupBy(s => new {s.InventorySourceId, s.DefaultDaypartId,s.Cpm}).Select(g => g.Count()).ToList();
			
			if(groupCount.Any(c=>c>1))
			{
				var summary = quarterHeaderSummary.FirstOrDefault();
				_LogInfo(
					$"Job AggregateInventoryProprietarySummary validation failed. CPM value is more than one for Inventory SourceId = '{summary.InventorySourceId}'; DaypartDefaultId = '{summary.DefaultDaypartId}'");
				return false;
			}

			return true;
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
			var QuarterDetails =
				_QuarterCalculationEngine.GetAllQuartersBetweenDates(inventoryProprietarySummaryRequest.FlightStartDate, inventoryProprietarySummaryRequest.FlightEndDate);
			if (QuarterDetails.Count() > 1)
			{
				response.ValidationMessage =
					$"Invalid request. More than two quarters for start date: {inventoryProprietarySummaryRequest.FlightStartDate} and end date: {inventoryProprietarySummaryRequest.FlightEndDate}";
			}
			else
			{
				List<List<int>> dayPartIdsFinalList = new List<List<int>>();

				// we need inventory proprietary summary day_part_ids and we are getting Plan day part info in request so we need following conversion
				List<int> summaryDayPartIds = ConvertPlanDayPartIdsToInventoryDayPartIds(inventoryProprietarySummaryRequest, QuarterDetails.Single(), dayPartIdsFinalList);

				var invPropSummaryList =
					_InventoryProprietarySummaryRepository.GetInventoryProprietarySummary(
						QuarterDetails.FirstOrDefault(), summaryDayPartIds);


				foreach (var invPropSummary in invPropSummaryList)
				{

					invPropSummary.ImpressionsTotal = Math.Round(
						_InventoryProprietarySummaryRepository.GetTotalImpressionsBySummaryIdAndAudienceId(invPropSummary.Id, inventoryProprietarySummaryRequest.AudienceId)
						);


					invPropSummary.MarketCoverageTotal =	_InventoryProprietarySummaryRepository.GetTotalMarketCoverageBySummaryId(invPropSummary.Id);

				}

				response.summaries = invPropSummaryList;
			}

			return response;
		}

		private List<int> ConvertPlanDayPartIdsToInventoryDayPartIds(InventoryProprietarySummaryRequest dto, QuarterDetailDto QuarterDetail, List<List<int>> dayPartIdsFinalList)
		{

			List<DisplayDaypart> displayDayPartList = new List<DisplayDaypart>();

			// First Get all Daypart Ids from InventoryProprietary Summary Service based on quarter
			var inventoryDayPartIds = _InventoryProprietarySummaryRepository.GetDayPartIds(QuarterDetail);

			// Get DisplayDayPart info based on Inventory dayPart Ids
			var displayDayPartDictionary = DaypartCache.Instance.GetDisplayDayparts(inventoryDayPartIds);
			//refactore this and add lambda
			displayDayPartDictionary.ToList();

			displayDayPartDictionary.ForEach(d => displayDayPartList.Add(d.Value));

			DisplayDaypart[] inventoryDayPartArrayList = displayDayPartList.ToArray();
		
			// For each PlanDayPart Request, get intersecting inventory day part id
			foreach (var PlanDaypartReq in dto.PlanDaypartRequests)
			{
				// get daypart Id from default day part Id
				var planDisplayDaypart = DaypartCache.Instance.GetDisplayDaypart(PlanDaypartReq.DefaultDayPartId);

				//int i = 1;
				//DisplayDaypart planDaypart = new DisplayDaypart(i, PlanDaypartReq.StartTimeSeconds, PlanDaypartReq.EndTimeSeconds, defaultDaypart.Monday, defaultDaypart.Tuesday, defaultDaypart.Wednesday, defaultDaypart.Thursday, defaultDaypart.Friday, defaultDaypart.Saturday, defaultDaypart.Sunday);
				//i++;

				DisplayDaypart[] displayDayparts = DisplayDaypart.GetIntersectingDayparts(planDisplayDaypart, inventoryDayPartArrayList);
				var IdList = displayDayparts.Select(d => d.Id);
				dayPartIdsFinalList.Add(IdList.ToList());

			}

			List<int> summaryDayPartIds = dayPartIdsFinalList.SelectMany(x => x).Distinct().ToList();
			return summaryDayPartIds;
		}
	}
}