using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public interface IBvsConverter : IApplicationService
    {
        TrackerFile<BvsFileDetail> ExtractBvsData(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<TrackerFileDetailKey<BvsFileDetail>, int> lineInfo);        
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
        private readonly IDateAdjustmentEngine _DateAdjustmentEngine;

        public BvsConverter(IDataRepositoryFactory dataRepositoryFactory, IDateAdjustmentEngine dateAdjustment)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
            _DateAdjustmentEngine = dateAdjustment;
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
        public TrackerFile<BvsFileDetail> ExtractBvsData(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<TrackerFileDetailKey<BvsFileDetail>, int> lineInfo)
        {
            //return ExtractBvsDataCsv(rawStream,hash,userName,bvsFileName);
            return ExtractBvsDataXls(rawStream, hash, userName, bvsFileName, out message, out lineInfo);
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

        public TrackerFile<BvsFileDetail> ExtractBvsDataXls(Stream rawStream, string hash, string userName, string bvsFileName, out string message, out Dictionary<TrackerFileDetailKey<BvsFileDetail>, int> lineInfo)
        {
            lineInfo = new Dictionary<TrackerFileDetailKey<BvsFileDetail>, int>();

            Dictionary<int, int> spotLengthDict = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            TrackerFile<BvsFileDetail> bvsFile = new TrackerFile<BvsFileDetail>();
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
                    bvsDetail.NsiDate = _DateAdjustmentEngine.ConvertToNSITime(bvsDetail.DateAired, time);
                    bvsDetail.NtiDate = _DateAdjustmentEngine.ConvertToNTITime(bvsDetail.DateAired, time);

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
                    lineInfo[new TrackerFileDetailKey<BvsFileDetail>(bvsDetail)] = row;
                    bvsFile.FileDetails.Add(bvsDetail);
                }
            }
            if (!bvsFile.FileDetails.Any())
            {
                if (row == cableTvCount) // if entire file is made of cable tv record, treat as empty.
                    throw new ExtractBvsExceptionCableTv();

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
    }
}
