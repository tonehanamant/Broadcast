using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public class PostParsingException : Exception
    {
        public override string Message { get { return _Header + string.Join("\n", ParsingErrors); } }

        private readonly string _Header;
        public readonly List<string> ParsingErrors;
        public PostParsingException(string header, List<string> parsingErrors) : this(parsingErrors)
        {
            _Header = header;
        }

        public PostParsingException(List<string> parsingErrors)
        {
            ParsingErrors = parsingErrors;
        }
    }

    public interface IPostFileParser : IApplicationService
    {
        List<post_file_details> ParseExcel(Stream stream);
        List<post_file_details> Parse(ExcelPackage package);
    }

    public class PostFileParser : IPostFileParser
    {
        private readonly IDataRepositoryFactory _Factory;

        internal static readonly HashSet<string> ExcelFileHeaders = new HashSet<string>
        {
            RANK,
            MARKET,
            STATION,
            AFFILIATE,
            WEEKSTART,
            DAY,
            DATE,
            TIMEAIRED,
            PROGRAMNAME,
            SPOTLENGTH,
            HOUSEISCI,
            CLIENTISCI,
            ADVERTISER,
            INVENTORYSOURCE,
            INVENTORYSOURCEDAYPART,
            INVENTORYOUTOFSPECREASON,
            ESTIMATE,
            DETECTEDVIA,
            SPOT
        };

        private readonly List<string> _ValidDays = new List<string> { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
        internal static readonly string ColumnRequiredErrorMessage = "\t'{0}' column is required\n";
        internal static readonly string InvalidDateErrorMessage = "\t'{0}' column is not a valid date\n";
        internal static readonly string InvalidNumberErrorMessage = "\t'{0}' field has invalid number '{1}'\n";
        internal static readonly string ErrorInColumn = "\tError in Column {0}\n";

        internal const string MARKET = "Market";
        internal const string STATION = "Station";
        internal const string RANK = "Rank";
        internal const string SPOT = "Spot";
        internal const string DETECTEDVIA = "Detected Via";
        internal const string ESTIMATE = "Estimate";
        internal const string INVENTORYOUTOFSPECREASON = "Inventory Out of Spec Reason";
        internal const string INVENTORYSOURCEDAYPART = "Inventory Source Daypart";
        internal const string INVENTORYSOURCE = "Inventory Source";
        internal const string ADVERTISER = "Advertiser";
        internal const string CLIENTISCI = "Client ISCI";
        internal const string HOUSEISCI = "House ISCI";
        internal const string SPOTLENGTH = "Length";
        internal const string PROGRAMNAME = "Program Name";
        internal const string TIMEAIRED = "Time Aired";
        internal const string DATE = "Date";
        internal const string DAY = "Day";
        internal const string WEEKSTART = "Weekstart";
        internal const string AFFILIATE = "Affiliate";
        internal DateTime MagicBaseDate = DateTime.Parse("12/30/1899");

        public PostFileParser(IDataRepositoryFactory factory)
        {
            _Factory = factory;
        }
        public List<post_file_details> ParseExcel(Stream stream)
        {
            using (var excelPackage = new ExcelPackage(stream))
                return Parse(excelPackage);
        }

        public List<post_file_details> Parse(ExcelPackage package)
        {
            var allParsingExceptions = new List<string>();

            var worksheet = package.Workbook.Worksheets.First();
            var headers = new Dictionary<string, int>();
            foreach (var header in ExcelFileHeaders)
            {
                for (var column = 1; column <= worksheet.Dimension.End.Column; column++)
                {
                    var cellValue = GetCellValue(1, column, worksheet);
                    if (cellValue.ToUpper() != header.ToUpper())
                        continue;
                    headers.Add(header, column);
                    break;
                }

                if (!headers.ContainsKey(header))
                    allParsingExceptions.Add(string.Format("Could not find header for column {0}", header));
            }

            if (allParsingExceptions.Any())
                throw new PostParsingException(allParsingExceptions);

            var spotLengthDict = _Factory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
            var postUploadRepository = _Factory.GetDataRepository<IPostRepository>();

            var fileDetails = new List<post_file_details>();
            for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var errorMessageHeader = string.Format("Row {0}:\n", row);
                var errorMessage = string.Empty;
                if (_IsEmptyRow(row, worksheet))
                    break;

                var bvsDetail = new post_file_details();

                var rankValue = GetCellValue(row, headers[RANK], worksheet);
                int rankNumber;
                if (!int.TryParse(rankValue, out rankNumber))
                    errorMessage += string.Format(InvalidNumberErrorMessage, RANK, rankValue);
                bvsDetail.rank = rankNumber;


                bvsDetail.market = GetCellValue(row, headers[MARKET], worksheet);
                if (string.IsNullOrEmpty(bvsDetail.market))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, MARKET);


                bvsDetail.station = GetCellValue(row, headers[STATION], worksheet);
                if (string.IsNullOrEmpty(bvsDetail.station))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, STATION);
                else if (!postUploadRepository.GetStationCode(bvsDetail.station).HasValue)
                    errorMessage += string.Format("\t'{0}' {1} does not exist\n", STATION, bvsDetail.station);

                bvsDetail.affiliate = GetCellValue(row, headers[AFFILIATE], worksheet);
                if (string.IsNullOrEmpty(bvsDetail.affiliate))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, AFFILIATE);

                var weekStart = GetDateValue(row, headers[WEEKSTART], worksheet);
                if (weekStart == DateTime.MinValue || weekStart.Date == MagicBaseDate.Date)
                    errorMessage += string.Format(InvalidDateErrorMessage, WEEKSTART);
                else
                {
                    if (weekStart.DayOfWeek != DayOfWeek.Monday)
                        errorMessage += string.Format("\t'{0}' {1} is {2}, not Monday\n", WEEKSTART, weekStart, weekStart.DayOfWeek);
                    else
                        bvsDetail.weekstart = weekStart;
                }


                bvsDetail.day = GetCellValue(row, headers[DAY], worksheet);
                if (string.IsNullOrEmpty(bvsDetail.day))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, DAY);
                else if (!_ValidDays.Contains(bvsDetail.day))
                    errorMessage += string.Format("\t'{0}' {1} is not a valid day\n", DAY, bvsDetail.day);


                var date = GetDateValue(row, headers[DATE], worksheet);
                if (date == DateTime.MinValue || date.Date == MagicBaseDate.Date)
                    errorMessage += string.Format(InvalidDateErrorMessage, DATE);
                else
                    bvsDetail.date = date;

                var timeAired = GetDateValue(row, headers[TIMEAIRED], worksheet);
                if (timeAired == DateTime.MinValue || timeAired.Date == MagicBaseDate.Date)
                    errorMessage += string.Format(InvalidDateErrorMessage, TIMEAIRED);
                else
                {
                    bvsDetail.time_aired = (int)timeAired.TimeOfDay.TotalSeconds;
                    if (timeAired.Date != bvsDetail.date && bvsDetail.date != DateTime.MinValue)
                        errorMessage += string.Format("\t'{0}' {1:d} is not the same day as the Date in '{2}', {3:d}\n", DATE, bvsDetail.date, TIMEAIRED, timeAired.Date);
                }

                bvsDetail.program_name = GetCellValue(row, headers[PROGRAMNAME], worksheet);
                if (string.IsNullOrEmpty(bvsDetail.program_name))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, PROGRAMNAME);

                var spotLength = GetCellValue(row, headers[SPOTLENGTH], worksheet);
                if (string.IsNullOrEmpty(spotLength))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, SPOTLENGTH);
                else
                {
                    int parsedSpotLength;
                    if (int.TryParse(spotLength, out parsedSpotLength))
                    {
                        bvsDetail.spot_length = parsedSpotLength;
                        int spotLengthId;
                        if (spotLengthDict.TryGetValue(parsedSpotLength, out spotLengthId))
                            bvsDetail.spot_length_id = spotLengthId;
                        else
                            errorMessage += string.Format(ErrorInColumn, SPOTLENGTH);
                    }
                    else
                        errorMessage += string.Format(ErrorInColumn, SPOTLENGTH);
                }

                bvsDetail.house_isci = GetCellValue(row, headers[HOUSEISCI], worksheet);

                bvsDetail.client_isci = GetCellValue(row, headers[CLIENTISCI], worksheet);
                if (string.IsNullOrEmpty(bvsDetail.client_isci))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, CLIENTISCI);

                bvsDetail.advertiser = GetCellValue(row, headers[ADVERTISER], worksheet);
                if (string.IsNullOrEmpty(bvsDetail.advertiser))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, ADVERTISER);

                bvsDetail.inventory_source = GetCellValue(row, headers[INVENTORYSOURCE], worksheet);

                bvsDetail.inventory_source_daypart = GetCellValue(row, headers[INVENTORYSOURCEDAYPART], worksheet);

                bvsDetail.inventory_out_of_spec_reason = GetCellValue(row, headers[INVENTORYOUTOFSPECREASON], worksheet);

                var estimateId = GetCellValue(row, headers[ESTIMATE], worksheet);
                if (string.IsNullOrEmpty(estimateId))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, ESTIMATE);
                else
                {
                    int estimateIdInt;
                    if (!int.TryParse(estimateId, out estimateIdInt))
                        errorMessage += string.Format(InvalidNumberErrorMessage, ESTIMATE, estimateId);
                    bvsDetail.estimate_id = estimateIdInt;
                }

                bvsDetail.detected_via = GetCellValue(row, headers[DETECTEDVIA], worksheet);
                if (string.IsNullOrEmpty(bvsDetail.detected_via))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, DETECTEDVIA);

                var spot = GetCellValue(row, headers[SPOT], worksheet);
                if (spot != "1")
                    errorMessage += string.Format("\t'{0}' field has invalid value '{1}', must be 1\n", SPOT, spot);
                else
                    bvsDetail.spot = int.Parse(spot);

                if (!string.IsNullOrEmpty(errorMessage))
                    allParsingExceptions.Add(errorMessageHeader + errorMessage);
                else
                    fileDetails.Add(bvsDetail);
            }

            if (allParsingExceptions.Any())
                throw new PostParsingException("Following lines were not imported due to data validation issues:\n", allParsingExceptions);
            return fileDetails;
        }

        private static string GetCellValue(int row, int column, ExcelWorksheet excelWorksheet)
        {
            var value = excelWorksheet.Cells[row, column].Value ?? "";
            return value.ToString().Trim();
        }

        private static DateTime GetDateValue(int row, int column, ExcelWorksheet excelWorksheet)
        {
            return excelWorksheet.Cells[row, column].GetValue<DateTime>();
        }

        private static bool _IsEmptyRow(int row, ExcelWorksheet excelWorksheet)
        {
            for (var c = 1; c < excelWorksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrEmpty(excelWorksheet.Cells[row, c].Text))
                    return false;

            return true;
        }
    }
}
