using Common.Services.Extensions;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Services.Broadcast.Cache
{
    public interface IAgencyCache
    {
        List<AgencyDto> GetAgencies();

        AgencyDto GetAgency(int agencyId);
    }

    public class AgencyCache : IAgencyCache
    {
        const int cacheItemTtlSeconds = 3600;
        private const string CACHE_NAME = "Agencies";
        private const string CACHE_KEY = "Agencies";

        private readonly MemoryCache _DataCache = new MemoryCache(CACHE_NAME);
        private List<AgencyDto> _Agencies { get; set; }
        private ITrafficApiClient _TrafficApiClient;

        public AgencyCache(ITrafficApiClient trafficApiClient)
        {
            _TrafficApiClient = trafficApiClient;
            BuildCache(null);
        }

        public List<AgencyDto> GetAgencies()
        {
            return _Agencies;
        }

        public AgencyDto GetAgency(int agencyId)
        {
            return _Agencies.Single(a => a.Id == agencyId, $"Agency with id '{agencyId}' not found.");
        }

        private void BuildCache(CacheEntryRemovedArguments args)
        {
            var agencies = GetAgenciesFromSource();
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(cacheItemTtlSeconds),
                RemovedCallback = BuildCache
            };

            _DataCache.Add(CACHE_KEY, agencies, policy);
            _Agencies = agencies;
        }

        // the API is not case sensitive
        private static readonly string[] _AgencyAllowedChars = { "!", "\"", "#", "$", "%", "&amp;", "'", "(", ")", "*", "+", ",", "-", ".", "/", ":", ";", "&lt;", "=", "&gt;", "?", "@", "[", "\\", "]", "^", "_", "`", "{", "|", "}", "~", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        private List<AgencyDto> GetAgenciesFromSource()
        {
            const int callMaxReturn = 50;

            var agencies = new List<AgencyDto>();
            foreach (var filter in _AgencyAllowedChars)
            {
                List<AgencyDto> found = _TrafficApiClient.GetFilteredAgencies(filter);
                if (found.Count == callMaxReturn)
                {
                    // according to the data set this should not happen.
                    // at time of this writing (9/2019) :
                    //      Max Call Count = 50
                    //      Max Agencies per Letter = 31
                    // 
                    //  This solution is temporary until late PI-5
                    //  Between now and then no Agency list per first letter will go beyond 50... right?
                    throw new InvalidOperationException(
                        $"The return count for filter '{filter}' returned more than the max count of {callMaxReturn}.");
                }

                if (found.Count > 0)
                {
                    agencies.AddRange(found);
                }
            }

            return agencies;
        }
    }
}