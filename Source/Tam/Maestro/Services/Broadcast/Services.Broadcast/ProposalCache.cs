using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;

namespace Services.Broadcast
{
    public interface IProposalCache
    {
        ProposalDto GetProposalByIdAndVersion(int proposalId, int version);
        ProposalDto GetProposalByGuid(string guidString);
        void SetCacheTimeout(int timeoutSeconds);
    }

    public class ProposalCache : IProposalCache
    {
        public static IProposalCache ProposalCacheInstance;
        private readonly MemoryCache _Cache = new MemoryCache("ProposalCache");
        private static ConcurrentDictionary<string, Guid> _CachedProposalVersions = new ConcurrentDictionary<string, Guid>();
        private int _CacheTimeoutInSeconds;
        private static readonly object SyncLock = new object();

        private ProposalCache()
        {

            //This is temporary. Will be moved to params table:
            _CacheTimeoutInSeconds =  60 * 60 * 8; //8 hours
        }

        public void SetCacheTimeout(int timeoutSeconds)
        {
            _CacheTimeoutInSeconds = timeoutSeconds;
        }

        public static IProposalCache Instance
        {
            get
            {
                lock (SyncLock)
                {
                    if (ProposalCacheInstance == null)
                    {
                        ProposalCacheInstance =
                            new ProposalCache();
                    }
                    return ProposalCacheInstance;
                }
            }
        }

        public ProposalDto GetProposalByIdAndVersion(int proposalId, int proposalVersion)
        {
            var lookupKey = _ProposalIdAndVersionToKey(proposalId, proposalVersion);

            Guid guid;
            ProposalDto proposal = null;
            if (_CachedProposalVersions.TryGetValue(lookupKey, out guid))
            {

                proposal = GetProposalByGuid(guid.ToString());
                if (proposal == null)
                {
                    //Remove from lookup list if already gone from cache
                    _CachedProposalVersions.TryRemove(lookupKey, out guid);
                }

            }

            return proposal;
        }

        public ProposalDto GetProposalByGuid(string guidString)
        {
            if (string.IsNullOrEmpty(guidString) || !_Cache.Contains(guidString))
                throw new ApplicationException(
                    "Proposal not found in cache. Possible cache timeout.");

            return (ProposalDto)_Cache.Get(guidString);
        }

        private void _RemoveOldCacheEntryForProposalVersion(ProposalDto proposalDto)
        {
            Guid guid;
            if (proposalDto.Id.HasValue && proposalDto.Version.HasValue) //removing a cache entry to be replaced
            {
                var hasOldGuid =
                    _CachedProposalVersions.TryGetValue(
                        _ProposalIdAndVersionToKey(proposalDto.Id.Value, proposalDto.Version.Value),
                        out guid);
                if (hasOldGuid && _Cache.Contains(guid.ToString()))
                {
                    _Cache.Remove(guid.ToString());
                }
            }
        }

        private static string _ProposalIdAndVersionToKey(int proposalId, int version)
        {
            return proposalId.ToString() + "|" + version.ToString();
        }

    }
}
