using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.IntegrationTests.Stubs;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Microsoft.Practices.Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class ProgramServiceIntegrationTests
    {
        private readonly IProgramService _ProgramService;

        public ProgramServiceIntegrationTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IProgramsSearchApiClient, ProgramsSearchApiClientStub>();
            _ProgramService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "jo"
            };

            var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

            _VerifyPrograms(programs);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms_Limited()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "jo",
                Start = 2,
                Limit = 1
            };

            var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

            _VerifyPrograms(programs);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms_IgnorePrograms()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "jo",
                IgnorePrograms = new List<string>
                {
                    "Joker"
                }
            };

            var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

            _VerifyPrograms(programs);
        }

        [Test]
        public void GetPrograms_NoPrograms()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "00000000000000"
            };

            var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

            Assert.IsFalse(programs.Any());
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms_DeDup()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "black-"
            };

            var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

            _VerifyPrograms(programs);
        }

        private static void _VerifyPrograms(List<ProgramDto> programs)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(LookupDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programs, jsonSettings));
        }
    }
}
