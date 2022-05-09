using Services.Broadcast.Entities.Enums.Inventory;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Helpers
{
    public static class GenreHelper
    {
        /// <summary>
        /// Gets the genre ids.
        /// </summary>
        /// <param name="genreType">Type of the genre.</param>
        /// <param name="genres">The genres.</param>
        /// <returns></returns>
        public static List<int> GetGenreIds(InventoryExportGenreTypeEnum genreType, List<LookupDto> genres)
        {
            const string newsGenreName = "NEWS";
            var result = genreType == InventoryExportGenreTypeEnum.News
                ? genres.Where(g => g.Display.ToUpper().Equals(newsGenreName)).Select(g => g.Id).ToList()
                : genres.Where(g => g.Display.ToUpper().Equals(newsGenreName) == false).Select(g => g.Id).ToList();
            return result;
        }
    }
}
