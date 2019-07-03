using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IShowTypeRepository : IDataRepository
    {
        /// <summary>
        /// Finds a show type based on the input string
        /// </summary>
        /// <param name="showTypeSearchString">Parameter to filter by</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> FindShowType(string showTypeSearchString);
    }

    public class ShowTypeRepository : BroadcastRepositoryBase, IShowTypeRepository
    {

        public ShowTypeRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        /// <summary>
        /// Finds a show type based on the input string
        /// </summary>
        /// <param name="showTypeSearchString">Parameter to filter by</param>
        /// <returns>List of LookupDto objects</returns>
        public List<LookupDto> FindShowType(string showTypeSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.show_types.Where(g => g.name.ToLower().Contains(showTypeSearchString.ToLower())).Select(
                        g => new LookupDto()
                        {
                            Display = g.name,
                            Id = g.id
                        }).ToList();
                });
        }
    }    
}
