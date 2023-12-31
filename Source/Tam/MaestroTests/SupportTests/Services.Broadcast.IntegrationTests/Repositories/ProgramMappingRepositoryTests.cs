﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Unity;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class ProgramMappingRepositoryTests
    {
        private readonly IGenreCache _GenreCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IGenreCache>();
        private readonly IShowTypeCache _ShowTypeCache = IntegrationTestApplicationServiceFactory.Instance.Resolve<IShowTypeCache>();
        private readonly IProgramMappingRepository _ProgramMappingRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateProgramMapping()
        {
            var createdBy = "testUser";
            var createdAt = new DateTime(2020, 10, 17, 8, 32, 12);
            var newProgramMapping = new ProgramMappingsDto
            {
                OriginalProgramName = "TestOriginalProgramName",
                OfficialProgramName = "TestOfficialProgramName",
                OfficialGenre = _GenreCache.GetMaestroGenreByName("News"),
                OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Mini-Movie")
            };

            List<ProgramMappingsDto> createdMappings;
            using (new TransactionScopeWrapper())
            {
                _ProgramMappingRepository.CreateProgramMappings(new List<ProgramMappingsDto> { newProgramMapping }, createdBy, createdAt);
                createdMappings = _ProgramMappingRepository.GetProgramMappingsByOriginalProgramNames(new List<string> { newProgramMapping.OriginalProgramName });
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(createdMappings, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryProgramMappings()
        {
            List<ProgramMappingsDto> programMappings = new List<ProgramMappingsDto>();
            string createdBy = "testUser";
            var createdAt = new DateTime(2022, 11, 08, 8, 32, 12);
            var newProgramMappings = new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Parasite",
                    OfficialProgramName = "Parasite",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("Entertainment"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Movie")
                },
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Jojo Rabbit",
                    OfficialProgramName = "Jojo Rabbit",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("Entertainment"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Movie")
                },
                new ProgramMappingsDto
                {
                     OriginalProgramName = "Harley Quinn: Birds of Prey",
                    OfficialProgramName = "Harley Quinn: Birds of Prey",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("Entertainment"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Movie")
                },
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Joker",
                    OfficialProgramName = "Joker",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("Entertainment"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Movie")
                }
            };
            using (new TransactionScopeWrapper())
            {
              _ProgramMappingRepository.UploadMasterProgramMappings(newProgramMappings, createdBy, createdAt);  
              programMappings = _ProgramMappingRepository.GetInventoryProgramMappings();             
            }
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(programMappings, _GetJsonSettings()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateProgramMapping()
        {
            var username = "testUser";
            var createdAt = new DateTime(2020, 10, 17, 8, 32, 12);
            var modifiedAt = new DateTime(2020, 10, 20, 8, 32, 12);
            var newProgramMappings = new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Program A",
                    OfficialProgramName = "Program A official",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("News"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Mini-Movie")
                },
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Program B",
                    OfficialProgramName = "Program B official",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("News"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Mini-Movie")
                },
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Program C",
                    OfficialProgramName = "Program C official",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("News"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Mini-Movie")
                },
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Program D",
                    OfficialProgramName = "Program D official",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("News"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Mini-Movie")
                }
            };
            var updatedProgramMappings = new List<ProgramMappingsDto>
            {
                // OfficialProgramName was changed
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Program B",
                    OfficialProgramName = "Program B official v1",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("News"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Mini-Movie")
                },

                // OfficialGenre was changed
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Program C",
                    OfficialProgramName = "Program C official",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("Comedy"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Mini-Movie")
                },

                // OfficialShowType was changed
                new ProgramMappingsDto
                {
                    OriginalProgramName = "Program D",
                    OfficialProgramName = "Program D official",
                    OfficialGenre = _GenreCache.GetMaestroGenreByName("News"),
                    OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName("Series")
                }
            };
            var createdPrograms = newProgramMappings.Select(x => x.OriginalProgramName);

            List<ProgramMappingsDto> updatedMappings;

            using (new TransactionScopeWrapper())
            {
                _ProgramMappingRepository.CreateProgramMappings(newProgramMappings, username, createdAt);

                var programMappingIdByOriginalProgramName = _ProgramMappingRepository
                    .GetProgramMappingsByOriginalProgramNames(createdPrograms)
                    .ToDictionary(x => x.OriginalProgramName, x => x.Id);
                updatedProgramMappings.ForEach(x => x.Id = programMappingIdByOriginalProgramName[x.OriginalProgramName]);

                _ProgramMappingRepository.UpdateProgramMappings(updatedProgramMappings, username, modifiedAt);
                updatedMappings = _ProgramMappingRepository.GetProgramMappingsByOriginalProgramNames(createdPrograms);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedMappings, _GetJsonSettings()));
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