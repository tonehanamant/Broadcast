

using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities.Enums.Inventory;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [Category("short_running")]
    public class GenreHelperUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetExportGenreIds_NotNews()
        {
            // Arrange
            var genres = _GetAllGenres();

            // Act
            var result = GenreHelper.GetGenreIds(InventoryExportGenreTypeEnum.NonNews, genres);

            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetExportGenreIds_News()
        {
            // Arrange
            var genres = _GetAllGenres();

            // Act
            var result = GenreHelper.GetGenreIds(InventoryExportGenreTypeEnum.News, genres);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<LookupDto> _GetAllGenres()
        {
            var allGenres = new List<LookupDto>
            {
                new LookupDto { Display = "Comedy", Id = 9 },
                new LookupDto { Display = "Crime", Id = 11 },
                new LookupDto { Display = "Documentary", Id = 12 },
                new LookupDto { Display = "Drama", Id = 14 },
                new LookupDto { Display = "Entertainment", Id = 15 },
                new LookupDto { Display = "Game Show", Id = 20 },
                new LookupDto { Display = "Horror", Id = 25 },
                new LookupDto { Display = "Informational", Id = 26 },
                new LookupDto { Display = "Nature", Id = 33 },
                new LookupDto { Display = "News", Id = 34 },
                new LookupDto { Display = "Reality", Id = 39 },
                new LookupDto { Display = "Religious", Id = 40 },
                new LookupDto { Display = "Science Fiction", Id = 42 },
                new LookupDto { Display = "Sports/Sports Talk", Id = 44 },
                new LookupDto { Display = "Talk", Id = 45 },
                new LookupDto { Display = "Action/Adventure", Id = 51 },
                new LookupDto { Display = "Children", Id = 52 },
                new LookupDto { Display = "Educational", Id = 53 },
                new LookupDto { Display = "Lifestyle", Id = 54 },
                new LookupDto { Display = "Paid Program", Id = 55 },
                new LookupDto { Display = "Special", Id = 56 }
            };
            return allGenres;
        }
    }
}
