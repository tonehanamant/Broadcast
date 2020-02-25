using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Enums
{
    public enum StationMapSetNamesEnum
    {
        [Description("Extended")]
        Extended = 1,

        [Description("NSI")]
        NSI = 2,

        [Description("NSILegacy")]
        NSILegacy = 3,

        [Description("Sigma")]
        Sigma = 4,
    }
}
