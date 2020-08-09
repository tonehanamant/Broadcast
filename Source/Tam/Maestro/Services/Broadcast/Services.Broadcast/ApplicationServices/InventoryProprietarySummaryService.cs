using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.ApplicationServices
{
	public interface IInventoryProprietarySummaryService : IApplicationService
	{
		[Queue("aggregateinventoryproprietarysummary")]
		[AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
		void AggregateInventoryProprietarySummary(int inventorySourceId, DateTime? startDate, DateTime? endDate);
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
			IQuarterCalculationEngine quarterCalculationEngine)
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
				{
					
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
							InventoryProprietarySummaryMarketDto marketDto = new InventoryProprietarySummaryMarketDto();
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
			if (quarterHeaderSummary.Select(q => q.Cpm).Distinct().Count() > 1)
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

	}
}