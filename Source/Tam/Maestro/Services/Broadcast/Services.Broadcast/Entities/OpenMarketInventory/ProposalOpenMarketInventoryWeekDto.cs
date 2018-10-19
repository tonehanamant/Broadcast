using System.Collections.Generic;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalOpenMarketInventoryWeekDto : BaseProposalInventoryWeekDto
    {
        public List<InventoryWeekMarket> Markets { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }

        public class InventoryWeekMarket
        {
            public InventoryWeekMarket()
            {
                Stations = new List<InventoryWeekStation>();
            }
            public int MarketId { get; set; }
            public int Spots { get; set; }
            public double Impressions { get; set; }
            public decimal Cost { get; set; }
            public List<InventoryWeekStation> Stations { get; set; }
        }

        public class InventoryWeekStation
        {
            public InventoryWeekStation()
            {
                Programs = new List<InventoryWeekProgram>();
            }
            public int StationCode { get; set; }
            public List<InventoryWeekProgram> Programs { get; set; }
        }

        public class InventoryWeekProgram
        {
            public int ProgramId { get; set; }
            public decimal UnitCost { get; set; }
            public double UnitImpression { get; set; }
            public double? ProvidedUnitImpressions { get; set; }
            public decimal Cost { get; set; }
            public double TargetImpressions { get; set; }
            public double TotalImpressions { get; set; }
            public double EFF { get; set; }
            public int Spots { get; set; }
            public decimal TargetCpm { get; set; }
            public bool HasImpressions { get; set; }
        }
    }
}
