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
    public interface IDetectionConverter : IApplicationService
    {
        TrackerFile<DetectionFileDetail> ExtractDetectionData(Stream rawStream, string hash, string userName, string detectionFileName, out string message, out Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int> lineInfo);        
    }

    public class ExtractDetectionException : Exception
    {
        private string _Message;

        public override string Message
        {
            get { return _Message; }
        }

        public ExtractDetectionException(string message, int? row = null)
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

    public class ExtractDetectionExceptionEmptyFiles : Exception
    {
        public override string Message
        {
            get
            {
                return "File does not contain valid detection detail data.";
            }
        }
    }

    public class ExtractDetectionExceptionCableTv : Exception
    {
        public override string Message
        {
            get
            {
                return "File contained only Cable TV records";
            }
        }
    }
    
    public class DetectionConverter : IDetectionConverter
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;
        private readonly IDateAdjustmentEngine _DateAdjustmentEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public DetectionConverter(
            IDataRepositoryFactory dataRepositoryFactory, 
            IDateAdjustmentEngine dateAdjustment,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
            _DateAdjustmentEngine = dateAdjustment;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
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

        //public Dictionary<DetectionFileDetailKey, int> FileDetailLineInfo { get; set; }
        public TrackerFile<DetectionFileDetail> ExtractDetectionData(Stream rawStream, string hash, string userName, string detectionFileName, out string message, out Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int> lineInfo)
        {
            //return ExtractDetectionDataCsv(rawStream,hash,userName,detectionFileName);
            return ExtractDetectionDataXls(rawStream, hash, userName, detectionFileName, out message, out lineInfo);
        }
        
        private void _SetSpotLengths(DetectionFileDetail detectionFileDetail, string spot_length, Dictionary<int, int> spotLengthDict)
        {
            spot_length = spot_length.Trim().Replace(":", "");

            if (!int.TryParse(spot_length, out int spotLength))
            {
                throw new ExtractDetectionException(string.Format("Invalid spot length '{0}' found.", spotLength));
            }

            if (!spotLengthDict.ContainsKey(spotLength))
                throw new ExtractDetectionException(string.Format("Invalid spot length '{0}' found.", spotLength));

            detectionFileDetail.SpotLength = spotLength;
            detectionFileDetail.SpotLengthId = spotLengthDict[spotLength];
        }

        private bool _IsEmptyRow(int row)
        {
            for (int c = 1; c < _Worksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrEmpty(_Worksheet.Cells[row, c].Text))
                    return false;
            return true;
        }

        public TrackerFile<DetectionFileDetail> ExtractDetectionDataXls(Stream rawStream, string hash, string userName, string detectionFileName, out string message, out Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int> lineInfo)
        {
            lineInfo = new Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int>();

            Dictionary<int, int> spotLengthDict = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            TrackerFile<DetectionFileDetail> detectionFile = new TrackerFile<DetectionFileDetail>();
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

                    var detectionFileDetail = new DetectionFileDetail
                    {
                        Market = _GetCellValue(row, "Market").ToUpper()
                    };
                    if (string.IsNullOrEmpty(detectionFileDetail.Market))
                    {
                        throw new ExtractDetectionException("'Market' field is missing.", row);
                    }

                    var cellContent = _GetCellValue(row, "Rank");
                    if (string.IsNullOrEmpty(cellContent))
                        continue;
                    if (!int.TryParse(cellContent, out int rankNumber))
                    {
                        throw new ExtractDetectionException(string.Format("Invalid rank value \"{0}\".", cellContent), row);
                    }
                    detectionFileDetail.Rank = rankNumber;
                    detectionFileDetail.Station = _GetCellValue(row, "Station").ToUpper();
                    detectionFileDetail.Affiliate = _GetCellValue(row, "Affiliate").ToUpper();
                    
                    var extractedDate = _GetAirTime(row);
                    detectionFileDetail.DateAired = extractedDate.Date;
                    
                    var time = extractedDate.TimeOfDay;
                    detectionFileDetail.TimeAired = (int)time.TotalSeconds;
                    detectionFileDetail.NsiDate = _DateAdjustmentEngine.ConvertToNSITime(detectionFileDetail.DateAired, time);
                    detectionFileDetail.NtiDate = _DateAdjustmentEngine.ConvertToNTITime(detectionFileDetail.DateAired, time);

                    detectionFileDetail.ProgramName = _GetCellValue(row, "Program Name").ToUpper();

                    detectionFileDetail.SpotLength = int.Parse(_GetCellValue(row, "Length"));
                    var spot_length = _GetCellValue(row, "Length");
                    _SetSpotLengths(detectionFileDetail, spot_length, spotLengthDict);

                    detectionFileDetail.Isci = _GetCellValue(row, "ISCI").ToUpper();
                    detectionFileDetail.Advertiser = _GetCellValue(row, "Advertiser").ToUpper();
                    int estimateId;
                    if (!int.TryParse(_GetCellValue(row, "Campaign"), out estimateId))
                    {
                        message += string.Format("Invalid campaign field in row={0}, skipping. <br />", row);
                        continue;   // go to next line
                    }
                    detectionFileDetail.EstimateId = estimateId;
                    lineInfo[new TrackerFileDetailKey<DetectionFileDetail>(detectionFileDetail)] = row;
                    detectionFile.FileDetails.Add(detectionFileDetail);
                }
            }
            if (!detectionFile.FileDetails.Any())
            {
                if (row == cableTvCount) // if entire file is made of cable tv record, treat as empty.
                    throw new ExtractDetectionExceptionCableTv();

                throw new ExtractDetectionExceptionEmptyFiles();
            }
            detectionFile.Name = detectionFileName;
            detectionFile.CreatedBy = userName;
            detectionFile.CreatedDate = DateTime.Now;
            detectionFile.FileHash = hash;
            detectionFile.StartDate = detectionFile.FileDetails.Min(x => x.DateAired).Date;
            detectionFile.EndDate = detectionFile.FileDetails.Max(x => x.DateAired).Date;

            return detectionFile;
        }

        private static bool EnsureRank(string cellContent, DetectionFileDetail detectionFileDetail, int row)
        {
            var numTestResult = int.TryParse(cellContent, out int rankNumber);
            if (!numTestResult)
            {
                if (detectionFileDetail.Market == CABLE_TV || detectionFileDetail.Market == NETWORK_TV)
                {
                    return true;
                }
                throw new ExtractDetectionException("Rank field required for non-CABLE TV and non-Netowrk TV records.", row);
            }
            detectionFileDetail.Rank = rankNumber;
            return false;
        }

        private DateTime _GetAirTime(int row)
        {
            var dateTime = _Worksheet.GetValue<DateTime>(row, _Headers["Time Aired"]);

            if (_MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDateOrNull(dateTime.Date) == null)
            {
                throw new ExtractDetectionException("Invalid Media Week for Time Aired", row);
            }

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
