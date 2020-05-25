namespace Services.Broadcast.BusinessEngines
{
    public static class GeneralMath
    {
        public static double ConvertPercentageToFraction(double percentage)
        {
            return percentage / 100;
        }

        public static double ConvertFractionToPercentage(double fraction)
        {
            return fraction * 100;
        }
    }
}
