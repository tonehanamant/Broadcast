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
    /// <seealso cref="Common.Services.ApplicationServices.IApplicationService" />
    public interface IDaypartTypeService : IApplicationService
    {
        /// <summary>
        /// Gets the daypart types.
        /// </summary>
        /// <returns>List of <see cref="LookupDto"/></returns>
        List<LookupDto> GetDaypartTypes();
    }

    /// <summary>
    /// Operations for DaypartType data.
    /// </summary>
    /// <seealso cref="Services.Broadcast.ApplicationServices.IDaypartTypeService" />
    public class DaypartTypeService : IDaypartTypeService
    {
        ///<inheritdoc/>
        public List<LookupDto> GetDaypartTypes()
        {
            List<LookupDto> types = Enum.GetValues(typeof(DaypartTypeEnum))
                .Cast<DaypartTypeEnum>()
                .Select(e => new LookupDto
                {
                    Id = (int)e,
                    Display = e.GetDescriptionAttribute()
                }).ToList();
            return types;
        }
    }
}