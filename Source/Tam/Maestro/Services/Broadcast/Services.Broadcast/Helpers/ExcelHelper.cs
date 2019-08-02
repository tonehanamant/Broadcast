using Common.Services.ApplicationServices;
using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Helpers
{
    public interface IExcelHelper : IApplicationService
    {
        /// <summary>
        /// Get worksheet to process based on worksheet name
        /// </summary>
        /// <param name="fileInfo">FileInfo object</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object for error loading</param>
        /// <param name="tabName">Optional Worksheet name</param>
        /// <returns>ExcelWorksheet object</returns>
        ExcelWorksheet GetWorksheetToProcess(FileInfo fileInfo, WWTVOutboundFileValidationResult fileValidationResult, string tabName = null);

        /// <summary>
        /// Checks if all the required columns are present in the worksheet 
        /// </summary>
        /// <param name="requiredColumns">List of required column names</param>
        /// <param name="worksheet">Excel worksheet</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object for error loading</param>
        /// <returns>Dictionary with the required columns position</returns>
        Dictionary<string, int> ValidateHeaders(List<string> requiredColumns, ExcelWorksheet worksheet, WWTVOutboundFileValidationResult fileValidationResult);

        /// <summary>
        /// Checks if the worksheet has data missing on required columns
        /// </summary>
        /// <param name="fileHeaders">Column names</param>
        /// <param name="fileColumns">Excel columns dictionary</param>
        /// <param name="worksheet">Excel worksheet to check</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object to load the errors</param>
        void CheckMissingDataOnRequiredColumns(List<string> fileHeaders, Dictionary<string, int> fileColumns, ExcelWorksheet worksheet, WWTVOutboundFileValidationResult fileValidationResult);
    }
    
    public class ExcelHelper : IExcelHelper
    {
        ///<inheritdoc/>
        public ExcelWorksheet GetWorksheetToProcess(FileInfo fileInfo, WWTVOutboundFileValidationResult fileValidationResult, string tabName = null)
        {
            var package = new ExcelPackage(fileInfo, true);
            if (string.IsNullOrWhiteSpace(tabName))
            {
                return package.Workbook.Worksheets.First();
            }
            else
            {
                return package.Workbook.Worksheets.FirstOrDefault(x => x.Name.Equals(tabName));
            }
        }

        ///<inheritdoc/>
        public Dictionary<string, int> ValidateHeaders(List<string> requiredColumns, ExcelWorksheet worksheet, WWTVOutboundFileValidationResult fileValidationResult)
        {
            var foundColumns = new Dictionary<string, int>();
            foreach (var header in requiredColumns)
            {
                for (var column = 1; column <= worksheet.Dimension.End.Column; column++)
                {
                    var cellValue = (string)worksheet.Cells[1, column].Value;

                    if (cellValue == null || !cellValue.Trim().Equals(header, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    foundColumns.Add(header, column);
                    break;
                }

                if (!foundColumns.ContainsKey(header))
                {
                    fileValidationResult.ErrorMessages.Add(string.Format("Could not find header for column {0} in file {1}", header, fileValidationResult.FilePath));
                }
            }
            if (foundColumns.Count != requiredColumns.Count)
            {
                fileValidationResult.Status = FileProcessingStatusEnum.Invalid;
            }
            return foundColumns;
        }

        ///<inheritdoc/>
        public void CheckMissingDataOnRequiredColumns(List<string> fileHeaders, Dictionary<string, int> fileColumns, ExcelWorksheet worksheet, WWTVOutboundFileValidationResult fileValidationResult)
        {
            var hasMissingData = false;
            for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                if (_IsEmptyRow(row, worksheet))
                {
                    continue;
                }
                foreach (string name in fileHeaders)
                {
                    if (string.IsNullOrWhiteSpace(worksheet.Cells[row, fileColumns[name]].Value?.ToString()))
                    {
                        fileValidationResult.ErrorMessages.Add($"Missing '{name}' on row {row}");
                        hasMissingData = true;
                    }
                }
            }
            if (hasMissingData)
            {
                fileValidationResult.Status = FileProcessingStatusEnum.Invalid;
            }
        }

        private bool _IsEmptyRow(int row, ExcelWorksheet excelWorksheet)
        {
            for (var c = 1; c < excelWorksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrWhiteSpace(excelWorksheet.Cells[row, c].Text))
                    return false;

            return true;
        }

    }
}
