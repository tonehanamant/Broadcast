using Common.Services.ApplicationServices;
using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Helpers
{
    public interface ICsvHelper : IApplicationService
    {
        /// <summary>
        /// Set up the file parser on the file path provided
        /// </summary>
        /// <param name="filePath">FilePath to process</param>
        /// <param name="result">WWTVOutboundFileValidationResult object to load the errors on</param>
        /// <returns>TextFieldParser object</returns>
        TextFieldParser SetupCSVParser(string filePath, WWTVOutboundFileValidationResult result);

        /// <summary>
        /// Gets the file header
        /// </summary>
        /// <param name="parser">TextFieldParser object</param>
        /// <returns>List of columns founded in the header of the file</returns>
        List<string> GetFileHeader(TextFieldParser parser);

        /// <summary>
        /// Validate required columns and returns list of required columns not found
        /// </summary>
        /// <param name="fileColumns">List of the file columns</param>
        /// <param name="requiredFields">List of required columns to check</param>
        /// <returns>Validation errors</returns>
        List<string> ValidateRequiredColumns(List<string> fileColumns, List<string> requiredFields);

        /// <summary>
        /// Gets the header dictionary
        /// </summary>
        /// <param name="fileColumns">List of file columns</param>
        /// <param name="requiredColumns">List of frequired columns</param>
        /// <returns>Dictionary with key=column name and value=position in the file</returns>
        Dictionary<string, int> GetHeaderDictionary(List<string> fileColumns, List<string> requiredColumns);

        /// <summary>
        /// Validates a row checking if the required columns contain data
        /// </summary>
        /// <param name="fields">File columns</param>
        /// <param name="headers">Dictionary of the required column index</param>
        /// <param name="requiredFileds">Required columns list</param>
        /// <param name="rowNumber">Row number</param>
        /// <returns>Validation errors</returns>
        List<string> GetRowValidationResults(string[] fields, Dictionary<string, int> headers, List<string> requiredFileds, int rowNumber);
    }
    
    public class CsvHelper : ICsvHelper
    {
        public CsvHelper() { }

        /// <summary>
        /// Set up the file parser on the file path provided
        /// </summary>
        /// <param name="filePath">FilePath to process</param>
        /// <param name="result">WWTVOutboundFileValidationResult object to load the errors on</param>
        /// <returns>TextFieldParser object</returns>
        public TextFieldParser SetupCSVParser(string filePath, WWTVOutboundFileValidationResult result)
        {
            var parser = new TextFieldParser(filePath);
            if (parser.EndOfData)
            {
                result.ErrorMessages.Add($"No data found in file {filePath}");
                return null;
            }

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

        /// <summary>
        /// Gets the file header
        /// </summary>
        /// <param name="parser">TextFieldParser object</param>
        /// <returns>List of columns founded in the header of the file</returns>
        public List<string> GetFileHeader(TextFieldParser parser)
        {
            var fileColumns = parser.ReadFields().ToList();

            //skip the first row if it's the copyright one
            if (fileColumns.Any() && fileColumns[0].Contains("Copyright"))
            {
                fileColumns = parser.ReadFields().ToList();
            }

            return fileColumns;
        }

        /// <summary>
        /// Validate required columns and returns list of required columns not found
        /// </summary>
        /// <param name="fileColumns">List of the file columns</param>
        /// <param name="requiredFields">List of required columns to check</param>
        /// <returns>Validation errors</returns>
        public List<string> ValidateRequiredColumns(List<string> fileColumns, List<string> requiredFields)
        {
            var validationErrors = new List<string>();
            Dictionary<string, int> headerDict = new Dictionary<string, int>();

            var missingColumns = requiredFields.Where(f => !fileColumns.Contains(f)).ToList();

            validationErrors.AddRange(missingColumns.Select(c => $"Could not find required column {c}."));

            return validationErrors;
        }

        /// <summary>
        /// Gets the header dictionary
        /// </summary>
        /// <param name="fileColumns">List of file columns</param>
        /// <param name="requiredColumns">List of frequired columns</param>
        /// <returns>Dictionary with key=column name and value=position in the file</returns>
        public Dictionary<string, int> GetHeaderDictionary(List<string> fileColumns, List<string> requiredColumns)
        {
            var headerDict = new Dictionary<string, int>();

            foreach (var header in requiredColumns)
            {
                int headerItemIndex = fileColumns.IndexOf(header);
                if (headerItemIndex >= 0)
                {
                    headerDict.Add(header, headerItemIndex);
                    continue;
                }
            }

            return headerDict;
        }

        /// <summary>
        /// Validates a row checking if the required columns contain data
        /// </summary>
        /// <param name="fields">File columns</param>
        /// <param name="headers">Dictionary of the required column index</param>
        /// <param name="requiredFileds">Required columns list</param>
        /// <param name="rowNumber">Row number</param>
        /// <returns>Validation errors</returns>
        public List<string> GetRowValidationResults(string[] fields, Dictionary<string, int> headers, List<string> requiredFileds, int rowNumber)
        {
            var rowValidationErrors = new List<string>();

            foreach (string field in requiredFileds)
            {
                if (string.IsNullOrWhiteSpace(fields[headers[field]]))
                {
                    rowValidationErrors.Add($"Required field {field} is null or empty in row {rowNumber}");
                }
            }

            return rowValidationErrors;
        }
    }
}
