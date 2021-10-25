using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.ProgramMapping;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IProgramNameMappingKeywordRepository : IDataRepository
    {
        List<ProgramNameMappingKeyword> GetProgramNameMappingKeywords();
    }

    public class ProgramNameMappingKeywordRepository : BroadcastRepositoryBase, IProgramNameMappingKeywordRepository
    {
        public ProgramNameMappingKeywordRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper) :
            base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
        }

        public List<ProgramNameMappingKeyword> GetProgramNameMappingKeywords()
        {
            return _InReadUncommitedTransaction(
                context => context.program_name_mapping_keywords.Include(k => k.genre).Include(k => k.show_types).Select(_MapToDto).ToList());
        }

        private ProgramNameMappingKeyword _MapToDto(program_name_mapping_keywords keyword)
        {
            return new ProgramNameMappingKeyword
            {
                Id = keyword.id,
                ProgramName = keyword.program_name,
                Keyword = keyword.keyword,
                Genre = new Tam.Maestro.Data.Entities.DataTransferObjects.LookupDto
                {
                    Id = keyword.genre_id,
                    Display = keyword.genre.name
                },
                ShowType = new Tam.Maestro.Data.Entities.DataTransferObjects.LookupDto
                {
                    Id = keyword.show_type_id,
                    Display = keyword.show_types.name
                }
            };
        }
    }
}
