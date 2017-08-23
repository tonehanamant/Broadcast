﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class RatesFile
    {
        public enum FileStatusEnum
        {
            Pending = 1,
            Loaded = 2,
            Failed = 3
        }

        public enum RateSourceType
        {
            Blank = 0,
            OpenMarket = 1,
            Assembly = 2,
            TVB = 3,
            TTNW = 4,
            CNN = 5
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public string UniqueIdentifier { get; set; }
        public FileStatusEnum FileStatus { get; set; }
        public string Hash { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RatingBook { get; set; }
        public ProposalEnums.ProposalPlaybackType PlaybackType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RateSourceType RateSource { get; set; }
        private List<StationContact> _stationContacts; 
        private List<StationProgram> _stationPrograms; 

        public List<StationProgram> StationPrograms
        {
            get
            {
                if (_stationPrograms == null)
                {
                    _stationPrograms = new List<StationProgram>();
                }
                return _stationPrograms;
            }
            set { _stationPrograms = value; }
        }

        public List<StationContact> StationContacts
        {
            get
            {
                if (_stationContacts == null)
                {
                    _stationContacts = new List<StationContact>();
                }
                return _stationContacts;
            }
            set { _stationContacts = value;  }
        }

    }
}
