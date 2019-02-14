namespace Services.Broadcast.BusinessEngines.InventoryDaypartParsing
{
    public class WeekdayFirstDaypartParser : DaypartParserBase
    {
        protected override string BuildSearchRegexPattern()
        {
            /*
                (?<![:-]) - that means that the matched string shouldn`t start from ':' or from '-'
            */

            var weekDayPattern = @"[a-z]{1,3}";
            var timePattern = @"(([0-9]{1,2}:[0-9]{1,2})|([0-9]{1,4}))[a-z]{0,2}";
            return $@"(?<![;:-]|(,\s))(?<WeekDays>{weekDayPattern}(\s*[-,+]\s*{weekDayPattern})*)\s(?<TimeFrom>{timePattern})-(?<TimeTo>{timePattern})";
        }
    }
}
