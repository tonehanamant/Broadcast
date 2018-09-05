using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Converters
{
    public interface ISigmaConverter : IApplicationService
    {
        TrackerFile<BvsFileDetail> ExtractSigmaData(Stream rawStream, string hash, string userName, string bvsFileName, out Dictionary<TrackerFileDetailKey<BvsFileDetail>, int> lineInfo);

        /// <summary>
        /// Extracts A to AA columns from a sigma file
        /// </summary>
        /// <param name="streamData">Streab contianing the file</param>
        /// <param name="hash">Hash of the file</param>
        /// <param name="username">User uploading the file</param>
        /// <param name="fileName">File name</param>
        /// <param name="lineInfo">Lines info</param>
        /// <returns></returns>
        TrackerFile<SpotTrackerFileDetail> ExtractSigmaDataExtended(Stream streamData, string hash, string username, string fileName, out Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> lineInfo);

        List<string> GetValidationResults(string filePath);
    }
    public class SigmaConverter : ISigmaConverter
    {
        private static readonly List<string> _RequiredSigmaFields = new List<string>()
        {
             "IDENTIFIER 1",
             "STATION",
             "DATE AIRED",
             "AIR START TIME",
             "ISCI/AD-ID"
        };

        private static readonly List<string> _BvsSigmaFileFields = new List<string>()
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

        private static readonly List<string> _SpotTrackerFileFields = new List<string>()
        {
            "CLIENT", 
            "CLIENT NAME",
            "PRODUCT",
            "RELEASE NAME",
            "ISCI/AD-ID",
            "DURATION",
            "CNTRY",
            "RANK",
            "DMA",
            "DMA CODE",
            "STATION",
            "STATION NAME",
            "AFFILIATION",
            "DATE AIRED",
            "DAY OF WEEK",
            "DAYPART",
            "AIR START TIME",
            "PROGRAM NAME",
            "ENCODE DATE",
            "ENCODE TIME",
            "REL TYPE",
            "IDENTIFIER 1",
            "IDENTIFIER 2",
            "IDENTIFIER 3",
            "SID",
            "DISCID"
        };

        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IDateAdjustmentEngine _DateAdjustmentEngine;
        private readonly Dictionary<int, int> _SpotLengthsAndIds;

        public SigmaConverter(IDataRepositoryFactory dataRepositoryFactory, IDateAdjustmentEngine dateAdjustmentEngine)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
            _DateAdjustmentEngine = dateAdjustmentEngine;
            _SpotLengthsAndIds = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
        }

        private Dictionary<string, int> _ValidateAndSetupHeaders(TextFieldParser parser, List<string> fileFields)
        {
            var fileColumns = _GetFileHeader(parser);

            var validationErrors = _GetColumnValidationResults(fileColumns);

            if (validationErrors.Any())
            {
                string message = "";
                validationErrors.ForEach(err => message += err + "<br />" + Environment.NewLine);
                throw new ExtractBvsException(message);
            }            

            return _GetHeaderDictionary(fileColumns, fileFields);
        }

        private Dictionary<string, int> _GetHeaderDictionary(List<string> fileColumns, List<string> fileFields)
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

        private List<string> _GetColumnValidationResults(List<string> fileColumns)
        {
            var validationErrors = new List<string>();
            Dictionary<string, int> headerDict = new Dictionary<string, int>();

            var missingColumns = _RequiredSigmaFields.Where(f => !fileColumns.Contains(f)).ToList();

            validationErrors.AddRange(missingColumns.Select(c => $"Could not find required column {c}."));

            return validationErrors;
        }

        private List<string> _GetFileHeader(TextFieldParser parser)
        {
            var fileColumns = parser.ReadFields().ToList();

            //skip the first row if it's the copyright one
            if (fileColumns.Any() && fileColumns[0].Contains("Copyright"))
            {
                fileColumns = parser.ReadFields().ToList();
            }

            return fileColumns;
        }

        private void _ValidateSigmaFieldData(string[] fields, Dictionary<string, int> headers, int rowNumber)
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

        private TextFieldParser _SetupCSVParser(Stream rawStream)
        {
            var parser = new TextFieldParser(rawStream);
            if (parser.EndOfData)
            {
                throw new Exception("File does not contain valid Sigma detail data.");
            }

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

        private TextFieldParser _SetupCSVParser (string filePath)
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
                parser = _SetupCSVParser(filePath);
                var fileColumns = _GetFileHeader(parser);
                validationResults.AddRange(_GetColumnValidationResults(fileColumns));
                if (validationResults.Any())
                {
                    return validationResults;
                }
                int rowNumber = 1;
                var headers = _GetHeaderDictionary(fileColumns, _BvsSigmaFileFields);
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

        public TrackerFile<BvsFileDetail> ExtractSigmaData(Stream rawStream, string hash, string userName, string bvsFileName, out Dictionary<TrackerFileDetailKey<BvsFileDetail>, int> lineInfo)
        {
            lineInfo = new Dictionary<TrackerFileDetailKey<BvsFileDetail>, int>();
            var bvsFile = new TrackerFile<BvsFileDetail>();

            int rowNumber = 0;
            using (var parser = _SetupCSVParser(rawStream))
            {
                Dictionary<string, int> headers = _ValidateAndSetupHeaders(parser, _BvsSigmaFileFields);
                while (!parser.EndOfData)
                {
                    rowNumber++;
                    var fields = parser.ReadFields();
                    _ValidateSigmaFieldData(fields, headers, rowNumber);

                    BvsFileDetail bvsDetail = _LoadBvsSigmaFileDetail(fields, headers, rowNumber);
                    lineInfo[new TrackerFileDetailKey<BvsFileDetail>(bvsDetail)] = rowNumber;
                    bvsFile.FileDetails.Add(bvsDetail);
                }
            }

            if (!bvsFile.FileDetails.Any())
            {
                throw new ExtractBvsExceptionEmptyFiles();
            }
            bvsFile.Name = bvsFileName;
            bvsFile.CreatedBy = userName;
            bvsFile.CreatedDate = DateTime.Now;
            bvsFile.FileHash = hash;
            bvsFile.StartDate = bvsFile.FileDetails.Min(x => x.DateAired).Date;
            bvsFile.EndDate = bvsFile.FileDetails.Max(x => x.DateAired).Date;

            return bvsFile;
        }

        /// <summary>
        /// Extracts A to AA columns from a sigma file
        /// </summary>
        /// <param name="streamData">Streab contianing the file</param>
        /// <param name="hash">Hash of the file</param>
        /// <param name="username">User uploading the file</param>
        /// <param name="fileName">File name</param>
        /// <param name="lineInfo">Lines info</param>
        /// <returns></returns>
        public TrackerFile<SpotTrackerFileDetail> ExtractSigmaDataExtended(Stream streamData, string hash, string username, string fileName, out Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> lineInfo)
        {
            lineInfo = new Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int>();
            var bvsFile = new TrackerFile<SpotTrackerFileDetail>();

            int rowNumber = 0;
            using (var parser = _SetupCSVParser(streamData))
            {
                Dictionary<string, int> headers = _ValidateAndSetupHeaders(parser, _SpotTrackerFileFields);
                while (!parser.EndOfData)
                {
                    rowNumber++;
                    var fields = parser.ReadFields();
                    _ValidateSigmaFieldData(fields, headers, rowNumber);

                    SpotTrackerFileDetail fileDetail = _LoadExtendedSigmaFileDetail(fields, headers, rowNumber);
                    lineInfo[new TrackerFileDetailKey<SpotTrackerFileDetail>(fileDetail)] = rowNumber;
                    bvsFile.FileDetails.Add(fileDetail);
                }
            }

            if (!bvsFile.FileDetails.Any())
            {
                throw new ExtractBvsExceptionEmptyFiles();
            }
            bvsFile.Name = fileName;
            bvsFile.CreatedBy = username;
            bvsFile.CreatedDate = DateTime.Now;
            bvsFile.FileHash = hash;
            bvsFile.StartDate = bvsFile.FileDetails.Min(x => x.DateAired).Date;
            bvsFile.EndDate = bvsFile.FileDetails.Max(x => x.DateAired).Date;

            return bvsFile;
        }

        private BvsFileDetail _LoadBvsSigmaFileDetail(string[] fields, Dictionary<string, int> headers, int row)
        {
            Dictionary<int, int> spotLengthDict = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            if (!int.TryParse(fields[headers["RANK"]].Trim(), out int rankNumber))
            {
                throw new ExtractBvsException("Invalid 'rank'", row);
            }
            var rawDate = fields[headers["DATE AIRED"]].Trim();
            var rawAiredDateTime = fields[headers["AIR START TIME"]].Trim().ToUpper();
            string someDate = rawDate + " " + rawAiredDateTime;
            if (!DateTime.TryParse(someDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                throw new ExtractBvsException("Invalid 'date aired' or 'air start time'", row);
            var time = parsedDate.TimeOfDay;

            if (!int.TryParse(fields[headers["IDENTIFIER 1"]].Trim(), out int estimateId))
            {
                throw new ExtractBvsException("Invalid 'identifier 1'", row);
            }

            BvsFileDetail bvsDetail = new BvsFileDetail()
            {
                Advertiser = fields[headers["PRODUCT"]].Trim().ToUpper(),
                Market = fields[headers["DMA"]].Trim().ToUpper(),
                Rank = rankNumber,
                Station = fields[headers["STATION"]].Trim().ToUpper(),
                Affiliate = fields[headers["AFFILIATION"]].Trim().ToUpper(),
                DateAired = parsedDate.Date,
                TimeAired = (int)time.TotalSeconds,
                NsiDate = _DateAdjustmentEngine.ConvertToNSITime(parsedDate.Date, time),
                NtiDate = _DateAdjustmentEngine.ConvertToNSITime(parsedDate.Date, time),
                ProgramName = fields[headers["PROGRAM NAME"]].Trim().ToUpper(),
                Isci = fields[headers["ISCI/AD-ID"]].Trim().ToUpper(),
                EstimateId = estimateId
            };


            var spot_length = fields[headers["DURATION"]].Trim();
            _SetSpotLengths(bvsDetail, spot_length);

            return bvsDetail;
        }

        private SpotTrackerFileDetail _LoadExtendedSigmaFileDetail(string[] fields, Dictionary<string, int> headers, int row)
        {
            if (!int.TryParse(fields[headers["RANK"]].Trim(), out int rankNumber))
            {
                throw new ExtractBvsException("Invalid 'rank'", row);
            }
            var rawDate = fields[headers["DATE AIRED"]].Trim();
            var rawAiredDateTime = fields[headers["AIR START TIME"]].Trim().ToUpper();
            if (!DateTime.TryParse(rawDate + " " + rawAiredDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                throw new ExtractBvsException("Invalid 'date aired' or 'air start time'", row);
            var time = parsedDate.TimeOfDay;

            if (!int.TryParse(fields[headers["IDENTIFIER 1"]].Trim(), out int estimateId))
            {
                throw new ExtractBvsException("Invalid 'identifier 1'", row);
            }
            DateTime encodedDate = DateTime.Parse(fields[headers["ENCODE DATE"]].Trim() + " " + fields[headers["ENCODE TIME"]].Trim());


            SpotTrackerFileDetail fileDetail = new SpotTrackerFileDetail()
            {
                Advertiser = fields[headers["PRODUCT"]].Trim().ToUpper(),
                Market = fields[headers["DMA"]].Trim().ToUpper(),
                Rank = rankNumber,
                Station = fields[headers["STATION"]].Trim().ToUpper(),
                Affiliate = fields[headers["AFFILIATION"]].Trim().ToUpper(),
                DateAired = parsedDate.Date,
                TimeAired = (int)time.TotalSeconds,
                ProgramName = fields[headers["PROGRAM NAME"]].Trim().ToUpper(),
                Isci = fields[headers["ISCI/AD-ID"]].Trim().ToUpper(),
                EstimateId = estimateId,
                Client = fields[headers["CLIENT"]].Trim().ToUpper(),
                ClientName = fields[headers["CLIENT NAME"]].Trim().ToUpper(),
                Country = fields[headers["CNTRY"]].Trim().ToUpper(),
                DayOfWeek = fields[headers["DAY OF WEEK"]].Trim().ToUpper(),
                Daypart = fields[headers["DAYPART"]].Trim().ToUpper(),
                Discid = string.IsNullOrWhiteSpace(fields[headers["DISCID"]]) ? (int?)null : int.Parse(fields[headers["DISCID"]].Trim()),
                EncodeDate = encodedDate.Date,
                EncodeTime = (int)encodedDate.TimeOfDay.TotalSeconds,
                Identifier2 = string.IsNullOrWhiteSpace(fields[headers["IDENTIFIER 2"]]) ? (int?)null : int.Parse(fields[headers["IDENTIFIER 2"]].Trim()),
                Identifier3 = string.IsNullOrWhiteSpace(fields[headers["IDENTIFIER 3"]]) ? (int?)null : int.Parse(fields[headers["IDENTIFIER 3"]].Trim()),
                MarketCode = string.IsNullOrWhiteSpace(fields[headers["DMA CODE"]]) ? (int?)null : int.Parse(fields[headers["DMA CODE"]].Trim()),
                ReleaseName = fields[headers["RELEASE NAME"]].Trim(),
                RelType = fields[headers["REL TYPE"]].Trim().ToUpper(),
                Sid = string.IsNullOrWhiteSpace(fields[headers["SID"]]) ? (int?)null : int.Parse(fields[headers["SID"]].Trim()),
                StationName = fields[headers["STATION NAME"]].Trim().ToUpper()
            };


            var spot_length = fields[headers["DURATION"]].Trim();
            _SetSpotLengths(fileDetail, spot_length);

            return fileDetail;
        }

        private void _SetSpotLengths(TrackerFileDetail detail, string spot_length)
        {
            spot_length = spot_length.Trim().Replace(":", "");

            if (!int.TryParse(spot_length, out int spotLength))
            {
                throw new ExtractBvsException(string.Format("Invalid spot length '{0}' found.", spotLength));
            }

            if (!_SpotLengthsAndIds.ContainsKey(spotLength))
                throw new ExtractBvsException(string.Format("Invalid spot length '{0}' found.", spotLength));

            detail.SpotLength = spotLength;
            detail.SpotLengthId = _SpotLengthsAndIds[spotLength];
        }               
    }
}
