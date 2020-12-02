using System.Collections.Generic;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxMarketDto
    {
        public int MarketId { get; set; }
        public string DmaMarketName { get; set; }
        public int TotalSpots { get; set; }
        public decimal TotalCost { get; set; }
        public List<ScxStation> Stations { get; set; }

        public class ScxStation
        {
            public int StationCode { get; set; }
            public string LegacyCallLetters { get; set; }
            public int TotalSpots { get; set; }
            public decimal TotalCost { get; set; }
            public List<ScxProgram> Programs { get; set; }
         
            public class ScxProgram
            {
                public string ProgramName { get; set; }
                public int DaypartId { get; set; }
                public string ProgramAssignedDaypartCode { get; set; }
                public string SpotLength { get; set; }
                public decimal SpotCost { get; set; }
                public int TotalSpots { get; set; }
                public decimal TotalCost { get; set; }
                public List<ScxWeek> Weeks { get; set; }
                public List<DemoValue> DemoValues { get; set; }

                public class ScxWeek
                {
                    public int Spots { get; set; }
                    public MediaWeek MediaWeek { get; set; }
                }

                public class DemoValue
                {
                    public int DemoRank { get; set; }
                    public double Impressions { get; set; }
                }
            }
        }
    }
}
