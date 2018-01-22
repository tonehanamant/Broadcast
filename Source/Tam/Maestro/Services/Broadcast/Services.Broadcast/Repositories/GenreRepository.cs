using System.Globalization;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IGenreRepository : IDataRepository
    {
        LookupDto GetGenre(int id);
        LookupDto GetGenre(string name);
        int AddGenre(LookupDto genre, string userName);
        List<LookupDto> GetAllGenres();

        List<LookupDto> FindGenres(string genreSearchString);
    }

    public class GenreRepository: BroadcastRepositoryBase, IGenreRepository
    {

        public GenreRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public LookupDto GetGenre(int id)
        {
            return _InReadUncommitedTransaction(
                context => (from g in context.genres
                            where g.id == id
                            select new LookupDto()
                            {
                                Id = g.id,
                                Display = g.name
                            }).SingleOrDefault());
        }

        public LookupDto GetGenre(string name)
        {
            return _InReadUncommitedTransaction(
                context => (from g in context.genres
                            where string.Compare(g.name, name, StringComparison.InvariantCultureIgnoreCase) == 0
                            select new LookupDto()
                            {
                                Id = g.id,
                                Display = g.name
                            }).SingleOrDefault());              
        }

        public int AddGenre(LookupDto genre, string userName)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var timeNow = DateTime.Now;
                    EntityFrameworkMapping.Broadcast.genre newGenre =
                        context.genres.Add(new EntityFrameworkMapping.Broadcast.genre()
                        {
                            name = genre.Display.Trim(),
                            created_by = userName,
                            created_date = timeNow,
                            modified_by = userName,
                            modified_date = timeNow
                        });

                    context.SaveChanges();

                    return newGenre.id;
                });
        }

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
