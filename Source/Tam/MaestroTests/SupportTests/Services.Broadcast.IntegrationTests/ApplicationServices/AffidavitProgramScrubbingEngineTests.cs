using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests
{

    [TestFixture]
    public class AffidavitProgramScrubbingEngineTests
    {
        private readonly IProgramScrubbingEngine _ScrubbingEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramScrubbingEngine>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MatchesMainProgram_NoExclusions()
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
        public void MatchesMainProgram_WithInclusions()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            proposalDetail.GenreCriteria = new List<GenreCriteria>
            {
                new GenreCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Genre = new LookupDto{ Id = 1, Display = "Action"}
                }
            };
            proposalDetail.ShowTypeCriteria = new List<ShowTypeCriteria>
            {
                new ShowTypeCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     ShowType = new LookupDto{ Id = 1, Display = "Movie"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.ProgramName = "West World";
            affidavitDetail.Genre = "Action";
            affidavitDetail.ShowType = "Movie";
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DoesNotMatchMainProgram_WithInclusions()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            proposalDetail.GenreCriteria = new List<GenreCriteria>
            {
                new GenreCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Genre = new LookupDto{ Id = 1, Display = "Action"}
                }
            };
            proposalDetail.ShowTypeCriteria = new List<ShowTypeCriteria>
            {
                new ShowTypeCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     ShowType = new LookupDto{ Id = 1, Display = "Movie"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.ProgramName = "East Works";
            affidavitDetail.Genre = "Nature";
            affidavitDetail.ShowType = "Documentary";
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MatchesMainProgram_WithExclusions()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Exclude,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            proposalDetail.GenreCriteria = new List<GenreCriteria>
            {
                new GenreCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Exclude,
                     Genre = new LookupDto{ Id = 1, Display = "Action"}
                }
            };
            proposalDetail.ShowTypeCriteria = new List<ShowTypeCriteria>
            {
                new ShowTypeCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Exclude,
                     ShowType = new LookupDto{ Id = 1, Display = "Movie"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.ProgramName = "East Works";
            affidavitDetail.Genre = "Nature";
            affidavitDetail.ShowType = "Documentary";
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DoesNotMatchMainProgram_WithExclusions()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Exclude,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            proposalDetail.GenreCriteria = new List<GenreCriteria>
            {
                new GenreCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Exclude,
                     Genre = new LookupDto{ Id = 1, Display = "Action"}
                }
            };
            proposalDetail.ShowTypeCriteria = new List<ShowTypeCriteria>
            {
                new ShowTypeCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Exclude,
                     ShowType = new LookupDto{ Id = 1, Display = "Movie"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.ProgramName = "West World";
            affidavitDetail.Genre = "Action";
            affidavitDetail.ShowType = "Movie";
            affidavitDetail.AirTime = 1800;
            affidavitDetail.LeadInEndTime = 0;
            affidavitDetail.LeadOutStartTime = 3600;
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MatchesLeadInProgram()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.ProgramName = "NFL Tonight";
            affidavitDetail.Genre = "Sports";
            affidavitDetail.ShowType = "News";
            affidavitDetail.LeadinProgramName = "West World";
            affidavitDetail.LeadinGenre = "Action";
            affidavitDetail.LeadInShowType = "Movie";
            affidavitDetail.AirTime = 32400;
            affidavitDetail.LeadInEndTime = 32280;
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MatchesOvernightLeadInProgram()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.ProgramName = "NFL Tonight";
            affidavitDetail.Genre = "Sports";
            affidavitDetail.ShowType = "News";
            affidavitDetail.LeadinProgramName = "West World";
            affidavitDetail.LeadinGenre = "Action";
            affidavitDetail.LeadInShowType = "Movie";
            affidavitDetail.AirTime = 60;
            affidavitDetail.LeadInEndTime = 86340;
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MatchesLeadOutProgram()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.ProgramName = "NFL Tonight";
            affidavitDetail.Genre = "Sports";
            affidavitDetail.ShowType = "News";
            affidavitDetail.LeadoutProgramName = "West World";
            affidavitDetail.LeadoutGenre = "Action";
            affidavitDetail.LeadOutShowType = "Movie";
            affidavitDetail.AirTime = 32280;
            affidavitDetail.LeadOutStartTime = 32400;
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MatchesOvernightLeadOutProgram()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.ProgramName = "NFL Tonight";
            affidavitDetail.Genre = "Sports";
            affidavitDetail.ShowType = "News";
            affidavitDetail.LeadoutProgramName = "West World";
            affidavitDetail.LeadoutGenre = "Action";
            affidavitDetail.LeadOutShowType = "Movie";
            affidavitDetail.AirTime = 86340;
            affidavitDetail.LeadOutStartTime = 60;
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DoesNotMatchProgram()
        {
            var proposalDetail = new ProposalDetailDto();
            proposalDetail.ProgramCriteria = new List<ProgramCriteria>
            {
                new ProgramCriteria
                {
                     Id = 1,
                     Contain = ContainTypeEnum.Include,
                     Program = new LookupDto{ Id = 1, Display = "West World"}
                }
            };
            var affidavitDetail = new ScrubbingFileDetail();
            affidavitDetail.LeadinProgramName = "NFL Tonight";
            affidavitDetail.LeadinGenre = "Sports";
            affidavitDetail.LeadInShowType = "News";
            affidavitDetail.ProgramName = "Wayne's World";
            affidavitDetail.Genre = "Comedy";
            affidavitDetail.ShowType = "Movie";
            affidavitDetail.AirTime = 32280;
            affidavitDetail.LeadInEndTime = 32400;
            var affidavitScrub = new ClientScrub();

            _ScrubbingEngine.Scrub(proposalDetail, affidavitDetail, affidavitScrub);

            var json = IntegrationTestHelper.ConvertToJson(affidavitScrub);
            Approvals.Verify(json);
        }

    }
}
