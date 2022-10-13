using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class SpotExceptionsOutOfSpecPlanSpotsResultDto
    {
        public SpotExceptionsOutOfSpecPlanSpotsResultDto()
        {
            Active = new List<SpotExceptionsOutOfSpecPlanSpotsDto>();
            Queued = new List<SpotExceptionsOutOfSpecDonePlanSpotsDto>();
            Synced = new List<SpotExceptionsOutOfSpecDonePlanSpotsDto>();
        }
        public List<SpotExceptionsOutOfSpecPlanSpotsDto> Active { get; set; }
        public List<SpotExceptionsOutOfSpecDonePlanSpotsDto> Queued { get; set; }
        public List<SpotExceptionsOutOfSpecDonePlanSpotsDto> Synced { get; set; }
    }

    public class SpotExceptionsOutOfSpecPlanSpotsDto
    {  
        public int Id { get; set; }
        public int EstimateId { get; set; }
        public string Reason { get; set; }
        public int? MarketRank { get; set; }
        public int DMA { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string TimeZone { get; set; }
        public string Affiliate { get; set; }
        public string Day { get; set; }
        public string GenreName { get; set; }
        public string HouseIsci { get; set; }
        public string ClientIsci { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
        public string ProgramName { get; set; }
        public string SpotLengthString { get; set; }
        public string DaypartCode { get; set; }
        public string Comments { get; set; }
        public List<string> PlanDaypartCodes { get; set; }
        public string InventorySourceName { get; set; }
    }

    public class SpotExceptionsOutOfSpecDonePlanSpotsDto
    {
        public int Id { get; set; }
        public int EstimateId { get; set; }
        public string Reason { get; set; }
        public int? MarketRank { get; set; }
        public int DMA { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string TimeZone { get; set; }
        public string Affiliate { get; set; }
        public string Day { get; set; }
        public string GenreName { get; set; }
        public string HouseIsci { get; set; }
        public string ClientIsci { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
        public string FlightEndDate { get; set; }
        public string ProgramName { get; set; }
        public string SpotLengthString { get; set; }
        public string DaypartCode { get; set; }
        public string DecisionString { get; set; }
        public string SyncedTimestamp { get; set; }
        public string Comments { get; set; }
        public List<string> PlanDaypartCodes { get; set; }
        public string InventorySourceName { get; set; }
    }
}