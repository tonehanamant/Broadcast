using Common.Services;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.spotcableXML;
using System;
using System.Collections.Generic;
using System.Linq;
using static Services.Broadcast.Entities.Scx.ScxMarketDto;

namespace Services.Broadcast.Converters.Scx
{
    public class ScxBaseConverter
    {
        protected const string HardCodedAgencyCode = "309";
        protected const string Currency = "USD";
        protected const string NccMarketKeyValue = "216";  // use this value for some reason.
        protected const string DocCode = "1";
        protected const string DocType = "Order";

        protected readonly IDaypartCache _DaypartCache;

        public ScxBaseConverter(IDaypartCache daypartCache)
        {
            _DaypartCache = daypartCache;
        }

        /// <summary>
        /// Creates an adx object from the ScxData object
        /// </summary>
        /// <param name="data">ScxData object to be converted</param>
        /// <returns>adx object</returns>
        public adx CreateAdxObject(ScxData data)
        {
            adx xp = new adx
            {
                document = new document()
            };

            _SetDocumentParts(xp);
            var camp = new campaign();
            xp.campaign = camp;

            _SetScxKeys(data, camp);
            _SetScxDateRange(data, camp);
            _SetScxCompanies(camp);
            _SetScxAdvertiser(camp, data.AdvertisersName);
            _SetScxProduct(camp);
            _SetScxEstimate(camp);
            _SetScxMakeGoodPolicy(camp);
            _SetScxDemographics(data, camp);
            _SetScxOrders(data, camp);

            return xp;
        }

        private void _SetScxOrders(ScxData data, campaign camp)
        {
            int orderIndex = 0;

            camp.order = new order[data.MarketIds.Count];

            foreach (var marketId in data.MarketIds)
            {   // for each market with slots create campaign.order

                var scxOrder = new order();
                if (!_SetOrderTotal(scxOrder, data, marketId))
                    continue;

                _SetScxOrderKeys(scxOrder, marketId);

                var market = data.InventoryMarkets.Where(x => x.MarketId == marketId).Single();
                scxOrder.market = new market()
                {
                    nsi_id = market.MarketId ?? 0,
                    name = market.MarketId.HasValue? market.DmaMarketName : null
                };

                _SetScxOrderSurvey(scxOrder, marketId, data);
                _SetOrderPopulations(scxOrder, data.Demos, market.MarketId);
                scxOrder.comment = string.Empty;
                var stations = market.Stations;
                if (stations.Any())
                {
                    scxOrder.systemOrder = new systemOrder[stations.Count];
                    int sysOrderIndex = 0;
                    foreach (var station in stations)
                    {   // for each station campaign.order.systemOrder

                        var sysOrder = _SetSystemOrders(data, station, market);
                        scxOrder.systemOrder[sysOrderIndex++] = sysOrder;
                    }
                }
                camp.order[orderIndex] = scxOrder;
                orderIndex++;
            }
        }

        private static void _SetDocumentParts(adx xp)
        {
            xp.document.date = DateTime.Today;
            xp.document.mediaType = documentMediaType.Spotcable;
            xp.document.schemaVersion = "1.0";
            xp.document.name = " ";
            xp.document.documentCode = DocCode;
            xp.document.documentType = DocType;
        }

        private systemOrder _SetSystemOrders(ScxData data,
                                        ScxStation station,
                                        ScxMarketDto market)
        {   // for each station
            systemOrder sysOrder = new systemOrder();
            if (!_SetSystemOrderTotals(sysOrder, data, market.MarketId))
                return null;    // no order so leave

            _SetScxSystemOrderKeys(sysOrder);
            _SetSystemOrderPopulations(sysOrder, data.Demos, market.MarketId);
            _SetSystemOrderWeekInfo(sysOrder, data.WeekData);
            sysOrder.comment = "OK";
            sysOrder.system = new system[]
            {
                new system() {name = String.Empty, syscode = String.Empty},
            };

            int detailIndex = 0;
            detailLine[] detLines = new detailLine[1];

            foreach (var program in station.Programs)
            {
                if (!_ProgramHasSpots(market.MarketId, station, data.WeekData, program))
                    continue;
                var detLine = new detailLine();
                Array.Resize(ref detLines, detailIndex + 1);

                detLine.program = program.ProgramNames.Single();
                _SetDaypartInfo(detLine, program, data.DaypartCode);
                detLine.length = string.Format("PT{0}S", data.SpotLength);
                detLine.comment = " ";

                _SetDetailLineNetworkInfo(detLine, station);
                _SetDetailLineDemoValue(detLine, data, station.LegacyCallLetters, program);
                _SetDetailLineTotalsAndCost(detLine, market.MarketId, station, data.WeekData, program);
                _SetSpotWeekQuantities(market.MarketId, station, data.WeekData, program, detLine);

                detLines[detailIndex++] = detLine;
            }

            if (detLines[0] != null)
                sysOrder.detailLine = detLines;

            return sysOrder;
        }

        private bool _ProgramHasSpots(int? marketId
            , ScxStation station
            , List<ScxMarketStationProgramSpotWeek> weeks
            , ScxProgram program)
        {
            var spots = weeks.Where(w => w.InventoryWeek != null)
                            .SelectMany(w => w.InventoryWeek.Markets)
                            .Where(m => m.MarketId == marketId)
                            .SelectMany(m => m.Stations)
                            .Where(s => s.StationCode == station.StationCode)
                            .SelectMany(s => s.Programs.Where(p => p != null && p.ProgramId == program.ProgramId))
                            .Sum(p => p.Spots);
            return spots > 0;
        }

        private static void _SetSpotWeekQuantities(int? marketId
                                                    , ScxStation station
                                                    , List<ScxMarketStationProgramSpotWeek> weeks
                                                    , ScxProgram program
                                                    , detailLine detLine)
        {
            detLine.spot = new spot[weeks.Count];

            int spotWeekIndex = 0;
            foreach (var week in weeks)
            {
                var spotWeek = new spot
                {
                    weekNumber = week.WeekNumber.ToString()
                };
                int spots = 0;
                if (week.InventoryWeek != null)
                {
                    spots = week.InventoryWeek.Markets
                        .Where(m => m.MarketId == marketId)
                        .SelectMany(m => m.Stations)
                        .Where(s => s.StationCode == station.StationCode)
                        .SelectMany(s => s.Programs.Where(p => p != null && p.ProgramId == program.ProgramId))
                        .Sum(p => p.Spots);
                }
                spotWeek.quantity = spots.ToString();

                detLine.spot[spotWeekIndex++] = spotWeek;
            }
        }

        private static void _SetOrderPopulations(order scxOrder, List<DemoData> demos, int? marketId)
        {
            if (demos == null)
            {
                scxOrder.populations = new populations[0];
                return;
            }
            scxOrder.populations = new populations[demos.Count];
            int populationIndexer = 0;
            foreach (var demo in demos)
            {
                var population = new populations();
                population.demoRank = demo.DemoRank;
                if (demo.MarketPopulations.TryGetValue((short)marketId, out double marketPopulation))
                    population.Value = marketPopulation.ToString();
                scxOrder.populations[populationIndexer] = population;
                populationIndexer++;
            }
        }

        private void _SetDetailLineTotalsAndCost(detailLine detLine
                                , int? marketId
                                , ScxStation station
                                , List<ScxMarketStationProgramSpotWeek> weeks
                                , ScxProgram program)
        {
            var totals = new totals();

            var items = weeks.Where(w => w.InventoryWeek != null)
                            .SelectMany(w => w.InventoryWeek.Markets)
                            .Where(m => m.MarketId == marketId)
                            .SelectMany(m => m.Stations)
                            .SelectMany(s => s.Programs)
                            .Where(p => p != null && p.Spots > 0 && p.ProgramId == program.ProgramId).ToList();

            totals.spots = items.Sum(i => i.Spots).ToString();
            totals.cost = new cost
            {
                currency = Currency,
                Value = items.Sum(i => i.Cost)
            };

            detLine.totals = totals;

            detLine.spotCost = new spotCost
            {
                currency = Currency,
                Value = items.First().UnitCost
            };
        }

        private void _SetDaypartInfo(detailLine detLine, ScxProgram program, string daypartCode)
        {
            detLine.startDay = detailLineStartDay.M;
            var daypart = _DaypartCache.GetDisplayDaypart(program.Dayparts.Single().Id);
            DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, daypart.StartTime, 0));
            detLine.startTime = lStartTime;
            DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, daypart.EndTime + 1, 0)); // add one second to bring the time up to full hour/half-hour/etc
            detLine.endTime = lEndTime;
            detLine.dayOfWeek = new dayOfWeek
            {
                Monday = daypart.Monday ? "Y" : "N",
                Tuesday = daypart.Tuesday ? "Y" : "N",
                Wednesday = daypart.Wednesday ? "Y" : "N",
                Thursday = daypart.Thursday ? "Y" : "N",
                Friday = daypart.Friday ? "Y" : "N",
                Saturday = daypart.Saturday ? "Y" : "N",
                Sunday = daypart.Sunday ? "Y" : "N"
            };

            detLine.daypartCode = daypartCode;
        }

        private void _SetDetailLineDemoValue(detailLine detLine, ScxData data, string legacyCallLetters, ScxProgram programInfo)
        {
            if (data.Demos == null)
            {
                detLine.demoValue = new demoValue[0];
                return;
            }
            detLine.demoValue = new demoValue[data.Demos.Count];
            int demoValueIndex = 0;
            foreach (var demo in data.Demos)
            {
                var demoValue = new demoValue();
                demoValue.demoRank = demo.DemoRank.ToString();
                demoValue.value = new demoValueValue[2];

                Ratingdata ratingValue;
                var ratingDisplay = string.Empty;
                if (demo.Ratings.Any())
                {
                    ratingValue = demo.Ratings.Single(r => programInfo.Dayparts.Any(dp => dp.Id == r.DaypartId) && r.LegacyCallLetters == legacyCallLetters);
                    ratingDisplay = string.Format("{0:#0.00}", ratingValue.Rating);
                }

                var impressions = programInfo.ProvidedUnitImpressions ?? demo.Impressions.SingleOrDefault(i => i.Id == programInfo.ProgramId)?.Impressions;
                var impressionsDisplay = string.Empty;

                if (impressions.HasValue)
                {
                    impressionsDisplay = string.Format("{0:####}", impressions);
                }

                demoValue.value[0] = new demoValueValue() { type = demoValueValueType.Ratings, Value = ratingDisplay, typeSpecified = true };
                demoValue.value[1] = new demoValueValue() { type = demoValueValueType.Impressions, Value = impressionsDisplay, typeSpecified = true };

                detLine.demoValue[demoValueIndex++] = demoValue;
            }
        }

        private void _SetDetailLineNetworkInfo(detailLine detLine,
                                    ScxStation station)
        {
            detLine.network = new network[1];
            var network = new network()
            {
                name = station.LegacyCallLetters + "-TV",
                ID = new codeComplexType[3]
                {
                    new codeComplexType() {code = new codeComplexTypeCode() { codeOwner = "Spotcable" ,codeDescription = station.StationCode.ToString() , Value=station.LegacyCallLetters} },
                    new codeComplexType() {code = new codeComplexTypeCode() { codeOwner = "Strata" ,codeDescription = "Station",Value=station.LegacyCallLetters + "-TV"} },
                    new codeComplexType() {code = new codeComplexTypeCode() { codeOwner = "Strata" ,codeDescription = "Band", Value="TV"} },
                }
            };
            detLine.network = new network[1];
            detLine.network[0] = network;
        }

        private static void _SetSystemOrderPopulations(systemOrder sysOrder, List<DemoData> demos, int? marketId)
        {
            if (demos == null)
            {
                sysOrder.populations = new populations[0];
                return;
            }
            sysOrder.populations = new populations[demos.Count];

            int populationIndexer = 0;
            foreach (var demo in demos)
            {
                var population = new populations
                {
                    demoRank = demo.DemoRank
                };
                if (demo.MarketPopulations.TryGetValue((short)marketId, out double marketPopulation))
                    population.Value = marketPopulation.ToString();
                sysOrder.populations[populationIndexer] = population;
                populationIndexer++;
            }
        }

        private static void _SetScxOrderSurvey(order scxOrder, int? marketId, ScxData data)
        {
            scxOrder.survey = new survey() { comment = new surveyComment[1] };
            scxOrder.survey.comment[0] = new surveyComment
            {
                codeOwner = "Spotcable",
                Value = marketId.HasValue
                            ? data.SurveyData.Any() ? data.SurveyData[marketId.Value] : string.Empty
                            : string.Empty
            };
            scxOrder.survey.ratingService = string.Empty;
            scxOrder.survey.geography = string.Empty;
            scxOrder.survey.shareBook = string.Empty;
            scxOrder.survey.PUTBook = string.Empty;
            scxOrder.survey.profile = string.Empty;
        }

        private static void _SetSystemOrderWeekInfo(systemOrder order, List<ScxMarketStationProgramSpotWeek> weekData)
        {
            order.weeks = new weeks();
            order.weeks.count = weekData.Count.ToString();
            order.weeks.week = new week[weekData.Count];

            int weekIndex = 0;
            weekData.ForEach(wd =>
                        order.weeks.week[weekIndex++] =
                        new week()
                        {
                            number = wd.WeekNumber.ToString(),
                            startDate = wd.StartDate

                        });
        }

        private static bool _SetSystemOrderTotals(systemOrder sysOrder, ScxData data, int? marketId)
        {
            var programSpots = data.WeekData.Where(w => w.InventoryWeek != null)
                .SelectMany(w => w.InventoryWeek.Markets)
                .Where(m => m.MarketId == marketId)
                .SelectMany(m => m.Stations)
                .SelectMany(s => s.Programs)
                .Where(p => p != null && p.Spots > 0).ToList();

            if (!programSpots.Any())
                return false;

            int totalSpots = programSpots.Sum(p => p.Spots);

            decimal totalCost = programSpots.Sum(p => p.Cost);
            sysOrder.totals = new totals();
            sysOrder.totals.cost = new cost();
            sysOrder.totals.cost.currency = Currency;
            sysOrder.totals.cost.Value = totalCost;

            sysOrder.totals.spots = totalSpots.ToString();
            return true;
        }

        private static bool _SetOrderTotal(order order, ScxData data, int? marketId)
        {
            var programSpots = data.WeekData.Where(w => w.InventoryWeek != null)
                .SelectMany(w => w.InventoryWeek.Markets)
                .Where(m => m.MarketId == marketId)
                .SelectMany(m => m.Stations)
                .SelectMany(s => s.Programs)
                .Where(p => p != null && p.Spots > 0).ToList();

            if (!programSpots.Any())
                return false;

            int totalSpots = programSpots.Sum(p => p.Spots);

            order.totals = new totals
            {
                cost = new cost
                {
                    currency = Currency,
                    Value = programSpots.Sum(p => p.Cost)
                },
                spots = totalSpots.ToString()
            };

            return true;
        }

        private static void _SetScxSystemOrderKeys(systemOrder order)
        {
            order.key = new key[3];
            var k = new key() { codeOwner = "Strata", codeDescription = "UseZonePop", id = "false" };
            order.key[0] = k;
        }

        private static void _SetScxOrderKeys(order order, int? marketId)
        {
            order.key = new key[3];
            var k = new key() { codeOwner = "NCC", codeDescription = "Market", id = marketId.HasValue ? marketId.ToString() : string.Empty };
            order.key[0] = k;
            k = new key() { codeOwner = "Strata", codeDescription = "Pops", id = "dma;book" };
            order.key[1] = k;
            k = new key() { codeOwner = "Strata", codeDescription = "UseBroadcastWeeks", id = "0" };
            order.key[2] = k;
        }

        private void _SetScxDemographics(ScxData data, campaign camp)
        {
            if (data.Demos == null) return; //demos for inventory scx file are not processed for the moment

            camp.demo = new demo[data.Demos.Count];
            camp.populations = new populations[data.Demos.Count];

            int demoIndex = 0;
            foreach (var demo in data.Demos)
            {
                var scxDemo = new demo
                {
                    ageFrom = demo.Demo.RangeStart.Value.ToString(),
                    ageTo = demo.Demo.RangeEnd.Value.ToString(),
                    demoRank = demo.DemoRank,
                    group = _GetGroupFromAudience(demo)
                };

                camp.demo[demoIndex] = scxDemo;

                var population = new populations
                {
                    demoRank = demo.DemoRank,
                    Value = demo.MarketPopulations[(short)data.MarketIds.First()].ToString()
                };
                camp.populations[demoIndex++] = population;
            }
        }

        private demoNameComplexTypeGroup _GetGroupFromAudience(DemoData demo)
        {
            if (demo.Demo.Name.Contains("House Hold"))
                return demoNameComplexTypeGroup.Households;
            if (demo.Demo.Name.Contains("Adult"))
                return demoNameComplexTypeGroup.Adults;
            if (demo.Demo.Name.StartsWith("Male"))
                return demoNameComplexTypeGroup.Men;            
            if (demo.Demo.Name.StartsWith("Women"))
                return demoNameComplexTypeGroup.Women;
            if (demo.Demo.Name.Contains("Children"))
                return demoNameComplexTypeGroup.Children;
            if (demo.Demo.Name.StartsWith("Person"))
                return demoNameComplexTypeGroup.Persons;

            throw new Exception ($"Unknown demo group {demo.Demo.Name}");
        }

        private void _SetScxEstimate(campaign camp)
        {
            var estimate = new estimate
            {
                //estimate.desc= string.Empty;
                ID = new codeComplexType()
            };
            estimate.ID.code = new codeComplexTypeCode() { codeOwner = "Agency", Value = HardCodedAgencyCode };
            estimate.desc = string.Empty;
            camp.estimate = estimate;
        }

        private void _SetScxMakeGoodPolicy(campaign camp)
        {
            camp.makeGoodPolicy = new codeComplexType[1]
            {
                new codeComplexType
                {
                    code = new codeComplexTypeCode()
                }
            };
        }

        private void _SetScxProduct(campaign camp)
        {
            var product = new product
            {
                name = string.Empty,
                ID = new codeComplexType[1]
            };
            product.ID[0] = new codeComplexType
            {
                code = new codeComplexTypeCode() { codeOwner = "Agency", Value = string.Empty }
            };
            camp.product = product;
        }

        private void _SetScxAdvertiser(campaign camp, string advertiserName)
        {
            var adv = new advertiser()
            {
                name = advertiserName ?? string.Empty,
                ID = new codeComplexType[1]
            };
            adv.ID[0] = new codeComplexType
            {
                code = new codeComplexTypeCode() { codeOwner = "Agency", Value = string.Empty }
            };
            camp.advertiser = adv;
        }

        private static void _SetScxCompanies(campaign camp)
        {
            var comps = new company[2];
            comps[0] = new company() { type = companyType.Rep, typeSpecified = true };
            comps[0].name = " ";
            comps[0].office = new office[1];
            comps[0].office[0] = new office
            {
                name = " ",
                address = new address
                {
                    street = string.Empty,
                    city = string.Empty,
                    state = new state() { code = "__" },
                    postalCode = "00000"
                }
            };
            comps[0].office[0].phone = new phone[1];
            comps[0].office[0].phone[0] = new phone() { type = phoneType.voice };
            comps[0].contact = new contact[1];
            comps[0].contact[0] = new contact() { role = "AE", firstName = string.Empty, lastName = string.Empty, email = new string[1] };
            comps[0].contact[0].email[0] = string.Empty;
            comps[0].contact[0].phone = new phone[1];
            comps[0].contact[0].phone[0] = new phone() { type = phoneType.voice, Value = string.Empty };

            comps[1] = new company
            {
                type = companyType.Agency,
                typeSpecified = true,
                name = " ",
                contact = new contact[1]
            };
            comps[1].contact[0] = new contact() { role = "Buyer", firstName = string.Empty, lastName = string.Empty, email = new string[1] };
            comps[1].contact[0].email[0] = string.Empty;
            comps[1].contact[0].phone = new phone[1];
            comps[1].contact[0].phone[0] = new phone() { type = phoneType.voice, Value = string.Empty };
            comps[1].ID = new codeComplexType[1];
            comps[1].ID[0] = new codeComplexType() { code = new codeComplexTypeCode() { codeOwner = "Agency", Value = string.Empty } };

            camp.company = comps;
        }

        private static void _SetScxDateRange(ScxData data, campaign camp)
        {
            camp.dateRange = new dateRange
            {
                startDate = data.StartDate,
                endDate = data.EndDate
            };
        }

        private static void _SetScxKeys(ScxData data, campaign camp)
        {
            camp.key = new key[4];
            var k = new key() { codeOwner = "NCC", codeDescription = "CampaignID", id = string.Empty };
            camp.key[0] = k;
            k = new key() { codeOwner = "Strata", codeDescription = "DMA Override", id = "0" };
            camp.key[1] = k;
            k = new key() { codeOwner = "Strata", codeDescription = "Zone Pops", id = "max" };
            camp.key[2] = k;
            k = new key() { codeOwner = "VIEW32", codeDescription = "CampaignName", id = string.Empty };
            camp.key[3] = k;
        }
    }
}
