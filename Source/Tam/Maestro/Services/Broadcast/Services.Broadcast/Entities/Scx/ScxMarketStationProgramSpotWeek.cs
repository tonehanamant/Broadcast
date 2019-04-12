using Services.Broadcast.Entities.OpenMarketInventory;
using System;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxMarketStationProgramSpotWeek
    {
        public MediaWeek MediaWeek { get; set; }
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public ProposalOpenMarketInventoryWeekDto InventoryWeek { get; set; }
    }
}
