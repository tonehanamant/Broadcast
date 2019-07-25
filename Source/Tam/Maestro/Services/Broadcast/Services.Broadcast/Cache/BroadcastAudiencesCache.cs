using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Cache
{
    public interface IBroadcastAudiencesCache : IApplicationService
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
        private readonly Dictionary<string, LookupDto> _AudienceMaps;

        public BroadcastAudiencesCache(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            using (var scope = new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var repository = broadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>();
                var audiences = repository.GetAllAudiences(BroadcastConstants.RatingsGroupId);
                _Audiences = _SortAudiences(audiences);
                _AudienceMaps = repository.GetAudienceMaps();
            }
        }

        private List<BroadcastAudience> _SortAudiences(List<BroadcastAudience> audiences)
        {
            var result = new List<BroadcastAudience>();

            var hhAudience = audiences.Single(x => x.SubCategoryCode == "H");
            var sortedAdults = _SortAudiencesByYearsRange(audiences.Where(x => x.SubCategoryCode == "A"));
            var sortedMen = _SortAudiencesByYearsRange(audiences.Where(x => x.SubCategoryCode == "M"));
            var sortedWomen = _SortAudiencesByYearsRange(audiences.Where(x => x.SubCategoryCode == "W"));
            var sortedChildren = _SortAudiencesByYearsRange(audiences.Where(x => x.SubCategoryCode == "C"));
            var sortedPersons = _SortAudiencesByYearsRange(audiences.Where(x => x.SubCategoryCode == "P"));

            result.Add(hhAudience);
            result.AddRange(sortedAdults);
            result.AddRange(sortedMen);
            result.AddRange(sortedWomen);
            result.AddRange(sortedChildren);
            result.AddRange(sortedPersons);

            var sortedOther = audiences.Except(result)
                                 .OrderBy(x => x.SubCategoryCode)
                                 .ThenBy(x => x.RangeStart)
                                 .ThenBy(x => x.Name.EndsWith("+") ? -1 : x.RangeEnd)
                                 .ToList();

            result.AddRange(sortedOther);

            return result;
        }

        private List<BroadcastAudience> _SortAudiencesByYearsRange(IEnumerable<BroadcastAudience> audiences)
        {
            return audiences
                .OrderBy(x => x.RangeStart)
                .ThenBy(x => x.Name.EndsWith("+") ? -1 : x.RangeEnd) // Audiences with plus e.g. M65+ should go first
                .ToList();
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
            //remove all the white spaces from the audience
            audienceCode = audienceCode.RemoveWhiteSpaces();
            
            var audience = _Audiences.SingleOrDefault(x => x.Code == audienceCode);

            if (audience == null && _Map(audienceCode, out var mappingResult))
            {
                audience = mappingResult;                
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
            return _Audiences.SingleOrDefault(a => a.Code == BroadcastConstants.HOUSEHOLD_CODE);
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

        private bool _Map(string audienceToMap, out BroadcastAudience audience)
        {
            var audienceMap = _AudienceMaps.Where(y => y.Key.Equals(audienceToMap)).SingleOrDefault().Value;

            //return the mapped code 
            audience = _Audiences.SingleOrDefault(x => x.Code == audienceMap?.Display);

            return audience != null;
        }
    }
}
