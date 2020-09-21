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
                    StandardDaypartId = 1,
                    ConversionRate = 0.75
                },
                new NtiToNsiConversionRate
                {
                    StandardDaypartId = 2,
                    ConversionRate = 0.85
                },
                new NtiToNsiConversionRate
                {
                    StandardDaypartId = 3,
                    ConversionRate = 0.65
                },
                new NtiToNsiConversionRate
                {
                    StandardDaypartId = 23,
                    ConversionRate = 0.75
                }
            };
        }
    }
}
