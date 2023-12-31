﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.IntegrationTests.Stubs;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class ProgramServiceIntegrationTests
    {
        private readonly IProgramService _ProgramService;

        public ProgramServiceIntegrationTests()
        {
            _ProgramService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramService>();
        }

        [Test]
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
        public void GetPrograms_DeDup()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "black-"
            };

            var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

            _VerifyPrograms(programs);
        }

        [Test]
        public void GetPrograms_VariousUnmatched()
        {
            var searchRequest = new SearchRequestProgramDto
            {
                ProgramName = "mat"
            };

            var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

            _VerifyPrograms(programs);
        }

        [Test]
        public void GetExceptionProgramsTest()
        {
	        var searchRequest = new SearchRequestProgramDto
	        {
		        ProgramName = "golf"
	        };

	        var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

	        _VerifyPrograms(programs);
        }

        [Test]      
        public void GetExceptionProgramsTest_SpecialCharacter()
        {
	        var searchRequest = new SearchRequestProgramDto
	        {
		        ProgramName = "golf'"
	        };

	        var programs = _ProgramService.GetPrograms(searchRequest, "IntegrationTestsUser");

	       Assert.IsEmpty(programs);
        }

        private static void _VerifyPrograms(List<ProgramDto> programs)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(LookupDto), "Id");
            // Lost ContentRating when we went to internal program search
            jsonResolver.Ignore(typeof(ProgramDto), "ContentRating");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programs, jsonSettings));
        }
    }
}
