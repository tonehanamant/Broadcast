using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Converters.Post
{
    public class BvsPostFileParser : BasePostFileParser
    {
        public static readonly string UnableToParseValueErrorMessage = "Unable to parse {0} column";

        internal static HashSet<string> ExcelFileHeaders = new HashSet<string>
        {
            RANK,
            MARKET,
            STATION,
            AFFILIATE,
            DATE,
            TIMEAIRED,
            PROGRAMNAME,
            SPOTLENGTH,
            HOUSEISCI,
            CLIENTISCI,
            ADVERTISER_AND_DAYPART,
            ESTIMATE_AND_INVENTORY_SOURCE,
            ADVERTISEROUTOFSPECREASON,
            INVENTORYOUTOFSPECREASON,
        };

        internal const string RANK = "Rank";
        internal const string MARKET = "Market";
        internal const string STATION = "Station";
        internal const string AFFILIATE = "Affiliate";
        internal const string DATE = "Date";
        internal const string TIMEAIRED = "Time Aired";
        internal const string PROGRAMNAME = "Program Name";
        internal const string SPOTLENGTH = "Length";
        internal const string HOUSEISCI = "House ISCI";
        internal const string CLIENTISCI = "Client ISCI";
        internal const string ADVERTISER_AND_DAYPART = "Advertiser/Daypart";
        internal const string ESTIMATE_AND_INVENTORY_SOURCE = "Estimate/Inventory Source";
        internal const string ADVERTISEROUTOFSPECREASON = "Advertiser Out of Spec";
        internal const string INVENTORYOUTOFSPECREASON = "Inventory Out of Spec";

        const char _EstimateInventorySourceSeparator = '/';

        public BvsPostFileParser(IDataRepositoryFactory dataRepositoryFactory,IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(dataRepositoryFactory, featureToggleHelper, configurationSettingsHelper)
        {
        }

        public override List<post_file_details> Parse(ExcelPackage package)
        {
            var allParsingExceptions = new List<string>();
            var worksheet = package.Workbook.Worksheets.First();
            var headers = new Dictionary<string, int>();

            _ValidateFileHeaders(worksheet, headers, allParsingExceptions);

            var spotLengthDict = DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsByDuration();
            var fileDetails = new List<post_file_details>();

            for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var errorMessageHeader = string.Format("Row {0}:\n", row);
                var errorMessage = string.Empty;

                if (IsEmptyRow(row, worksheet))
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

                bvsDetail.affiliate = GetCellValue(row, headers[AFFILIATE], worksheet);

                if (string.IsNullOrEmpty(bvsDetail.affiliate))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, AFFILIATE);

                var date = GetDateValue(row, headers[DATE], worksheet);

                if (date == DateTime.MinValue || date.Date == MagicBaseDate.Date)
                    errorMessage += string.Format(InvalidDateErrorMessage, DATE);
                else
                {
                    bvsDetail.date = date;
                    bvsDetail.weekstart = date.GetWeekMonday();
                    bvsDetail.day = _GetDayOfWeek(date);
                }

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
                        int spotLengthId;

                        bvsDetail.spot_length = parsedSpotLength;

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

                var advertiserAndDaypartCellValue = GetCellValue(row, headers[ADVERTISER_AND_DAYPART], worksheet);

                if (advertiserAndDaypartCellValue.IndexOf('(') == -1 ||
                    advertiserAndDaypartCellValue.IndexOf(')') == -1)
                    errorMessage += string.Format(UnableToParseValueErrorMessage, ADVERTISER_AND_DAYPART);
                else
                {
                    bvsDetail.advertiser = _ParseAdvertiser(advertiserAndDaypartCellValue);
                    bvsDetail.inventory_source_daypart = _ParseDaypart(advertiserAndDaypartCellValue);

                    if (string.IsNullOrEmpty(bvsDetail.advertiser) ||
                        string.IsNullOrEmpty(bvsDetail.inventory_source_daypart))
                        errorMessage += string.Format(ColumnRequiredErrorMessage, ADVERTISER_AND_DAYPART);
                }

                if (string.IsNullOrEmpty(bvsDetail.advertiser))
                    errorMessage += string.Format(ColumnRequiredErrorMessage, ADVERTISER_AND_DAYPART);

                var estimateAndInventorySourceCellValue = GetCellValue(row, headers[ESTIMATE_AND_INVENTORY_SOURCE],
                    worksheet);

                if (!estimateAndInventorySourceCellValue.Contains(_EstimateInventorySourceSeparator))
                    errorMessage += string.Format(UnableToParseValueErrorMessage, ESTIMATE_AND_INVENTORY_SOURCE);
                else
                {
                    bvsDetail.inventory_source = _ParseInventorySource(estimateAndInventorySourceCellValue);

                    int estimateIdInt;

                    if (!_TryParseEstimate(estimateAndInventorySourceCellValue, out estimateIdInt))
                        errorMessage += string.Format(InvalidNumberErrorMessage, ESTIMATE_AND_INVENTORY_SOURCE, estimateAndInventorySourceCellValue);

                    bvsDetail.estimate_id = estimateIdInt;
                }

                bvsDetail.inventory_out_of_spec_reason = GetCellValue(row, headers[INVENTORYOUTOFSPECREASON], worksheet);

                bvsDetail.advertiser_out_of_spec_reason = GetCellValue(row, headers[ADVERTISEROUTOFSPECREASON], worksheet);

                if (!string.IsNullOrEmpty(errorMessage))
                    allParsingExceptions.Add(errorMessageHeader + errorMessage);
                else
                    fileDetails.Add(bvsDetail);
            }

            if (allParsingExceptions.Any())
                throw new PostParsingException("Following lines were not imported due to data validation issues:\n", allParsingExceptions);

            return fileDetails;
        }

        private string _ParseAdvertiser(string value)
        {
            var firstParenthesis = value.IndexOf('(');

            return value.Substring(0, firstParenthesis).Trim();
        }

        private string _ParseDaypart(string value)
        {
            var firstParenthesis = value.IndexOf('(');

            return value.Substring(firstParenthesis, value.Length - firstParenthesis).Trim('(', ')');
        }

        private bool _TryParseEstimate(string value, out int estimateIdInt)
        {
            var estimateId = value.Split(_EstimateInventorySourceSeparator)[0];

            return int.TryParse(estimateId, out estimateIdInt);
        }

        private string _ParseInventorySource(string value)
        {
            return value.Split(_EstimateInventorySourceSeparator)[1];
        }

        private string _GetDayOfWeek(DateTime date)
        {
            return WeekDays[(int)date.DayOfWeek];
        }

        private void _ValidateFileHeaders(ExcelWorksheet worksheet, Dictionary<string, int> headers, List<string> allParsingExceptions)
        {
            foreach (var header in ExcelFileHeaders)
            {
                for (var column = 1; column <= worksheet.Dimension.End.Column; column++)
                {
                    var cellValue = GetCellValue(1, column, worksheet);

                    if (!string.Equals(cellValue, header, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    headers.Add(header, column);

                    break;
                }

                if (!headers.ContainsKey(header))
                    allParsingExceptions.Add(string.Format("Could not find header for column {0}", header));
            }

            if (allParsingExceptions.Any())
                throw new PostParsingException(allParsingExceptions);
        }
    }
}
