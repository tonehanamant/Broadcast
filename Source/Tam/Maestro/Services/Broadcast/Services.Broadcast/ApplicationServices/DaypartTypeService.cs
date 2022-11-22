using Common.Services.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    /// <summary>
    /// Operations for DaypartType data.
    /// </summary>
    /// <seealso cref="IApplicationService" />
    public interface IDaypartTypeService : IApplicationService
    {
        /// <summary>
        /// Gets the daypart types.
        /// </summary>
        /// <returns>List of <see cref="LookupDto"/></returns>
        List<LookupDto> GetDaypartTypes();
    }

    /// <summary>
    /// Daypart type service
    /// </summary>
    /// <seealso cref="IDaypartTypeService" />
    public class DaypartTypeService : IDaypartTypeService
    {
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaypartTypeService"/> class.
        /// </summary>        
        /// <param name="featureToggleHelper">The feature toggle helper.</param>      
        public DaypartTypeService(IFeatureToggleHelper featureToggleHelper)
        {
            _FeatureToggleHelper = featureToggleHelper;
        }
        ///<inheritdoc/>
        public List<LookupDto> GetDaypartTypes()
        {
            List<LookupDto> types = Enum.GetValues(typeof(DaypartTypeEnum))
                .Cast<DaypartTypeEnum>()
                .Select(e => new LookupDto
                {
                    Id = (int)e,
                    Display = e.GetDescriptionAttribute()
                })
                .OrderBy(x => x.Display)
                .ToList();
            return types;
        }
    }
}