using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkMapping.Broadcast;

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

        public static MarketCoverageDto GetLatestMarketCoverages()
        {
            return new MarketCoverageDto
            {
                MarketCoverageFileId = 1,
                MarketCoveragesByMarketCode = GetMarketsWithLatestCoverage().ToDictionary(x => x.MarketCode, x => x.PercentageOfUS)                
            };
        }

        /// <summary>
        /// Formatted for a plan, get all the markets as available markets
        /// </summary>
        /// <param name="marketCodes">Null to return all.  Provide values for just those values.</param>
        public static List<PlanAvailableMarketDto> GetPlanAvailableMarkets(List<int> marketCodes = null)
        {
            var markets = GetMarketsWithLatestCoverage();
            if (marketCodes?.Any() == true)
            {
                var requestedMarkets = markets.Where(s => marketCodes.Contains(s.MarketCode)).ToList();
                markets = requestedMarkets;
            }

            var idIndex = 0;
            var result = markets.Select(m => new PlanAvailableMarketDto
            {
                Id = ++idIndex,
                Market = m.Market,
                MarketCode = (short)m.MarketCode,
                MarketCoverageFileId = m.MarketCoverageFileId,
                PercentageOfUS = m.PercentageOfUS,
                Rank = m.Rank.HasValue ? m.Rank.Value : -1,
                ShareOfVoicePercent = null
            }).ToList();

            return result;
        }

        /// <summary>
        /// Formatted for a plan, get all the markets as blackout markets
        /// </summary>
        /// <param name="marketCodes">Null to return all.  Provide values for just those values.</param>
        public static List<PlanBlackoutMarketDto> GetPlanBlackoutMarketDtos(List<int> marketCodes = null)
        {
            var markets = GetMarketsWithLatestCoverage();
            if (marketCodes?.Any() == true)
            {
                var requestedMarkets = markets.Where(s => marketCodes.Contains(s.MarketCode)).ToList();
                markets = requestedMarkets;
            }

            var result = markets.Select(m => new PlanBlackoutMarketDto
            {
                Market = m.Market,
                MarketCode = (short)m.MarketCode,
                MarketCoverageFileId = m.MarketCoverageFileId,
                PercentageOfUS = m.PercentageOfUS,
                Rank = m.Rank.HasValue ? m.Rank.Value : -1
            }).ToList();

            return result;
        }

        public static List<market_dma_map> GetMarketMapFromMarketCodes(IEnumerable<int> marketCodes)
        {
            var markets = GetMarketsWithLatestCoverage();
            var result = markets.Where(m => marketCodes.Contains(m.MarketCode))
                .Select(m => new market_dma_map
                {
                    market_code = (short)m.MarketCode,
                    dma_mapped_value = m.Market
                }).ToList();
            return result;
        }

        public static List<MarketCoverage> GetMarketsWithLatestCoverage()
        {
            #region BigList - Market Coverages
            
            var marketCoverages = new List<MarketCoverage>
            {
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 79, Market = "Portland-Auburn", TVHomes = 367720, PercentageOfUS = 0.328, MarketCode = 100 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 1, Market = "New York", TVHomes = 7074750, PercentageOfUS = 6.309, MarketCode = 101 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 161, Market = "Binghamton", TVHomes = 120060, PercentageOfUS = 0.107, MarketCode = 102 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 120, Market = "Macon", TVHomes = 222970, PercentageOfUS = 0.199, MarketCode = 103 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 4, Market = "Philadelphia", TVHomes = 2869580, PercentageOfUS = 2.559, MarketCode = 104 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 14, Market = "Detroit", TVHomes = 1779380, PercentageOfUS = 1.587, MarketCode = 105 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 10, Market = "Boston (Manchester)", TVHomes = 2425440, PercentageOfUS = 2.163, MarketCode = 106 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 90, Market = "Savannah", TVHomes = 328860, PercentageOfUS = 0.293, MarketCode = 107 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 24, Market = "Pittsburgh", TVHomes = 1141950, PercentageOfUS = 1.018, MarketCode = 108 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 111, Market = "Ft. Wayne", TVHomes = 249130, PercentageOfUS = 0.222, MarketCode = 109 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 19, Market = "Cleveland-Akron (Canton)", TVHomes = 1447310, PercentageOfUS = 1.291, MarketCode = 110 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 6, Market = "Washington, DC (Hagrstwn)", TVHomes = 2492170, PercentageOfUS = 2.222, MarketCode = 111 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 26, Market = "Baltimore", TVHomes = 1108010, PercentageOfUS = 0.988, MarketCode = 112 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 71, Market = "Flint-Saginaw-Bay City", TVHomes = 411210, PercentageOfUS = 0.367, MarketCode = 113 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 53, Market = "Buffalo", TVHomes = 592750, PercentageOfUS = 0.529, MarketCode = 114 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 35, Market = "Cincinnati", TVHomes = 871970, PercentageOfUS = 0.778, MarketCode = 115 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 150, Market = "Erie", TVHomes = 141020, PercentageOfUS = 0.126, MarketCode = 116 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 23, Market = "Charlotte", TVHomes = 1145270, PercentageOfUS = 1.021, MarketCode = 117 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 48, Market = "Greensboro-H.Point-W.Salem", TVHomes = 672650, PercentageOfUS = 0.6, MarketCode = 118 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 92, Market = "Charleston, SC", TVHomes = 320980, PercentageOfUS = 0.286, MarketCode = 119 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 112, Market = "Augusta-Aiken", TVHomes = 249090, PercentageOfUS = 0.222, MarketCode = 120 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 52, Market = "Providence-New Bedford", TVHomes = 597990, PercentageOfUS = 0.533, MarketCode = 121 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 127, Market = "Columbus, GA (Opelika, AL)", TVHomes = 206520, PercentageOfUS = 0.184, MarketCode = 122 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 97, Market = "Burlington-Plattsburgh", TVHomes = 294020, PercentageOfUS = 0.262, MarketCode = 123 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 9, Market = "Atlanta", TVHomes = 2449460, PercentageOfUS = 2.184, MarketCode = 124 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 154, Market = "Albany, GA", TVHomes = 134510, PercentageOfUS = 0.12, MarketCode = 125 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 171, Market = "Utica", TVHomes = 93930, PercentageOfUS = 0.084, MarketCode = 126 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 28, Market = "Indianapolis", TVHomes = 1026260, PercentageOfUS = 0.915, MarketCode = 127 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 16, Market = "Miami-Ft. Lauderdale", TVHomes = 1677850, PercentageOfUS = 1.496, MarketCode = 128 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 49, Market = "Louisville", TVHomes = 657030, PercentageOfUS = 0.586, MarketCode = 129 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 108, Market = "Tallahassee-Thomasville", TVHomes = 257570, PercentageOfUS = 0.23, MarketCode = 130 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 99, Market = "Tri-Cities, TN-VA", TVHomes = 290530, PercentageOfUS = 0.259, MarketCode = 131 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 60, Market = "Albany-Schenectady-Troy", TVHomes = 521820, PercentageOfUS = 0.465, MarketCode = 132 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 32, Market = "Hartford & New Haven", TVHomes = 921500, PercentageOfUS = 0.822, MarketCode = 133 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 18, Market = "Orlando-Daytona Bch-Melbrn", TVHomes = 1531130, PercentageOfUS = 1.365, MarketCode = 134 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 34, Market = "Columbus, OH", TVHomes = 896980, PercentageOfUS = 0.8, MarketCode = 135 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 117, Market = "Youngstown", TVHomes = 234120, PercentageOfUS = 0.209, MarketCode = 136 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 156, Market = "Bangor", TVHomes = 125970, PercentageOfUS = 0.112, MarketCode = 137 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 76, Market = "Rochester, NY", TVHomes = 384380, PercentageOfUS = 0.343, MarketCode = 138 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 13, Market = "Tampa-St. Pete (Sarasota)", TVHomes = 1879760, PercentageOfUS = 1.676, MarketCode = 139 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 118, Market = "Traverse City-Cadillac", TVHomes = 233370, PercentageOfUS = 0.208, MarketCode = 140 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 63, Market = "Lexington", TVHomes = 459300, PercentageOfUS = 0.41, MarketCode = 141 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 64, Market = "Dayton", TVHomes = 453960, PercentageOfUS = 0.405, MarketCode = 142 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 116, Market = "Springfield-Holyoke", TVHomes = 237580, PercentageOfUS = 0.212, MarketCode = 143 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 47, Market = "Norfolk-Portsmth-Newpt Nws", TVHomes = 673820, PercentageOfUS = 0.601, MarketCode = 144 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 100, Market = "Greenville-N.Bern-Washngtn", TVHomes = 285650, PercentageOfUS = 0.255, MarketCode = 145 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 77, Market = "Columbia, SC", TVHomes = 384190, PercentageOfUS = 0.343, MarketCode = 146 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 78, Market = "Toledo", TVHomes = 379120, PercentageOfUS = 0.338, MarketCode = 147 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 37, Market = "West Palm Beach-Ft. Pierce", TVHomes = 829110, PercentageOfUS = 0.739, MarketCode = 148 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 179, Market = "Watertown", TVHomes = 81630, PercentageOfUS = 0.073, MarketCode = 149 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 130, Market = "Wilmington", TVHomes = 191440, PercentageOfUS = 0.171, MarketCode = 150 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 115, Market = "Lansing", TVHomes = 238990, PercentageOfUS = 0.213, MarketCode = 151 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 206, Market = "Presque Isle", TVHomes = 25480, PercentageOfUS = 0.023, MarketCode = 152 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 180, Market = "Marquette", TVHomes = 78000, PercentageOfUS = 0.07, MarketCode = 153 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 158, Market = "Wheeling-Steubenville", TVHomes = 121320, PercentageOfUS = 0.108, MarketCode = 154 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 85, Market = "Syracuse", TVHomes = 350100, PercentageOfUS = 0.312, MarketCode = 155 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 55, Market = "Richmond-Petersburg", TVHomes = 566930, PercentageOfUS = 0.506, MarketCode = 156 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 61, Market = "Knoxville", TVHomes = 516920, PercentageOfUS = 0.461, MarketCode = 157 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 189, Market = "Lima", TVHomes = 62840, PercentageOfUS = 0.056, MarketCode = 158 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 163, Market = "Bluefield-Beckley-Oak Hill", TVHomes = 118520, PercentageOfUS = 0.106, MarketCode = 159 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 25, Market = "Raleigh-Durham (Fayetvlle)", TVHomes = 1133160, PercentageOfUS = 1.01, MarketCode = 160 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 42, Market = "Jacksonville", TVHomes = 700890, PercentageOfUS = 0.625, MarketCode = 161 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 43, Market = "Grand Rapids-Kalmzoo-B.Crk", TVHomes = 689950, PercentageOfUS = 0.615, MarketCode = 163 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 73, Market = "Charleston-Huntington", TVHomes = 406310, PercentageOfUS = 0.362, MarketCode = 164 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 176, Market = "Elmira (Corning)", TVHomes = 86230, PercentageOfUS = 0.077, MarketCode = 165 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 45, Market = "Harrisburg-Lncstr-Leb-York", TVHomes = 682460, PercentageOfUS = 0.609, MarketCode = 166 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 38, Market = "Greenvll-Spart-Ashevll-And", TVHomes = 809190, PercentageOfUS = 0.722, MarketCode = 167 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 175, Market = "Harrisonburg", TVHomes = 86700, PercentageOfUS = 0.077, MarketCode = 169 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 101, Market = "Myrtle Beach-Florence", TVHomes = 281550, PercentageOfUS = 0.251, MarketCode = 170 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 56, Market = "Ft. Myers-Naples", TVHomes = 549760, PercentageOfUS = 0.49, MarketCode = 171 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 70, Market = "Roanoke-Lynchburg", TVHomes = 414620, PercentageOfUS = 0.37, MarketCode = 173 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 107, Market = "Johnstown-Altoona-St Colge", TVHomes = 262020, PercentageOfUS = 0.234, MarketCode = 174 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 89, Market = "Chattanooga", TVHomes = 336580, PercentageOfUS = 0.3, MarketCode = 175 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 143, Market = "Salisbury", TVHomes = 155240, PercentageOfUS = 0.138, MarketCode = 176 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 57, Market = "Wilkes Barre-Scranton-Hztn", TVHomes = 523450, PercentageOfUS = 0.467, MarketCode = 177 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 155, Market = "Terre Haute", TVHomes = 127470, PercentageOfUS = 0.114, MarketCode = 181 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 188, Market = "Lafayette, IN", TVHomes = 66710, PercentageOfUS = 0.059, MarketCode = 182 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 208, Market = "Alpena", TVHomes = 15360, PercentageOfUS = 0.014, MarketCode = 183 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 183, Market = "Charlottesville", TVHomes = 72320, PercentageOfUS = 0.064, MarketCode = 184 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 96, Market = "South Bend-Elkhart", TVHomes = 297680, PercentageOfUS = 0.265, MarketCode = 188 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 159, Market = "Gainesville", TVHomes = 121060, PercentageOfUS = 0.108, MarketCode = 192 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 204, Market = "Zanesville", TVHomes = 30550, PercentageOfUS = 0.027, MarketCode = 196 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 194, Market = "Parkersburg", TVHomes = 56980, PercentageOfUS = 0.051, MarketCode = 197 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 169, Market = "Clarksburg-Weston", TVHomes = 97020, PercentageOfUS = 0.087, MarketCode = 198 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 128, Market = "Corpus Christi", TVHomes = 198820, PercentageOfUS = 0.177, MarketCode = 200 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 3, Market = "Chicago", TVHomes = 3299720, PercentageOfUS = 2.942, MarketCode = 202 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 152, Market = "Joplin-Pittsburg", TVHomes = 136740, PercentageOfUS = 0.122, MarketCode = 203 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 135, Market = "Columbia-Jefferson City", TVHomes = 163790, PercentageOfUS = 0.146, MarketCode = 204 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 139, Market = "Topeka", TVHomes = 161010, PercentageOfUS = 0.144, MarketCode = 205 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 173, Market = "Dothan", TVHomes = 92300, PercentageOfUS = 0.082, MarketCode = 206 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 21, Market = "St. Louis", TVHomes = 1189890, PercentageOfUS = 1.061, MarketCode = 209 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 138, Market = "Rockford", TVHomes = 161530, PercentageOfUS = 0.144, MarketCode = 210 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 153, Market = "Rochestr-Mason City-Austin", TVHomes = 134990, PercentageOfUS = 0.12, MarketCode = 211 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 83, Market = "Shreveport", TVHomes = 352540, PercentageOfUS = 0.314, MarketCode = 212 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 15, Market = "Minneapolis-St. Paul", TVHomes = 1730430, PercentageOfUS = 1.543, MarketCode = 213 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 33, Market = "Kansas City", TVHomes = 901020, PercentageOfUS = 0.803, MarketCode = 216 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 36, Market = "Milwaukee", TVHomes = 868500, PercentageOfUS = 0.774, MarketCode = 217 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 7, Market = "Houston", TVHomes = 2467140, PercentageOfUS = 2.2, MarketCode = 218 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 75, Market = "Springfield, MO", TVHomes = 389750, PercentageOfUS = 0.348, MarketCode = 219 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 51, Market = "New Orleans", TVHomes = 638020, PercentageOfUS = 0.569, MarketCode = 222 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 5, Market = "Dallas-Ft. Worth", TVHomes = 2648490, PercentageOfUS = 2.362, MarketCode = 223 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 148, Market = "Sioux City", TVHomes = 144180, PercentageOfUS = 0.129, MarketCode = 224 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 86, Market = "Waco-Temple-Bryan", TVHomes = 346750, PercentageOfUS = 0.309, MarketCode = 225 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 203, Market = "Victoria", TVHomes = 31380, PercentageOfUS = 0.028, MarketCode = 226 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 149, Market = "Wichita Falls & Lawton", TVHomes = 142990, PercentageOfUS = 0.127, MarketCode = 227 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 137, Market = "Monroe-El Dorado", TVHomes = 161950, PercentageOfUS = 0.144, MarketCode = 228 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 44, Market = "Birmingham (Ann and Tusc)", TVHomes = 687180, PercentageOfUS = 0.613, MarketCode = 230 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 200, Market = "Ottumwa-Kirksville", TVHomes = 42990, PercentageOfUS = 0.038, MarketCode = 231 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 82, Market = "Paducah-Cape Girard-Harsbg", TVHomes = 354790, PercentageOfUS = 0.316, MarketCode = 232 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 144, Market = "Odessa-Midland", TVHomes = 153830, PercentageOfUS = 0.137, MarketCode = 233 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 131, Market = "Amarillo", TVHomes = 179920, PercentageOfUS = 0.16, MarketCode = 234 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 39, Market = "Austin", TVHomes = 791480, PercentageOfUS = 0.706, MarketCode = 235 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 84, Market = "Harlingen-Wslco-Brnsvl-Mca", TVHomes = 351810, PercentageOfUS = 0.314, MarketCode = 236 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 91, Market = "Cedar Rapids-Wtrlo-IWC&Dub", TVHomes = 325780, PercentageOfUS = 0.29, MarketCode = 237 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 201, Market = "St. Joseph", TVHomes = 42230, PercentageOfUS = 0.038, MarketCode = 238 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 177, Market = "Jackson, TN", TVHomes = 85540, PercentageOfUS = 0.076, MarketCode = 239 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 50, Market = "Memphis", TVHomes = 649360, PercentageOfUS = 0.579, MarketCode = 240 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 31, Market = "San Antonio", TVHomes = 924480, PercentageOfUS = 0.824, MarketCode = 241 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 121, Market = "Lafayette, LA", TVHomes = 222450, PercentageOfUS = 0.198, MarketCode = 242 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 174, Market = "Lake Charles", TVHomes = 91490, PercentageOfUS = 0.082, MarketCode = 243 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 178, Market = "Alexandria, LA", TVHomes = 82270, PercentageOfUS = 0.073, MarketCode = 244 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 193, Market = "Greenwood-Greenville", TVHomes = 58410, PercentageOfUS = 0.052, MarketCode = 247 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 88, Market = "Champaign&Sprngfld-Decatur", TVHomes = 344180, PercentageOfUS = 0.307, MarketCode = 248 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 103, Market = "Evansville", TVHomes = 264890, PercentageOfUS = 0.236, MarketCode = 249 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 41, Market = "Oklahoma City", TVHomes = 705840, PercentageOfUS = 0.629, MarketCode = 250 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 145, Market = "Lubbock", TVHomes = 153370, PercentageOfUS = 0.137, MarketCode = 251 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 74, Market = "Omaha", TVHomes = 399010, PercentageOfUS = 0.356, MarketCode = 252 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 151, Market = "Panama City", TVHomes = 137830, PercentageOfUS = 0.123, MarketCode = 256 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 160, Market = "Sherman-Ada", TVHomes = 120100, PercentageOfUS = 0.107, MarketCode = 257 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 69, Market = "Green Bay-Appleton", TVHomes = 415890, PercentageOfUS = 0.371, MarketCode = 258 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 27, Market = "Nashville", TVHomes = 1030650, PercentageOfUS = 0.919, MarketCode = 259 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 196, Market = "San Angelo", TVHomes = 54100, PercentageOfUS = 0.048, MarketCode = 261 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 165, Market = "Abilene-Sweetwater", TVHomes = 107760, PercentageOfUS = 0.096, MarketCode = 262 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 81, Market = "Madison", TVHomes = 366690, PercentageOfUS = 0.327, MarketCode = 269 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 98, Market = "Ft. Smith-Fay-Sprngdl-Rgrs", TVHomes = 292160, PercentageOfUS = 0.261, MarketCode = 270 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 62, Market = "Tulsa", TVHomes = 516540, PercentageOfUS = 0.461, MarketCode = 271 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 133, Market = "Columbus-Tupelo-W Pnt-Hstn", TVHomes = 172520, PercentageOfUS = 0.154, MarketCode = 273 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 122, Market = "Peoria-Bloomington", TVHomes = 222210, PercentageOfUS = 0.198, MarketCode = 275 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 140, Market = "Duluth-Superior", TVHomes = 157070, PercentageOfUS = 0.14, MarketCode = 276 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 67, Market = "Wichita-Hutchinson Plus", TVHomes = 416400, PercentageOfUS = 0.371, MarketCode = 278 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 68, Market = "Des Moines-Ames", TVHomes = 416020, PercentageOfUS = 0.371, MarketCode = 279 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 102, Market = "Davenport-R.Island-Moline", TVHomes = 277950, PercentageOfUS = 0.248, MarketCode = 282 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 59, Market = "Mobile-Pensacola (Ft Walt)", TVHomes = 522260, PercentageOfUS = 0.466, MarketCode = 286 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 141, Market = "Minot-Bsmrck-Dcknsn(Wlstn)", TVHomes = 156240, PercentageOfUS = 0.139, MarketCode = 287 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 80, Market = "Huntsville-Decatur (Flor)", TVHomes = 367510, PercentageOfUS = 0.328, MarketCode = 291 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 142, Market = "Beaumont-Port Arthur", TVHomes = 156020, PercentageOfUS = 0.139, MarketCode = 292 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 58, Market = "Little Rock-Pine Bluff", TVHomes = 522530, PercentageOfUS = 0.466, MarketCode = 293 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 124, Market = "Montgomery-Selma", TVHomes = 218740, PercentageOfUS = 0.195, MarketCode = 298 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 129, Market = "La Crosse-Eau Claire", TVHomes = 196160, PercentageOfUS = 0.175, MarketCode = 302 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 134, Market = "Wausau-Rhinelander", TVHomes = 166030, PercentageOfUS = 0.148, MarketCode = 305 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 109, Market = "Tyler-Longview(Lfkn&Ncgd)", TVHomes = 253230, PercentageOfUS = 0.226, MarketCode = 309 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 168, Market = "Hattiesburg-Laurel", TVHomes = 102840, PercentageOfUS = 0.092, MarketCode = 310 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 191, Market = "Meridian", TVHomes = 61460, PercentageOfUS = 0.055, MarketCode = 311 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 94, Market = "Baton Rouge", TVHomes = 314970, PercentageOfUS = 0.281, MarketCode = 316 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 172, Market = "Quincy-Hannibal-Keokuk", TVHomes = 93920, PercentageOfUS = 0.084, MarketCode = 317 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 95, Market = "Jackson, MS", TVHomes = 306410, PercentageOfUS = 0.273, MarketCode = 318 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 106, Market = "Lincoln & Hastings-Krny", TVHomes = 263110, PercentageOfUS = 0.235, MarketCode = 322 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 113, Market = "Fargo-Valley City", TVHomes = 240560, PercentageOfUS = 0.214, MarketCode = 324 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 110, Market = "Sioux Falls(Mitchell)", TVHomes = 252660, PercentageOfUS = 0.225, MarketCode = 325 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 182, Market = "Jonesboro", TVHomes = 76860, PercentageOfUS = 0.069, MarketCode = 334 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 181, Market = "Bowling Green", TVHomes = 77360, PercentageOfUS = 0.069, MarketCode = 336 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 199, Market = "Mankato", TVHomes = 49610, PercentageOfUS = 0.044, MarketCode = 337 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 209, Market = "North Platte", TVHomes = 13640, PercentageOfUS = 0.012, MarketCode = 340 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 147, Market = "Anchorage", TVHomes = 149120, PercentageOfUS = 0.133, MarketCode = 343 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 66, Market = "Honolulu", TVHomes = 419540, PercentageOfUS = 0.374, MarketCode = 344 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 202, Market = "Fairbanks", TVHomes = 35180, PercentageOfUS = 0.031, MarketCode = 345 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 157, Market = "Biloxi-Gulfport", TVHomes = 124130, PercentageOfUS = 0.111, MarketCode = 346 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 207, Market = "Juneau", TVHomes = 24390, PercentageOfUS = 0.022, MarketCode = 347 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 184, Market = "Laredo", TVHomes = 70980, PercentageOfUS = 0.063, MarketCode = 349 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 17, Market = "Denver", TVHomes = 1589560, PercentageOfUS = 1.417, MarketCode = 351 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 87, Market = "Colorado Springs-Pueblo", TVHomes = 344250, PercentageOfUS = 0.307, MarketCode = 352 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 11, Market = "Phoenix (Prescott)", TVHomes = 1919930, PercentageOfUS = 1.712, MarketCode = 353 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 185, Market = "Butte-Bozeman", TVHomes = 69060, PercentageOfUS = 0.062, MarketCode = 354 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 192, Market = "Great Falls", TVHomes = 60220, PercentageOfUS = 0.054, MarketCode = 355 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 167, Market = "Billings", TVHomes = 105470, PercentageOfUS = 0.094, MarketCode = 356 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 104, Market = "Boise", TVHomes = 264300, PercentageOfUS = 0.236, MarketCode = 357 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 162, Market = "Idaho Fals-Pocatllo(Jcksn)", TVHomes = 119590, PercentageOfUS = 0.107, MarketCode = 358 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 197, Market = "Cheyenne-Scottsbluf", TVHomes = 53720, PercentageOfUS = 0.048, MarketCode = 359 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 190, Market = "Twin Falls", TVHomes = 62360, PercentageOfUS = 0.056, MarketCode = 360 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 164, Market = "Missoula", TVHomes = 113110, PercentageOfUS = 0.101, MarketCode = 362 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 170, Market = "Rapid City", TVHomes = 95320, PercentageOfUS = 0.085, MarketCode = 364 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 93, Market = "El Paso (Las Cruces)", TVHomes = 318260, PercentageOfUS = 0.284, MarketCode = 365 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 205, Market = "Helena", TVHomes = 27430, PercentageOfUS = 0.024, MarketCode = 366 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 198, Market = "Casper-Riverton", TVHomes = 52190, PercentageOfUS = 0.047, MarketCode = 367 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 30, Market = "Salt Lake City", TVHomes = 948840, PercentageOfUS = 0.846, MarketCode = 370 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 166, Market = "Yuma-El Centro", TVHomes = 105690, PercentageOfUS = 0.094, MarketCode = 371 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 187, Market = "Grand Junction-Montrose", TVHomes = 67150, PercentageOfUS = 0.06, MarketCode = 373 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 65, Market = "Tucson (Sierra Vista)", TVHomes = 433330, PercentageOfUS = 0.386, MarketCode = 389 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 46, Market = "Albuquerque-Santa Fe", TVHomes = 674930, PercentageOfUS = 0.602, MarketCode = 390 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 210, Market = "Glendive", TVHomes = 4030, PercentageOfUS = 0.004, MarketCode = 398 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 126, Market = "Bakersfield", TVHomes = 212180, PercentageOfUS = 0.189, MarketCode = 400 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 119, Market = "Eugene", TVHomes = 231570, PercentageOfUS = 0.206, MarketCode = 401 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 195, Market = "Eureka", TVHomes = 56660, PercentageOfUS = 0.051, MarketCode = 402 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 2, Market = "Los Angeles", TVHomes = 5318630, PercentageOfUS = 4.743, MarketCode = 403 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 146, Market = "Palm Springs", TVHomes = 152840, PercentageOfUS = 0.136, MarketCode = 404 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 8, Market = "San Francisco-Oak-San Jose", TVHomes = 2451640, PercentageOfUS = 2.186, MarketCode = 407 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 114, Market = "Yakima-Pasco-Rchlnd-Knnwck", TVHomes = 239760, PercentageOfUS = 0.214, MarketCode = 410 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 105, Market = "Reno", TVHomes = 263990, PercentageOfUS = 0.235, MarketCode = 411 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 136, Market = "Medford-Klamath Falls", TVHomes = 163600, PercentageOfUS = 0.146, MarketCode = 413 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 12, Market = "Seattle-Tacoma", TVHomes = 1880750, PercentageOfUS = 1.677, MarketCode = 419 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 22, Market = "Portland, OR", TVHomes = 1180980, PercentageOfUS = 1.053, MarketCode = 420 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 186, Market = "Bend, OR", TVHomes = 67170, PercentageOfUS = 0.06, MarketCode = 421 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 29, Market = "San Diego", TVHomes = 1002770, PercentageOfUS = 0.894, MarketCode = 425 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 125, Market = "Monterey-Salinas", TVHomes = 217560, PercentageOfUS = 0.194, MarketCode = 428 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 40, Market = "Las Vegas", TVHomes = 757400, PercentageOfUS = 0.675, MarketCode = 439 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 123, Market = "Santabarbra-Sanmar-Sanluob", TVHomes = 222190, PercentageOfUS = 0.198, MarketCode = 455 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 20, Market = "Sacramnto-Stkton-Modesto", TVHomes = 1412940, PercentageOfUS = 1.26, MarketCode = 462 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 54, Market = "Fresno-Visalia", TVHomes = 574610, PercentageOfUS = 0.512, MarketCode = 466 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 132, Market = "Chico-Redding", TVHomes = 179370, PercentageOfUS = 0.16, MarketCode = 468 },
                new MarketCoverage { MarketCoverageFileId = 1, Rank = 72, Market = "Spokane", TVHomes = 410900, PercentageOfUS = 0.366, MarketCode = 481 },
            };

            #endregion // #region BigList - Market Coverages

            return marketCoverages;
        }

        public static MarketCoverageFile GetLatestMarketCoverageFile()
        {
            return new MarketCoverageFile
            {
                Id = 1
            };
        }
    }
}
