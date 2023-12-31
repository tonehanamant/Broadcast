﻿using System.Collections.Generic;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalOpenMarketInventoryWeekDto : BaseProposalInventoryWeekDto
    {
        public List<InventoryWeekMarket> Markets { get; set; } = new List<InventoryWeekMarket>();
        public int ProposalVersionDetailQuarterWeekId { get; set; }

        public class InventoryWeekMarket
        {
            public int MarketId { get; set; }
            public int Spots { get; set; }
            public double Impressions { get; set; }
            public decimal Cost { get; set; }
            public List<InventoryWeekStation> Stations { get; set; } = new List<InventoryWeekStation>();
            public double DisplayImpressions { get; set; }
            public double DisplayStationImpressions { get; set; }
        }

        public class InventoryWeekStation
        {
            public int? StationCode { get; set; }
            public List<InventoryWeekProgram> Programs { get; set; } = new List<InventoryWeekProgram>();
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
            public double DisplayStationImpressions
            {
                get
                {
                    if (!ProvidedUnitImpressions.HasValue)
                    {
                        return 0;
                    }

                    if(Spots == 0)
                    {
                        return ProvidedUnitImpressions.Value;
                    }
                    else
                    {
                        return ProvidedUnitImpressions.Value * Spots;
                    }
                }
            }

            public double DisplayImpressions
            {
                get
                {
                    if (Spots == 0)
                    {
                        return UnitImpression;
                    }
                    else
                    {
                        return UnitImpression * Spots;
                    }
                }
            }
        }
    }
}
