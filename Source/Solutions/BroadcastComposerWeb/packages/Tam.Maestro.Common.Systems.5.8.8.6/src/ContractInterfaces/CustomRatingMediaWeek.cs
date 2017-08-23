namespace Tam.Maestro.Services.ContractInterfaces.Common
{
    public class CustomRatingMediaWeek
    {
        public readonly int MediaMonthId;
        public readonly int WeekNumber;
        public bool Selected;

        public CustomRatingMediaWeek(int mediaMonthId, int weekNumber)
        {
            MediaMonthId = mediaMonthId;
            WeekNumber = weekNumber;
            Selected = true;
        }
    }
}