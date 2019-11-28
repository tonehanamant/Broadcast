using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System.Collections.Generic;
using System.Linq;
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
        List<PlanAudienceDisplay> GetAudiences();

        /// <summary>
        /// Gets the audience by identifier.
        /// </summary>
        /// <param name="audienceId">The audience identifier.</param>
        /// <returns></returns>
        PlanAudienceDisplay GetAudienceById(int audienceId);
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
        public List<PlanAudienceDisplay> GetAudiences()
        {
            return _AudiencesCache.GetAllEntities()
                .Select(x => new PlanAudienceDisplay
                {
                    Id = x.Id,
                    Code = x.Code,
                    Display = x.Name
                }).ToList();
        }

        ///<inheritdoc/>
        public PlanAudienceDisplay GetAudienceById(int audienceId)
        {
            return _AudiencesCache.GetAllEntities()
                .Where(x=>x.Id == audienceId)
                .Select(x => new PlanAudienceDisplay
                {
                    Id = x.Id,
                    Code = x.Code,
                    Display = x.Name
                }).Single(x=>x.Id == audienceId, $"Could not find audience with id = {audienceId}");
        }
    }
}
