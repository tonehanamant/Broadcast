using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.MarketCoverageByStation;

namespace Services.Broadcast.Entities.Campaign
{
    public class ProgramLineupProprietaryInventory
    {
        public Station Station { get; set; }
        public int MarketCode { get; set; }
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public int InventoryProprietaryDaypartProgramId { get; set; }
        public DisplayDaypart Daypart { get; set; }
        public int DaypartId { get; set; }
        public int SpotLengthId { get; set; }
        public double ImpressionsPerWeek { get; set; }
        public double TotalImpressions { get; set; }
    }
}
