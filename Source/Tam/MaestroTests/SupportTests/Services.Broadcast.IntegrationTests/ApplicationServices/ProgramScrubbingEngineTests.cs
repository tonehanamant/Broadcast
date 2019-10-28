using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class ProgramScrubbingEngineTests
    {
        private readonly IProgramScrubbingEngine _ScrubbingEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramScrubbingEngine>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_NullValues()
        {
            var proposalDetail = new ProposalDetailDto();
            var affidavitDetail = new ScrubbingFileDetail();
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_MatchesMainProgram_WithInclusions()
        {
            var proposalDetail = _GetProposalDto_IncludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                ProgramName = "West World",
                Genre = "Action",
                ShowType = "Movie"
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_DoesNotMatchMainProgram_WithInclusions()
        {
            var proposalDetail = _GetProposalDto_IncludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                ProgramName = "East Works",
                Genre = "Nature",
                ShowType = "Documentary"
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_WithInclusions_NullGenreProgramShowTime()
        {
            var proposalDetail = _GetProposalDto_IncludeData();
            var affidavitDetail = new ScrubbingFileDetail();
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_WithExclusion_NullGenreProgramShowTime()
        {
            var proposalDetail = _GetProposalDto_ExcludeData();
            var affidavitDetail = new ScrubbingFileDetail();
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_WithGenre()
        {
            var proposalDetail = new ProposalDetailDto();
            var affidavitDetail = new ScrubbingFileDetail
            {
                Genre = "Comedy",
                SuppliedProgramName = "Some program name",
                ShowType = "Crime"
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_MatchesMainProgram_WithExclusions()
        {
            ProposalDetailDto proposalDetail = _GetProposalDto_ExcludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                ProgramName = "East Works",
                Genre = "Nature",
                ShowType = "Documentary"
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_DoesNotMatchMainProgram_WithExclusions()
        {
            var proposalDetail = _GetProposalDto_ExcludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                ProgramName = "West World",
                Genre = "Action",
                ShowType = "Movie",
                AirTime = 1800,
                LeadInEndTime = 0,
                LeadOutStartTime = 3600
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_MatchesLeadInProgram()
        {
            var proposalDetail = _GetProposalDto_IncludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                ProgramName = "NFL Tonight",
                Genre = "Sports",
                ShowType = "News",
                LeadinProgramName = "West World",
                LeadinGenre = "Action",
                LeadInShowType = "Movie",
                AirTime = 32400,
                LeadInEndTime = 32280
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_MatchesOvernightLeadInProgram()
        {
            var proposalDetail = _GetProposalDto_IncludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                ProgramName = "NFL Tonight",
                Genre = "Sports",
                ShowType = "News",
                LeadinProgramName = "West World",
                LeadinGenre = "Action",
                LeadInShowType = "Movie",
                AirTime = 60,
                LeadInEndTime = 86340
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_MatchesLeadOutProgram()
        {
            var proposalDetail = _GetProposalDto_IncludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                ProgramName = "NFL Tonight",
                Genre = "Sports",
                ShowType = "News",
                LeadoutProgramName = "West World",
                LeadoutGenre = "Action",
                LeadOutShowType = "Movie",
                AirTime = 32280,
                LeadOutStartTime = 32400
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_MatchesOvernightLeadOutProgram()
        {
            var proposalDetail = _GetProposalDto_IncludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                ProgramName = "NFL Tonight",
                Genre = "Sports",
                ShowType = "News",
                LeadoutProgramName = "West World",
                LeadoutGenre = "Action",
                LeadOutShowType = "Movie",
                AirTime = 86340,
                LeadOutStartTime = 60
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubbingEngine_DoesNotMatchProgram()
        {
            var proposalDetail = _GetProposalDto_IncludeData();
            var affidavitDetail = new ScrubbingFileDetail
            {
                LeadinProgramName = "NFL Tonight",
                LeadinGenre = "Sports",
                LeadInShowType = "News",
                ProgramName = "Wayne's World",
                Genre = "Comedy",
                ShowType = "Movie",
                AirTime = 32280,
                LeadInEndTime = 32400
            };
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }


        private static ProposalDetailDto _GetProposalDto_IncludeData()
        {
            return new ProposalDetailDto
            {
                ProgramCriteria = new List<ProgramCriteria>
                {
                    new ProgramCriteria
                    {
                         Id = 1,
                         Contain = ContainTypeEnum.Include,
                         Program = new LookupDto{ Id = 1, Display = "West World"}
                    }
                },
                    GenreCriteria = new List<GenreCriteria>
                {
                    new GenreCriteria
                    {
                         Id = 1,
                         Contain = ContainTypeEnum.Include,
                         Genre = new LookupDto{ Id = 1, Display = "Action"}
                    }
                },
                    ShowTypeCriteria = new List<ShowTypeCriteria>
                {
                    new ShowTypeCriteria
                    {
                         Id = 1,
                         Contain = ContainTypeEnum.Include,
                         ShowType = new LookupDto{ Id = 1, Display = "Movie"}
                    }
                }
            };
        }

        private static ProposalDetailDto _GetProposalDto_ExcludeData()
        {
            return new ProposalDetailDto
            {
                ProgramCriteria = new List<ProgramCriteria>
                {
                    new ProgramCriteria
                    {
                         Id = 1,
                         Contain = ContainTypeEnum.Exclude,
                         Program = new LookupDto{ Id = 1, Display = "West World"}
                    }
                },
                    GenreCriteria = new List<GenreCriteria>
                {
                    new GenreCriteria
                    {
                         Id = 1,
                         Contain = ContainTypeEnum.Exclude,
                         Genre = new LookupDto{ Id = 1, Display = "Action"}
                    }
                },
                    ShowTypeCriteria = new List<ShowTypeCriteria>
                {
                    new ShowTypeCriteria
                    {
                         Id = 1,
                         Contain = ContainTypeEnum.Exclude,
                         ShowType = new LookupDto{ Id = 1, Display = "Movie"}
                    }
                }
            };
        }
    }
}
