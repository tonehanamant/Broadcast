using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class SpotExceptionOutOfSpecSpotInventorySourcesDtoV2
    {
        /// <summary>
        /// Name Of Inventory Source
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get the count of inventory Sources
        /// </summary>
        public int Count { get; set; }
    }
}
