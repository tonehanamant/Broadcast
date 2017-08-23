using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalDetailDto : IHavePostingBooks
    {
        public int? Id { get; set; }
        private List<ProposalFlightWeek> _ProposalFlightWeeks;

        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<ProposalFlightWeek> FlightWeeks
        {
            get { return _ProposalFlightWeeks ?? (_ProposalFlightWeeks = new List<ProposalFlightWeek>()); }
            set { _ProposalFlightWeeks = value; }
        }
        public int SpotLengthId { get; set; }
        public DaypartDto Daypart { get; set; }
        [JsonIgnore]
        public int DaypartId { get; set; }
        public string DaypartCode { get; set; }
        public int TotalUnits { get; set; }
        public double TotalImpressions { get; set; }
        public decimal TotalCost { get; set; }
        public bool Adu { get; set; }
        public List<ProposalQuarterDto> Quarters { get; set; }
        public bool FlightEdited { get; set; }
        public int? SinglePostingBookId { get; set; }
        public int? SharePostingBookId { get; set; }
        public int? HutPostingBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType? PlaybackType { get; set; }
        public DefaultPostingBooksDto DefaultPostingBooks { get; set; }
    }
}
