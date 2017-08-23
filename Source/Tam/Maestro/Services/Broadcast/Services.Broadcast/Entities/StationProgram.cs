using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class StationProgram
    {
        public int Id { get; set; }
        public string ProgramName { get; set; }

        private List<LookupDto> _genres;

        public List<LookupDto> Genres
        {
            get { return _genres ?? (_genres = new List<LookupDto>()); } 
            set { _genres = value; }  
            
        }
        public string StationLegacyCallLetters { get; set; }
        public short StationCode { get; set; }
        public DisplayDaypart Daypart { get; set; }
        public string DayPartName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SpotLength { get; set; }
        public string DaypartCode { get; set; }
        public RatesFile.RateSourceType? RateSource { get; set; }
        public decimal? FixedPrice { get; set; }

        private List<StationProgramFlightWeek> _flightWeeks;

        public List<StationProgramFlightWeek> FlightWeeks
        {
            get { return _flightWeeks ?? (_flightWeeks = new List<StationProgramFlightWeek>()); }
            set { _flightWeeks = value; }
        }

    }
}
