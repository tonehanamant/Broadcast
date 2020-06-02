using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines.InventoryProgramsProcessing
{
    public static class InventoryProgramsProcessingTestHelper
    {
        public static List<StationInventoryManifest> GetManifests(int count, bool oddDaypartsHaveMappedPrograms = false)
        {
            var daypartIdIndex = 1;
            var programIdIndex = 1;
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
                            ProgramName = "wonder woman 1984",
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
                            Programs = new List<StationInventoryManifestDaypartProgram>
                            {
                                new StationInventoryManifestDaypartProgram
                                {
                                    Id = programIdIndex++,
                                    StationInventoryManifestDaypartId = daypartIdIndex,
                                    ProgramName = $"Program {programIdIndex}",
                                    ShowType = "AShow",
                                    SourceGenreId = (int)ProgramSourceEnum.Forecasted,
                                    ProgramSourceId = (int)ProgramSourceEnum.Forecasted,
                                    MaestroGenreId = 12,
                                    StartDate = new DateTime(),
                                    EndDate = new DateTime(),
                                    StartTime = 0,
                                    EndTime = 3600,
                                    CreatedDate = new DateTime()
                                },
                                new StationInventoryManifestDaypartProgram
                                {
                                    Id = programIdIndex++,
                                    StationInventoryManifestDaypartId = daypartIdIndex,
                                    ProgramName = $"Program {programIdIndex}",
                                    ShowType = "AShow",
                                    SourceGenreId = (int)ProgramSourceEnum.Forecasted,
                                    ProgramSourceId = _GetProgramSourceId(i, oddDaypartsHaveMappedPrograms),
                                    MaestroGenreId = 12,
                                    StartDate = new DateTime(),
                                    EndDate = new DateTime(),
                                    StartTime = 0,
                                    EndTime = 3600,
                                    CreatedDate = new DateTime()
                                }
                            }
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
                            },
                            Programs = new List<StationInventoryManifestDaypartProgram>
                            {
                                new StationInventoryManifestDaypartProgram
                                {
                                    Id = programIdIndex++,
                                    StationInventoryManifestDaypartId = daypartIdIndex,
                                    ProgramName = $"Program {programIdIndex}",
                                    ShowType = "AShow",
                                    SourceGenreId = (int)ProgramSourceEnum.Forecasted,
                                    ProgramSourceId = _GetProgramSourceId(i, oddDaypartsHaveMappedPrograms),
                                    MaestroGenreId = 12,
                                    StartDate = new DateTime(),
                                    EndDate = new DateTime(),
                                    StartTime = 0,
                                    EndTime = 3600,
                                    CreatedDate = new DateTime()
                                },
                                new StationInventoryManifestDaypartProgram
                                {
                                    Id = programIdIndex++,
                                    StationInventoryManifestDaypartId = daypartIdIndex,
                                    ProgramName = $"Program {programIdIndex}",
                                    ShowType = "AShow",
                                    SourceGenreId = (int)ProgramSourceEnum.Forecasted,
                                    ProgramSourceId = (int)ProgramSourceEnum.Forecasted,
                                    MaestroGenreId = 12,
                                    StartDate = new DateTime(),
                                    EndDate = new DateTime(),
                                    StartTime = 0,
                                    EndTime = 3600,
                                    CreatedDate = new DateTime()
                                }
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
                manifest.ManifestDayparts.ForEach(d => d.PrimaryProgramId = d.Programs.FirstOrDefault()?.Id);
            }

            return result;
        }

        private static int _GetProgramSourceId(int daypartId, bool oddDaypartsHaveMappedPrograms)
        {
            if (daypartId % 2 == 1 && oddDaypartsHaveMappedPrograms)
            {
                return (int)ProgramSourceEnum.Mapped;
            }
            return (int)ProgramSourceEnum.Forecasted;
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