using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public interface IBvsConverter : IApplicationService
    {
        BvsFile ExtractBvsData(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<BvsFileDetailKey, int> lineInfo);


        BvsFile ExtractSigmaData(Stream rawStream, string hash, string userName, string bvsFileName, out Dictionary<BvsFileDetailKey, int> lineInfo);

        /// <summary>
        /// 3a-3a rule
        /// </summary>
        /// <param name="date"></param>
        /// <param name="airTime"></param>
        /// <returns></returns>
        DateTime ConvertToNSITime(DateTime date, TimeSpan airTime);

        /// <summary>
        /// 6a-6a Rule
        /// </summary>
        /// <param name="date"></param>
        /// <param name="airTime"></param>
        /// <returns></returns>
        DateTime ConvertToNTITime(DateTime date, TimeSpan airTime);
    }

    public class ExtractBvsException : Exception
    {
        private string _Message;

        public override string Message
        {
            get { return _Message; }
        }

        public ExtractBvsException(string message, int? row = null)
        {
            if (row.HasValue)
            {
                _Message = string.Format("Error in row {0}: {1}", row, message);
            }
            else
            {
                _Message = message;
            }
        }

    }

    public class ExtractBvsExceptionEmptyFiles : Exception
    {
        public override string Message
        {
            get
            {
                return "File does not contain valid BVS detail data.";
            }
        }
    }

    public class ExtractBvsExceptionCableTv : Exception
    {
        public override string Message
        {
            get
            {
                return "File contained only Cable TV records";
            }
        }
    }


    public class BvsConverter : IBvsConverter
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly ISigmaConverter _SigmaConverter;

        public BvsConverter(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
            _SigmaConverter = new SigmaConverter();
        }

        const string CABLE_TV = "CABLE TV";
        const string NETWORK_TV = "NETWORK TV";

        private const int HeaderRow = 8;
        private int DataStartRow;

        private OfficeOpenXml.ExcelWorksheet _Worksheet;
        private Dictionary<string, int> _Headers;
        private static readonly List<string> XlsFileHeaders = new List<string>()
        {
            "Rank"
            ,"Market"
            ,"Station"
            ,"Affiliate"
            ,"Date"
            ,"Time Aired"
            ,"Program Name"
            ,"Length"
            ,"ISCI"
            ,"Campaign"
            ,"Advertiser"
        };

        //public Dictionary<BvsFileDetailKey, int> FileDetailLineInfo { get; set; }
        public BvsFile ExtractBvsData(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<BvsFileDetailKey, int> lineInfo)
        {
            //return ExtractBvsDataCsv(rawStream,hash,userName,bvsFileName);
            return ExtractBvsDataXls(rawStream, hash, userName, bvsFileName, out message, out lineInfo);
        }

        public BvsFile ExtractSigmaData(Stream rawStream, string hash, string userName, string bvsFileName, out Dictionary<BvsFileDetailKey, int> lineInfo)
        {
            //return ExtractBvsDataCsv(rawStream,hash,userName,bvsFileName);
            return ExtractSigmaDataCsv(rawStream, hash, userName, bvsFileName, out lineInfo);
        }

        public BvsFile ExtractSigmaDataCsv(Stream rawStream, string hash, string userName, string bvsFileName, out Dictionary<BvsFileDetailKey, int> lineInfo)
        {
            lineInfo = new Dictionary<BvsFileDetailKey, int>();
            var bvsFile = new BvsFile();

            int rowNumber = 0;
            using (var parser = _SigmaConverter.SetupCSVParser(rawStream))
            {
                Dictionary<string, int> headers = _SigmaConverter.ValidateAndSetupHeaders(parser);
                while (!parser.EndOfData)
                {
                    rowNumber++;
                    var fields = parser.ReadFields();
                    _SigmaConverter.ValidateSigmaFieldData(fields, headers, rowNumber);

                    BvsFileDetail bvsDetail = _LoadBvsFileDetail(fields, headers, rowNumber);
                    lineInfo[new BvsFileDetailKey(bvsDetail)] = rowNumber;
                    bvsFile.BvsFileDetails.Add(bvsDetail);
                }
            }

            if (!bvsFile.BvsFileDetails.Any())
            {
                throw new ExtractBvsExceptionEmptyFiles();
            }
            bvsFile.Name = bvsFileName;
            bvsFile.CreatedBy = userName;
            bvsFile.CreatedDate = DateTime.Now;
            bvsFile.FileHash = hash;
            bvsFile.StartDate = bvsFile.BvsFileDetails.Min(x => x.DateAired).Date;
            bvsFile.EndDate = bvsFile.BvsFileDetails.Max(x => x.DateAired).Date;

            return bvsFile;
        }

        private BvsFileDetail _LoadBvsFileDetail(string[] fields, Dictionary<string, int> headers, int row)
        {
            Dictionary<int, int> spotLengthDict = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            if (!int.TryParse(fields[headers["RANK"]].Trim(), out int rankNumber))
            {
                throw new ExtractBvsException("Invalid 'rank'", row);
            }
            var rawDate = fields[headers["DATE AIRED"]].Trim();
            var rawAiredDateTime = fields[headers["AIR START TIME"]].Trim().ToUpper();
            string someDate = rawDate + " " + rawAiredDateTime;
            if (!DateTime.TryParseExact(someDate, "M/dd/yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
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
                NsiDate = ConvertToNSITime(parsedDate.Date, time),
                NtiDate = ConvertToNSITime(parsedDate.Date, time),
                ProgramName = fields[headers["PROGRAM NAME"]].Trim().ToUpper(),
                Isci = fields[headers["ISCI/AD-ID"]].Trim().ToUpper(),
                EstimateId = estimateId
            };


        var spot_length = fields[headers["DURATION"]].Trim();
            _SetSpotLengths(bvsDetail, spot_length, spotLengthDict);

            return bvsDetail;
        }

        private void _SetSpotLengths(BvsFileDetail bvsDetail, string spot_length, Dictionary<int, int> spotLengthDict)
        {
            spot_length = spot_length.Trim().Replace(":", "");

            if (!int.TryParse(spot_length, out int spotLength))
            {
                throw new ExtractBvsException(string.Format("Invalid spot length '{0}' found.", spotLength));
            }

            if (!spotLengthDict.ContainsKey(spotLength))
                throw new ExtractBvsException(string.Format("Invalid spot length '{0}' found.", spotLength));

            bvsDetail.SpotLength = spotLength;
            bvsDetail.SpotLengthId = spotLengthDict[spotLength];
        }

        private bool _IsEmptyRow(int row)
        {
            for (int c = 1; c < _Worksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrEmpty(_Worksheet.Cells[row, c].Text))
                    return false;
            return true;
        }

        public BvsFile ExtractBvsDataXls(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<BvsFileDetailKey, int> lineInfo)
        {
            lineInfo = new Dictionary<BvsFileDetailKey, int>();

            Dictionary<int, int> spotLengthDict = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            BvsFile bvsFile = new BvsFile();
            int row;
            int cableTvCount = 0;
            message = string.Empty;

            using (var excelPackage = new OfficeOpenXml.ExcelPackage(rawStream))
            {
                _Worksheet = excelPackage.Workbook.Worksheets.First();
                _Headers = _SetupHeadersValidateSheet();

                for (row = DataStartRow; row <= _Worksheet.Dimension.End.Row; row++)
                {
                    if (_IsEmptyRow(row))
                        break;  // empty row, done!

                    var bvsDetail = new BvsFileDetail
                    {
                        Market = _GetCellValue(row, "Market").ToUpper()
                    };
                    if (string.IsNullOrEmpty(bvsDetail.Market))
                    {
                        throw new ExtractBvsException("'Market' field is missing.", row);
                    }

                    var cellContent = _GetCellValue(row, "Rank");
                    if (string.IsNullOrEmpty(cellContent))
                        continue;
                    if (!int.TryParse(cellContent, out int rankNumber))
                    {
                        throw new ExtractBvsException(string.Format("Invalid rank value \"{0}\".", cellContent), row);
                    }
                    bvsDetail.Rank = rankNumber;
                    bvsDetail.Station = _GetCellValue(row, "Station").ToUpper();
                    bvsDetail.Affiliate = _GetCellValue(row, "Affiliate").ToUpper();


                    var extractedDate = _GetAirTime(row);
                    bvsDetail.DateAired = extractedDate.Date;

                    var time = extractedDate.TimeOfDay;
                    bvsDetail.TimeAired = (int)time.TotalSeconds;
                    bvsDetail.NsiDate = ConvertToNSITime(bvsDetail.DateAired, time);
                    bvsDetail.NtiDate = ConvertToNTITime(bvsDetail.DateAired, time);

                    bvsDetail.ProgramName = _GetCellValue(row, "Program Name").ToUpper();

                    bvsDetail.SpotLength = int.Parse(_GetCellValue(row, "Length"));
                    var spot_length = _GetCellValue(row, "Length");
                    _SetSpotLengths(bvsDetail, spot_length, spotLengthDict);

                    bvsDetail.Isci = _GetCellValue(row, "ISCI").ToUpper();
                    bvsDetail.Advertiser = _GetCellValue(row, "Advertiser").ToUpper();
                    int estimateId;
                    if (!int.TryParse(_GetCellValue(row, "Campaign"), out estimateId))
                    {
                        message += string.Format("Invalid campaign field in row={0}, skipping. <br />", row);
                        continue;   // go to next line
                    }
                    bvsDetail.EstimateId = estimateId;
                    lineInfo[new BvsFileDetailKey(bvsDetail)] = row;
                    bvsFile.BvsFileDetails.Add(bvsDetail);
                }
            }
            if (!bvsFile.BvsFileDetails.Any())
            {
                if (row == cableTvCount) // if entire file is made of cable tv record, treat as empty.
                    throw new ExtractBvsExceptionCableTv();

                throw new ExtractBvsExceptionEmptyFiles();
            }
            bvsFile.Name = bvsFileName;
            bvsFile.CreatedBy = userName;
            bvsFile.CreatedDate = DateTime.Now;
            bvsFile.FileHash = hash;
            bvsFile.StartDate = bvsFile.BvsFileDetails.Min(x => x.DateAired).Date;
            bvsFile.EndDate = bvsFile.BvsFileDetails.Max(x => x.DateAired).Date;

            return bvsFile;
        }

        private static bool EnsureRank(string cellContent, BvsFileDetail bvsDetail, int row)
        {
            var numTestResult = int.TryParse(cellContent, out int rankNumber);
            if (!numTestResult)
            {
                if (bvsDetail.Market == CABLE_TV || bvsDetail.Market == NETWORK_TV)
                {
                    return true;
                }
                throw new ExtractBvsException("Rank field required for non-CABLE TV and non-Netowrk TV records.", row);
            }
            bvsDetail.Rank = rankNumber;
            return false;
        }

        private DateTime _GetAirTime(int row)
        {
            var dateTime = _Worksheet.GetValue<DateTime>(row, _Headers["Time Aired"]);
            return dateTime;
        }
        private string _GetCellValue(int row, string columnName)
        {
            var value = _Worksheet.Cells[row, _Headers[columnName]].Value ?? "";
            return value.ToString().Trim();

        }

        private string _GetCellValue(int row, int column)
        {
            return _Worksheet.Cells[row, column].Text.Trim();
        }
        private Dictionary<string, int> _SetupHeadersValidateSheet()
        {
            var validationErrors = new List<string>();
            Dictionary<string, int> headerDict = new Dictionary<string, int>();

            int headerRow = 1;
            while (headerRow < _Worksheet.Dimension.End.Row)
            {
                var cellValue = _GetCellValue(headerRow, 1);
                if (XlsFileHeaders.Contains(cellValue))
                    break;
                headerRow++;
            }
            if (headerRow >= _Worksheet.Dimension.End.Row)
                throw new Exception("Could not find starting row, stopped!");

            DataStartRow = headerRow + 1;

            foreach (var header in XlsFileHeaders)
            {
                for (int column = 1; column <= _Worksheet.Dimension.End.Column; column++)
                {
                    var cellValue = _GetCellValue(headerRow, column);
                    if (cellValue.ToUpper() == header.ToUpper())
                    {
                        headerDict.Add(header, column);
                        break;
                    }
                }
                if (!headerDict.ContainsKey(header))
                    validationErrors.Add(string.Format("Could not find required column {0}.<br />", header));
            }

            if (validationErrors.Any())
            {
                string message = "";
                validationErrors.ForEach(err => message += err + Environment.NewLine);
                throw new Exception(message);
            }
            return headerDict;
        }

        /// <summary>
        /// 3a-3a rule
        /// </summary>
        /// <param name="date"></param>
        /// <param name="airTime"></param>
        /// <returns></returns>
        public DateTime ConvertToNSITime(DateTime date, TimeSpan airTime)
        {
            if (airTime.TotalSeconds <= new TimeSpan(3, 0, 0).TotalSeconds)
            {
                return date.AddDays(-1);
            }

            return date;
        }

        /// <summary>
        /// Uses ConvertToNSITime
        /// </summary>
        /// <param name="date"></param>
        /// <param name="airTime"></param>
        /// <returns></returns>
        public DateTime ConvertToNTITime(DateTime date, TimeSpan airTime)
        {
            return ConvertToNSITime(date, airTime);
        }
    }
}
