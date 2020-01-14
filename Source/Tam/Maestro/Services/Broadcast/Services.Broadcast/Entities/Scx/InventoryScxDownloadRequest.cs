using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Scx
{
    /// <summary>
    /// Object used to pass the filters
    /// </summary>
    public class InventoryScxDownloadRequest
    {
        /// <summary>
        /// Inventory source id used to filter data
        /// </summary>
        public int InventorySourceId { get; set; }

        /// <summary>
        /// Daypart code id used to filter data
        /// </summary>
        /// <value>12344</value>
        public int DaypartDefaultId { get; set; }

        /// <summary>
        /// Start Date of the start quarter
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date of the end quarter
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// List of unit ids used to filter data
        /// </summary>
        public List<string> UnitNames { get; set; }        
    }
}
