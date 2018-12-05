using System.Collections.Generic;

namespace Services.Broadcast.Entities.Nti
{
    public class NtiRatingDocument
    {
        /// <summary>
        /// Header information of the rating document
        /// </summary>
        public NtiRatingHeaderDto Header { get; set; }

        /// <summary>
        /// Contains list of ratings with categories & subcategories from the rating document
        /// </summary>
        public List<NtiRatingCategory> Ratings { get; set; }
    }
}
