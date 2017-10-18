using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class NewStationProgramDto
    {
        public int Id { get; set; }
        public int StationCode { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public InventoryFile.InventorySource InventorySource { get; set; }
        public DaypartDto Airtime { get; set; }
        public string Program { get; set; }
        public List<LookupDto> Genres { get; set; }
        public decimal? Rate15 { get; set; }
        public decimal? Rate30 { get; set; }
        public double Impressions { get; set; }
        public double Rating { get; set; }
        public List<FlightWeekDto> Flights { get; set; }
        public string Flight { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<StationProgramConflictChangeDto> Conflicts { get; set; }
    }

    public class StationProgramConflictChangeDto
    {
        public int Id { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<FlightWeekDto> Flights { get; set; }
    }
}
