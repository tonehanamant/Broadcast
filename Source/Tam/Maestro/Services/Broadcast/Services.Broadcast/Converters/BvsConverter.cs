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
        bvs_files ExtractBvsData(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<BvsFileDetailKey, int> lineInfo);

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

        public BvsConverter(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
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

        private static readonly List<string> CsvFileHeaders = new List<string>()
        {
            "Encoding Id",
            "Market",
            "Rank", // int
            "Station",
            "Channel",
            "Affiliate",
            "Start Date", // DateTime
            "Start Time",
            "Stop Date",
            "Stop Time",
            "Start Second",
            "Stop Second",
            "Seconds",  // int
            "Program",
            "ISCI-20",
            "Campaign",
            "Advertiser",
            "Product Name",
            "Module Code",
            "Phone Number",
            "Length",   // int
            "Donovan Agency Estimate Code",
            "Donovan Agency Advertiser Code",
            "Donovan Agency Product Code",
        };

        private TextFieldParser _SetupCSVParser(Stream rawStream)
        {
            var parser = new TextFieldParser(rawStream);
            if (parser.EndOfData)
            {
                throw new ExtractBvsExceptionEmptyFiles();
            }

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

        //public Dictionary<BvsFileDetailKey, int> FileDetailLineInfo { get; set; }
        public bvs_files ExtractBvsData(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<BvsFileDetailKey, int> lineInfo)
        {
            //return ExtractBvsDataCsv(rawStream,hash,userName,bvsFileName);
            return ExtractBvsDataXls(rawStream, hash, userName, bvsFileName, out message, out lineInfo);
        }

        public bvs_files ExtractBvsDataCsv(Stream rawStream, string hash, string userName, string bvsFileName, out Dictionary<BvsFileDetailKey, int> lineInfo)
        {
            lineInfo = new Dictionary<BvsFileDetailKey, int>();
            Dictionary<int, int> spotLengthDict = null;
            var bvsFile = new bvs_files();

            int row = 0;
            int cableTvCount = 0;
            using (var parser = _SetupCSVParser(rawStream))
            {
                var headers = _ValidateAndSetupHeaders(parser);
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    row++;

                    var bvsDetail = new bvs_file_details();
                    bvsDetail.market = fields[headers["Market"]].Trim().ToUpper();
                    if (string.IsNullOrEmpty(bvsDetail.market))
                    {
                        throw new ExtractBvsException("'Market' field is missing.", row);
                    }

                    var rankNumber = 0;
                    var cellContent = fields[headers["Rank"]].Trim();
                    if (!EnsureRank(cellContent, bvsDetail, row, ref cableTvCount))
                        continue; // do nothing and continue since we don't want cable

                    bvsDetail.station = fields[headers["Station"]].Trim().ToUpper();
                    bvsDetail.affiliate = fields[headers["Affiliate"]].Trim().ToUpper();

                    var rawDate = fields[headers["Start Date"]].Trim();
                    var rawAiredDateTime = fields[headers["Start Time"]].Trim().ToUpper();
                    DateTime parsedDate;
                    if (!DateTime.TryParseExact(rawDate + " " + rawAiredDateTime, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                        throw new ExtractBvsException("Invalid 'start date' or 'start time'", row);
                    bvsDetail.date_aired = parsedDate.Date;
                    var time = parsedDate.TimeOfDay;
                    bvsDetail.time_aired = (int)time.TotalSeconds;
                    bvsDetail.nsi_date = ConvertToNSITime(bvsDetail.date_aired, time);
                    bvsDetail.nti_date = ConvertToNSITime(bvsDetail.date_aired, time);

                    bvsDetail.program_name = fields[headers["Program"]].Trim().ToUpper(); //"Program Name
                    var spot_length = fields[headers["Length"]].Trim();
                    GetSpotlength(bvsDetail, spot_length, ref spotLengthDict);
                    bvsDetail.isci = fields[headers["ISCI-20"]].Trim().ToUpper();

                    int estimateId;
                    if (!int.TryParse(fields[headers["Campaign"]].Trim(), out estimateId))
                    {
                        throw new ExtractBvsException("Campaign field in row invalid.", row);
                    }
                    bvsDetail.estimate_id = estimateId;
                    lineInfo[new BvsFileDetailKey(bvsDetail)] = row;
                    bvsFile.bvs_file_details.Add(bvsDetail);
                }
            }

            if (!bvsFile.bvs_file_details.Any())
            {
                if (row == cableTvCount) // if entire file is made of cable tv record, treat as empty.
                    throw new ExtractBvsExceptionCableTv();

                throw new ExtractBvsExceptionEmptyFiles();
            }
            bvsFile.name = bvsFileName;
            bvsFile.created_by = userName;
            bvsFile.created_date = DateTime.Now;
            bvsFile.file_hash = hash;
            bvsFile.start_date = bvsFile.bvs_file_details.Min(x => x.date_aired).Date;
            bvsFile.end_date = bvsFile.bvs_file_details.Max(x => x.date_aired).Date;

            return bvsFile;
        }

        private void GetSpotlength(bvs_file_details bvsDetail, string spot_length, ref Dictionary<int, int> spotLengthDict)
        {
            spot_length = spot_length.Trim().Replace(":", "");

            int spotLength = int.Parse(spot_length);
            bvsDetail.spot_length = spotLength;

            if (spotLengthDict == null)
                spotLengthDict = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

            if (!spotLengthDict.ContainsKey(spotLength))
                throw new ExtractBvsException(string.Format("Invalid spot length '{0}' found.", spotLength));

            bvsDetail.spot_length_id = spotLengthDict[spotLength];
        }

        private Dictionary<string, int> _ValidateAndSetupHeaders(TextFieldParser parser)
        {
            var fields = parser.ReadFields().ToList();
            var validationErrors = new List<string>();
            Dictionary<string, int> headerDict = new Dictionary<string, int>();

            foreach (var header in CsvFileHeaders)
            {
                int headerItemIndex = fields.IndexOf(header);
                if (headerItemIndex >= 0)
                {
                    headerDict.Add(header, headerItemIndex);
                    continue;
                }
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

        private bool _IsEmptyRow(int row)
        {
            for (int c = 1; c < _Worksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrEmpty(_Worksheet.Cells[row, c].Text))
                    return false;
            return true;
        }
        public bvs_files ExtractBvsDataXls(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<BvsFileDetailKey, int> lineInfo)
        {
            lineInfo = new Dictionary<BvsFileDetailKey, int>();

            Dictionary<int, int> spotLengthDict = null;
            var bvsFile = new bvs_files();
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

                    var bvsDetail = new bvs_file_details();

                    bvsDetail.market = _GetCellValue(row, "Market").ToUpper();
                    if (string.IsNullOrEmpty(bvsDetail.market))
                    {
                        throw new ExtractBvsException("'Market' field is missing.", row);
                    }

                    var cellContent = _GetCellValue(row, "Rank");
                    if (string.IsNullOrEmpty(cellContent))
                        continue;
                    int rankNumber;
                    if (!int.TryParse(cellContent, out rankNumber))
                    {
                        throw new ExtractBvsException(string.Format("Invalid rank value \"{0}\".", cellContent), row);
                    }
                    bvsDetail.rank = rankNumber;
                    bvsDetail.station = _GetCellValue(row, "Station").ToUpper();
                    bvsDetail.affiliate = _GetCellValue(row, "Affiliate").ToUpper();


                    var extractedDate = _GetAirTime(row);
                    bvsDetail.date_aired = extractedDate.Date;

                    var time = extractedDate.TimeOfDay;
                    bvsDetail.time_aired = (int)time.TotalSeconds;
                    bvsDetail.nsi_date = ConvertToNSITime(bvsDetail.date_aired, time);
                    bvsDetail.nti_date = ConvertToNTITime(bvsDetail.date_aired, time);

                    bvsDetail.program_name = _GetCellValue(row, "Program Name").ToUpper();

                    bvsDetail.spot_length = int.Parse(_GetCellValue(row, "Length"));
                    var spot_length = _GetCellValue(row, "Length");
                    GetSpotlength(bvsDetail, spot_length, ref spotLengthDict);

                    bvsDetail.isci = _GetCellValue(row, "ISCI").ToUpper();
                    bvsDetail.advertiser = _GetCellValue(row, "Advertiser").ToUpper();
                    int estimateId;
                    if (!int.TryParse(_GetCellValue(row, "Campaign"), out estimateId))
                    {
                        message += string.Format("Invalid campaign field in row={0}, skipping. <br />", row);
                        continue;   // go to next line
                    }
                    bvsDetail.estimate_id = estimateId;
                    lineInfo[new BvsFileDetailKey(bvsDetail)] = row;
                    bvsFile.bvs_file_details.Add(bvsDetail);
                }
            }
            if (!bvsFile.bvs_file_details.Any())
            {
                if (row == cableTvCount) // if entire file is made of cable tv record, treat as empty.
                    throw new ExtractBvsExceptionCableTv();

                throw new ExtractBvsExceptionEmptyFiles();
            }
            bvsFile.name = bvsFileName;
            bvsFile.created_by = userName;
            bvsFile.created_date = DateTime.Now;
            bvsFile.file_hash = hash;
            bvsFile.start_date = bvsFile.bvs_file_details.Min(x => x.date_aired).Date;
            bvsFile.end_date = bvsFile.bvs_file_details.Max(x => x.date_aired).Date;

            return bvsFile;
        }

        private static bool EnsureRank(string cellContent, bvs_file_details bvsDetail, int row, ref int cableTvCount)
        {
            int rankNumber = 0;
            var numTestResult = int.TryParse(cellContent, out rankNumber);
            if (!numTestResult)
            {
                if (bvsDetail.market == CABLE_TV || bvsDetail.market == NETWORK_TV)
                {
                    cableTvCount++;
                    return true;
                }
                throw new ExtractBvsException("Rank field required for non-CABLE TV and non-Netowrk TV records.", row);
            }
            bvsDetail.rank = rankNumber;
            return false;
        }

        private DateTime _GetAirTime(int row)
        {
            var dateTime = _Worksheet.GetValue<DateTime>(row, _Headers["Time Aired"]);
            return dateTime;
        }
        private string _GetCellValue(int row, string columnName)
        {
            return _Worksheet.Cells[row, _Headers[columnName]].Text.Trim();
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
