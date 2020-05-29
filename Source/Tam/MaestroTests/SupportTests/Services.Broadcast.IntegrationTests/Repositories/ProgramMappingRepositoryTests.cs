using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System.Linq;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Microsoft.Practices.Unity;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class ProgramMappingRepositoryTests
    {
        private readonly IGenreRepository _GenreRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
        private readonly IShowTypeRepository _ShowTypeRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IShowTypeRepository>();
        private readonly IProgramMappingRepository _ProgramMappingRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateProgramMapping()
        {
            var createdBy = "testUser";
            var createdAt = new DateTime(2020, 10, 17, 8, 32, 12);
            var genre = _GenreRepository.GetGenreByName("News", ProgramSourceEnum.Mapped);
            var showType = _ShowTypeRepository.GetShowTypes().First();
            var newProgramMapping = new ProgramMappingsDto
            {
                OriginalProgramName = "TestOriginalProgramName",
                OfficialProgramName = "TestOfficialProgramName",
                OfficialGenre = genre,
                OfficialShowType = new ShowTypeDto
                {
                    Id = showType.Id,
                    Name = showType.Display
                }
            };

            ProgramMappingsDto createdMapping;
            using (new TransactionScopeWrapper())
            {
                _ProgramMappingRepository.CreateProgramMapping(newProgramMapping, createdBy, createdAt);
                createdMapping = _ProgramMappingRepository.GetProgramMappingByOriginalProgramName(newProgramMapping.OriginalProgramName);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(createdMapping, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateProgramMapping()
        {
            var testProgramName = "testProgramName";
            var username = "testUser";
            var createdAt = new DateTime(2020, 10, 17, 8, 32, 12);
            var modifiedAt = new DateTime(2020, 10, 20, 8, 32, 12);
            var genre = _GenreRepository.GetGenreByName("News", ProgramSourceEnum.Mapped);
            var genreTwo = _GenreRepository.GetGenreByName("Reality", ProgramSourceEnum.Mapped);
            var showType = _ShowTypeRepository.GetShowTypes().First();
            var showTypeTwo = _ShowTypeRepository.GetShowTypes().Last();
            var newProgramMapping = new ProgramMappingsDto
            {
                OriginalProgramName = testProgramName,
                OfficialProgramName = "TestOfficialProgramName",
                OfficialGenre = genre,
                OfficialShowType = new ShowTypeDto
                {
                    Id = showType.Id,
                    Name = showType.Display
                }
            };
            var updatedProgramMapping = new ProgramMappingsDto
            {
                OriginalProgramName = testProgramName,
                OfficialProgramName = "TestOfficialProgramNameNowModified",
                OfficialGenre = genreTwo,
                OfficialShowType = new ShowTypeDto
                {
                    Id = showTypeTwo.Id,
                    Name = showTypeTwo.Display
                }
            };

            ProgramMappingsDto updatedMapping;
            using (new TransactionScopeWrapper())
            {
                var id = _ProgramMappingRepository.CreateProgramMapping(newProgramMapping, username, createdAt);
                updatedProgramMapping.Id = id;
                _ProgramMappingRepository.UpdateProgramMapping(updatedProgramMapping, username, modifiedAt);
                updatedMapping = _ProgramMappingRepository.GetProgramMappingByOriginalProgramName(testProgramName);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedMapping, _GetJsonSettings()));
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ProgramMappingsDto), "Id");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }
    }
}