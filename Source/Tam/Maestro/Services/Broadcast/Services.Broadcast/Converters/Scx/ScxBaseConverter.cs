using Common.Services;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.spotcableXML;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using static Services.Broadcast.Entities.Scx.ScxMarketDto;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation.ScxProgram;

namespace Services.Broadcast.Converters.Scx
{
    public class ScxBaseConverter
    {
        protected const string HardCodedAgencyCode = "309";
        protected const string Currency = "USD";
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
            camp.order = new order[data.MarketIds.Count];

            // for each market with slots create campaign.order
            for (var orderIndex = 0; orderIndex < data.MarketIds.Count; orderIndex++)
            {
                var marketId = data.MarketIds[orderIndex];
                var market = data.InventoryMarkets.Single(x => x.MarketId == marketId);
                var scxOrder = new order();

                if (!_SetOrderTotal(scxOrder, market))
                    continue;

                _SetScxOrderKeys(scxOrder, marketId);
                
                scxOrder.market = new market()
                {
                    nsi_id = marketId,
                    name = market.DmaMarketName
                };

                _SetScxOrderSurvey(scxOrder, marketId, data);
                _SetOrderPopulations(scxOrder, data.Demos, marketId);
                scxOrder.comment = string.Empty;

                var stations = market.Stations;

                if (stations.Any())
                {
                    scxOrder.systemOrder = stations
                        .Select(station => _GetSystemOrders(data, station, marketId))
                        .Where(x => x != null)
                        .ToArray();
                }

                camp.order[orderIndex] = scxOrder;
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

        private systemOrder _GetSystemOrders(ScxData data,
                                             ScxStation station,
                                             int marketId)
        {   
            systemOrder sysOrder = new systemOrder();

            if (!_SetSystemOrderTotals(sysOrder, station))
                return null;

            _SetScxSystemOrderKeys(sysOrder);
            _SetSystemOrderPopulations(sysOrder, data.Demos, marketId);
            _SetSystemOrderWeekInfo(sysOrder, data.AllSortedMediaWeeks);

            sysOrder.comment = "OK";
            sysOrder.system = new system[]
            {
                new system() { name = String.Empty, syscode = String.Empty },
            };

            var detLines = new List<detailLine>();

            foreach (var program in station.Programs)
            {
                if (!_ProgramHasSpots(program))
                    continue;

                var detLine = new detailLine
                {
                    program = program.ProgramNames.First()
                };

                _SetDaypartInfo(detLine, program, data.DaypartCode);

                detLine.length = $"PT{data.SpotLength}S";
                detLine.comment = " ";

                _SetDetailLineNetworkInfo(detLine, station);
                _SetDetailLineDemoValue(detLine, data, station.LegacyCallLetters, program);
                _SetDetailLineTotalsAndCost(detLine, program);
                _SetSpotWeekQuantities(data.AllSortedMediaWeeks, program, detLine);

                detLines.Add(detLine);
            }

            if (detLines.Any())
                sysOrder.detailLine = detLines.ToArray();

            return sysOrder;
        }

        private bool _ProgramHasSpots(ScxProgram program)
        {
            return program.Weeks.Sum(x => x.Spots) > 0;
        }

        private static void _SetSpotWeekQuantities(IOrderedEnumerable<MediaWeek> weeks, ScxProgram program, detailLine detLine)
        {
            detLine.spot = weeks.Select((week, index) => new spot
            {
                weekNumber = (index + 1).ToString(),
                quantity = program.Weeks.Where(x => x.MediaWeek.Id == week.Id).Sum(x => x.Spots).ToString()
            }).ToArray();
        }

        private static void _SetOrderPopulations(order scxOrder, List<DemoData> demos, int marketId)
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
                var population = new populations
                {
                    demoRank = demo.DemoRank
                };

                if (demo.MarketPopulations.TryGetValue((short)marketId, out double marketPopulation))
                    population.Value = marketPopulation.ToString();

                scxOrder.populations[populationIndexer] = population;
                populationIndexer++;
            }
        }

        private void _SetDetailLineTotalsAndCost(detailLine detLine, ScxProgram program)
        {
            var weeksWithSpots = program.Weeks.Where(x => x.Spots > 0).ToList();

            detLine.totals = _GetTotalsFromWeeks(weeksWithSpots);

            detLine.spotCost = new spotCost
            {
                currency = Currency,
                //Value = weeksWithSpots.First().UnitCost // add the logic in the future story
            };
        }

        private void _SetDaypartInfo(detailLine detLine, ScxProgram program, string daypartCode)
        {
            detLine.startDay = detailLineStartDay.M;
            var daypart = _DaypartCache.GetDisplayDaypart(program.Dayparts.First().Id);
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
                var demoValue = new demoValue
                {
                    demoRank = demo.DemoRank.ToString(),
                    value = new demoValueValue[2]
                };

                Ratingdata ratingValue;
                var ratingDisplay = string.Empty;

                if (demo.Ratings.Any())
                {
                    ratingValue = demo.Ratings.Single(r => programInfo.Dayparts.Any(dp => dp.Id == r.DaypartId) && r.LegacyCallLetters == legacyCallLetters);
                    ratingDisplay = string.Format("{0:#0.00}", ratingValue.Rating);
                }

                var impressions = demo.Impressions.SingleOrDefault(i => i.Id == programInfo.ProgramId)?.Impressions;
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

        private void _SetDetailLineNetworkInfo(detailLine detLine, ScxStation station)
        {
            detLine.network = new network[1];
            detLine.network[0] = new network()
            {
                name = station.LegacyCallLetters + "-TV",
                ID = new codeComplexType[3]
                {
                    new codeComplexType() { code = new codeComplexTypeCode() { codeOwner = "Spotcable", codeDescription = station.StationCode.ToString(), Value = station.LegacyCallLetters } },
                    new codeComplexType() { code = new codeComplexTypeCode() { codeOwner = "Strata", codeDescription = "Station", Value = station.LegacyCallLetters + "-TV" } },
                    new codeComplexType() { code = new codeComplexTypeCode() { codeOwner = "Strata", codeDescription = "Band", Value = "TV" } },
                }
            };
        }

        private static void _SetSystemOrderPopulations(systemOrder sysOrder, List<DemoData> demos, int marketId)
        {
            if (demos == null)
            {
                sysOrder.populations = new populations[0];
                return;
            }

            sysOrder.populations = demos.Select(demo =>
            {
                var population = new populations
                {
                    demoRank = demo.DemoRank
                };

                if (demo.MarketPopulations.TryGetValue((short)marketId, out double marketPopulation))
                    population.Value = marketPopulation.ToString();

                return population;
            }).ToArray();
        }

        private static void _SetScxOrderSurvey(order scxOrder, int marketId, ScxData data)
        {
            scxOrder.survey = new survey() { comment = new surveyComment[1] };
            scxOrder.survey.comment[0] = new surveyComment
            {
                codeOwner = "Spotcable",
                Value = data.SurveyData.Any() ? data.SurveyData[marketId] : string.Empty
            };
            scxOrder.survey.ratingService = string.Empty;
            scxOrder.survey.geography = string.Empty;
            scxOrder.survey.shareBook = string.Empty;
            scxOrder.survey.PUTBook = string.Empty;
            scxOrder.survey.profile = string.Empty;
        }

        private static void _SetSystemOrderWeekInfo(systemOrder order, IOrderedEnumerable<MediaWeek> weeks)
        {
            order.weeks = new weeks
            {
                count = weeks.Count().ToString(),
                week = weeks
                    .Select((item, index) => new week
                    {
                        startDate = item.StartDate,
                        number = (index + 1).ToString()
                    })
                    .ToArray()
            };
        }

        private static bool _SetSystemOrderTotals(systemOrder sysOrder, ScxStation station)
        {
            var weeksWithSpots = station.Programs
                .SelectMany(x => x.Weeks)
                .Where(x => x.Spots > 0)
                .ToList();

            if (!weeksWithSpots.Any())
                return false;

            sysOrder.totals = _GetTotalsFromWeeks(weeksWithSpots);

            return true;
        }

        private static bool _SetOrderTotal(order order, ScxMarketDto market)
        {
            var weeksWithSpots = market.Stations
                .SelectMany(s => s.Programs)
                .SelectMany(w => w.Weeks)
                .Where(w => w.Spots > 0)
                .ToList();

            if (!weeksWithSpots.Any())
                return false;

            order.totals = _GetTotalsFromWeeks(weeksWithSpots);

            return true;
        }

        private static totals _GetTotalsFromWeeks(List<ScxWeek> weeks)
        {
            return new totals
            {
                cost = new cost
                {
                    currency = Currency,
                    //Value = weeksWithSpots.Sum(p => p.Cost) // add the logic in the future story
                },
                spots = weeks.Sum(p => p.Spots).ToString()
            };
        }

        private static void _SetScxSystemOrderKeys(systemOrder order)
        {
            order.key = new key[1];
            order.key[0] = new key() { codeOwner = "Strata", codeDescription = "UseZonePop", id = "false" };
        }

        private static void _SetScxOrderKeys(order order, int marketId)
        {
            order.key = new key[3]; 
            order.key[0] = new key() { codeOwner = "NCC", codeDescription = "Market", id = marketId.ToString() };
            order.key[1] = new key() { codeOwner = "Strata", codeDescription = "Pops", id = "dma;book" };
            order.key[2] = new key() { codeOwner = "Strata", codeDescription = "UseBroadcastWeeks", id = "0" };
        }

        private void _SetScxDemographics(ScxData data, campaign camp)
        {
            if (data.Demos == null) return; //demos for inventory scx file are not processed for the moment

            var demosCount = data.Demos.Count;
            camp.demo = new demo[demosCount];
            camp.populations = new populations[demosCount];
            
            for (var i = 0; i < demosCount; i++)
            {
                var demo = data.Demos[i];
                var scxDemo = new demo
                {
                    ageFrom = demo.Demo.RangeStart.Value.ToString(),
                    ageTo = demo.Demo.RangeEnd.Value.ToString(),
                    demoRank = demo.DemoRank,
                    group = _GetGroupFromAudience(demo)
                };

                camp.demo[i] = scxDemo;

                var population = new populations
                {
                    demoRank = demo.DemoRank,
                    Value = demo.MarketPopulations[(short)data.MarketIds.First()].ToString()
                };
                camp.populations[i] = population;
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
            camp.key[0] = new key() { codeOwner = "NCC", codeDescription = "CampaignID", id = string.Empty };
            camp.key[1] = new key() { codeOwner = "Strata", codeDescription = "DMA Override", id = "0" };
            camp.key[2] = new key() { codeOwner = "Strata", codeDescription = "Zone Pops", id = "max" };
            camp.key[3] = new key() { codeOwner = "VIEW32", codeDescription = "CampaignName", id = string.Empty };
        }
    }
}
