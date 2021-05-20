using Common.Services;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.spotcableXML;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.BusinessEngines;
using Tam.Maestro.Data.Entities;
using static Services.Broadcast.Entities.Scx.ScxMarketDto;
using static Services.Broadcast.Entities.Scx.ScxMarketDto.ScxStation;

namespace Services.Broadcast.Converters.Scx
{
    public class ScxBaseConverter
    {
        protected const string HardCodedAgencyCode = "309";
        protected const string Currency = "USD";
        protected const string DocCode = "1";
        protected const string DocType = "Order";

        protected readonly IDaypartCache _DaypartCache;
        protected readonly IDateTimeEngine _DateTimeEngine;

        public ScxBaseConverter(IDaypartCache daypartCache, 
            IDateTimeEngine dateTimeEngine)
        {
            _DaypartCache = daypartCache;
            _DateTimeEngine = dateTimeEngine;
        }

        /// <summary>
        /// Creates an adx object from the ScxData object
        /// </summary>
        /// <param name="data">ScxData object to be converted</param>
        /// <param name="filterUnallocated">When true will filter out inventory that do not have spots allocated.</param>
        /// <returns>adx object</returns>
        protected adx CreateAdxObject(ScxData data, bool filterUnallocated = true)
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
            _SetScxAdvertiser(camp);
            _SetScxProduct(camp);
            _SetScxEstimate(camp);
            _SetScxMakeGoodPolicy(camp);
            _SetScxDemographics(data, camp);
            _SetScxOrders(data, camp, filterUnallocated);

            return xp;
        }

        private void _SetScxOrders(ScxData data, campaign camp, bool filterUnallocated)
        {
            var orders = new List<order>();

            foreach (var order in data.Orders)
            {
                var markets = filterUnallocated
                    ? order.InventoryMarkets.Where(x => x.TotalSpots > 0)
                    : order.InventoryMarkets;

                foreach (var market in markets)
                {
                    var scxOrder = new order
                    {
                        comment = string.Empty,
                        populations = new populations[0],
                        totals = _GetTotals(market.TotalCost, market.TotalSpots)
                    };

                    _SetScxOrderKeys(scxOrder, market.MarketId);
                    _SetMarket(scxOrder, market);
                    _SetScxOrderSurvey(scxOrder, order);
                    
                    var stations = market.Stations;

                    if (stations.Any())
                    {
                        scxOrder.systemOrder = stations
                            .Where(x => x.TotalSpots > 0 && filterUnallocated || !filterUnallocated)
                            .Select(station => _GetSystemOrders(data, station, filterUnallocated))
                            .ToArray();
                    }

                    orders.Add(scxOrder);
                }
            }

            camp.order = orders.ToArray();
        }

        private void _SetMarket(order sxcOrder, ScxMarketDto market)
        {
            sxcOrder.market = new market()
            {
                nsi_id = market.MarketId,
                name = market.DmaMarketName
            };
        }

        private void _SetDocumentParts(adx xp)
        {
            xp.document.date = _DateTimeEngine.GetCurrentMoment().Date;
            xp.document.mediaType = documentMediaType.Spotcable;
            xp.document.schemaVersion = "1.0";
            xp.document.name = " ";
            xp.document.documentCode = DocCode;
            xp.document.documentType = DocType;
        }

        private systemOrder _GetSystemOrders(ScxData data, ScxStation station, bool filterUnallocated)
        {
            systemOrder sysOrder = new systemOrder
            {
                populations = new populations[0],
                comment = "OK",
                totals = _GetTotals(station.TotalCost, station.TotalSpots),
                system = new system[]
                {
                    new system() { name = String.Empty, syscode = String.Empty }
                }
            };

            _SetScxSystemOrderKeys(sysOrder);
            _SetSystemOrderWeekInfo(sysOrder, data.AllSortedMediaWeeks);
            
            var detLines = new List<detailLine>();
            var programs = filterUnallocated 
                ? station.Programs.Where(x => x.TotalSpots > 0) 
                : station.Programs;

            var detLineIndex = 0;

            foreach (var program in programs)
            {
                detLineIndex++;
                var detLine = new detailLine
                {
                    detailLineID = detLineIndex,
                    program = program.ProgramName,
                    length = $"PT{program.SpotLength}S",
                    comment = " ",
                    totals = _GetTotals(program.TotalCost, program.TotalSpots)
                };

                _SetDaypartInfo(detLine, program, data.DaypartCode);
                _SetDetailLineNetworkInfo(detLine, station);
                _SetDetailLineDemoValue(detLine, data, program);
                _SetDetailLineCost(detLine, program);
                _SetSpotWeekQuantities(data.AllSortedMediaWeeks, program, detLine);

                detLines.Add(detLine);
            }

            if (detLines.Any())
                sysOrder.detailLine = detLines.ToArray();

            return sysOrder;
        }

        private static void _SetSpotWeekQuantities(IOrderedEnumerable<MediaWeek> weeks, ScxProgram program, detailLine detLine)
        {
            detLine.spot = weeks.Select((week, index) => new spot
            {
                weekNumber = (index + 1).ToString(),
                quantity = program.Weeks.Where(x => x.MediaWeek.Id == week.Id).Sum(x => x.Spots).ToString()
            }).ToArray();
        }

        private void _SetDetailLineCost(detailLine detLine, ScxProgram program)
        {
            detLine.spotCost = new spotCost
            {
                currency = Currency,
                Value = program.SpotCost
            };
        }

        private totals _GetTotals(decimal cost, int spots)
        {
            return new totals
            {
                cost = new cost
                {
                    currency = Currency,
                    Value = cost
                },
                spots = spots.ToString()
            };
        }

        private void _SetDaypartInfo(detailLine detLine, ScxProgram program, string daypartCode)
        {
            var daypart = _DaypartCache.GetDisplayDaypart(program.DaypartId);

            detLine.startDay = detailLineStartDay.M;
            detLine.startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            detLine.startTime = detLine.startTime.AddSeconds(daypart.StartTime);

            detLine.endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);

            // add one second to bring the time up to full hour/half-hour/etc
            detLine.endTime = detLine.endTime.AddSeconds(daypart.EndTime + 1);

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

            if (!string.IsNullOrWhiteSpace(daypartCode))
            {
                detLine.daypartCode = daypartCode;
            }
            else if (!string.IsNullOrWhiteSpace(program.ProgramAssignedDaypartCode))
            {
                detLine.daypartCode = program.ProgramAssignedDaypartCode;
            }
        }

        private void _SetDetailLineDemoValue(detailLine detLine, ScxData data, ScxProgram program)
        {
            if (data.Demos == null)
            {
                detLine.demoValue = new demoValue[0];
                return;
            }

            var result = new List<demoValue>();

            foreach (var demo in data.Demos)
            {
                var demoValue = new demoValue
                {
                    demoRank = demo.DemoRank.ToString(),
                    value = new demoValueValue[2]
                };

                var impressions = program.DemoValues.SingleOrDefault(x => x.DemoRank == demo.DemoRank)?.Impressions;
                var impressionsDisplay = impressions.HasValue && impressions.Value > 0 ?
                    string.Format("{0:####}", impressions.Value) : 
                    string.Empty;

                // ratingDisplay = string.Format("{0:#0.00}", rating) - for future using
                demoValue.value[0] = new demoValueValue() { type = demoValueValueType.Ratings, Value = string.Empty, typeSpecified = true };
                demoValue.value[1] = new demoValueValue() { type = demoValueValueType.Impressions, Value = impressionsDisplay, typeSpecified = true };

                result.Add(demoValue);
            }
            
            detLine.demoValue = result.ToArray();
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

        private static void _SetScxOrderSurvey(order scxOrder, OrderData order)
        {
            scxOrder.survey = new survey { comment = new surveyComment[1] };
            scxOrder.survey.comment[0] = new surveyComment
            {
                codeOwner = "Spotcable",
                Value = order.SurveyString
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
            camp.demo = data.Demos.Select(demo => new demo
            {
                ageFrom = demo.Demo.RangeStart.Value.ToString(),
                ageTo = demo.Demo.RangeEnd.Value.ToString(),
                demoRank = demo.DemoRank,
                group = _GetGroupFromAudience(demo)
            }).ToArray();
        }

        private demoNameComplexTypeGroup _GetGroupFromAudience(DemoData demo)
        {
            if (demo.Demo.Name.Contains("Households"))
                return demoNameComplexTypeGroup.Households;
            if (demo.Demo.Name.Contains("Adult"))
                return demoNameComplexTypeGroup.Adults;
            if (demo.Demo.Name.StartsWith("Men"))
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

        private void _SetScxAdvertiser(campaign camp)
        {
            var adv = new advertiser()
            {
                name = string.Empty,
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
