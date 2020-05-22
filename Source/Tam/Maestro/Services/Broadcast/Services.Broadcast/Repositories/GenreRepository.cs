using Common.Services.Extensions;
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

        List<GenreMapping> GetGenreMappings();

        Genre GetGenreByName(string genreName, ProgramSourceEnum source);
    }

    public class GenreRepository : BroadcastRepositoryBase, IGenreRepository
    {

        public GenreRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<LookupDto> GetAllMaestroGenres()
        {
            return _InReadUncommitedTransaction(
                context => context.genres.Where(g => g.program_source_id == (int)ProgramSourceEnum.Maestro).OrderBy(g => g.name).Select(_MapToDto).ToList());
        }

        public List<Genre> GetAllGenres()
        {
            return _InReadUncommitedTransaction(context => context.genres.Select(_MapToGenre).ToList());
        }
        
        public List<LookupDto> FindGenres(string genreSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                     context.genres.Where(g => g.program_source_id == (int)ProgramSourceEnum.Maestro && g.name.ToLower().Contains(genreSearchString.ToLower())).Select(_MapToDto).ToList()
                );
        }

        public List<LookupDto> GetGenresBySourceId(int sourceId)
        {
            return _InReadUncommitedTransaction(
                context =>
                    context.genres
                        .Where(g => g.program_source_id == sourceId)
                        .OrderBy(x => x.name)
                        .Select(_MapToDto)
                        .ToList()
                );
        }

        public List<GenreMapping> GetGenreMappings()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var query = from genre_mapping in context.genre_mappings
                            join genre in context.genres on genre_mapping.mapped_genre_id equals genre.id
                            select new { genre_mapping, genre.program_source_id };

                return query
                    .ToList()
                    .Select(x => new GenreMapping
                    {
                        SourceGenreId = x.genre_mapping.mapped_genre_id,
                        ProgramSourceId = x.program_source_id,
                        MaestroGenreId = x.genre_mapping.maestro_genre_id
                    })
                    .ToList();
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
                ProgramSourceId = genre.program_source_id
            };
        }

        public Genre GetGenreByName(string genreName, ProgramSourceEnum source)
        {
            return _InReadUncommitedTransaction(context =>
                _MapToGenre(
                    context.genres
                    .Single(item => item.program_source_id == (int)source && item.name == genreName, $"No genre found with name: {genreName}")));
        }
    }
}
