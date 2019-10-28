using Common.Services.Repositories;
using ConfigurationService.Client;
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
    public interface IGenreRepository : IDataRepository
    {
        List<LookupDto> GetAllGenres();

        List<LookupDto> FindGenres(string genreSearchString);

        List<LookupDto> GetGenresBySourceId(int sourceId);
    }

    public class GenreRepository : BroadcastRepositoryBase, IGenreRepository
    {

        public GenreRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<LookupDto> GetAllGenres()
        {
            return _InReadUncommitedTransaction(
                context => context.genres.Where(g => g.source_id == (int)GenreSourceEnum.Maestro).OrderBy(g => g.name).Select(_MapToDto).ToList());
        }
        
        public List<LookupDto> FindGenres(string genreSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                     context.genres.Where(g => g.source_id == (int)GenreSourceEnum.Maestro && g.name.ToLower().Contains(genreSearchString.ToLower())).Select(_MapToDto).ToList()
                );
        }

        public List<LookupDto> GetGenresBySourceId(int sourceId)
        {
            return _InReadUncommitedTransaction(
                context =>
                    context.genres.Where(g => g.source_id == sourceId).Select(_MapToDto).ToList()
                );
        }

        private LookupDto _MapToDto(genre genre)
        {
            return new LookupDto
            {
                Display = genre.name,
                Id = genre.id
            };
        }
    }
}
