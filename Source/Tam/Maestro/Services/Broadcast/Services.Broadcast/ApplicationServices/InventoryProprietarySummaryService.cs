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

			response.ValidationMessage = Validate(inventoryProprietarySummaryRequest, QuarterDetails);

			if (string.IsNullOrEmpty(response.ValidationMessage))
			{
				HashSet<int> summaryDayPartIds = _ConvertPlanDayPartIdsToInventoryDayPartIds(inventoryProprietarySummaryRequest, QuarterDetails.Single());

				var invPropSummaryList =
					_InventoryProprietarySummaryRepository.GetInventoryProprietarySummary(
						QuarterDetails.FirstOrDefault(), summaryDayPartIds);
			
				var summaryAudienceIds = _AudienceRepository.GetRatingsAudiencesByMaestroAudience(new List<int> { inventoryProprietarySummaryRequest.AudienceId }).Select(am => am.rating_audience_id).Distinct().ToList();

				foreach (var invPropSummary in invPropSummaryList)
				{

					invPropSummary.ImpressionsTotal=  GetImpressions(inventoryProprietarySummaryRequest, invPropSummary.Id, summaryAudienceIds);

					invPropSummary.MarketCoverageTotal = Math.Round(_InventoryProprietarySummaryRepository.GetTotalMarketCoverageBySummaryId(invPropSummary.Id), 0); 
				}

				response.summaries = invPropSummaryList;
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
			var inventoryDayPartIds = _InventoryProprietarySummaryRepository.GetDayPartIds(QuarterDetail);

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