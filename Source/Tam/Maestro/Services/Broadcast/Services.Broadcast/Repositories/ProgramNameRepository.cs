using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IProgramNameRepository : IDataRepository
    {
        List<LookupDto> FindPrograms(string programSearchString, int start, int limit);
    }
    public class ProgramNameRepository : BroadcastRepositoryBase, IProgramNameRepository
    {
        public ProgramNameRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<LookupDto> FindPrograms(string programSearchString, int start, int limit)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.program_names.Where(p => p.program_name.ToLower().Contains(programSearchString.ToLower()))
                    .OrderBy(p => p.program_name)
                    .Skip(start - 1).Take(limit)
                    .Select(
                        p => new LookupDto()
                        {
                            Display = p.program_name,
                            Id = p.id
                        }).ToList();
                });
        }
    }
}
