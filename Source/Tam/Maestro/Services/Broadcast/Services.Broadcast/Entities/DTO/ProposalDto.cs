using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;


namespace Services.Broadcast.Entities.DTO
{
    public class ProposalDto
    {     
        public int? Id { get; set; }
        public string ProposalName { get; set; }
        public int AdvertiserId { get; set; }
        public List<LookupDto> SpotLengths { get; set; } = new List<LookupDto>();
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public int GuaranteedDemoId { get; set; }
        public ProposalEnums.ProposalMarketGroups? MarketGroupId { get; set; }
        public ProposalEnums.ProposalMarketGroups? BlackoutMarketGroupId { get; set; }
        public MarketGroupDto MarketGroup { get; set; }
        public MarketGroupDto BlackoutMarketGroup { get; set; }
        public double? MarketCoverage { get; set; }
        public List<ProposalMarketDto> Markets { get; set; } = new List<ProposalMarketDto>();
        public decimal? TargetBudget { get; set; }
        public int? TargetUnits { get; set; }
        public double TargetImpressions { get; set; } = 0;
        public decimal? TargetCPM { get; set; }
        public double? Margin { get; set; }
        public string Notes { get; set; }
        public short? Version { get; set; }
        public int? PrimaryVersionId { get; set; }
        public int VersionId { get; set; }
        public ProposalEnums.ProposalStatusType Status { get; set; }
        public PostingTypeEnum PostType { get; set; }
        public bool Equivalized { get; set; }
        public List<ProposalFlightWeek> FlightWeeks { get; set; } = new List<ProposalFlightWeek>();
        public List<int> SecondaryDemos { get; set; } = new List<int>();
        public List<ProposalDetailDto> Details { get; set; } = new List<ProposalDetailDto>();
        public bool ForceSave { get; set; } = false;
        public ValidationWarningDto ValidationWarning { get; set; }
        public decimal TotalCost { get; set; } = 0;
        public double TotalCostPercent { get; set; } = 0;
        public bool TotalCostMarginAchieved { get; set; } = false;
        public double TotalImpressions { get; set; } = 0;
        public double TotalImpressionsPercent { get; set; } = 0;
        public bool TotalImpressionsMarginAchieved { get; set; } = false;
        public decimal TotalCPM { get; set; } = 0;
        public double TotalCPMPercent { get; set; } = 0;
        public bool TotalCPMMarginAchieved { get; set; } = false;
        public bool CanDelete { get; set; } = false;
    }
}
