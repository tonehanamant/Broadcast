namespace Services.Broadcast.BusinessEngines.InventoryDaypartParsing
{
    public class TimeFirstDaypartParser : DaypartParserBase
    {
        protected override string BuildSearchRegexPattern()
        {
            var weekDayPattern = @"[a-z]{1,3}";
            var timePattern = @"(([0-9]{1,2}:[0-9]{1,2})|([0-9]{1,4}))[a-z]{0,2}";

            return $@"(?<TimeFrom>{timePattern})-(?<TimeTo>{timePattern})[:-](?<WeekDays>{weekDayPattern}([-,;]{weekDayPattern})*)";
        }
    }
}
