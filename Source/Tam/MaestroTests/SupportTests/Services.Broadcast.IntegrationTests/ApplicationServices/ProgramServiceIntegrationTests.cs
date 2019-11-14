using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Microsoft.Practices.Unity;
using Services.Broadcast.Entities.DTO.Program;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProgramServiceIntegrationTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPrograms()
        {
            var programService = _ResolveProgramService();
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "search string",
                IgnorePrograms = new List<string>
                {
                    "Ignore this program",
                    "18 AgaiN!"
                }
            };

            var programs = programService.GetPrograms(searchRequest, "IntegrationTestsUser");

            _VerifyPrograms(programs);
        }

        private static IProgramService _ResolveProgramService()
        {
            var programsSearchApiClientMock = new Mock<IProgramsSearchApiClient>();
            programsSearchApiClientMock
                .Setup(x => x.GetPrograms(It.IsAny<SearchRequestProgramDto>()))
                .Returns(_ProgramSearchApiClientPrograms);

            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance(programsSearchApiClientMock.Object);

            return IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramService>();
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

        private static readonly List<SearchProgramDativaResponseDto> _ProgramSearchApiClientPrograms = new List<SearchProgramDativaResponseDto>
        {
            new SearchProgramDativaResponseDto
            {
                ProgramId = "1",
                ProgramName = "18 Again!",
                GenreId = "1",
                Genre = "Comedy",
                ShowType = "Movie",
                MpaaRating = "PG",
                SyndicationType = "Syn"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "2",
                ProgramName = "ABBA: The Movie",
                GenreId = "2",
                Genre = "Investigative",
                ShowType = "Movie",
                MpaaRating = "PG1",
                SyndicationType = "Syn"
            },
            new SearchProgramDativaResponseDto
            {
                ProgramId = "2",
                ProgramName = "Ignore this program",
                GenreId = "2",
                Genre = "Investigative",
                ShowType = "Movie",
                MpaaRating = "PG1",
                SyndicationType = "Syn"
            }
        };
    }
}
