using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingBandInventoryStationsDto
    {
        public PlanBuyingBandInventoryStationsDto()
        {
            Details = new List<PlanBuyingBandStationDetailDto>();
            Totals = new PlanBuyingProgramTotalsDto();
        }
        public int Id { get; set; }
        public int? BuyingJobId { get; set; }
        public int PlanVersionId { get; set; }
        public PlanBuyingProgramTotalsDto Totals { get; set; }
        public SpotAllocationModelMode SpotAllocationModelMode { get; set; }
        public PostingTypeEnum PostingType { get; set; }
        public List<PlanBuyingBandStationDetailDto> Details { get; set; }
    }

    public class PlanBuyingBandStationDetailDto
    {
        public PlanBuyingBandStationDetailDto()
        {
            PlanBuyingBandInventoryStationDayparts = new List<PlanBuyingBandInventoryStationDaypartDto>();
        }
        public int StationId { get; set; }
        public double PostingTypeConversionRate { get; set; }
        public string RepFirm { get; set; }
        public string OwnerName { get; set; }
        public string LegacyCallLetters { get; set; }
        public int? MarketCode { get; set; }
        public double Impressions { get; set; }
        public decimal Cost { get; set; }
        public decimal Cpm { get; set; }
        public int ManifestWeeksCount { get; set; }
        public List<PlanBuyingBandInventoryStationDaypartDto> PlanBuyingBandInventoryStationDayparts { get; set; }
    }
}
