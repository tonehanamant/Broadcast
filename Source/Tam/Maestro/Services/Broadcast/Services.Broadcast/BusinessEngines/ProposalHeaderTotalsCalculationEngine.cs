using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProposalHeaderTotalsCalculationEngine
    {
        ProposalHeaderTotalsDto CalculateProposalHeaderTotals(List<ProposalDetailTotalsDto> allTotals);
    }

    public class ProposalHeaderTotalsCalculationEngine : IProposalHeaderTotalsCalculationEngine
    {
        public ProposalHeaderTotalsDto CalculateProposalHeaderTotals(List<ProposalDetailTotalsDto> allTotals)
        {
            return new ProposalHeaderTotalsDto
            {
                ImpressionsTotal = allTotals.Sum(x => x.OpenMarketImpressionsTotal + x.ProprietaryImpressionsTotal),
                CostTotal = allTotals.Sum(x => x.OpenMarketCostTotal + x.ProprietaryCostTotal)
            };
        }
    }
}
