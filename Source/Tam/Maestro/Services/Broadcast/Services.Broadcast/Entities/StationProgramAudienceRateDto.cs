using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class StationProgramAudienceRateDto
    {
        public int Id { get; set; }
        public string Airtime { get; set; }
        public string Program { get; set; }
        public int AudienceId { get; set; }
        public List<LookupDto> Genres { get; set; }
        public decimal? Rate15 { get; set; }
        public decimal? Rate30 { get; set; }
        public double? Impressions { get; set; }
        public double? Rating { get; set; }
        public List<FlightWeekDto> Flights { get; set; }
        public string Flight { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public int? Spots { get; set; }

        private List<StationProgramFlightWeekAudience> _stationProgramFlightWeekAudiences;

        public List<StationProgramFlightWeekAudience> Audiences
        {
            get
            {
                return _stationProgramFlightWeekAudiences ??
                       (_stationProgramFlightWeekAudiences = new List<StationProgramFlightWeekAudience>());
            }
            set { _stationProgramFlightWeekAudiences = value; }
        }

    }
}
