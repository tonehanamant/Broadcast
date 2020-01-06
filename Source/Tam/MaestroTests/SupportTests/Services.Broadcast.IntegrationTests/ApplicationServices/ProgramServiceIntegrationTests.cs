using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.DTO.Program;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProgramServiceIntegrationTests
    {
        private readonly IProgramService _ProgramService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "batman"
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
                ProgramName = "batman",
                Start = 1,
                Limit = 2
            };

            var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

            _VerifyPrograms(programs);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms_StartAt2ndIndex()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "batman",
                Start = 2,
                Limit = 2
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
                ProgramName = "batman",
                IgnorePrograms = new List<string>
                {
                    "Batman: Asalto en Arkham"
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
