using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public interface IPostLogBaseFileConverter : IApplicationService
    {
        Dictionary<string, int> ValidateAndSetupHeaders(TextFieldParser parser, List<string> fileFields, List<string> requiredFields);
        TextFieldParser SetupCSVParser(Stream rawStream);
        TextFieldParser SetupCSVParser(string filePath);
        List<string> GetFileHeader(TextFieldParser parser);
        List<string> GetColumnValidationResults(List<string> fileColumns, List<string> requiredFields);
        Dictionary<string, int> GetHeaderDictionary(List<string> fileColumns, List<string> fileFields);
        List<string> GetRowValidationResults(string[] fields, Dictionary<string, int> headers, List<string> requiredFileds, int rowNumber);
        void SetSpotLengths(TrackerFileDetail detail, string spot_length);
        void ValidateSigmaFieldData(string[] fields, Dictionary<string, int> headers, int rowNumber, List<string> requiredFields);
    }


    public class PostLogBaseFileConverter : IPostLogBaseFileConverter
    {
        private readonly Dictionary<int, int> _SpotLengthsAndIds;
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        public PostLogBaseFileConverter(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
            _SpotLengthsAndIds = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsByDuration();
        }
        
        public void ValidateSigmaFieldData(string[] fields, Dictionary<string, int> headers, int rowNumber, List<string> requiredFields)
        {
            var rowValidationErrors = GetRowValidationResults(fields, headers, requiredFields, rowNumber);
            if (rowValidationErrors.Any())
            {
                string message = "";
                rowValidationErrors.ForEach(err => message += err + "<br />" + Environment.NewLine);
                throw new ExtractDetectionException(message);
            }
        }

        public Dictionary<string, int> ValidateAndSetupHeaders(TextFieldParser parser, List<string> fileFields, List<string> requiredFields)
        {
            var fileColumns = GetFileHeader(parser);

            var validationErrors = GetColumnValidationResults(fileColumns, requiredFields);

            if (validationErrors.Any())
            {
                string message = "";
                validationErrors.ForEach(err => message += err + "<br />" + Environment.NewLine);
                throw new ExtractDetectionException(message);
            }

            return GetHeaderDictionary(fileColumns, fileFields);
        }

        public TextFieldParser SetupCSVParser(Stream rawStream)
        {
            var parser = new TextFieldParser(rawStream);
            if (parser.EndOfData)
            {
                throw new Exception("File does not contain valid Sigma detail data.");
            }

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

        public TextFieldParser SetupCSVParser(string filePath)
        {
            var parser = new TextFieldParser(filePath);
            if (parser.EndOfData)
            {
                throw new ApplicationException($"No data found in file {filePath}");
            }

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

        public List<string> GetColumnValidationResults(List<string> fileColumns, List<string> requiredFields)
        {
            var validationErrors = new List<string>();
            Dictionary<string, int> headerDict = new Dictionary<string, int>();

            var missingColumns = requiredFields.Where(f => !fileColumns.Contains(f)).ToList();

            validationErrors.AddRange(missingColumns.Select(c => $"Could not find required column {c}."));

            return validationErrors;
        }

        public Dictionary<string, int> GetHeaderDictionary(List<string> fileColumns, List<string> fileFields)
        {
            var headerDict = new Dictionary<string, int>();

            foreach (var header in fileFields)
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

        public void SetSpotLengths(TrackerFileDetail detail, string spot_length)
        {
            spot_length = spot_length.Trim().Replace(":", "");

            if (!int.TryParse(spot_length, out int spotLength))
            {
                throw new ExtractDetectionException(string.Format("Invalid spot length '{0}' found.", spotLength));
            }

            if (!_SpotLengthsAndIds.ContainsKey(spotLength))
                throw new ExtractDetectionException(string.Format("Invalid spot length '{0}' found.", spotLength));

            detail.SpotLength = spotLength;
            detail.SpotLengthId = _SpotLengthsAndIds[spotLength];
        }

    }
}
