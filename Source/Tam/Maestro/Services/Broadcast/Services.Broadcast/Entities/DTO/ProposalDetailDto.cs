﻿using Newtonsoft.Json;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class ProposalDetailDto : IHaveSingleSharedPostingBooks
    {
        public int? Id { get; set; }
        public List<ProposalFlightWeek> ProposalFlightWeeks { get; set; } = new List<ProposalFlightWeek>();
        public List<GenreCriteria> GenreCriteria { get; set; } = new List<GenreCriteria>();
        public List<ProgramCriteria> ProgramCriteria { get; set; } = new List<ProgramCriteria>();
        public List<ShowTypeCriteria> ShowTypeCriteria { get; set; } = new List<ShowTypeCriteria>();
        public List<CpmCriteria> CpmCriteria { get; set; } = new List<CpmCriteria>();
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<ProposalFlightWeek> FlightWeeks { get; set; } = new List<ProposalFlightWeek>();
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
        public int? SingleProjectionBookId { get; set; }
        public int? ShareProjectionBookId { get; set; }
        public int? HutProjectionBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType ProjectionPlaybackType { get; set; }
        public DefaultProjectionBooksDto DefaultProjectionBooks { get; set; }        
        public int? Sequence { get; set; }
        public int? PostingBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType? PostingPlaybackType { get; set; }
        public bool HasPostingDataChanged { get; set; }
        public double? NtiConversionFactor { get; set; }
        public int? EstimateId { get; set; }
    }
}
