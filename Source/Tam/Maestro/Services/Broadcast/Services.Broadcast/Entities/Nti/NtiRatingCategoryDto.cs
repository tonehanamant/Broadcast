using System.Collections.Generic;

namespace Services.Broadcast.Entities.Nti
{
    /// <summary>
    /// DTO contains Nielsen's Category information
    /// </summary>
    public class NtiRatingCategoryDto
    {
        /// <summary>
        /// Category Name
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Contains list of relevant Subcategories and its ratings
        /// </summary>
        public List<NtiRatingSubCategoryDto> SubCategories { get; set; }

        /// <summary>
        /// Percentage recorded for the category
        /// </summary>
        public string Percent { get; set; }

        /// <summary>
        /// Impressions recorded for the category
        /// </summary>
        public string Impressions { get; set; }

        /// <summary>
        /// VPVH recorded for the category
        /// </summary>
        public string VPVH { get; set; }
    }
}
