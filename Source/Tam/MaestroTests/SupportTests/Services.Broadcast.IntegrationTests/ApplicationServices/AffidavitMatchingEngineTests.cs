using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests.Reporters;
using Common.Services;
using NUnit.Framework;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using ApprovalTests;
using IntegrationTests.Common;
using Newtonsoft.Json;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class AffidavitMatchingEngineTests
    {

        private IMatchingEngine _AffidavitMatchingEngine =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IMatchingEngine>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UnmatchedIsci()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123"
            };

            var proposalWeeks = new List<MatchingProposalWeek>();

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(FileDetailProblem), "DetailId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_AffidavitMatchingEngine.GetMatchingProblems(), jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MultipleUnmarriedIscisOnSeparateProposals()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123"
            };

            var proposalWeeks = new List<MatchingProposalWeek>();
            proposalWeeks.Add(new MatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 1,
                ProposalVersionDetailQuarterWeekId = 1,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });
            proposalWeeks.Add(new MatchingProposalWeek()
            {
                ProposalVersionId = 2,
                ProposalVersionDetailId = 2,
                ProposalVersionDetailQuarterWeekId = 2,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(FileDetailProblem), "DetailId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_AffidavitMatchingEngine.GetMatchingProblems(), jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MarriedAndUnmarriedIscisOnSeparateProposals()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123"
            };

            var proposalWeeks = new List<MatchingProposalWeek>();
            proposalWeeks.Add(new MatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 1,
                ProposalVersionDetailQuarterWeekId = 1,
                HouseIsci = "ABC123",
                MarriedHouseIsci = true
            });
            proposalWeeks.Add(new MatchingProposalWeek()
            {
                ProposalVersionId = 2,
                ProposalVersionDetailId = 2,
                ProposalVersionDetailQuarterWeekId = 2,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(FileDetailProblem), "DetailId");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_AffidavitMatchingEngine.GetMatchingProblems(), jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MultipleUnmarriedIscisOnSameProposal()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123"
            };

            var proposalWeeks = new List<MatchingProposalWeek>();
            proposalWeeks.Add(new MatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 5,
                ProposalVersionDetailQuarterWeekId = 1,
                ProposalVersionDetailDaypartId = 1,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });
            proposalWeeks.Add(new MatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 4,
                ProposalVersionDetailQuarterWeekId = 2,
                ProposalVersionDetailDaypartId = 1,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_AffidavitMatchingEngine.GetMatchingProblems()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailPicksAirtimeMatch()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-05T07:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-05T07:00:00").TimeOfDay.TotalSeconds),
                SpotLengthId = 1
            };

            var proposalWeeks = new List<MatchingProposalWeek>
            {
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 5,
                    ProposalVersionDetailQuarterWeekId = 1,
                    ProposalVersionDetailDaypartId = 9,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                },
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 4,
                    ProposalVersionDetailQuarterWeekId = 2,
                    ProposalVersionDetailDaypartId = 1,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                }
            };

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailMatchDateOnly()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-05T01:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-05T01:00:00").TimeOfDay.TotalSeconds),
                SpotLengthId = 1
            };

            var proposalWeeks = new List<MatchingProposalWeek>
            {
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 5,
                    ProposalVersionDetailQuarterWeekId = 1,
                    ProposalVersionDetailDaypartId = 9,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                },
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 4,
                    ProposalVersionDetailQuarterWeekId = 2,
                    ProposalVersionDetailDaypartId = 1,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                }
            };

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailMatchTimeOnly()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-08T09:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-08T09:00:00").TimeOfDay.TotalSeconds),
                SpotLengthId = 1
            };

            var proposalWeeks = new List<MatchingProposalWeek>
            {
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 5,
                    ProposalVersionDetailQuarterWeekId = 1,
                    ProposalVersionDetailDaypartId = 9,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                },
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 4,
                    ProposalVersionDetailQuarterWeekId = 2,
                    ProposalVersionDetailDaypartId = 1,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                }
            };

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailPicksAirtimeMatchWithLeadIn()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-05T08:58:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-05T08:58:00").TimeOfDay.TotalSeconds),
                SpotLengthId = 1
            };

            var proposalWeeks = new List<MatchingProposalWeek>
            {
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 5,
                    ProposalVersionDetailQuarterWeekId = 1,
                    ProposalVersionDetailDaypartId = 15,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                },
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 4,
                    ProposalVersionDetailQuarterWeekId = 2,
                    ProposalVersionDetailDaypartId = 9,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                }
            };

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailPicksFirstIfBothMatchAirtime()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-05T07:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-05T07:00:00").TimeOfDay.TotalSeconds),
                SpotLengthId = 1
            };

            var proposalWeeks = new List<MatchingProposalWeek>
            {
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 5,
                    ProposalVersionDetailQuarterWeekId = 1,
                    ProposalVersionDetailDaypartId = 2,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                },
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 4,
                    ProposalVersionDetailQuarterWeekId = 2,
                    ProposalVersionDetailDaypartId = 1,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                }
            };

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailPicksFirstIfNoneMatchAirtime()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-08T05:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-08T05:00:00").TimeOfDay.TotalSeconds),
                SpotLengthId = 1
            };

            var proposalWeeks = new List<MatchingProposalWeek>
            {
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 5,
                    ProposalVersionDetailQuarterWeekId = 1,
                    ProposalVersionDetailDaypartId = 2,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                },
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 4,
                    ProposalVersionDetailQuarterWeekId = 2,
                    ProposalVersionDetailDaypartId = 1,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                }
            };

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitMatchingEngine_IncludeDatepartEndtime()
        {
            var affidavitDetail = new ScrubbingFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-07T09:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-07T09:00:00").TimeOfDay.TotalSeconds),
                SpotLengthId = 1
            };

            var proposalWeeks = new List<MatchingProposalWeek>
            {
                new MatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 5,
                    ProposalVersionDetailQuarterWeekId = 1,
                    ProposalVersionDetailDaypartId = 5,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1,
                    SpotLengthId = 1
                }
            };
            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks, affidavitDetail.SpotLengthId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }
    }
}
