using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class WebUtilityHelperUnitTests
    {
        private static string[] testNames = new[]
        {
            // HTML to decode
            @"Routine &amp; Routine",
            @"Routine &lt; Routine",
            // Unicode to decode - Removed - Unicode Decode as we decide if we really want that.
            //@"AMERICA RISING: FIGHTING THE PANDEMIC - A SPECIAL EDITION OF 20/20",
            //@"POPSTARS BEST OF 2018",
            //@"POPSTARS BEST OF 2019",

            // ensure handles null
            null
        };

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HtmlDecodeProgramNames_ProgramMappingsFileRequestDtos()
        {
            // Arrange
            var programMappings = testNames.Select(n =>
                new ProgramMappingsFileRequestDto {OriginalProgramName = n, OfficialProgramName = n})
                .ToList();

            // Act
            WebUtilityHelper.HtmlDecodeProgramNames(programMappings);

            // Assert
            var afterOriginalProgramNames = programMappings.Select(s => s.OriginalProgramName).ToList();
            var afterOfficialProgramNames = programMappings.Select(s => s.OfficialProgramName).ToList();

            var verifiableResult = new 
            {
                testNames,
                afterOriginalProgramNames,
                afterOfficialProgramNames
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(verifiableResult));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HtmlDecodeProgramNames_InventoryFileBase()
        {
            // Arrange
            var inventoryFileBase = new InventoryFileBase
            {
                InventoryManifests = testNames.Select(n => new StationInventoryManifest
                    {
                        ManifestDayparts = new List<StationInventoryManifestDaypart> {new StationInventoryManifestDaypart {ProgramName = n}}
                    })
                    .ToList()
            };

            // Act
            WebUtilityHelper.HtmlDecodeProgramNames(inventoryFileBase);

            // Assert
            var afterManifestDaypartNames = inventoryFileBase.InventoryManifests
                .SelectMany(i => i.ManifestDayparts)
                .Select(d => d.ProgramName)
                .ToList();

            var verifiableResult = new
            {
                testNames,
                afterManifestDaypartNames
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(verifiableResult));
        }
    }
}