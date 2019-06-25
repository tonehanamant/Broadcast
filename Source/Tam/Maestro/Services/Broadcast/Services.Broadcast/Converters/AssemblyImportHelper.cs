using Common.Services;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters
{
    public static class AssemblyImportHelper
    {
        public const string WomanSubcategoryCode = "W";

        public const int SecondsPerHour = 60 * 60;
        public const int SecondsPerMinute = 60;
        public const int SecondsPerDay = 24 * 60 * 60;

        public static DisplayDaypart LookupDaypartByTimeAndRotation(string times, string rotation, IDaypartCache daypartCache)
        {
            var daypart = new DisplayDaypart();
            if (String.IsNullOrEmpty(rotation))
                throw new Exception("Required day part / rotation missing");

            _ParseDaypartRotation(rotation, daypart);
            _ParseDaypartTimes(times, daypart);

            daypart.Id = daypartCache.GetIdByDaypart(daypart);
            return daypart;
        }

        private static void _ParseDaypartTimes(string times, DisplayDaypart daypart)
        {
            times = times.Replace("M", "A").Replace("N", "P"); //convert noon/midnight to AM/PM for consistency
            var timeParts = times.Split('-');
            var startTimeString = timeParts[0];
            var endTimeString = timeParts[1];

            //apply same suffix as on the end time if missing
            int parseResult;
            if (Int32.TryParse(startTimeString[startTimeString.Length - 1].ToString(), out parseResult))
            {
                startTimeString = startTimeString + endTimeString[endTimeString.Length - 1];
            }

            daypart.StartTime = GetTimeSecondsFromString(startTimeString);
            daypart.EndTime = GetTimeSecondsFromString(endTimeString) - 1;
            if (daypart.EndTime < 0)
            {
                daypart.EndTime += SecondsPerDay;
            }

        }

        private static int GetTimeSecondsFromString(string timeString)
        {
            var numericPart = timeString.Substring(0, timeString.Length - 1);

            var isAfternoon = timeString[timeString.Length - 1].Equals('P');

            if (numericPart.Length <= 2)
            {
                numericPart = numericPart + "00";
            }

            if (numericPart.Length == 3)
            {
                numericPart = "0" + numericPart;
            }

            var hours = Convert.ToInt32(numericPart.Substring(0, 2));
            if (hours == 12 && !isAfternoon)
            {
                hours = hours - 12;
            }
            if (hours < 12 && isAfternoon)
            {
                hours = hours + 12;
            }
            hours = hours % 24;

            var minutes = Convert.ToInt32(numericPart.Substring(2, 2));
            return hours * SecondsPerHour + minutes * SecondsPerMinute; //in seconds

        }

        private static void _ParseDaypartRotation(string rotation, DisplayDaypart daypart)
        {
            var shortDayNames = new List<string>()
            {
                "M",
                "TU",
                "W",
                "TH",
                "F",
                "SA",
                "SU"
            };
            if (rotation.Contains("-")) //mutli-day range
            {
                //we're assuming only a single range M/W-F/SU.... no support for multiple ranges like M-W/F-SU
                var rangeParts = rotation.Split('-');
                var head = rangeParts[0];
                var tail = rangeParts[1];

                string rangeStart;
                if (head.Contains("/"))
                {
                    var headParts = head.Split('/');
                    _AddDaypartDay(headParts[0], daypart);
                    rangeStart = headParts[1];
                }
                else
                {
                    rangeStart = head;
                }

                string rangeEnd;
                if (tail.Contains("/"))
                {
                    var tailParts = tail.Split('/');
                    _AddDaypartDay(tailParts[1], daypart);
                    rangeEnd = tailParts[0];
                }
                else
                {
                    rangeEnd = tail;
                }

                var rangeStartIndex = shortDayNames.IndexOf(rangeStart);
                var rangeEndIndex = shortDayNames.IndexOf(rangeEnd);

                for (int i = rangeStartIndex; i <= rangeEndIndex; i++)
                {
                    _AddDaypartDay(shortDayNames[i], daypart);
                }
            }
            else if (rotation.Contains("/")) //multiple select days
            {
                var days = rotation.Split('/');
                foreach (var day in days)
                {
                    _AddDaypartDay(day, daypart);
                }
            }
            else //single day
            {
                switch (rotation)
                {
                    case "MON":
                        daypart.Monday = true;
                        break;
                    case "TUE":
                        daypart.Tuesday = true;
                        break;
                    case "WED":
                        daypart.Wednesday = true;
                        break;
                    case "THU":
                        daypart.Thursday = true;
                        break;
                    case "FRI":
                        daypart.Friday = true;
                        break;
                    case "SAT":
                        daypart.Saturday = true;
                        break;
                    case "SUN":
                        daypart.Sunday = true;
                        break;
                    default:
                        throw new ApplicationException(String.Format("Unable to parse daypart for rotation: {0}", rotation));
                }
            }
        }

        private static void _AddDaypartDay(string day, DisplayDaypart daypart)
        {
            switch (day)
            {
                case "M":
                    daypart.Monday = true;
                    break;
                case "TU":
                    daypart.Tuesday = true;
                    break;
                case "W":
                    daypart.Wednesday = true;
                    break;
                case "TH":
                    daypart.Thursday = true;
                    break;
                case "F":
                    daypart.Friday = true;
                    break;
                case "SA":
                    daypart.Saturday = true;
                    break;
                case "SU":
                    daypart.Sunday = true;
                    break;
                default:
                    throw new ApplicationException(String.Format("Unable to parse day name from value: {0}", day));
            }
        }

        public static string ExtractAudienceInfo(string audienceCode, out int startAge, out int endAge)
        {
            audienceCode = audienceCode.Replace("+", "99");

            var demoGroup = audienceCode.Substring(0, 2);

            string subcategoryCode;

            switch (demoGroup)
            {
                case "AD":
                    subcategoryCode = "A";
                    break;
                case "WM":
                    subcategoryCode = "W";
                    break;
                case "MN":
                    subcategoryCode = "M";
                    break;
                case "VW":
                    subcategoryCode = "P";
                    break;
                case "KD":
                    subcategoryCode = "C";
                    break;
                case "HH":
                    subcategoryCode = "H";
                    break;
                default:
                    throw new Exception(String.Format("Unknown demo group {0} in assembly rate file.", demoGroup));
            }

            var agePart = audienceCode.Substring(2);

            if (subcategoryCode == "H")
            {
                startAge = 0;
                endAge = 99;
            }
            else if (subcategoryCode == "C")
            {
                startAge = Int32.Parse(agePart.Split('-')[0]);
                endAge = Int32.Parse(agePart.Split('-')[1]);
            }
            else
            {
                startAge = Int32.Parse(agePart.Substring(0, 2));
                endAge = Int32.Parse(agePart.Substring(2));
            }
            return subcategoryCode;
        }
    }
}
