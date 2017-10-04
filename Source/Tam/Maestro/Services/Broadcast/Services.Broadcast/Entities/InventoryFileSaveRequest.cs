using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Entities
{
    [Serializable]
    public class InventoryFileSaveRequest
    {
        private List<FlightWeekDto> _RequestFlightWeeks;

        private Stream _ratesStream;
        public string UserName { get; set; }
        public string FileName { get; set; }
        public string RawData { get; set; }
        public string RateSource { get; set; }

        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<FlightWeekDto> FlightWeeks
        {
            get { return _RequestFlightWeeks ?? (_RequestFlightWeeks = new List<FlightWeekDto>()); }
            set { _RequestFlightWeeks = value; }
        }
        public DaypartDto Daypart { get; set; }
        public string BlockName { get; set; }
        public int? RatingBook { get; set; }
        public ProposalEnums.ProposalPlaybackType PlaybackType { get; set; }

        public Stream RatesStream
        {
            get
            {
                if (_ratesStream != null)
                {
                    return _ratesStream;
                }
                if (RawData == null) return null;
                byte[] rawBytes = Convert.FromBase64String(RawData);
                MemoryStream ms = new MemoryStream(rawBytes, 0, rawBytes.Length);
                return ms;
            }
            set { _ratesStream = value; }
        }

    }
}
