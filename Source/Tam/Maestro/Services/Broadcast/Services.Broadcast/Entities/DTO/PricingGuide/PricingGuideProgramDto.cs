using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideProgramDto
    {
        public int ProgramId { get; set; }
        public int ManifestDaypartId { get; set; }
        public LookupDto Daypart { get; set; } = new LookupDto();
        public string ProgramName { get; set; }
        public decimal BlendedCpm { get; set; }
        public int Spots { get; set; }
        public double ImpressionsPerSpot { get; set; }
        public double Impressions { get; set; }
        public double StationImpressionsPerSpot { get; set; }
        public decimal CostPerSpot { get; set; }
        public decimal Cost { get; set; }
        public bool HasImpressions { get { return EffectiveImpressionsPerSpot > 0; } }
        public List<LookupDto> Genres { get; set; } = new List<LookupDto>();

        public double EffectiveImpressionsPerSpot
        {
            get
            {
                if (StationImpressionsPerSpot != 0)
                {
                    return StationImpressionsPerSpot;
                }
                else
                {
                    return ImpressionsPerSpot;
                }
            }
        }

        public double DisplayImpressions
        {
            get
            {
                if (Spots == 0)
                {
                    return ImpressionsPerSpot;
                }
                else
                {
                    return ImpressionsPerSpot * Spots;
                }
            }
        }

        public double DisplayStationImpressions
        {
            get
            {
                if (Spots == 0)
                {
                    return StationImpressionsPerSpot;
                }
                else
                {
                    return StationImpressionsPerSpot * Spots;
                }
            }
        }

        public decimal DisplayCost
        {
            get
            {
                if (Spots == 0)
                {
                    return CostPerSpot;
                }
                else
                {
                    return Cost;
                }

            }
        }
    }
}
