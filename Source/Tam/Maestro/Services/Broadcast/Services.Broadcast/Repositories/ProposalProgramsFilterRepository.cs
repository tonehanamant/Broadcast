﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IProposalProgramsCriteriaRepository : IDataRepository
    {
        OpenMarketCriterion UpdateCriteria(int proposalDetailId, List<CpmCriteria> newCpmCriterion, List<int> deleteCpmCriterion, List<GenreCriteria> newGenreCriteria, List<int> deleteGenreCriteria, List<ProgramCriteria> newProgramNameCriteria, List<int> oldProgramNameCriteria);
    }

    public class ProposalProgramsCriteriaRepository : BroadcastRepositoryBase, IProposalProgramsCriteriaRepository
    {
        public ProposalProgramsCriteriaRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public OpenMarketCriterion UpdateCriteria(int proposalDetailId, List<CpmCriteria> newCpmCriterion, List<int> deleteCpmCriterion, List<GenreCriteria> newGenreCriteria, List<int> deleteGenreCriteria, List<ProgramCriteria> newProgramNameCriteria, List<int> oldProgramNameCriteria)
        {
            return _InReadUncommitedTransaction(c =>
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

                return new OpenMarketCriterion
                {
                    CpmCriteria = c.proposal_version_detail_criteria_cpm
                        .Where(criteria => criteria.proposal_version_detail_id == proposalDetailId).Select(Convert)
                        .ToList(),
                    GenreSearchCriteria = c.proposal_version_detail_criteria_genres
                        .Where(criteria => criteria.proposal_version_detail_id == proposalDetailId).Select(Convert)
                        .ToList(),
                    ProgramNameSearchCriteria = c.proposal_version_detail_criteria_programs.Where(criteria => criteria.proposal_version_detail_id == proposalDetailId).Select(Convert)
                        .ToList()
                };
            });
        }

        private static ProgramCriteria Convert(proposal_version_detail_criteria_programs c)
        {
            return new ProgramCriteria { Id = c.id, Contain = (ContainTypeEnum)c.contain_type, Program = new LookupDto {Id = c.program_name_id, Display = c.program_name } };
        }

        private static GenreCriteria Convert(proposal_version_detail_criteria_genres c)
        {
            return new GenreCriteria() { Id = c.id, Contain = (ContainTypeEnum)c.contain_type,Genre = new LookupDto(c.genre_id,"") };
        }

        private static CpmCriteria Convert(proposal_version_detail_criteria_cpm c)
        {
            return new CpmCriteria { Id = c.id, MinMax = (MinMaxEnum)c.min_max, Value = c.value };
        }

        private static proposal_version_detail_criteria_programs Convert(ProgramCriteria filter, int proposalDetailId)
        {
            return new proposal_version_detail_criteria_programs { contain_type = (byte)filter.Contain, proposal_version_detail_id = proposalDetailId, program_name = filter.Program.Display, program_name_id = filter.Program.Id };
        }

        private static proposal_version_detail_criteria_genres Convert(GenreCriteria filter, int proposalDetailId)
        {
            return new proposal_version_detail_criteria_genres { contain_type = (byte)filter.Contain, proposal_version_detail_id = proposalDetailId, genre_id = filter.Genre.Id };
        }

        private static proposal_version_detail_criteria_cpm Convert(CpmCriteria filter, int proposalDetailId)
        {
            return new proposal_version_detail_criteria_cpm { min_max = (byte)filter.MinMax, proposal_version_detail_id = proposalDetailId, value = filter.Value };
        }
    }
}