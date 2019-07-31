using System.Collections.Generic;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxMarketDto
    {
        public string DmaMarketName { get; set; }
        public int? MarketId { get; set; }
        public List<ScxStation> Stations { get; set; } = new List<ScxStation>();

        public class ScxStation
        {
            public int? StationCode { get; set; }
            public string LegacyCallLetters { get; set; }
            public List<ScxProgram> Programs { get; set; } = new List<ScxProgram>();
         
            public class ScxProgram
            {
                public int ProgramId { get; set; }
                public List<string> ProgramNames { get; set; } = new List<string>();
                public List<LookupDto> Dayparts { get; set; } = new List<LookupDto>();
                public List<ScxWeek> Weeks { get; set; } = new List<ScxWeek>();

                public class ScxWeek
                {
                    public int Spots { get; set; }
                    public MediaWeek MediaWeek { get; set; }
                }
            }
        }
    }
}
