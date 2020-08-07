using Services.Broadcast.Entities;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class MarketsTestData
    {
        public static MarketCoverageDto GetTop100Markets()
        {
            return new MarketCoverageDto
            {
                MarketCoverageFileId = 1,
                MarketCoveragesByMarketCode = new Dictionary<int, double>
                {
                    { 101, 0.101d },
                    { 100, 0.1d },
                    { 302, 0.302d },
                    { 403, 0.04743d },
                    { 202, 0.02942d },
                }
            };
        }
    }
}
