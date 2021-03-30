using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAgencyService : IApplicationService
    {
        /// <summary>
        /// Gets the agencies.
        /// </summary>
        List<AgencyDto> GetAgencies();

        /// <summary>
        /// Clears the agencies cache.
        /// </summary>
        void ClearAgenciesCache();
    }

    public class AgencyService : BroadcastBaseClass, IAgencyService
    {
        private readonly IAabEngine _AabEngine;

        public AgencyService(IAabEngine aabEngine)
        {
            _AabEngine = aabEngine;
        }

        /// <inheritdoc />
        public List<AgencyDto> GetAgencies()
        {
            return _AabEngine.GetAgencies();
        }

        /// <inheritdoc />
        public void ClearAgenciesCache()
        {
            _AabEngine.ClearAgenciesCache();
        }
    }
}
