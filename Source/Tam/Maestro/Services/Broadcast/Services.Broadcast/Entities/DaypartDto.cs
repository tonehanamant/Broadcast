
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{

    public class DaypartDto
    {
        public bool mon;
        public bool tue;
        public bool wed;
        public bool thu;
        public bool fri;
        public bool sat;
        public bool sun;
        public int startTime;
        public int endTime;
        public string Text { get; set; }

        public static DisplayDaypart ConvertDaypartDto(DaypartDto daypartDto)
        {
            // convert daypart dto into display daypart
            DisplayDaypart daypart = new DisplayDaypart()
            {
                Monday = daypartDto.mon,
                Tuesday = daypartDto.tue,
                Wednesday = daypartDto.wed,
                Thursday = daypartDto.thu,
                Friday = daypartDto.fri,
                Saturday = daypartDto.sat,
                Sunday = daypartDto.sun,
                StartTime = daypartDto.startTime,
                EndTime = daypartDto.endTime
            };

            // substract 1 second because the end is not included in the daypart/timespan
            daypart.EndTime -= 1;
            if (daypart.EndTime < 0)
            {
                daypart.EndTime += 60 * 60 * 24;
            }

            return daypart;
        }

        public static DaypartDto ConvertDisplayDaypart(DisplayDaypart displayDaypart)
        {
            return new DaypartDto()
            {
                mon = displayDaypart.Monday,
                tue = displayDaypart.Tuesday,
                wed = displayDaypart.Wednesday,
                thu = displayDaypart.Thursday,
                fri = displayDaypart.Friday,
                sat = displayDaypart.Saturday,
                sun = displayDaypart.Sunday,
                startTime = displayDaypart.StartTime,
                endTime = displayDaypart.EndTime + 1,
                Text = displayDaypart.ToString()
            };
        }

    }
}
