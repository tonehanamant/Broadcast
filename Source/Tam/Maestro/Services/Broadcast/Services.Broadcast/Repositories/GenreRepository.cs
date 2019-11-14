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
        List<LookupDto> GetAllMaestroGenres();

        List<Genre> GetAllGenres();

        List<LookupDto> FindGenres(string genreSearchString);

        List<LookupDto> GetGenresBySourceId(int sourceId);

        List<GenreMapping> GetGenreMappingsBySourceId(int sourceId);
    }

    public class GenreRepository : BroadcastRepositoryBase, IGenreRepository
    {

        public GenreRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<LookupDto> GetAllMaestroGenres()
        {
            return _InReadUncommitedTransaction(
                context => context.genres.Where(g => g.source_id == (int)GenreSourceEnum.Maestro).OrderBy(g => g.name).Select(_MapToDto).ToList());
        }

        public List<Genre> GetAllGenres()
        {
            return _InReadUncommitedTransaction(context => context.genres.Select(_MapToGenre).ToList());
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
                    context.genres
                        .Where(g => g.source_id == sourceId)
                        .OrderBy(x => x.name)
                        .Select(_MapToDto)
                        .ToList()
                );
        }

        public List<GenreMapping> GetGenreMappingsBySourceId(int sourceId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var query = from genre_mapping in context.genre_mappings
                            join genre in context.genres on genre_mapping.mapped_genre_id equals genre.id
                            where genre.source_id == sourceId
                            select genre_mapping;

                return query.ToList().Select(_MapToGenreMapping).ToList();
            });
        }

        private LookupDto _MapToDto(genre genre)
        {
            return new LookupDto
            {
                Display = genre.name,
                Id = genre.id
            };
        }

        private Genre _MapToGenre(genre genre)
        {
            return new Genre
            {
                Id = genre.id,
                Name = genre.name,
                SourceId = genre.source_id
            };
        }

        private GenreMapping _MapToGenreMapping(genre_mappings genre_mapping)
        {
            return new GenreMapping
            {
                GenreIdToMap = genre_mapping.mapped_genre_id,
                MaestroGenreId = genre_mapping.maestro_genre_id
            };
        }
    }
}
