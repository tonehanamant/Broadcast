using System.Collections.Generic;

namespace Services.Broadcast.Entities.Nti
{
    /// <summary>
    /// DTO contains Header and Ratings information for the Documents
    /// </summary>
    public class NtiRatingDocumentDto
    {
        /// <summary>
        /// Header information of the rating document
        /// </summary>
        public NtiRatingHeaderDto Header { get; set; }

        /// <summary>
        /// Contains list of ratings with categories and subcategories from the rating document
        /// </summary>
        /// <value>
        /// The ratings.
        /// </value>
        public List<NtiRatingCategoryDto> Ratings { get; set; }
    }
}
