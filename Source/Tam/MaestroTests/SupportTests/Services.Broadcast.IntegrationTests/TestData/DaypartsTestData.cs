using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class DaypartsTestData
    {
        public static DisplayDaypart GetDisplayDaypart(int daypartId)
        {
            DisplayDaypart result;

            switch(daypartId)
            {
                case 1:
                    result = new DisplayDaypart
                    {
                        Id = 1,
                        Monday = true,
                        Tuesday = true,
                        Wednesday = true,
                        Thursday = true,
                        Friday = true,
                        Saturday = false,
                        Sunday = false,
                        StartTime = 3600, // 1am
                        EndTime = 7199 // 2am
                    };
                    break;

                case 2:
                    result = new DisplayDaypart
                    {
                        Id = 2,
                        Monday = false,
                        Tuesday = false,
                        Wednesday = false,
                        Thursday = false,
                        Friday = false,
                        Saturday = true,
                        Sunday = true,
                        StartTime = 46800, // 1pm
                        EndTime = 53999 // 3pm
                    };
                    break;

                case 3:
                    result = new DisplayDaypart
                    {
                        Id = 3,
                        Monday = true,
                        Tuesday = false,
                        Wednesday = true,
                        Thursday = false,
                        Friday = true,
                        Saturday = false,
                        Sunday = true,
                        StartTime = 72000, // 8pm
                        EndTime = 79199 // 10pm
                    };
                    break;

                case 4:
                    result = new DisplayDaypart
                    {
                        Id = 4,
                        Monday = true,
                        Tuesday = false,
                        Wednesday = true,
                        Thursday = false,
                        Friday = true,
                        Saturday = false,
                        Sunday = true,
                        StartTime = 79200, // 10pm
                        EndTime = 82799 // 11pm
                    };
                    break;

                default:
                    result = new DisplayDaypart
                    {
                        Id = 999,
                        Monday = true,
                        Tuesday = true,
                        Wednesday = true,
                        Thursday = true,
                        Friday = true,
                        Saturday = true,
                        Sunday = true,
                        StartTime = 32400, // 9am
                        EndTime = 39599 // 11am
                    };
                    break;
            }

            return result;
        }
    }
}
