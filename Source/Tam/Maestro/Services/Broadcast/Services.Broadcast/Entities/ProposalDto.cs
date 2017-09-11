using System;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Tam.Maestro.Data.Entities.DataTransferObjects;


namespace Services.Broadcast.Entities
{
    public class ProposalDto
    {
        private List<ProposalFlightWeek> _ProposalFlightWeeks;
        private List<int> _SecondaryDemos;
        private List<ProposalDetailDto> _ProposalDetails;
        private List<LookupDto> _SpotLengths;
        private List<ProposalMarketDto> _Markets;

        public ProposalDto()
        {
            _SpotLengths = new List<LookupDto>();
            _ProposalFlightWeeks = new List<ProposalFlightWeek>();
            _SecondaryDemos = new List<int>();
            _ProposalDetails = new List<ProposalDetailDto>();
            _Markets = new List<ProposalMarketDto>();
        }

        public int? Id { get; set; }
        public string ProposalName { get; set; }
        public int AdvertiserId { get; set; }
        
        public List<LookupDto> SpotLengths
        {
            get { return _SpotLengths; }
            set { _SpotLengths = value; }
        }
 
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public int? GuaranteedDemoId { get; set; }
        public ProposalEnums.ProposalMarketGroups? MarketGroupId { get; set; }
        public ProposalEnums.ProposalMarketGroups? BlackoutMarketGroupId { get; set; }
        public MarketGroupDto MarketGroup { get; set; }
        public MarketGroupDto BlackoutMarketGroup { get; set; }

        public List<ProposalMarketDto> Markets
        {
            get { return _Markets; } 
            set { _Markets = value; }
        }
        public decimal? TargetBudget { get; set; }
        public int? TargetUnits { get; set; }
        public double TargetImpressions { get; set; }
        public decimal? TargetCPM { get; set; }
        public double? Margin { get; set; }
        public string Notes { get; set; }
        public short? Version { get; set; }
        public int? PrimaryVersionId { get; set; }
        public int VersionId { get; set; }
        public ProposalEnums.ProposalStatusType Status { get; set; }

        public SchedulePostType? PostType { get; set; }
        public bool? Equivalized { get; set; }

        public List<ProposalFlightWeek> FlightWeeks
        {
            get { return _ProposalFlightWeeks; }
            set { _ProposalFlightWeeks = value; }
        }

        public List<int> SecondaryDemos
        {
            get { return _SecondaryDemos; } 
            set { _SecondaryDemos = value; } 
        }

        public List<ProposalDetailDto> Details
        {
            get { return _ProposalDetails; }
            set { _ProposalDetails = value; }
        }

        public bool ForceSave { get; set; }

        public ValidationWarningDto ValidationWarning { get; set; }
        public decimal TotalCost { get; set; }
        public double TotalCostPercent { get; set; }
        public bool TotalCostMarginAchieved { get; set; }
        public double TotalImpressions { get; set; }
        public double TotalImpressionsPercent { get; set; }
        public bool TotalImpressionsMarginAchieved { get; set; }
        public decimal TotalCPM { get; set; }
        public double TotalCPMPercent { get; set; }
        public bool TotalCPMMarginAchieved { get; set; }
    }
}
