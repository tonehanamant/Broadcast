using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    public static class InventoryProgramsProcessingTestHelper
    {
        public static List<StationInventoryManifest> GetManifests(int count)
        {
            var daypartIdIndex = 1;
            var weekIdIndex = 1;
            var result = new List<StationInventoryManifest>();
            for (var i = 1; i <= count; i++)
            {
                var manifest = new StationInventoryManifest
                {
                    Id = i,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = $"CadentStationName{i}",
                        Affiliation = "ABC"
                    },
                    ManifestDayparts = new List<StationInventoryManifestDaypart>
                    {
                        new StationInventoryManifestDaypart
                        {
                            Id = daypartIdIndex++,
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 3600 * 2,
                                EndTime = (3600 * 4) - 1,
                                Sunday = i % 2 == 0,
                                Monday = i % 2 == 1,
                                Tuesday = i % 2 == 1,
                                Wednesday = i % 2 == 1,
                                Thursday = i % 2 == 1,
                                Friday = i % 2 == 1,
                                Saturday = i % 2 == 0
                            },
                        },
                        new StationInventoryManifestDaypart
                        {
                            Id = daypartIdIndex++,
                            Daypart = new DisplayDaypart
                            {
                                StartTime = 3600 * 4,
                                EndTime = (3600 * 6) - 1,
                                Sunday = i % 2 == 1,
                                Monday = i % 2 == 0,
                                Tuesday = i % 2 == 0,
                                Wednesday = i % 2 == 0,
                                Thursday = i % 2 == 0,
                                Friday = i % 2 == 1,
                                Saturday = i % 2 == 1
                            }
                        }
                    },
                    ManifestWeeks = new List<StationInventoryManifestWeek>
                    {
                        new StationInventoryManifestWeek
                        {
                            Id = weekIdIndex++,
                            MediaWeek = new MediaWeek
                            {
                                Id = 1,
                                StartDate = new DateTime(2020, 01, 01),
                                EndDate = new DateTime(2020, 01, 07)
                            },
                            StartDate = new DateTime(2020, 01, 01),
                            EndDate = new DateTime(2020, 01, 07)
                        },
                        new StationInventoryManifestWeek
                        {
                            Id = weekIdIndex++,
                            MediaWeek = new MediaWeek
                            {
                                Id = 2,
                                StartDate = new DateTime(2020, 01, 08),
                                EndDate = new DateTime(2020, 01, 14)
                            },
                            StartDate = new DateTime(2020, 01, 08),
                            EndDate = new DateTime(2020, 01, 14)
                        },
                        new StationInventoryManifestWeek
                        {
                            Id = weekIdIndex++,
                            MediaWeek = new MediaWeek
                            {
                                Id = 3,
                                StartDate = new DateTime(2020, 01, 15),
                                EndDate = new DateTime(2020, 01, 21)
                            },
                            StartDate = new DateTime(2020, 01, 15),
                            EndDate = new DateTime(2020, 01, 21)
                        }
                    }
                };
                result.Add(manifest);
            }

            return result;
        }

        public static List<LookupDto> GetGenres()
        {
            return new List<LookupDto>
            {
                new LookupDto(1, "SourceGenreOne"),
                new LookupDto(2, "SourceGenreTwo"),
                new LookupDto(3, "SourceGenreThree"),
                new LookupDto(4, "SourceGenreFour"),
                new LookupDto(4, "SourceGenreFive"),
                new LookupDto(4, "SourceGenreSix"),
            };
        }
    }
}