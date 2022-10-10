using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    public class ProgramNameMappingsExportEngineUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetColumnDescriptors()
        {
            
            var engine = new ProgramNameMappingsExportEngineUnitTestClass(null,null);

            var result = engine.UT_GetColumnDescriptors();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]         
        public void UT_GetColumnDescriptorsWithoutGenres()
        {
            var engine = new ProgramNameMappingsExportEngineUnitTestClass(null, null);
            var result = engine.UT_GetColumnDescriptorsWithoutGenres();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDateGeneratedCellValue()
        {
            var engine = new ProgramNameMappingsExportEngineUnitTestClass(null, null)
            {
                UT_CurrentDateTime = new DateTime(2020, 10, 17, 7, 31, 26)
            };

            var result = engine.UT_GetDateGeneratedCellValue();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TransformToExportLines_CreatedDates()
        {
            var createDateTime = new DateTime(2020, 10, 17, 7, 31, 26);
            DateTime? modifiedDateTime = null;
            const int testProgramCount = 10;
            var mappings = _GetTestPrograms(testProgramCount, createDateTime, modifiedDateTime);
            var engine = new ProgramNameMappingsExportEngineUnitTestClass(null, null)
            {
                UT_CurrentDateTime = createDateTime
            };

            var result = engine.UT_TransformToExportLines(mappings);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TransformToExportLines_ModifiedDates()
        {
            var createDateTime = new DateTime(2020, 10, 17, 7, 31, 26);
            DateTime? modifiedDateTime = new DateTime(2020, 10, 20, 7, 31, 26);
            ;
            const int testProgramCount = 10;
            var mappings = _GetTestPrograms(testProgramCount, createDateTime, modifiedDateTime);
            var engine = new ProgramNameMappingsExportEngineUnitTestClass(null, null)
            {
                UT_CurrentDateTime = createDateTime
            };

            var result = engine.UT_TransformToExportLines(mappings);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TransformToExportLines_WithoutGenre()
        {
            var createDateTime = new DateTime(2020, 10, 17, 7, 31, 26);
            DateTime? modifiedDateTime = new DateTime(2020, 10, 20, 7, 31, 26);
            ;
            const int testProgramCount = 10;
            var mappings = _GetTestPrograms(testProgramCount, createDateTime, modifiedDateTime);
            var engine = new ProgramNameMappingsExportEngineUnitTestClass(null, null)
            {
                UT_CurrentDateTime = createDateTime
            };

            var result = engine.UT_TransformToExportLinesWithoutGenres(mappings);                

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<ProgramMappingsDto> _GetTestPrograms(int count, DateTime createdDateTime, DateTime? modifiedDateTime)
        {
            var programs = new List<ProgramMappingsDto>();
            for (var i = 0; i < count; i++)
            {
                var program = new ProgramMappingsDto
                {
                    Id = 1,
                    OriginalProgramName = $"OriginalProgramName{i}",
                    OfficialProgramName = $"OfficialProgramName{i}",
                    OfficialGenre = new Genre {Name = $"OfficialGenre{i}"},
                    OfficialShowType = new ShowTypeDto {Name = $"OfficialShowType{i}"},
                    CreatedBy = "TestUser",
                    CreatedAt = createdDateTime,
                    ModifiedBy = modifiedDateTime.HasValue ? "TestUser" : null,
                    ModifiedAt = modifiedDateTime
                };
                programs.Add(program);
            }
            return programs;
        }
    }
}