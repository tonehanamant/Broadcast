using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities;
using Common.Services.Extensions;

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
        /// <summary>
        /// Gets the show type by name.
        /// </summary>
        /// <param name="showTypeName">Name of the show type.</param>
        /// <returns>ShowTypeDto</returns>
        ShowTypeDto GetShowTypeByName(string showTypeName);
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

        /// <inheritdoc />
        public ShowTypeDto GetShowTypeByName(string showTypeName)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return _MapToDto(context.show_types
                    .Single(item => item.name.ToUpper() == showTypeName.ToUpper(), $"No show type was found by name : {showTypeName}"));
            });
        }

        private ShowTypeDto _MapToDto(show_types showType)
        {
            return new ShowTypeDto
            {
                Id = showType.id,
                Name = showType.name,
            };
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
