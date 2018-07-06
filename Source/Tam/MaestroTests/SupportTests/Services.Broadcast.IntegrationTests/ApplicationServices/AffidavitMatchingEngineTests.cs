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

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class AffidavitMatchingEngineTests
    {

        private IAffidavitMatchingEngine _AffidavitMatchingEngine =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitMatchingEngine>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UnmatchedIsci()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123"
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_AffidavitMatchingEngine.MatchingProblems()));

        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MultipleUnmarriedIscisOnSeparateProposals()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123"
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 1,
                ProposalVersionDetailQuarterWeekId = 1,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 2,
                ProposalVersionDetailId = 2,
                ProposalVersionDetailQuarterWeekId = 2,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_AffidavitMatchingEngine.MatchingProblems()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MarriedAndUnmarriedIscisOnSeparateProposals()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123"
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 1,
                ProposalVersionDetailQuarterWeekId = 1,
                HouseIsci = "ABC123",
                MarriedHouseIsci = true
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 2,
                ProposalVersionDetailId = 2,
                ProposalVersionDetailQuarterWeekId = 2,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_AffidavitMatchingEngine.MatchingProblems()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MultipleUnmarriedIscisOnSameProposal()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123"
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 5,
                ProposalVersionDetailQuarterWeekId = 1,
                ProposalVersionDetailDaypartId = 1,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 4,
                ProposalVersionDetailQuarterWeekId = 2,
                ProposalVersionDetailDaypartId = 1,
                HouseIsci = "ABC123",
                MarriedHouseIsci = false
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(_AffidavitMatchingEngine.MatchingProblems()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailPicksAirtimeMatch()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-05T07:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-05T07:00:00").TimeOfDay.TotalSeconds)
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 5,
                ProposalVersionDetailQuarterWeekId = 1,
                ProposalVersionDetailDaypartId = 9,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 4,
                ProposalVersionDetailQuarterWeekId = 2,
                ProposalVersionDetailDaypartId = 1,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailMatchDateOnly()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-05T01:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-05T01:00:00").TimeOfDay.TotalSeconds)
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 5,
                ProposalVersionDetailQuarterWeekId = 1,
                ProposalVersionDetailDaypartId = 9,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 4,
                ProposalVersionDetailQuarterWeekId = 2,
                ProposalVersionDetailDaypartId = 1,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailMatchTimeOnly()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-08T09:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-08T09:00:00").TimeOfDay.TotalSeconds)
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 5,
                ProposalVersionDetailQuarterWeekId = 1,
                ProposalVersionDetailDaypartId = 9,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 4,
                ProposalVersionDetailQuarterWeekId = 2,
                ProposalVersionDetailDaypartId = 1,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailPicksAirtimeMatchWithLeadIn()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-05T08:58:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-05T08:58:00").TimeOfDay.TotalSeconds)
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 5,
                ProposalVersionDetailQuarterWeekId = 1,
                ProposalVersionDetailDaypartId = 15,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 4,
                ProposalVersionDetailQuarterWeekId = 2,
                ProposalVersionDetailDaypartId = 9,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailPicksFirstIfBothMatchAirtime()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-05T07:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-05T07:00:00").TimeOfDay.TotalSeconds)
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 5,
                ProposalVersionDetailQuarterWeekId = 1,
                ProposalVersionDetailDaypartId = 2,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 4,
                ProposalVersionDetailQuarterWeekId = 2,
                ProposalVersionDetailDaypartId = 1,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void IsciOnMultipleProposalDetailPicksFirstIfNoneMatchAirtime()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-08T05:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-08T05:00:00").TimeOfDay.TotalSeconds)
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>();
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 5,
                ProposalVersionDetailQuarterWeekId = 1,
                ProposalVersionDetailDaypartId = 2,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });
            proposalWeeks.Add(new AffidavitMatchingProposalWeek()
            {
                ProposalVersionId = 1,
                ProposalVersionDetailId = 4,
                ProposalVersionDetailQuarterWeekId = 2,
                ProposalVersionDetailDaypartId = 1,
                ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                HouseIsci = "ABC123",
                MarriedHouseIsci = false,
                Spots = 1
            });

            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void AffidavitMatchingEngine_IncludeDatepartEndtime()
        {
            var affidavitDetail = new AffidavitFileDetail
            {
                Isci = "ABC123",
                OriginalAirDate = DateTime.Parse("2018-01-07T09:00:00"),
                AirTime = Convert.ToInt32(DateTime.Parse("2018-01-07T09:00:00").TimeOfDay.TotalSeconds)
            };

            var proposalWeeks = new List<AffidavitMatchingProposalWeek>
            {
                new AffidavitMatchingProposalWeek()
                {
                    ProposalVersionId = 1,
                    ProposalVersionDetailId = 5,
                    ProposalVersionDetailQuarterWeekId = 1,
                    ProposalVersionDetailDaypartId = 5,
                    ProposalVersionDetailWeekStart = DateTime.Parse("2018-01-01"),
                    ProposalVersionDetailWeekEnd = DateTime.Parse("2018-01-07"),
                    HouseIsci = "ABC123",
                    MarriedHouseIsci = false,
                    Spots = 1
                }
            };
            var matchedWeeks = _AffidavitMatchingEngine.Match(affidavitDetail, proposalWeeks);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(matchedWeeks));
        }
    }
}
