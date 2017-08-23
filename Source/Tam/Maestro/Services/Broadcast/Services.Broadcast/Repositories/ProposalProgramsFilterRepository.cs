using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IProposalProgramsCriteriaRepository : IDataRepository
    {
        void UpdateCriteria(int proposalDetailId, List<CpmCriteria> newCpmCriterion, List<int> deleteCpmCriterion, List<GenreCriteria> newGenreCriteria, List<int> deleteGenreCriteria, List<ProgramCriteria> newProgramNameCriteria, List<int> oldProgramNameCriteria);
    }

    public class ProposalProgramsCriteriaRepository : BroadcastRepositoryBase, IProposalProgramsCriteriaRepository
    {
        public ProposalProgramsCriteriaRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper) { }

        public void UpdateCriteria(int proposalDetailId, List<CpmCriteria> newCpmCriterion, List<int> deleteCpmCriterion, List<GenreCriteria> newGenreCriteria, List<int> deleteGenreCriteria, List<ProgramCriteria> newProgramNameCriteria, List<int> oldProgramNameCriteria)
        {
            _InReadUncommitedTransaction(c =>
            {
                if (newCpmCriterion.Any())
                    c.proposal_version_detail_criteria_cpm.AddRange(newCpmCriterion.Select(crit => Convert(crit, proposalDetailId)));

                if (deleteCpmCriterion.Any())
                    c.proposal_version_detail_criteria_cpm.RemoveRange(c.proposal_version_detail_criteria_cpm.Where(crit => deleteCpmCriterion.Contains(crit.id)));

                if (newGenreCriteria.Any())
                    c.proposal_version_detail_criteria_genres.AddRange(newGenreCriteria.Select(crit => Convert(crit, proposalDetailId)));

                if (deleteGenreCriteria.Any())
                    c.proposal_version_detail_criteria_genres.RemoveRange(c.proposal_version_detail_criteria_genres.Where(crit => deleteGenreCriteria.Contains(crit.id)));

                if (newProgramNameCriteria.Any())
                    c.proposal_version_detail_criteria_programs.AddRange(newProgramNameCriteria.Select(crit => Convert(crit, proposalDetailId)));

                if (oldProgramNameCriteria.Any())
                    c.proposal_version_detail_criteria_programs.RemoveRange(c.proposal_version_detail_criteria_programs.Where(crit => oldProgramNameCriteria.Contains(crit.id)));

                c.SaveChanges();
            });
        }

        private static proposal_version_detail_criteria_programs Convert(ProgramCriteria filter, int proposalDetailId)
        {
            return new proposal_version_detail_criteria_programs { contain_type = (byte) filter.Contain, proposal_version_detail_id = proposalDetailId, program_name = filter.ProgramName };
        }

        private static proposal_version_detail_criteria_genres Convert(GenreCriteria filter, int proposalDetailId)
        {
            return new proposal_version_detail_criteria_genres { contain_type = (byte) filter.Contain, proposal_version_detail_id = proposalDetailId, genre_id = filter.GenreId };
        }

        private static proposal_version_detail_criteria_cpm Convert(CpmCriteria filter, int proposalDetailId)
        {
            return new proposal_version_detail_criteria_cpm { min_max = (byte) filter.MinMax, proposal_version_detail_id = proposalDetailId, value = filter.Value };
        }
    }
}