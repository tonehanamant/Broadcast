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
    /// Operations related to the Pricing domain.
    /// </summary>
    /// <seealso cref="Common.Services.ApplicationServices.IApplicationService" />
    public interface IPricingService : IApplicationService
    {
        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetUnitCaps();
    }

    public class PricingService : IPricingService
    {
        public List<LookupDto> GetUnitCaps()
        {
            return Enum.GetValues(typeof(UnitCapEnum))
                .Cast<UnitCapEnum>()
                .Select(e => new LookupDto
                {
                    Id = (int)e,
                    Display = e.GetDescriptionAttribute()
                })
                .OrderBy(x => x.Id)
                .ToList();
        }
    }
}
