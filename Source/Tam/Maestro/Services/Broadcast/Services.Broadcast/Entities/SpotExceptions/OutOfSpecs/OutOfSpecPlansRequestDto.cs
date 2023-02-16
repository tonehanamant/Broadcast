using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary>
    /// Gets the request parameters from UI for getting Done plans.
    /// </summary>
    public class OutOfSpecPlansRequestDto
    {
        /// <summary>
        /// Gets or sets the week start date.
        /// </summary>
        /// <value>The week start date.</value>
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// Gets or sets the week end date.
        /// </summary>
        /// <value>The week end date.</value>
        public DateTime WeekEndDate { get; set; }

        /// <summary>
        /// Inventory Sources.
        /// </summary>
        public List<string> InventorySourceNames { get; set; }
    }
}
