using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class StationProgram
    {
        public StationProgram()
        {
            FlightWeeks = new List<FlightWeekDto>();
            Genres = new List<LookupDto>();
        }

        public int Id { get; set; }
        public int StationCode { get; set; }
        public int SpotLength { get; set; }
        public int? SpotsPerWeek { get; set; }
        public List<string> ProgramNames { get; set; }
        public string RateSource { get; set; }
        public List<DaypartDto> Airtimes { get; set; }
        public List<string> AirtimePreviews { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Rate15 { get; set; }
        public decimal? Rate30 { get; set; }
        public double? HouseHoldImpressions { get; set; }
        public double? Rating { get; set; }
        public List<FlightWeekDto> FlightWeeks { get; set; }
        public List<StationProgramConflictChangeDto> Conflicts { get; set; }
        public List<LookupDto> Genres { get; set; }

        public class StationProgramConflictChangeDto
        {
            public int Id { get; set; }
            public DateTime FlightStartDate { get; set; }
            public DateTime FlightEndDate { get; set; }
            public List<FlightWeekDto> Flights { get; set; }
        }
    }
}
