using Services.Broadcast.Entities;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class ConversionTestData
    {
        public static List<NtiToNsiConversionRate> GetNtiToNsiConversionRates()
        {
            return new List<NtiToNsiConversionRate>
            {
                new NtiToNsiConversionRate
                {
                    DaypartDefaultId = 1,
                    ConversionRate = 0.75
                },
                new NtiToNsiConversionRate
                {
                    DaypartDefaultId = 2,
                    ConversionRate = 0.85
                },
                new NtiToNsiConversionRate
                {
                    DaypartDefaultId = 3,
                    ConversionRate = 0.65
                },
                new NtiToNsiConversionRate
                {
                    DaypartDefaultId = 23,
                    ConversionRate = 0.75
                }
            };
        }
    }
}
