using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities
{
    public class OpenMarketInventoryAllocationSnapshotDto
    {
        public int StationProgramFlightId { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public int Spots { get; set; }
        public string CreatedBy { get; set; }
        public string Isci { get; set; }
        public double Impressions { get; set; }
        public decimal SpotCost { get; set; }
        public int SpotLengthId { get; set; }
        public short StationCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DaypartCode { get; set; }
        public InventorySourceEnum InventorySource { get; set; }
        public int DaypartId { get; set; }
    }
}
