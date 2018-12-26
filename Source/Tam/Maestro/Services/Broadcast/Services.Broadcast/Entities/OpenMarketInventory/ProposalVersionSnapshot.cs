using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalVersionSnapshot
    {
        public int Id { get; set; }
        
        public short Proposal_Version { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int GuaranteedAudienceId { get; set; }

        public byte? Markets { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public decimal? TargetBudget { get; set; }

        public int? TargetUnits { get; set; }

        public double TargetImpressions { get; set; }

        public string Notes { get; set; }

        public byte PostType { get; set; }

        public bool Equivalized { get; set; }

        public byte? BlackoutMarkets { get; set; }

        public byte Status { get; set; }

        public decimal TargetCpm { get; set; }

        public double Margin { get; set; }

        public decimal CostTotal { get; set; }

        public double ImpressionsTotal { get; set; }

        public double? MarketCoverage { get; set; }

        public DateTime? SnapshotDate { get; set; }

        public ICollection<ProposalVersionAudience> ProposalVersionAudiences { get; set; }

        public ICollection<ProposalVersionFlightWeek> ProposalVersionFlightWeeks { get; set; }

        public ICollection<ProposalVersionMarket> ProposalVersionMarkets { get; set; }

        public ICollection<ProposalVersionDetail> ProposalVersionDetails { get; set; }

        public ICollection<ProposalVersionSpotLength> ProposalVersionSpotLengths { get; set; }

        public class ProposalVersionAudience
        {
            public int Id { get; set; }

            public int AudienceId { get; set; }

            public byte Rank { get; set; }
        }

        public class ProposalVersionFlightWeek
        {
            public int Id { get; set; }

            public int MediaWeekId { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public bool Active { get; set; }
        }

        public class ProposalVersionMarket
        {
            public int Id { get; set; }

            public short MarketCode { get; set; }

            public bool IsBlackout { get; set; }
        }

        public class ProposalVersionSpotLength
        {
            public int Id { get; set; }

            public int SpotLengthId { get; set; }
        }

        public class ProposalVersionDetail
        {
            public int Id { get; set; }

            public int SpotLengthId { get; set; }

            public int DaypartId { get; set; }

            public string DaypartCode { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public int? UnitsTotal { get; set; }

            public double ImpressionsTotal { get; set; }

            public decimal? CostTotal { get; set; }

            public bool Adu { get; set; }

            public int? SingleProjectionBookId { get; set; }

            public int? HutProjectionBookId { get; set; }

            public int? ShareProjectionBookId { get; set; }

            public byte ProjectionPlaybackType { get; set; }

            public double OpenMarketImpressionsTotal { get; set; }

            public decimal OpenMarketCostTotal { get; set; }

            public double ProprietaryImpressionsTotal { get; set; }

            public decimal ProprietaryCostTotal { get; set; }

            public int? Sequence { get; set; }

            public int? PostingBookId { get; set; }

            public byte? PostingPlaybackType { get; set; }

            public double? NtiConversionFactor { get; set; }

            public ICollection<ProposalVersionDetailCriteriaCpm> ProposalVersionDetailCriteriaCpms { get; set; }

            public ICollection<ProposalVersionDetailCriteriaGenre> ProposalVersionDetailCriteriaGenres { get; set; }

            public ICollection<ProposalVersionDetailCriteriaProgram> ProposalVersionDetailCriteriaPrograms { get; set; }

            public ICollection<ProposalVersionDetailCriteriaShowType> ProposalVersionDetailCriteriaShowTypes { get; set; }

            public ICollection<ProposalVersionDetailQuarter> ProposalVersionDetailQuarters { get; set; }

            public class ProposalVersionDetailCriteriaCpm
            {
                public int Id { get; set; }

                public byte MinMax { get; set; }

                public decimal Value { get; set; }
            }

            public class ProposalVersionDetailCriteriaGenre
            {
                public int Id { get; set; }

                public byte ContainType { get; set; }

                public int GenreId { get; set; }
            }

            public class ProposalVersionDetailCriteriaProgram
            {
                public int Id { get; set; }

                public byte ContainType { get; set; }

                public string ProgramName { get; set; }

                public int ProgramNameId { get; set; }
            }

            public class ProposalVersionDetailCriteriaShowType
            {
                public int Id { get; set; }

                public byte ContainType { get; set; }

                public int ShowTypeId { get; set; }
            }

            public class ProposalVersionDetailQuarter
            {
                public int Id { get; set; }

                public byte Quarter { get; set; }

                public int Year { get; set; }

                public decimal Cpm { get; set; }

                public double ImpressionsGoal { get; set; }

                public ICollection<ProposalVersionDetailQuarterWeek> ProposalVersionDetailQuarterWeeks { get; set; }

                public class ProposalVersionDetailQuarterWeek
                {
                    public int Id { get; set; }

                    public int MediaWeekId { get; set; }

                    public DateTime StartDate { get; set; }

                    public DateTime EndDate { get; set; }

                    public bool IsHiatus { get; set; }

                    public int Units { get; set; }

                    public double ImpressionsGoal { get; set; }

                    public decimal Cost { get; set; }

                    public double OpenMarketImpressionsTotal { get; set; }

                    public decimal OpenMarketCostTotal { get; set; }

                    public double ProprietaryImpressionsTotal { get; set; }

                    public decimal ProprietaryCostTotal { get; set; }

                    public string MyEventsReportName { get; set; }

                    public ICollection<ProposalVersionDetailQuarterWeekIsci> ProposalVersionDetailQuarterWeekIscis { get; set; }

                    public ICollection<StationInventorySpotSnapshot> StationInventorySpotSnapshots { get; set; }

                    public class ProposalVersionDetailQuarterWeekIsci
                    {
                        public int Id { get; set; }

                        public string ClientIsci { get; set; }

                        public string HouseIsci { get; set; }

                        public string Brand { get; set; }

                        public bool MarriedHouseIsci { get; set; }

                        public bool? Monday { get; set; }

                        public bool? Tuesday { get; set; }

                        public bool? Wednesday { get; set; }

                        public bool? Thursday { get; set; }

                        public bool? Friday { get; set; }

                        public bool? Saturday { get; set; }

                        public bool? Sunday { get; set; }
                    }

                    public class StationInventorySpotSnapshot : IHaveSingleSharedPostingBooks
                    {
                        public int Id { get; set; }

                        public int? ProposalVersionDetailQuarterWeekId { get; set; }

                        public int MediaWeekId { get; set; }

                        public int SpotLengthId { get; set; }

                        public string ProgramName { get; set; }

                        public int DaypartId { get; set; }

                        public short StationCode { get; set; }

                        public string StationCallLetters { get; set; }

                        public short StationMarketCode { get; set; }

                        public int StationMarketRank { get; set; }

                        public double? SpotImpressions { get; set; }

                        public decimal? SpotCost { get; set; }

                        public int AudienceId { get; set; }

                        #region Temporary data for market rank calculation

                        [JsonIgnore]
                        public int? SingleProjectionBookId { get; set; }

                        [JsonIgnore]
                        public int? ShareProjectionBookId { get; set; }

                        [JsonIgnore]
                        public int BookId { get; set; }

                        #endregion
                    }
                }
            }
        }
    }
}
