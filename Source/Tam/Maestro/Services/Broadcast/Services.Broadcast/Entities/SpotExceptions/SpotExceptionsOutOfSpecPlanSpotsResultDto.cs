using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecPlanSpotsResultDto
    {
        public SpotExceptionsOutOfSpecPlanSpotsResultDto()
        {
            Active = new List<SpotExceptionsOutOfSpecActivePlanSpotsDto>();
            Queued = new List<SpotExceptionsOutOfSpecQueuedPlanSpotsDto>();
            Synced = new List<SpotExceptionsOutOfSpecSyncedPlanSpotsDto>();
        }
        public List<SpotExceptionsOutOfSpecActivePlanSpotsDto> Active { get; set; }
        public List<SpotExceptionsOutOfSpecQueuedPlanSpotsDto> Queued { get; set; }
        public List<SpotExceptionsOutOfSpecSyncedPlanSpotsDto> Synced { get; set; }
    }

    public class SpotExceptionsOutOfSpecActivePlanSpotsDto
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
    public class SpotExceptionsOutOfSpecQueuedPlanSpotsDto
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
        public string Comments { get; set; }
        public List<string> PlanDaypartCodes { get; set; }
        public string InventorySourceName { get; set; }
    }
    public class SpotExceptionsOutOfSpecSyncedPlanSpotsDto
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