using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAudienceService : IApplicationService
    {
        /// <summary>
        /// Gets the audience types.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetAudienceTypes();

        /// <summary>
        /// Gets the audiences.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetAudiences();
    }

    public class AudienceService : IAudienceService
    {
        private readonly IBroadcastAudiencesCache _AudiencesCache;

        public AudienceService(IBroadcastAudiencesCache broadcastAudiencesCache)
        {
            _AudiencesCache = broadcastAudiencesCache;
        }

        ///<inheritdoc/>
        public List<LookupDto> GetAudienceTypes()
        {
            return EnumExtensions.ToLookupDtoList<AudienceTypeEnum>();
        }

        ///<inheritdoc/>
        public List<LookupDto> GetAudiences()
        {
            return _AudiencesCache.GetAllLookups();
        }
    }
}
