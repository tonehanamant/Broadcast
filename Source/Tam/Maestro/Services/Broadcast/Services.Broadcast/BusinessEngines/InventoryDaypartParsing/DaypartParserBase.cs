using Services.Broadcast.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Services.Broadcast.BusinessEngines.InventoryDaypartParsing
{
    public interface IDaypartParser
    {
        bool TryParse(string daypartText, out List<string> result);
    }

    public abstract class DaypartParserBase : IDaypartParser
    {
        private static readonly List<string> _DayConstants1 = new List<string> { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
        private static readonly List<string> _DayConstants2 = new List<string> { "M", "TU", "W", "TH", "F", "SA", "SU" };
        private static readonly List<string> _DayConstants3 = new List<string> { "MO", "TU", "WE", "TH", "FR", "SA", "SU" };
        private static readonly List<string> _DayConstants4 = new List<string> { "M", "T", "W", "R", "F", "S", "SU" };

        public static List<string> AllowedWeekDays { get; private set; }

        static DaypartParserBase()
        {
            AllowedWeekDays = _DayConstants1
                .Union(_DayConstants2)
                .Union(_DayConstants3)
                .Union(_DayConstants4)
                .ToList();
        }

        public virtual bool TryParse(string daypartText, out List<string> result)
        {
            result = new List<string>();
            var searchRegexPattern = BuildSearchRegexPattern();
            var searchRegex = new Regex(searchRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = searchRegex.Matches(daypartText);

            if (matches.Count == 0)
            {
                return false;
            }

            foreach (Match match in matches)
            {
                var weekDays = match.Groups["WeekDays"].Value;
                var timeFrom = match.Groups["TimeFrom"].Value;
                var timeTo = match.Groups["TimeTo"].Value;

                if (WeekDaysValid(weekDays))
                {
                    var commonFormatDaypartString = GetCommonFormatDaypartString(weekDays, timeFrom, timeTo);
                    result.Add(commonFormatDaypartString);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        protected abstract string BuildSearchRegexPattern();

        protected bool WeekDaysValid(string weekDaysText)
        {
            var weekDayStrings = weekDaysText.Split(new char[] { '-', ',', ';', '+' });

            foreach (var weekDayString in weekDayStrings)
            {
                if (!_WeekDayValid(weekDayString))
                {
                    return false;
                }
            }

            return true;
        }

        protected string GetCommonFormatDaypartString(string weekDays, string timeFrom, string timeTo)
        {
            // support of spaces is requested in PRI-8845. Correct format is {time}tt
            timeFrom = timeFrom.RemoveWhiteSpaces();
            timeTo = timeTo.RemoveWhiteSpaces();

            weekDays = _GetCommonFormatWeekDays(weekDays);

            // lets replace all range weekdays definitions with enumerations because the general parser doesn`t support this format of weekdays: M-F,SU
            if (weekDays.Contains("-"))
            {
                var weekDaysRangeMatches = Regex.Matches(weekDays, @"[a-z]{1,3}\s*-\s*[a-z]{1,3}", RegexOptions.IgnoreCase);

                foreach (Match match in weekDaysRangeMatches)
                {
                    var weekDaysRange = match.Value;
                    var weekDaysEnumeration = _ConvertDaypartRangeToEnumeration(weekDaysRange);
                    weekDays = weekDays.Replace(weekDaysRange, weekDaysEnumeration);
                }
            }

            return $"{weekDays} {timeFrom}-{timeTo}";
        }

        private string _GetCommonFormatWeekDays(string weekDays)
        {
            const string separators = @"[,-]";

            weekDays = weekDays.RemoveWhiteSpaces();
            weekDays = Regex.Replace(weekDays, @";", ",");
            weekDays = Regex.Replace(weekDays, @"\+", ",");

            var weekDaysSplit = weekDays.Split(new char[] { '-', ',' });

            foreach (var weekDay in weekDaysSplit)
            {
                for (var i = 0; i < 7; i++)
                {
                    if (weekDay.Equals(_DayConstants2[i], System.StringComparison.OrdinalIgnoreCase) ||
                        weekDay.Equals(_DayConstants3[i], System.StringComparison.OrdinalIgnoreCase) ||
                        weekDay.Equals(_DayConstants4[i], System.StringComparison.OrdinalIgnoreCase))
                    {
                        var replaceRegexPattern = $"(^{weekDay}{separators})|({separators}{weekDay}{separators})|({separators}{weekDay}$)|(^{weekDay}$)";
                        weekDays = Regex.Replace(
                            weekDays, 
                            string.Format(replaceRegexPattern, weekDay), 
                            (match) => match.Value.Replace(weekDay, _DayConstants1[i]));
                    }
                }
            }

            return weekDays;
        }

        private bool _WeekDayValid(string weekDayString)
        {
            return AllowedWeekDays.FindIndex(x => x.Equals(weekDayString.Trim(), System.StringComparison.OrdinalIgnoreCase)) != -1;
        }

        private string _ConvertDaypartRangeToEnumeration(string daypartRange)
        {
            var result = new List<string>();
            var startIndex = -1;
            var endIndex = -1;

            var splitDays = daypartRange.Split(new char[] { '-' });

            for (int i = 0; i < splitDays.Length; i++)
            {
                splitDays[i] = splitDays[i].Trim();
            }

            for (int i = 0; i < 7; i++)
            {
                if (splitDays[0].Equals(_DayConstants1[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    startIndex = i;
                }
                else if (splitDays[1].Equals(_DayConstants1[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    endIndex = i;
                }
            }

            if (startIndex >= 0 && endIndex >= 0)
            {
                if (startIndex <= endIndex) // handles M-F range
                {
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        result.Add(_DayConstants1[i]);
                    }
                }
                else // handles Sa-We range
                {
                    for (int i = 0; i <= endIndex; i++)
                    {
                        result.Add(_DayConstants1[i]);
                    }

                    for (int i = startIndex; i < 7; i++)
                    {
                        result.Add(_DayConstants1[i]);
                    }
                }
            }

            return string.Join(",", result);
        }
    }
}
