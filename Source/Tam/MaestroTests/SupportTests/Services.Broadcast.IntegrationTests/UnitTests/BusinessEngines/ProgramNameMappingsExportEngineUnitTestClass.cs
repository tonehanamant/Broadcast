using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class ProgramNameMappingsExportEngineUnitTestClass : ProgramNameMappingsExportEngine
    {
        public List<ColumnDescriptor> UT_GetColumnDescriptors()
        {
            return _GetColumnDescriptors();
        }

        public string UT_GetDateGeneratedCellValue()
        {
            return _GetDateGeneratedCellValue();
        }

        public List<List<object>> UT_TransformToExportLines(List<ProgramMappingsDto> mappings)
        {
            return _TransformToExportLines(mappings);
        }

        public DateTime? UT_CurrentDateTime { get; set; }

        protected override DateTime _GetCurrentDateTime()
        {
            return UT_CurrentDateTime ?? base._GetCurrentDateTime();
        }
    }
}