using Common.Services;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalSpotDistributionEngine
    {
        void DistributeProposalSpots(ProposalDto proposal);
    }

    public class ProposalSpotDistributionEngine : IProposalSpotDistributionEngine
    {
        private readonly IDaypartCache _DaypartCache;

        public ProposalSpotDistributionEngine(IDaypartCache daypartCache)
        {
            _DaypartCache = daypartCache;
        }

        public void DistributeProposalSpots(ProposalDto proposal)
        {
            //_ClearExistingSpots(proposal);
            //_AllocateTargetSpotsPerMarket(proposal);
        }

        private List<ProposalProgramDto> _FilterProgramsAlphabetically(List<ProposalProgramDto> programList)
        {
            return
                programList.Where(
                    p => p.ProgramName.Equals(programList.OrderBy(q => q.ProgramName).First().ProgramName)).ToList();
        }

        private List<ProposalProgramDto> _FilterProgramsByMostHoursPerDay(List<ProposalProgramDto> programList)
        {
            return
                programList.Where(
                    p =>
                        _DaypartCache.GetDisplayDaypart(p.DayPartId).Hours ==
                        programList.Max(q => _DaypartCache.GetDisplayDaypart(q.DayPartId).Hours)).ToList();
        }

        private List<ProposalProgramDto> _FilterProgramsByMostDays(List<ProposalProgramDto> programList)
        {
            return
                programList.Where(
                    p =>
                        _DaypartCache.GetDisplayDaypart(p.DayPartId).ActiveDays ==
                        programList.Max(q => _DaypartCache.GetDisplayDaypart(q.DayPartId).ActiveDays)).ToList();
        }

        private List<ProposalProgramDto> _FilterProgramsByLowestCpm(List<ProposalProgramDto> programList)
        {
            return programList.Where(p => p.TargetCpm == programList.Min(q => q.TargetCpm)).ToList();
        }
    }
}
