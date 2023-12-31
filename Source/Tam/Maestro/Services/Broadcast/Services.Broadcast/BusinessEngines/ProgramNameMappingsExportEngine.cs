﻿using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Broadcast.BusinessEngines
{
    public interface IProgramNameMappingsExportEngine
    {
        /// <summary>
        /// Generate a file for export.
        /// </summary>
        ExcelPackage GenerateExportFile(List<ProgramMappingsDto> mappings);
    }

    public class ProgramNameMappingsExportEngine : BroadcastBaseClass, IProgramNameMappingsExportEngine
    {
        private readonly Lazy<bool> _PROGRAMGENRERELATIONV2;
        public ProgramNameMappingsExportEngine(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _PROGRAMGENRERELATIONV2 = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.PROGRAM_GENRE_RELATION_V_2));
        }
        /// <summary>
        /// The types of columns used in this export
        /// </summary>
        public enum ColumnTypeEnum
        {
            Text,
            DateTime
        }

        /// <summary>
        /// Local column descriptor
        /// </summary>
        public class ColumnDescriptor
        {
            public int ColumnIndex { get; set; }
            public string Name { get; set; }
            public ColumnTypeEnum ColumnType { get; set; }
            public double Width { get; set; }
        }

        /// <summary>
        /// The base column headers
        /// </summary>
        private readonly List<ColumnDescriptor> _BaseColumnHeaders = new List<ColumnDescriptor>
        {
            new ColumnDescriptor {ColumnIndex = 1, Name = "Rate Card Program Name", ColumnType = ColumnTypeEnum.Text, Width = 38.86},
            new ColumnDescriptor {ColumnIndex = 2, Name = "Official Program Name", ColumnType = ColumnTypeEnum.Text, Width = 32.14},
            new ColumnDescriptor {ColumnIndex = 3, Name = "Official Genre", ColumnType = ColumnTypeEnum.Text, Width = 13.29},
            new ColumnDescriptor {ColumnIndex = 4, Name = "Official Show Type", ColumnType = ColumnTypeEnum.Text, Width = 17.14},
            new ColumnDescriptor {ColumnIndex = 5, Name = "Last Updated By", ColumnType = ColumnTypeEnum.Text, Width = 22.71},
            new ColumnDescriptor {ColumnIndex = 6, Name = "Last Updated", ColumnType = ColumnTypeEnum.DateTime, Width = 20},
        };

        /// <summary>
        /// The base column headers without genre
        /// </summary>
        private readonly List<ColumnDescriptor> _BaseColumnHeadersWithoutGenre = new List<ColumnDescriptor>
        {
            new ColumnDescriptor {ColumnIndex = 1, Name = "Rate Card Program Name", ColumnType = ColumnTypeEnum.Text, Width = 38.86},
            new ColumnDescriptor {ColumnIndex = 2, Name = "Official Program Name", ColumnType = ColumnTypeEnum.Text, Width = 32.14},            
            new ColumnDescriptor {ColumnIndex = 3, Name = "Official Show Type", ColumnType = ColumnTypeEnum.Text, Width = 17.14},
            new ColumnDescriptor {ColumnIndex = 4, Name = "Last Updated By", ColumnType = ColumnTypeEnum.Text, Width = 22.71},
            new ColumnDescriptor {ColumnIndex = 5, Name = "Last Updated", ColumnType = ColumnTypeEnum.DateTime, Width = 20},
        };

        private const int ROW_NUMBER_GENERATED_TIMESTAMP = 1;
        private const int ROW_NUMBER_TABLE_START = 3;

        /// <inheritdoc />
        public ExcelPackage GenerateExportFile(List<ProgramMappingsDto> mappings)
        {
            var columnDescriptors = new List<ColumnDescriptor>();
            if(_PROGRAMGENRERELATIONV2.Value)
            {
                columnDescriptors = _GetColumnDescriptorsWithoutGenres();
            }
            else
            {
                columnDescriptors = _GetColumnDescriptors();
            }
            var lines = new List<List<object>>();
            if (_PROGRAMGENRERELATIONV2.Value)
            {
                lines = _TransformToExportLinesWithoutGenres(mappings);
            }
            else
            {
                lines = _TransformToExportLines(mappings);
            }
            var excelPackage = new ExcelPackage();
            var excelProgramMappingsTab = excelPackage.Workbook.Worksheets.Add("Program Mappings");

            // Add the date generated time stamp.
            excelProgramMappingsTab.Cells[ROW_NUMBER_GENERATED_TIMESTAMP, 1].Value = _GetDateGeneratedCellValue();

            var rowIndex = ROW_NUMBER_TABLE_START;
            // add the header
            for (var i = 0; i < columnDescriptors.Count; i++)
            {
                excelProgramMappingsTab.Cells[rowIndex, (i + 1)].Value = columnDescriptors[i].Name;
                excelProgramMappingsTab.Column((i + 1)).Width = columnDescriptors[i].Width;
                excelProgramMappingsTab.Column((i + 1)).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                excelProgramMappingsTab.Column((i + 1)).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            }

            // add the lines
            lines.ForEach(line =>
            {
                rowIndex++;
                for (var i = 0; i < columnDescriptors.Count; i++)
                {
                    switch (columnDescriptors[i].ColumnType)
                    {
                        case ProgramNameMappingsExportEngine.ColumnTypeEnum.DateTime:
                            excelProgramMappingsTab.Cells[rowIndex, (i + 1)].Style.Numberformat.Format = "MM/dd/yyyy HH:mm:ss";
                            break;
                    }
                    excelProgramMappingsTab.Cells[rowIndex, (i + 1)].Value = line[i];
                }
            });

            return excelPackage;
        }

        protected List<ColumnDescriptor> _GetColumnDescriptors()
        {
            var headers = _BaseColumnHeaders;
            return headers;
        }

        protected List<ColumnDescriptor> _GetColumnDescriptorsWithoutGenres()
        {
            var headers = _BaseColumnHeadersWithoutGenre;
            return headers;
        }



        protected string _GetDateGeneratedCellValue()
        {
            var formattedDateGenerated = _GetCurrentDateTime().ToString("MM/dd/yyyy HH:mm:ss");
            var dateGeneratedString = $"Date Generated : {formattedDateGenerated}";
            return dateGeneratedString;
        }

        protected List<List<object>> _TransformToExportLines(List<ProgramMappingsDto> mappings)
        {
            var lineStrings = new ConcurrentBag<List<object>>();
            Parallel.ForEach(mappings, (mapping) =>
            {
                var lastUpdatedBy = string.IsNullOrWhiteSpace(mapping.ModifiedBy) ? mapping.CreatedBy : mapping.ModifiedBy;
                var lastUpdatedAt = mapping.ModifiedAt ?? mapping.CreatedAt;

                var lineColumnValues = new List<object>
                    {
                        mapping.OriginalProgramName,
                        mapping.OfficialProgramName,
                        mapping.OfficialGenre.Name,
                        mapping.OfficialShowType.Name,
                        lastUpdatedBy,
                        lastUpdatedAt
                    };
                lineStrings.Add(lineColumnValues);
            }
           );
            var rateCardProgramNameIndex = _BaseColumnHeaders.First(s => s.Name.Equals("Rate Card Program Name")).ColumnIndex - 1; // 0 indexed
            var officialCardProgramNameIndex = _BaseColumnHeaders.First(s => s.Name.Equals("Official Program Name")).ColumnIndex - 1; // 0 indexed

            var orderedLines = lineStrings
                .OrderBy(s => s[rateCardProgramNameIndex])
                .ThenBy(s => s[officialCardProgramNameIndex])
                .ToList();

            return orderedLines;
        }

        protected List<List<object>> _TransformToExportLinesWithoutGenres(List<ProgramMappingsDto> mappings)
        {
            var lineStrings = new ConcurrentBag<List<object>>();
            Parallel.ForEach(mappings, (mapping) =>
            {
                var lastUpdatedBy = string.IsNullOrWhiteSpace(mapping.ModifiedBy) ? mapping.CreatedBy : mapping.ModifiedBy;
                var lastUpdatedAt = mapping.ModifiedAt ?? mapping.CreatedAt;

                var lineColumnValues = new List<object>
                    {
                        mapping.OriginalProgramName,
                        mapping.OfficialProgramName,                        
                        mapping.OfficialShowType.Name,
                        lastUpdatedBy,
                        lastUpdatedAt
                    };
                lineStrings.Add(lineColumnValues);
            }
           );
            var rateCardProgramNameIndex = _BaseColumnHeaders.First(s => s.Name.Equals("Rate Card Program Name")).ColumnIndex - 1; // 0 indexed
            var officialCardProgramNameIndex = _BaseColumnHeaders.First(s => s.Name.Equals("Official Program Name")).ColumnIndex - 1; // 0 indexed

            var orderedLines = lineStrings
                .OrderBy(s => s[rateCardProgramNameIndex])
                .ThenBy(s => s[officialCardProgramNameIndex])
                .ToList();

            return orderedLines;
        }
    }
}