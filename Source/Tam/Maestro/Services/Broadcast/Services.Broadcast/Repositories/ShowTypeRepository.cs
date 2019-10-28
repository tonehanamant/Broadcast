using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;

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
        List<LookupDto> GetShowTypes();
    }

    public class ShowTypeRepository : BroadcastRepositoryBase, IShowTypeRepository
    {

        public ShowTypeRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

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
                    return context.show_types
                        .Where(g => g.name.ToLower().Contains(showTypeSearchString.ToLower()))
                        .ToList()
                        .Select(_MapToLookupDto)
                        .ToList();
                });
        }

        public List<LookupDto> GetShowTypes()
        {
            return _InReadUncommitedTransaction(context => 
            {
                return context.show_types
                    .ToList()
                    .Select(_MapToLookupDto)
                    .OrderBy(x => x.Display)
                    .ToList();
            });
        }

        private LookupDto _MapToLookupDto(show_types show_Type)
        {
            return new LookupDto()
            {
                Id = show_Type.id,
                Display = show_Type.name
            };
        }
    }    
}
