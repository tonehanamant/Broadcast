using Common.Services.ApplicationServices;
using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.ApplicationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Converters
{
    public interface ISigmaConverter
    {
        Dictionary<string, int> ValidateAndSetupHeaders(TextFieldParser parser);
        void ValidateSigmaFieldData(string[] fields, Dictionary<string, int> headers, int rowNumber);
        TextFieldParser SetupCSVParser(Stream rawStream);
    }
    public class SigmaConverter : ISigmaConverter, IPostLogPreprocessingValidator
    {
        private static readonly List<string> _RequiredSigmaFields = new List<string>()
        {
             "IDENTIFIER 1",
             "STATION",
             "DATE AIRED",
             "AIR START TIME",
             "ISCI/AD-ID"
        };

        private static readonly List<string> _AllSigmaFileFields = new List<string>()
        {
            "IDENTIFIER 1",
            "RANK",
            "DMA",
            "STATION",
            "AFFILIATION",
            "DATE AIRED",
            "AIR START TIME",
            "PROGRAM NAME",
            "DURATION",
            "ISCI/AD-ID",
            "PRODUCT",
            "RELEASE NAME"
        };
        public Dictionary<string, int> ValidateAndSetupHeaders(TextFieldParser parser)
        {
            var fileColumns = _GetFileColumns(parser);

            var validationErrors = _GetColumnValidationResults(fileColumns);

            if (validationErrors.Any())
            {
                string message = "";
                validationErrors.ForEach(err => message += err + "<br />" + Environment.NewLine);
                throw new ExtractBvsException(message);
            }            

            return _GetHeaderDictionary(fileColumns);
        }

        private Dictionary<string, int> _GetHeaderDictionary(List<string> fileColumns)
        {
            var headerDict = new Dictionary<string, int>();

            foreach (var header in _AllSigmaFileFields)
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

        private List<string> _GetColumnValidationResults(List<string> fileColumns)
        {
            var validationErrors = new List<string>();
            Dictionary<string, int> headerDict = new Dictionary<string, int>();

            var missingColumns = _AllSigmaFileFields.Where(f => !fileColumns.Contains(f)).ToList();

            validationErrors.AddRange(missingColumns.Select(c => $"Could not find required column {c}."));

            return validationErrors;
        }

        private List<string> _GetFileColumns(TextFieldParser parser)
        {
            var fileColumns = parser.ReadFields().ToList();

            //skip the first row if it's the copyright one
            if (fileColumns.Any() && fileColumns[0].Contains("Copyright"))
            {
                fileColumns = parser.ReadFields().ToList();
            }

            return fileColumns;
        }

        public void ValidateSigmaFieldData(string[] fields, Dictionary<string, int> headers, int rowNumber)
        {
            var rowValidationErrors = _GetRowValidationResults(fields, headers, rowNumber);
            if (rowValidationErrors.Any())
            {
                string message = "";
                rowValidationErrors.ForEach(err => message += err + "<br />" + Environment.NewLine);
                throw new ExtractBvsException(message);
            }
        }

        private List<string> _GetRowValidationResults(string[] fields, Dictionary<string, int> headers, int rowNumber)
        {
            var rowValidationErrors = new List<string>();

            foreach (string field in _RequiredSigmaFields)
            {
                if (string.IsNullOrWhiteSpace(fields[headers[field]]))
                {
                    rowValidationErrors.Add($"Required field {field} is null or empty in row {rowNumber}");
                }
            }

            return rowValidationErrors;
        }

        public TextFieldParser SetupCSVParser(Stream rawStream)
        {
            var parser = new TextFieldParser(rawStream);
            if (parser.EndOfData)
            {
                throw new ExtractBvsExceptionEmptyFiles();
            }

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

        public TextFieldParser SetupCSVParser (string filePath)
        {
            var parser = new TextFieldParser(filePath);
            if (parser.EndOfData)
            {
                throw new ApplicationException($"No data found in file {filePath}");
            }

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

        public List<string> GetValidationResults(string filePath)
        {
            var validationResults = new List<string>();
            TextFieldParser parser = null;

            try
            {
                parser = SetupCSVParser(filePath);
                var fileColumns = _GetFileColumns(parser);
                validationResults.AddRange(_GetColumnValidationResults(fileColumns));
                if (validationResults.Any())
                {
                    return validationResults;
                }
                int rowNumber = 1;
                var headers = _GetHeaderDictionary(fileColumns);
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    var rowValidationErrors = _GetRowValidationResults(fields, headers, rowNumber);
                    validationResults.AddRange(rowValidationErrors);
                    rowNumber++;
                }

            }
            catch(Exception e)
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
    }
}
