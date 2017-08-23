using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast
{
    public interface IBroadcastAudiencesCache
    {
        List<BroadcastAudience> GetAllEntities();
        List<LookupDto> GetAllLookups();
        DisplayAudience GetDisplayAudienceById(int id);
        DisplayAudience GetDisplayAudienceByCode(string audienceCode);
        IEnumerable<BroadcastAudience> FindByAgeRange(int fromAge, int toAge);
        BroadcastAudience GetDefaultAudience();
        LookupDto FindDto(int id);
    }

    public class BroadcastAudiencesCache : IBroadcastAudiencesCache
    {
        private readonly List<BroadcastAudience> _Audiences;

        public BroadcastAudiencesCache(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            using (var scope = new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                _Audiences = broadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>().GetAllAudiences(BroadcastConstants.RatingsGroupId);
            }
        }

        public List<BroadcastAudience> GetAllEntities()
        {
            return _Audiences;
        }

        public List<LookupDto> GetAllLookups()
        {
            return (from x in _Audiences
                    select new LookupDto()
                {
                    Id = x.Id,
                    Display = x.Name
                }).ToList();
        }

        public DisplayAudience GetDisplayAudienceById(int id)
        {
            return (from a in _Audiences
                    where a.Id == id
                select new DisplayAudience()
                {
                    Id = id,
                    AudienceString = a.Name
                }).SingleOrDefault();
        }

        public DisplayAudience GetDisplayAudienceByCode(string audienceCode)
        {
            return (from a in _Audiences
                    where a.Code == audienceCode
                select new DisplayAudience()
                {
                    Id = a.Id,
                    AudienceString = a.Name
                }).SingleOrDefault();
        }

        public IEnumerable<BroadcastAudience> FindByAgeRange(int fromAge, int toAge)
        {
            return _Audiences.Where(a => a.RangeStart == fromAge && a.RangeEnd == toAge);
        }

        public BroadcastAudience GetDefaultAudience()
        {
            return _Audiences.SingleOrDefault(a => a.Code == "HH");
        }

        public LookupDto FindDto(int id)
        {
            return (from x in _Audiences
                    where x.Id == id
                select new LookupDto()
                {
                    Id = x.Id,
                    Display = x.Code
                }).Single(string.Format("Could not find Audience with id {0}", id));
        }
    }
}
