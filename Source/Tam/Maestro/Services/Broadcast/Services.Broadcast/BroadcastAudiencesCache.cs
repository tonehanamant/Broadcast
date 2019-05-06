using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Converters;
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
        bool IsValidAudienceCode(string audienceCode);
        IEnumerable<BroadcastAudience> FindByAgeRange(int fromAge, int toAge);
        IEnumerable<BroadcastAudience> FindByAgeRangeAndSubcategory(int fromAge, int toAge, string subcategory);
        BroadcastAudience GetDefaultAudience();
        LookupDto FindDto(int id);
        BroadcastAudience GetBroadcastAudienceByCode(string audienceCode);
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

        public bool IsValidAudienceCode(string audienceCode)
        {
            return GetBroadcastAudienceByCode(audienceCode) != null;
        }

        public DisplayAudience GetDisplayAudienceByCode(string audienceCode)
        {
            var audience = GetBroadcastAudienceByCode(audienceCode);

            return audience == null ? null : new DisplayAudience()
            {
                Id = audience.Id,
                AudienceString = audience.Name
            };
        }

        public BroadcastAudience GetBroadcastAudienceByCode(string audienceCode)
        {
            var audience = _Audiences.SingleOrDefault(x => x.Code == audienceCode);

            if (audience == null && AudienceHelper.TryMapToSupportedFormat(audienceCode, out var mappingResult))
            {
                audience = _Audiences.SingleOrDefault(x => x.Code == mappingResult);
            }

            return audience;
        }

        public IEnumerable<BroadcastAudience> FindByAgeRange(int fromAge, int toAge)
        {
            return _Audiences.Where(a => a.RangeStart == fromAge && a.RangeEnd == toAge);
        }

        public IEnumerable<BroadcastAudience> FindByAgeRangeAndSubcategory(int fromAge, int toAge, string subcategory)
        {
            return _Audiences.Where(a => a.RangeStart == fromAge && a.RangeEnd == toAge && a.SubCategoryCode.Equals(subcategory, System.StringComparison.InvariantCultureIgnoreCase));
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
