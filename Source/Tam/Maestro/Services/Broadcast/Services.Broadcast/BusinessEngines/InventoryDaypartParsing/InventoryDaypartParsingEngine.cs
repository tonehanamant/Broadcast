using Common.Services.ApplicationServices;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.BusinessEngines.InventoryDaypartParsing
{
    public interface IInventoryDaypartParsingEngine : IApplicationService
    {
        bool TryParse(string daypartText, out List<DisplayDaypart> result);
    }

    public class InventoryDaypartParsingEngine : IInventoryDaypartParsingEngine
    {
        private readonly string _validTimeExpression = @"(^([0-9]|[0-1][0-9]|[2][0-3])(:)?([0-5][0-9])(\s{0,1})(A|AM|P|PM|a|am|p|pm|aM|Am|pM|Pm{2,2})$)|(^([0-9]|[1][0-9]|[2][0-3])(\s{0,1})(A|AM|P|PM|a|am|p|pm|aM|Am|pM|Pm{2,2})$)";
        private readonly List<IDaypartParser> _Parsers;

        public InventoryDaypartParsingEngine()
        {
            _Parsers = new List<IDaypartParser>
            {
                new WeekdayFirstDaypartParser(),
                new TimeFirstDaypartParser()
            };
        }

        public bool TryParse(string daypartText, out List<DisplayDaypart> result)
        {
            result = new List<DisplayDaypart>();

            if (_HasForbiddenSymbols(daypartText) || 
                _HasIncorrectDaypartFormat(daypartText) ||
                _HasWrongWeekDay(daypartText))
            {
                return false;
            }

            var daypartStrings = _ParseToCommonFormat(daypartText);

            if (daypartStrings.IsEmpty())
            {
                return false;
            }
            
            foreach (var daypartString in daypartStrings)
            {
                if (_TryParseDaypart(daypartString, out var daypart))
                {
                    result.Add(daypart);
                }
                else
                {
                    return false;
                }
            }

            return result.Any();
        }

        private bool _HasForbiddenSymbols(string daypartText)
        {
            // split should work faster than several calls of Contains
            var splitResult = daypartText.Split(new char[] { '(', ')' });
            return splitResult.Count() > 1;
        }

        private bool _HasIncorrectDaypartFormat(string daypartText)
        {
            var timePattern = @"(([0-9]{1,2}:[0-9]{1,2})|([0-9]{1,4}))[a-z]{0,2}";
            var timeRange = $@"{timePattern}-{timePattern}";
            var separator = @"[\/+]";
            var wrongFormats = new List<string>
            {
                $@"{separator}\s*{timeRange}\s*{separator}",
                $@"^\s*{timeRange}\s*{separator}",
                $@"{separator}\s*{timeRange}\s*$",
                @":-",
                @"[:-](a|am|p|pm)"
            };
            
            return wrongFormats.Any(x => Regex.Match(daypartText, x, RegexOptions.IgnoreCase).Success);
        }

        private bool _HasWrongWeekDay(string daypartText)
        {
            var weekDayMatches = Regex.Matches(daypartText, @"[-:](?<WeekDay>[a-z]+)", RegexOptions.IgnoreCase);

            foreach (Match match in weekDayMatches)
            {
                var weekDay = match.Groups["WeekDay"].Value;

                if (DaypartParserBase.AllowedWeekDays.FindIndex(x => weekDay.Equals(x, StringComparison.OrdinalIgnoreCase)) == -1)
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> _ParseToCommonFormat(string daypartText)
        {
            foreach (var parser in _Parsers)
            {
                if (parser.TryParse(daypartText, out var daypartStrings))
                {
                    return daypartStrings;
                }
            }

            return null;
        }

        private bool _TryParseDaypart(string value, out DisplayDaypart result)
        {
            result = null;
            value = value.Trim();

            if (!string.IsNullOrEmpty(value))
            {
                string lDays;
                string lTimes;
                string[] lTimesSplit;

                DisplayDaypart lDisplayDaypart = new DisplayDaypart();
                lDisplayDaypart.Code = "CUS";
                lDisplayDaypart.Name = "Custom";

                if (value.Split(new char[] { ' ' }).Length != 2)
                    return (false);

                lDays = value.Split(new char[] { ' ' })[0].Trim();
                lTimes = value.Split(new char[] { ' ' })[1].Trim();

                lTimesSplit = lTimes.Split(new char[] { '-' });
                if (lTimesSplit.Length != 2)
                    return (false);

                #region Days
                string[] daysList = lDays.Split(new char[] { ',' });

                if (!_TryParseDays(lDisplayDaypart, daysList))
                    return false;
                #endregion

                #region Times

                #region Start Time
                string startMeridiem = null;
                string endMeridiem = null;
                if (lTimesSplit[0].EndsWith("am", StringComparison.InvariantCultureIgnoreCase)
                    || lTimesSplit[0].EndsWith("a", StringComparison.InvariantCultureIgnoreCase))
                {
                    startMeridiem = "am";
                }
                else if (lTimesSplit[0].EndsWith("pm", StringComparison.InvariantCultureIgnoreCase)
                   || lTimesSplit[0].EndsWith("p", StringComparison.InvariantCultureIgnoreCase))
                {
                    startMeridiem = "pm";
                }

                if (lTimesSplit[1].EndsWith("am", StringComparison.InvariantCultureIgnoreCase)
                    || lTimesSplit[1].EndsWith("a", StringComparison.InvariantCultureIgnoreCase))
                {
                    endMeridiem = "am";
                }
                else if (lTimesSplit[1].EndsWith("pm", StringComparison.InvariantCultureIgnoreCase)
                   || lTimesSplit[1].EndsWith("p", StringComparison.InvariantCultureIgnoreCase))
                {
                    endMeridiem = "pm";
                }

                if (startMeridiem == null && endMeridiem == null)
                {
                    return false;
                }

                if (startMeridiem == null)
                {
                    lTimesSplit[0] = lTimesSplit[0] + endMeridiem;
                }

                if (endMeridiem == null)
                {
                    lTimesSplit[1] = lTimesSplit[1] + startMeridiem;
                }

                int? lStartTime = _ParseTime(lTimesSplit[0]);
                if (!lStartTime.HasValue)
                    return false;
                lDisplayDaypart.StartTime = lStartTime.Value;
                #endregion

                #region End Time
                int? lEndTime = _ParseTime(lTimesSplit[1]);
                if (!lEndTime.HasValue)
                    return false;
                lDisplayDaypart.EndTime = lEndTime.Value - 1;
                #endregion

                #endregion

                if (lDisplayDaypart.StartTime == 86400)
                {
                    lDisplayDaypart.StartTime = 0;
                }

                result = lDisplayDaypart;
            }

            return (result != null);
        }

        private bool _TryParseDays(DisplayDaypart pDaypart, string[] days)
        {
            string[] dayConstants = new string[7] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };

            foreach (string day in days)
            {
                if (day.Trim() == "")
                    continue;

                for (int i = 0; i < 7; i++)
                {
                    if (day.Equals(dayConstants[i], StringComparison.OrdinalIgnoreCase))
                    {
                        switch (i)
                        {
                            case (0):
                                pDaypart.Monday = true;
                                break;
                            case (1):
                                pDaypart.Tuesday = true;
                                break;
                            case (2):
                                pDaypart.Wednesday = true;
                                break;
                            case (3):
                                pDaypart.Thursday = true;
                                break;
                            case (4):
                                pDaypart.Friday = true;
                                break;
                            case (5):
                                pDaypart.Saturday = true;
                                break;
                            case (6):
                                pDaypart.Sunday = true;
                                break;
                        }
                    }
                }
            }

            return true;
        }

        private int? _ParseTime(string pTimeString)
        {
            int lReturn = -1;

            var checkTime = new Regex(_validTimeExpression);

            if (checkTime.IsMatch(pTimeString))
            {
                int lHour;
                int lMinute;
                int lSecond;

                bool AM = pTimeString.EndsWith("A", StringComparison.InvariantCultureIgnoreCase) || pTimeString.EndsWith("AM", StringComparison.InvariantCultureIgnoreCase);
                bool PM = pTimeString.EndsWith("P", StringComparison.InvariantCultureIgnoreCase) || pTimeString.EndsWith("PM", StringComparison.InvariantCultureIgnoreCase);

                pTimeString =
                    pTimeString.Replace("A", "")
                        .Replace("a", "")
                        .Replace("P", "")
                        .Replace("p", "")
                        .Replace("M", "")
                        .Replace("m", "");

                //ad colon separating minutes and hours if missing
                if (!pTimeString.Contains(":") && pTimeString.Length > 2)
                {
                    pTimeString = pTimeString.Substring(0, pTimeString.Length - 2) + ":" + pTimeString.Substring(pTimeString.Length - 2);
                }

                if (pTimeString.Contains(":"))
                {
                    string[] lTimePieces = pTimeString.Split(new char[] { ':' });
                    if (lTimePieces.Length == 2)
                    {
                        if (!int.TryParse(lTimePieces[0], out lHour))
                            return (null);
                        if (!int.TryParse(lTimePieces[1], out lMinute))
                            return (null);

                        if (PM && lHour < 12)
                            lHour += 12;
                        if (AM && lHour >= 12)
                            lHour -= 12;

                        lReturn = (lHour * 3600) + (lMinute * 60);
                    }
                    else if (lTimePieces.Length == 3)
                    {
                        if (!int.TryParse(lTimePieces[0], out lHour))
                            return (null);
                        if (!int.TryParse(lTimePieces[1], out lMinute))
                            return (null);
                        if (!int.TryParse(lTimePieces[2], out lSecond))
                            return (null);

                        if (PM && lHour < 12)
                            lHour += 12;
                        if (AM && lHour >= 12)
                            lHour -= 12;

                        lReturn = (lHour * 3600) + (lMinute * 60) + lSecond;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (!int.TryParse(pTimeString, out lHour))
                        return (null);

                    if (PM && lHour < 12)
                        lHour += 12;
                    if (AM && lHour >= 12)
                        lHour -= 12;

                    if (lHour == 0)
                        lHour = 24;

                    lReturn = (lHour * 3600);
                }
            }

            return lReturn == -1 ? null : (int?)lReturn;
        }
    }
}
