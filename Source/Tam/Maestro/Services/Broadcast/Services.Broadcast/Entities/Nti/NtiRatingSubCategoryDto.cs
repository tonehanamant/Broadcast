using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Nti
{
    /// <summary>
    /// DTO contains Nielsen's SubCategory information
    /// </summary>
    public class NtiRatingSubCategoryDto
    {
        /// <summary>
        /// SubCategory Name
        /// </summary>
        public string SubCategory { get; set; }

        /// <summary>
        /// Percentage recorded for the subcategory
        /// </summary>
        public string Percent { get; set; }

        /// <summary>
        /// Impressions recorded for the subcategory
        /// </summary>
        public string Impressions { get; set; }

        /// <summary>
        /// VPVH recorded for the subcategory
        /// </summary>
        public string VPVH { get; set; }
    }
}
