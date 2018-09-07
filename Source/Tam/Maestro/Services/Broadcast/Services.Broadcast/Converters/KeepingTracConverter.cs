using Common.Services.ApplicationServices;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public interface IKeepingTracConverter : IApplicationService
    {
        List<string> GetValidationResults(string filePath);
    }

    public class KeepingTracConverter : IKeepingTracConverter
    {
        private static readonly List<string> _RequiredKeepingTrac = new List<string>()
        {
             "Estimate",
             "Station",
             "Air Date",
             "Air Time",
             "Air ISCI",
             "Demographic",
             "Act Ratings",
             "Act Impression"
        };
        private readonly IPostLogBaseFileConverter _PostLogBaseFileConverter;

        public KeepingTracConverter(IPostLogBaseFileConverter postLogBaseFileConverter)
        {
            _PostLogBaseFileConverter = postLogBaseFileConverter;
        }

        /// <summary>
        /// Validates a KeepingTrac file
        /// </summary>
        /// <param name="filePath">FilePath for the file to validate</param>
        /// <returns>List of validation errors</returns>
        public List<string> GetValidationResults(string filePath)
        {
            var validationResults = new List<string>();
            TextFieldParser parser = null;

            try
            {
                parser = _PostLogBaseFileConverter.SetupCSVParser(filePath);
                var fileColumns = _PostLogBaseFileConverter.GetFileHeader(parser);
                validationResults.AddRange(_GetColumnValidationResults(fileColumns));
                if (validationResults.Any())
                {
                    return validationResults;
                }
                int rowNumber = 1;
                var headers = _PostLogBaseFileConverter.GetHeaderDictionary(fileColumns, _RequiredKeepingTrac);
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    var rowValidationErrors = _PostLogBaseFileConverter.GetRowValidationResults(fields, headers, _RequiredKeepingTrac, rowNumber);
                    validationResults.AddRange(rowValidationErrors);
                    rowNumber++;
                }

            }
            catch (Exception e)
            {
                validationResults.Add(e.Message);
            }
            finally
            {
                if (parser != null)
                {
                    parser.Close();
                }
            }

            return validationResults;
        }

        private List<string> _GetColumnValidationResults(List<string> fileColumns)
        {
            var validationErrors = new List<string>();
            Dictionary<string, int> headerDict = new Dictionary<string, int>();

            var missingColumns = _RequiredKeepingTrac.Where(f => !fileColumns.Contains(f)).ToList();

            validationErrors.AddRange(missingColumns.Select(c => $"Could not find required column {c}."));

            return validationErrors;
        }
    }
}
