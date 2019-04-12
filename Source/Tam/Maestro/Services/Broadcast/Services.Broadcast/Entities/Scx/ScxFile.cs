using Services.Broadcast.Entities.spotcableXML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxFile
    {
        private readonly adx _RawScx;
        private readonly string _FileHash;
        private readonly List<ScxFileDetail> _Details;

        public ScxFile(Stream scxStream)
        {
            var rawBytes = StreamHelper.ReadToEnd(scxStream);            
            _FileHash = HashGenerator.ComputeHash(rawBytes);
            _RawScx = adx.Deserialize(_ScrubContents(rawBytes));
            _Details = _BuildScxFileDetails();
        }

        public adx RawScx
        {
            get { return _RawScx;  }
        }

        public string FileHash
        {
            get { return _FileHash; }
        }

        public DateTime CampaignStartDate
        {
            get { return _RawScx.campaign.dateRange.startDate; }
        }

        public DateTime CampaignEndDate
        {
            get { return _RawScx.campaign.dateRange.endDate; }
        }

        public List<ScxFileAudience> Audiences
        {
            get { return _RawScx.campaign.demo.Select(d => new ScxFileAudience(d)).ToList(); }
        }

        public List<ScxFileDetail> Details
        {
            get { return _Details; }
        }

        private static string _ScrubContents(byte[] rawBytes)
        {
            var fileText = Encoding.UTF8.GetString(rawBytes);

            var replacements = new Dictionary<string, string>
            {
                {"<affiliateSplit/>","<affiliateSplit>0</affiliateSplit>"}
                ,{"&lt;",string.Empty}
                ,{"NaN:00:00", "00:00:00"}
            };            

            foreach (var replacement in replacements)
            {
                fileText = fileText.Replace(replacement.Key, replacement.Value);
            }

            return fileText;
        }

        private List<ScxFileDetail> _BuildScxFileDetails()
        {
            var details = new List<ScxFileDetail>();

            foreach(var order in _RawScx.campaign.order)
            {
                foreach(var sysorder in order.systemOrder)
                {
                    foreach(var detailLine in sysorder.detailLine)
                    {
                        if (detailLine.network == null)
                            continue;
                        if (detailLine.totals == null)
                            continue;

                        var weeks = new List<ScxFileDetailWeek>();
                        foreach (var spot in detailLine.spot)
                        {
                            var startDate = sysorder.weeks.week.Single(x => x.number == spot.weekNumber).startDate;
                            var quantity = int.Parse(spot.quantity);
                            var detailWeek = new ScxFileDetailWeek(
                                startDate,
                                quantity
                                );

                            weeks.Add(detailWeek);
                        }

                        var audiences = new List<ScxFileDetailAudience>();
                        if (detailLine.demoValue != null)
                        {
                            foreach (var demoValue in detailLine.demoValue)
                            {
                                var demoRank = int.Parse(demoValue.demoRank);
                                var impressions = double.Parse(demoValue.value.Single(pVal => pVal.type == demoValueValueType.Impressions).Value);
                                var population = sysorder.populations.Single(pop => pop.demoRank == demoRank).Value != null ?
                                    int.Parse(sysorder.populations.Single(pop => pop.demoRank == demoRank).Value) : 0;
                                var audience = new ScxFileDetailAudience(
                                    demoRank,
                                    impressions,
                                    population
                                    );
                                audiences.Add(audience);
                            }
                        }

                        var totalSpots = weeks.Sum(w => w.Spots);
                        var totalCost = (decimal)totalSpots * detailLine.spotCost.Value;
                        var detail = new ScxFileDetail(
                            order.market.name,
                            detailLine.network.First().name,
                            detailLine.program,
                            _ParseSpotLength(detailLine.length),
                            detailLine.dayOfWeek.Monday == "Y",
                            detailLine.dayOfWeek.Tuesday == "Y",
                            detailLine.dayOfWeek.Wednesday == "Y",
                            detailLine.dayOfWeek.Thursday == "Y",
                            detailLine.dayOfWeek.Friday == "Y",
                            detailLine.dayOfWeek.Saturday == "Y",
                            detailLine.dayOfWeek.Sunday == "Y",
                            (int)detailLine.startTime.TimeOfDay.TotalSeconds,
                            (int)detailLine.endTime.TimeOfDay.TotalSeconds == 0 ? 86399 : (int)detailLine.endTime.TimeOfDay.TotalSeconds - 1,
                            detailLine.spotCost.Value,
                            totalSpots,
                            totalCost,
                            weeks,
                            audiences
                            );
                        details.Add(detail);

                    }
                }
            }

            return details;
        }

        private int _ParseSpotLength(string spotLengthString)
        {
            spotLengthString = spotLengthString.Replace("PT", "").Replace("S", "");
            var spotLength = int.Parse(spotLengthString);

            return spotLength;
        }

        public class ScxFileAudience
        {
            private demo d;

            public int Rank { get; }
            public string Group { get; }
            public int AgeFrom { get; }
            public int AgeTo { get; }

            public ScxFileAudience(demo audience)
            {
                Rank = audience.demoRank;
                switch (audience.group.ToString())
                {
                    case "Households":
                        {
                            Group = "H";
                            break;
                        }
                    case "Adults":
                        {
                            Group = "A";
                            break;
                        }
                    case "Men":
                        {
                            Group = "M";
                            break;

                        }
                    case "Women":
                        {
                            Group = "W";
                            break;

                        }
                }
                AgeFrom = int.Parse(audience.ageFrom);
                AgeTo = int.Parse(audience.ageTo);
            }
        }

        public class ScxFileDetail
        {
            public string Market { get; }
            public string Network { get; }
            public string Program { get; }
            public int SpotLength { get; }
            public bool Monday { get; }
            public bool Tuesday { get; }
            public bool Wednesday { get; }
            public bool Thursday { get; }
            public bool Friday { get; }
            public bool Saturday { get; }
            public bool Sunday { get; }
            public int StartTime { get; }
            public int EndTime { get; }
            public decimal SpotCost { get; }
            public int TotalSpots { get; }
            public decimal TotalCost { get; }
            public List<ScxFileDetailWeek> Weeks { get; }
            public List<ScxFileDetailAudience> Audiences { get; }


            public ScxFileDetail(string market, string network, string program, int spotLength,
                                 bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday, bool sunday,
                                 int startTime, int endTime, decimal spotCost, int totalSpots, decimal totalCost,
                                 List<ScxFileDetailWeek> weeks, List<ScxFileDetailAudience> audiences)
            {
                Market = market;
                Network = network;
                Program = program;
                SpotLength = spotLength;
                Monday = monday;
                Tuesday = tuesday;
                Wednesday = wednesday;
                Thursday = thursday;
                Friday = friday;
                Saturday = saturday;
                Sunday = sunday;
                StartTime = startTime;
                EndTime = endTime;
                SpotCost = spotCost;
                TotalSpots = totalSpots;
                TotalCost = totalCost;
                Weeks = weeks;
                Audiences = audiences;
            }

        }

        public class ScxFileDetailWeek
        {
            public DateTime StartDate { get; }
            public int Spots { get; }

            public ScxFileDetailWeek(DateTime startDate, int spots)
            {
                StartDate = startDate;
                Spots = spots;
            }
        }

        public class ScxFileDetailAudience
        {
            public int Rank { get; }
            public double Impressions { get; }
            public int Population { get; }
            public ScxFileDetailAudience(int rank, double impressions, int population)
            {
                Rank = rank;
                Impressions = impressions;
                Population = population;
            }
        }
    }
}
