using System;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalProgramsCalculationEngine
    {
        List<ProposalProgramDto> CalculateIndividualProposalSchedule(List<ProposalProgramDto> programsList,
            NsiUniverseData nsiData, ProposalDto proposal);

        ProposalTotalFields CalculateProposalTotalFieldsFromListOfTotals(List<ProposalTotalFields> listOfTotals,
            NsiUniverseData nsiData);

        Dictionary<short, Dictionary<short, ProposalTotalFields>>
            GetStationTotalsGroupedByMarketFromProposalProgramList(List<ProposalProgramDto> programList,
                NsiUniverseData nsiData);

        Dictionary<short, ProposalTotalFields> GetMarketTotalsFromProposalProgramList(
            Dictionary<short, Dictionary<short, ProposalTotalFields>> groupedMarket,
            NsiUniverseData nsiData);

        Dictionary<short, ProposalTotalFields> GetStationTotalsFromProposalProgramList(
            Dictionary<short, Dictionary<short, ProposalTotalFields>> groupedStations, NsiUniverseData nsiData);

        ProposalTotalFields GetOverallTotalsFromProposalProgramList(
            Dictionary<short, Dictionary<short, ProposalTotalFields>> marketStationTotal, NsiUniverseData nsiData);

        ProposalTotalFields GetTargetUnitTotalsFromProposalProgramList(
            Dictionary<short, Dictionary<short, ProposalTotalFields>> marketStationTotal, NsiUniverseData nsiData,
            int? targetUnit);

        /// <summary>
        /// Calculates CPM for a list of programs
        /// </summary>
        /// <param name="programs"></param>
        /// <param name="spotLength"></param>
        /// <returns></returns>
        void CalculateCpmForPrograms(List<ProposalProgramDto> programs, int spotLength);

        /// <summary>
        /// Calculates blended CPM for the programs across all program weeks assuming 1 spot per week.
        /// </summary>
        /// <param name="programs"></param>
        /// <param name="spotLength"></param>
        /// <returns></returns>
        void CalculateBlendedCpmForPrograms(List<ProposalProgramDto> programs, int spotLength);

        void CalculateAvgCostForPrograms(List<ProposalProgramDto> programs);

        void CalculateTotalCostForPrograms(List<ProposalProgramDto> programs);
        void CalculateTotalImpressionsForPrograms(List<ProposalProgramDto> programs);
    }

    public class ProposalProgramsCalculationEngine : IProposalProgramsCalculationEngine
    {
        private readonly IBroadcastAudiencesCache _AudienceCache;

        public ProposalProgramsCalculationEngine(IBroadcastAudiencesCache audienceCache)
        {
            _AudienceCache = audienceCache;
        }

        public List<ProposalProgramDto> CalculateIndividualProposalSchedule(List<ProposalProgramDto> programsList,
            NsiUniverseData nsiData, ProposalDto proposal)
        {
            /*
                Total Cost = Rate of program * spot(s) allocated per program 
                Target Impressions per Spot. = RATING of the program by the demographic chosen * market coverage universe (provided by Nielsen)
                Target CPM = ( Cost of the program / (target impressions / 1000) )
                TRP = (Total Demo Impressions / Total US Demo Universe) * 100
             * 
             HH Impressions will ONLY appear, if the user has NOT selected "Household" as their target demographic.
                        Household Rating * market coverage universe (Nielsen provided) = Household Impressions
             GRP will ONLY appear, if the user has NOT selected "Household" as their target demographic.
                        GRP = (Total HH Impressions /  Total US HH Universe) * 100. 
             Impressions will be calculated by:
                RATING of the program by the demographic chosen * market coverage universe (provided by Nielsen) = Target Impressions per Spot.

             */
            var houseHoldAudienceId = _AudienceCache.GetDisplayAudienceByCode(BroadcastConstants.HOUSEHOLD_CODE).Id;
            foreach (var programDetails in programsList)
            {
                int spotsForCalculation;
                if (programDetails.TotalSpots > 0)
                {
                    spotsForCalculation = programDetails.TotalSpots;
                }
                else
                {
                    //we still want to display the cost and impressions per spot when there are no spots allocated
                    spotsForCalculation = 1;
                }

                programDetails.TotalCost = spotsForCalculation * programDetails.SpotCost;

                // demo specific calculation
                var targetImpressions = programDetails.DemoRating * programDetails.MarketSubscribers *
                                        spotsForCalculation;

                programDetails.TargetImpressions = targetImpressions;
                programDetails.TargetCpm = ProposalMath.CalculateCpm(programDetails.TotalCost, targetImpressions);
                programDetails.TRP = ProposalMath.CalculateTRP_GRP(targetImpressions, nsiData.TotalDemoUniverse);

                //household specific calculation (only if user didn't select HH as target)
                if (proposal.GuaranteedDemoId != houseHoldAudienceId)
                {
                    var hhImpressions = programDetails.HouseHoldRating * programDetails.HouseHoldMarketSubscribers * spotsForCalculation;

                    programDetails.HHImpressions = hhImpressions;
                    programDetails.HHeCPM = ProposalMath.CalculateCpm(programDetails.TotalCost, hhImpressions);

                    programDetails.GRP = ProposalMath.CalculateTRP_GRP(hhImpressions, nsiData.TotalHHUniverse);
                }
            }

            return programsList;
        }

        public ProposalTotalFields CalculateProposalTotalFieldsFromListOfTotals(List<ProposalTotalFields> listOfTotals, NsiUniverseData nsiData)
        {
            var targetImpressions = listOfTotals.Sum(q => q.TotalSpots > 0 ? q.TotalTargetImpressions : 0);
            var hhImpressions = listOfTotals.Sum(q => q.TotalSpots > 0 ? q.TotalHHImpressions : 0);
            var stationSubtotalAdditionalAudienceImpressions = listOfTotals.Sum(q => q.TotalSpots > 0 ? q.TotalAdditionalAudienceImpressions : 0);
            var totalCost = listOfTotals.Sum(q => q.TotalSpots > 0 ? q.TotalCost : 0);
            var p = new ProposalTotalFields()
            {
                TotalSpots = listOfTotals.Sum(q => q.TotalSpots),
                TotalCost = totalCost,
                TotalTargetImpressions = targetImpressions,

                TotalTargetCPM = ProposalMath.CalculateCpm(totalCost, targetImpressions),
                TotalTRP = ProposalMath.CalculateTRP_GRP(targetImpressions, nsiData.TotalDemoUniverse),
                TotalGRP = ProposalMath.CalculateTRP_GRP(hhImpressions, nsiData.TotalHHUniverse),
                TotalHHCPM = ProposalMath.CalculateCpm(totalCost, hhImpressions),
                TotalHHImpressions = hhImpressions,
                //additional audience
                TotalAdditionalAudienceImpressions = stationSubtotalAdditionalAudienceImpressions,
                TotalAdditionalAudienceCPM = ProposalMath.CalculateCpm(totalCost, stationSubtotalAdditionalAudienceImpressions)
            };

            return p;
        }

        public Dictionary<short, Dictionary<short, ProposalTotalFields>>
            GetStationTotalsGroupedByMarketFromProposalProgramList(List<ProposalProgramDto> programList,
                NsiUniverseData nsiData)
        {
            /*
            Total Spots: The sum of all station spots allocated to programs
            Total Cost: The sum of all program costs, which have spots allocated against them
            Target Impressions: Sum of all the impressions of programs within the station
            Target CPM: Total Cost of programs/target impressions * 1000
            TRP: Total demographic impressions/Total US Demographic Universe * 100
            
            Additional Demographic Impression: Sum of all the impressions of programs within the station for the additional demographic
            Additional Demographic CPM: Total Cost of programs/target impressions * 1000
            Additional Demographic GRP: Total HH Impressions/ Total us HH Universe * 100
            */

            return programList.GroupBy(m => (short)m.Market.Id)
                .ToDictionary(mark => mark.Key, sa => sa.GroupBy(s => s.Station.StationCode)
                    .ToDictionary(station => station.Key, programs =>
                    {
                        var listOfValues = programs.Select(q => new ProposalTotalFields()
                        {
                            TotalAdditionalAudienceCPM = q.AdditonalAudienceCPM,
                            TotalAdditionalAudienceImpressions = q.AdditionalAudienceImpressions,
                            TotalCost = q.TotalCost,
                            TotalGRP = q.GRP,
                            TotalHHCPM = q.HHeCPM,
                            TotalHHImpressions = q.HHImpressions,
                            TotalSpots = q.TotalSpots,
                            TotalTRP = q.TRP,
                            TotalTargetCPM = q.TargetCpm,
                            TotalTargetImpressions = q.TargetImpressions
                        }).ToList();
                        var total = CalculateProposalTotalFieldsFromListOfTotals(listOfValues, nsiData);
                        return total;
                    }));
        }

        // return a list of market totals
        public Dictionary<short, ProposalTotalFields> GetMarketTotalsFromProposalProgramList(
            Dictionary<short, Dictionary<short, ProposalTotalFields>> groupedMarket,
            NsiUniverseData nsiData)
        {
            /* todo
            Total Spots: The sum of all the program spots allocated inside the market
            Total Cost: The sum of all program costs inside the market, which have spots allocated against them
            Target Impressions: Sum of all the impressions of programs within the market
            Target CPM: Total Cost of programs/target impressions * 1000
            TRP: Total demographic impressions/Total US Demographic Universe * 100
            Additional Demographic Impression: Sum of all the impressions of programs within the market for the additional demographic
            Additional Demographic CPM: Total Cost of programs/target impressions * 1000
            Additional Demographic GRP: Total HH Impressions/ Total us HH Universe * 100
             */

            return groupedMarket.ToDictionary(a => a.Key, grp =>
            {
                var listOfTotals = grp.Value
                    .Select(a => a.Value).ToList();
                var total = CalculateProposalTotalFieldsFromListOfTotals(listOfTotals, nsiData);
                return total;
            });
        }

        // return staition totals taken from dictionary of market/station totals
        public Dictionary<short, ProposalTotalFields> GetStationTotalsFromProposalProgramList(
            Dictionary<short, Dictionary<short, ProposalTotalFields>> groupedStations, NsiUniverseData nsiData)
        {
            return (from m in groupedStations
                    from s in m.Value
                    select new { s.Key, s.Value }).ToDictionary(z => z.Key, v => v.Value);
        }

        // return overall totals
        public ProposalTotalFields GetOverallTotalsFromProposalProgramList(
            Dictionary<short, Dictionary<short, ProposalTotalFields>> marketStationTotal, NsiUniverseData nsiData)
        {
            /*
             * Total Spots: The sum of all the program spots allocated inside the proposal
            Total Cost: The sum of all program costs inside the proposal, which have spots allocated against them
            Target Impressions: the sum of all impressions in the proposal
            Target CPM: Total Cost of programs/target impressions * 1000
            TRP: Total demographic impressions/Total US Demographic Universe * 100
            Additional Demographic Impression: Sum of all the impressions of programs within the proposal for the additional demographic
            Additional Demographic CPM: Total Cost of programs/target impressions * 1000
            Additional Demographic GRP: Total HH Impressions/ Total us HH Universe * 100
            */

            var marketTotal = GetMarketTotalsFromProposalProgramList(marketStationTotal, nsiData);
            // target impressions of all levels

            var listOfTotals = marketTotal.Values.Select(a => a).ToList();
            var total = CalculateProposalTotalFieldsFromListOfTotals(listOfTotals, nsiData);
            return total;
        }

        // return target unit totals
        public ProposalTotalFields GetTargetUnitTotalsFromProposalProgramList(
            Dictionary<short, Dictionary<short, ProposalTotalFields>> marketStationTotal, NsiUniverseData nsiData,
            int? targetUnit)
        {
            /*
            Total Spots: No total spots will be displayed for total per target unit
            Total Cost: The sum of all program costs inside the market, which have spots allocated against them divided by the TARGET UNITS defined in the proposal header
            Target Impressions: Sum of all the impressions of programs within the proposal
            Target CPM: Total Cost of programs/target impressions * 1000
            TRP: Total demographic impressions/Total US Demographic Universe * 100
            Additional Demographic Impression: Sum of all the impressions of programs within the proposal for the additional demographic
            Additional Demographic CPM: Total Cost of programs/target impressions * 1000
            Additional Demographic GRP: Total HH Impressions/ Total us HH Universe * 100
            */

            var marketTotal = GetMarketTotalsFromProposalProgramList(marketStationTotal, nsiData);

            var listOfTotals = marketTotal.Values.Select(a => a).ToList();
            var total = CalculateProposalTotalFieldsFromListOfTotals(listOfTotals, nsiData);
            total.TotalSpots = 0;
            total.TotalCost = targetUnit.HasValue && targetUnit.Value > 0
                ? (marketTotal.Values.Sum(q => q.TotalCost) / targetUnit.Value)
                : 0;

            return total;
        }

        public void CalculateCpmForPrograms(List<ProposalProgramDto> programs, int spotLength)
        {
            foreach (var program in programs)
            {
                var impressions = program.ProvidedUnitImpressions ?? program.UnitImpressions;

                program.TargetCpm = ProposalMath.CalculateCpm(program.SpotCost, impressions);
            }
        }

        public void CalculateBlendedCpmForPrograms(List<ProposalProgramDto> programs, int spotLength)
        {
            foreach (var program in programs)
            {
                var activeWeeks = program.FlightWeeks.Where(w => !w.IsHiatus).ToList();
                var totalCost = activeWeeks.Sum(w => w.Rate);
                var unitImpressions = program.ProvidedUnitImpressions ?? program.UnitImpressions;
                var totalImpressions = unitImpressions * activeWeeks.Count;

                program.TargetCpm = ProposalMath.CalculateCpm(totalCost, totalImpressions);
            }
        }

        public void CalculateAvgCostForPrograms(List<ProposalProgramDto> programs)
        {
            foreach (var program in programs)
            {
                var activeWeeks = program.FlightWeeks.Where(w => !w.IsHiatus).ToList();
                var totalCost = activeWeeks.Sum(w => w.Rate);

                if (activeWeeks.Count == 0)
                    program.SpotCost = 0;
                else
                    program.SpotCost = totalCost / activeWeeks.Count;
            }
        }

        public void CalculateTotalCostForPrograms(List<ProposalProgramDto> programs)
        {
            foreach (var program in programs)
            {
                if (program.TotalSpots == 0)
                    program.TotalCost = program.SpotCost;
                else
                    program.TotalCost = program.SpotCost * program.TotalSpots;
            }
        }

        public void CalculateTotalImpressionsForPrograms(List<ProposalProgramDto> programs)
        {
            foreach (var program in programs)
            {
                if (program.TotalSpots == 0)
                    program.TotalImpressions = program.UnitImpressions;
                else
                    program.TotalImpressions = program.UnitImpressions * program.TotalSpots;
            }
        }
    }
}
