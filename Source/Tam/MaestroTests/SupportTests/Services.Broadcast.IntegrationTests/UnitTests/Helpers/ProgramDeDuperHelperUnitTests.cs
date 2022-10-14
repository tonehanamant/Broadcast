using NUnit.Framework;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    public class ProgramDeDuperHelperUnitTests
    {
        [Test]
        public void Test()
        {
            var candidates = new List<ProgramDto>
            {
                new ProgramDto
                {
                    Name = "UniqueName1",
                    Genre = new LookupDto
                    {
                        Id = 1,
                        Display = "Genre1"
                    },
                    ContentRating = "G"
                },
                new ProgramDto
                {
                    Name = "DupName1",
                    Genre = new LookupDto
                    {
                        Id = 1,
                        Display = "Genre1"
                    },
                    ContentRating = "G"
                },
                // duplicate
                new ProgramDto
                {
                    Name = "DupName1",
                    Genre = new LookupDto
                    {
                        Id = 1,
                        Display = "Genre1"
                    },
                    ContentRating = "G"
                },
                // duplicate, different casing
                new ProgramDto
                {
                    Name = "DUPNAME1",
                    Genre = new LookupDto
                    {
                        Id = 1,
                        Display = "Genre1"
                    },
                    ContentRating = "G"
                },
                // duplicate, different genre
                new ProgramDto
                {
                    Name = "DupName1",
                    Genre = new LookupDto
                    {
                        Id = 2,
                        Display = "Genre2"
                    },
                    ContentRating = "G"
                },
                // duplicate, different rating
                new ProgramDto
                {
                    Name = "DupName1",
                    Genre = new LookupDto
                    {
                        Id = 1,
                        Display = "Genre1"
                    },
                    ContentRating = "R"
                }
            };

            // Act
            var results = ProgramDeDuperHelper.RemoveDuplicateProgramsByName(candidates);

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.True(results.Exists(s => s.Name.Equals("UniqueName1", StringComparison.InvariantCultureIgnoreCase)));
            Assert.True(results.Exists(s => s.Name.Equals("DupName1", StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}
