using Common.Services.ApplicationServices;
using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Helpers
{
    public interface IExcelHelper : IApplicationService
    {
        /// <summary>
        /// Checks if a certain file has .xlsx extention
        /// </summary>
        /// <param name="fileInfo">FileInfo Object</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object for error loading</param>
        void CheckForExcelFileType(FileInfo fileInfo, WWTVOutboundFileValidationResult fileValidationResult);

        /// <summary>
        /// Get worksheet to process based on worksheet name
        /// </summary>
        /// <param name="fileInfo">FileInfo object</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object for error loading</param>/// <param name="fileValidationResult"></param>
        /// <param name="tabName">Worksheet name</param>
        /// <returns>ExcelWorksheet object</returns>
        ExcelWorksheet GetWorksheetToProcess(FileInfo fileInfo, WWTVOutboundFileValidationResult fileValidationResult, string tabName);

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

        /// <summary>
        /// Checks if a certain file has .xlsx extention
        /// </summary>
        /// <param name="fileInfo">FileInfo Object</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object for error loading</param>
        public void CheckForExcelFileType(FileInfo fileInfo, WWTVOutboundFileValidationResult fileValidationResult)
        {
            if (!fileInfo.Extension.Equals(".xlsx", StringComparison.InvariantCultureIgnoreCase))
            {
                fileValidationResult.ErrorMessages.Add($"Unknown extension type for file {fileValidationResult.FilePath}");
                fileValidationResult.Status = FileProcessingStatusEnum.Invalid;
            }
        }

        /// <summary>
        /// Get worksheet to process based on worksheet name
        /// </summary>
        /// <param name="fileInfo">FileInfo object</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object for error loading</param>/// <param name="fileValidationResult"></param>
        /// <param name="tabName">Worksheet name</param>
        /// <returns>ExcelWorksheet object</returns>
        public ExcelWorksheet GetWorksheetToProcess(FileInfo fileInfo, WWTVOutboundFileValidationResult fileValidationResult, string tabName)
        {
            ExcelWorksheet result = null;
            var package = new ExcelPackage(fileInfo, true);
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                if (worksheet.Name.Equals(tabName))
                {                    
                    result = worksheet;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if all the required columns are present in the worksheet 
        /// </summary>
        /// <param name="requiredColumns">List of required column names</param>
        /// <param name="worksheet">Excel worksheet</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object for error loading</param>
        /// <returns>Dictionary with the required columns position</returns>
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

        /// <summary>
        /// Checks if the worksheet has data missing on required columns
        /// </summary>
        /// <param name="fileHeaders">Column names</param>
        /// <param name="fileColumns">Excel columns dictionary</param>
        /// <param name="worksheet">Excel worksheet to check</param>
        /// <param name="fileValidationResult">WWTVOutboundFileValidationResult object to load the errors</param>
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
