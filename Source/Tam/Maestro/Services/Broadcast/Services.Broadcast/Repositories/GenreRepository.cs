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
    public interface IGenreRepository : IDataRepository
    {
        List<LookupDto> GetAllGenres();

        List<LookupDto> FindGenres(string genreSearchString);
    }

    public class GenreRepository: BroadcastRepositoryBase, IGenreRepository
    {

        public GenreRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<LookupDto> GetAllGenres()
        {
            return _InReadUncommitedTransaction(
                context => (from x in context.genres
                    orderby x.name ascending
                    select new LookupDto() {Id = x.id, Display = x.name}).ToList());
        }


        public List<LookupDto> FindGenres(string genreSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.genres.Where(g => g.name.ToLower().Contains(genreSearchString.ToLower())).Select(
                        g => new LookupDto()
                        {
                            Display = g.name,
                            Id = g.id
                        }).ToList();
                });
        }
    }    
}
