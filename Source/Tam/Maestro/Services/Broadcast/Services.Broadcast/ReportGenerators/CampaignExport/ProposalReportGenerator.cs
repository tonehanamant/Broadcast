using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using static Services.Broadcast.Entities.Campaign.ProposalQuarterTableData;
using SecondaryDemoTable = Services.Broadcast.Entities.Campaign.ProposalQuarterTableData.SecondayAudienceTable;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class ProposalReportGenerator : BroadcastBaseClass
    {
        public string MarketsCoverage { get; private set; }
        public string Dayparts { get; private set; }
        public string FlightHiatuses { get; private set; }
        public string Notes { get; private set; }

        #region Cells addresses
        private readonly string START_COLUMN = "A";
        private readonly string START_TOTAL_COLUMN = "E";
        private readonly string START_SECONDARY_AUDIENCE_COLUMN = "H";

        private readonly (int ColumnIndex, int RowIndex) CREATED_DATE_CELL = (20, 2);
        private readonly (int ColumnIndex, int RowIndex) CAMPAIGN_NAME_CELL = (6, 2);
        private readonly (int ColumnIndex, int RowIndex) AGENCY_NAME_CELL = (3, 5);
        private readonly (int ColumnIndex, int RowIndex) CLIENT_NAME_CELL = (5, 5);
        private readonly (int ColumnIndex, int RowIndex) CAMPAIGN_FLIGHT_CELL = (8, 5);
        private readonly (int ColumnIndex, int RowIndex) GUARANTEED_DEMO_CELL = (10, 5);
        private readonly (int ColumnIndex, int RowIndex) CAMPAIGN_SPOT_LENGTH_CELL = (12, 5);
        private readonly (int ColumnIndex, int RowIndex) POSTING_TYPE_CELL = (14, 5);
        private readonly (int ColumnIndex, int RowIndex) STATUS_CELL = (19, 5);
        private readonly (int ColumnIndex, int RowIndex) Fluidity_CELL = (15, 5);
        private readonly (int ColumnIndex, int RowIndex) Account_Executive_CELL = (16, 5);
        private readonly (int ColumnIndex, int RowIndex) Client_Contact_CELL = (17, 5);
        private readonly string PLAN_NAME_COLUMN = "C";
        private readonly string GUARANTEED_DEMO_COLUMN = "N";
        private readonly string FOOTER_INFO_COLUMN_INDEX = "F";
        private readonly string SECONDARY_AUDIENCE_LABEL_FIRST_COLUMN = "H";
        private readonly string SECONDARY_AUDIENCE_LABEL_SECOND_COLUMN = "O";
        #endregion

        private readonly int SPACE_BETWEEN_QUARTER_TABLES = 4;
        private readonly int SPACE_BETWEEN_QUARTER_TABLES_WITH_SECONDARY_AUDIENCES = 6;
        private readonly int ROWS_TO_COPY_QUARTER_MAIN_TABLE = 4;
        private readonly int ROWS_TO_COPY_QUARTER_AND_SECONDARY_TABLE = 12;
        private readonly int ROWS_TO_COPY_SECONDARY_ONLY_TABLE = 4;
        private readonly int ROWS_SECONDARY_TABLE_WITH_SEPARATOR = 8;
        private ExcelWorksheet WORKSHEET;
        private int quarterLabelRowIndex = 7;
        private int firstDataRowIndex = 9;
        private int currentRowIndex = 0;
        private int SecondaryAudiencesOffset;
        private bool HasSecondaryAudiences;
        private const string customDaypartSportCode = "CSP";
        private const string isCustomDaypartOrganizationNameisOther = "Other";

        public ProposalReportGenerator(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configSettingsHelper)
            : base(featureToggleHelper, configSettingsHelper)
        {
        }

        public void PopulateProposalTab(CampaignReportData campaignReportData, ExcelWorksheet proposalWorksheet)            
        {
            WORKSHEET = proposalWorksheet;
            HasSecondaryAudiences = campaignReportData.HasSecondaryAudiences;
            SecondaryAudiencesOffset = HasSecondaryAudiences ? 1 : 0;

            _LogInfo("Campaign Export Generation: Populating headers...");
            _PopulateHeader(campaignReportData);

            _LogInfo("Campaign Export Generation: Populating quarters tables...");
            _PopulateQuarterTables(campaignReportData);

            _LogInfo("Campaign Export Generation: Populating totals tables...");
            _PopulateTotalsTable(campaignReportData);

            _LogInfo("Campaign Export Generation: Populating market coverages...");
            _PopulateMarketCoverage(campaignReportData.MarketCoverageData);

            _LogInfo("Campaign Export Generation: Populating dayparts...");
            _PopulateDayparts(campaignReportData.DaypartsData);

            //content restrictions row is the second row after current index
            _LogInfo("Campaign Export Generation: Populating Content Restrictions...");
            currentRowIndex += 2;
            ExportSharedLogic.PopulateContentRestrictions(WORKSHEET, campaignReportData.DaypartsData
                , $"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}" +
                $"" +
                $"");

            _LogInfo("Campaign Export Generation: Populating flight hiatuses...");
            _PopulateFlightHiatuses(campaignReportData.FlightHiatuses);

            _LogInfo("Campaign Export Generation: Populating notes...");
            _PopulateNotes(campaignReportData.Notes);
        }

        private void _PopulateNotes(string notes)
        {
            //notes row is the second row after current index
            currentRowIndex += 2;
            if (!string.IsNullOrWhiteSpace(notes))
            {
                Notes = notes;
                WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = Notes;
            }
        }

        private void _PopulateFlightHiatuses(List<string> flightHiatuses)
        {
            //flight hiatuses row is the second row after current index
            currentRowIndex += 2;
            if (flightHiatuses.Any())
            {
                FlightHiatuses = string.Join(", ", flightHiatuses);
                WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = FlightHiatuses;
            }
        }

        private void _PopulateDayparts(List<DaypartData> daypartsData)
        {
            //dayparts row is the second row after current index
            currentRowIndex += 2;
            if (daypartsData.Any())
            {
                Dayparts = string.Join(", ", daypartsData.Select(x => _FormatDaypart(x)).ToList());
                WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = Dayparts;
            }
        }

        private string _FormatDaypart(DaypartData daypart)
        {
            string format;
            if(daypart.DaypartCode == customDaypartSportCode)
            {
                format = (daypart.CustomDayartOrganizationName != isCustomDaypartOrganizationNameisOther) ? $"{daypart.CustomDayartOrganizationName}: {daypart.CustomDaypartName}" :
                    $" {daypart.CustomDaypartName}";
            }
            else
            {
                format = $" {daypart.DaypartCode}";
            }
            format += $" - {daypart.FlightDays} {daypart.StartTime} - {daypart.EndTime}";
            return format;
        }


        private void _PopulateMarketCoverage(MarketCoverageData data)
        {
            //markets row is the second row after current index
            currentRowIndex += 2;
            MarketsCoverage = string.Format("~{0}% Minimum TV HH Coverage{1}{2}"
                , data.CoveragePercentage
                , data.BlackoutMarketsName.Any() ? $" | Blackout Markets: {string.Join(", ", data.BlackoutMarketsName)}" : string.Empty
                , data.PreferentialMarketsName.Any() ? $" | Preferential Markets: {string.Join(", ", data.PreferentialMarketsName)}" : string.Empty
                );
            WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = MarketsCoverage;
        }

        private void _PopulateQuarterTables(CampaignReportData campaignReportData)
        {
            ExportSharedLogic.AddEmptyTables(WORKSHEET
                , campaignReportData.ProposalQuarterTables.Count + 1    //+1 for the campaign totals table
                , quarterLabelRowIndex
                , campaignReportData.HasSecondaryAudiences ? ROWS_TO_COPY_QUARTER_AND_SECONDARY_TABLE : ROWS_TO_COPY_QUARTER_MAIN_TABLE);
            _PutDataIntoPlanQuarterTables(campaignReportData);
        }

        private void _PopulateTotalsTable(CampaignReportData campaignReportData)
        {
            //if the last quarter had secondary audiences, we need to account for the additional space between the tables
            currentRowIndex += campaignReportData.ProposalQuarterTables.Last().HasSecondaryAudiences
                ? SPACE_BETWEEN_QUARTER_TABLES_WITH_SECONDARY_AUDIENCES
                : SPACE_BETWEEN_QUARTER_TABLES;

            //plan name row is 2 rows before the data row
            quarterLabelRowIndex = currentRowIndex - 2;
            firstDataRowIndex = currentRowIndex;

            _InsertQuarterTableRows(campaignReportData.GuaranteedDemo,
                campaignReportData.ProposalCampaignTotalsTable);

            //add total row data
            _SetRowData(currentRowIndex, START_TOTAL_COLUMN
                , _GetQuarterTableTotalRowObjects(campaignReportData.ProposalCampaignTotalsTable.TotalRow));

            //this logic needs adjusting when populating secondary audiences tables for campaign totals
            if (campaignReportData.ProposalCampaignTotalsTable.HasSecondaryAudiences)
            {
                _AddSecondaryAudiencesTables(campaignReportData.ProposalCampaignTotalsTable);

                //delete the extra space (2 rows) that was added by the secondary audiences template
                WORKSHEET.DeleteRow(currentRowIndex + 1, 2);
            }
            else if(HasSecondaryAudiences)
            {
                _DeleteSecondaryAudienceEmptyTemplateTable();
            }
        }

        private void _PopulateHeader(CampaignReportData data)
        {
            WORKSHEET.Cells[CREATED_DATE_CELL.RowIndex, CREATED_DATE_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value += data.CreatedDate;
            WORKSHEET.Cells[CAMPAIGN_NAME_CELL.RowIndex, CAMPAIGN_NAME_CELL.ColumnIndex]
                .Value += data.CampaignName;
            WORKSHEET.Cells[AGENCY_NAME_CELL.RowIndex, AGENCY_NAME_CELL.ColumnIndex]
                .Value = data.AgencyName;
            WORKSHEET.Cells[CLIENT_NAME_CELL.RowIndex, CLIENT_NAME_CELL.ColumnIndex]
                .Value = data.ClientName;
            WORKSHEET.Cells[CAMPAIGN_FLIGHT_CELL.RowIndex, CAMPAIGN_FLIGHT_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value = $"{data.CampaignFlightStartDate} - {data.CampaignFlightEndDate}";
            WORKSHEET.Cells[GUARANTEED_DEMO_CELL.RowIndex, GUARANTEED_DEMO_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value = string.Join(",", data.GuaranteedDemo);
            WORKSHEET.Cells[CAMPAIGN_SPOT_LENGTH_CELL.RowIndex, CAMPAIGN_SPOT_LENGTH_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value = string.Join(", ", data.SpotLengths);
            WORKSHEET.Cells[POSTING_TYPE_CELL.RowIndex, POSTING_TYPE_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value = data.PostingType;
            WORKSHEET.Cells[STATUS_CELL.RowIndex, STATUS_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value = data.Status;
            WORKSHEET.Cells[Fluidity_CELL.RowIndex, Fluidity_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value = data.Fluidity;
            WORKSHEET.Cells[Account_Executive_CELL.RowIndex, Account_Executive_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value = data.AccountExecutive;
            WORKSHEET.Cells[Client_Contact_CELL.RowIndex, Client_Contact_CELL.ColumnIndex + SecondaryAudiencesOffset]
                .Value = data.ClientContact;

        }

        private void _PutDataIntoPlanQuarterTables(CampaignReportData campaignReportData)
        {
            currentRowIndex = firstDataRowIndex;
            for (int j = 0; j < campaignReportData.ProposalQuarterTables.Count; j++)
            {
                var table = campaignReportData.ProposalQuarterTables[j];

                //from the second table forward, we need to increment the row indices
                if (j > 0)
                {
                    //if the previous table had secondary audiences, we need to increment the curent row 
                    //index to account for all the space between the tables
                    currentRowIndex += (campaignReportData.ProposalQuarterTables[j - 1].HasSecondaryAudiences)
                        ? SPACE_BETWEEN_QUARTER_TABLES_WITH_SECONDARY_AUDIENCES
                        : SPACE_BETWEEN_QUARTER_TABLES;

                    //plan name row is 2 rows before the data row
                    quarterLabelRowIndex = currentRowIndex - 2;
                    firstDataRowIndex = currentRowIndex;
                }

                _InsertQuarterTableRows(campaignReportData.GuaranteedDemo, table);

                _SetRowData(currentRowIndex, START_TOTAL_COLUMN, _GetQuarterTableTotalRowObjects(table.TotalRow));
                if (table.HasSecondaryAudiences)
                {
                    _AddSecondaryAudiencesTables(table);
                }
                else if (HasSecondaryAudiences)
                {
                    _DeleteSecondaryAudienceEmptyTemplateTable();
                }
            }
        }

        private void _AddSecondaryAudiencesTables(ProposalQuarterTableData table)
        {
            currentRowIndex += 3; //there are 3 rows from the total row to the secondary audiences row

            //the secondary audiences are displayed in pears
            //add missing tables
            int numberOfSecondaryDoubleTables = table.SecondaryAudiencesTables.Count % 2 == 0
                            ? table.SecondaryAudiencesTables.Count / 2
                            : (table.SecondaryAudiencesTables.Count / 2) + 1;
            if (numberOfSecondaryDoubleTables - 1 > 0)
            {
                ExportSharedLogic.AddEmptyTables(WORKSHEET,
                    numberOfSecondaryDoubleTables,
                    currentRowIndex, ROWS_TO_COPY_SECONDARY_ONLY_TABLE);
            }

            for (int i = 0; i < table.SecondaryAudiencesTables.Count; i += 2)
            {
                //we display 2 tables side by side
                SecondaryDemoTable firstTable = table.SecondaryAudiencesTables[i];
                SecondaryDemoTable secondTable = null;
                WORKSHEET.Cells[$"{SECONDARY_AUDIENCE_LABEL_FIRST_COLUMN}{currentRowIndex}"].Value = firstTable.AudienceCode;

                if (i + 1 < table.SecondaryAudiencesTables.Count)   //if there is a second table
                {
                    secondTable = table.SecondaryAudiencesTables[i + 1];
                    WORKSHEET.Cells[$"{SECONDARY_AUDIENCE_LABEL_SECOND_COLUMN}{currentRowIndex}"].Value = secondTable.AudienceCode;
                }
                else
                {
                    _DeleteEmptyTable();
                }

                //set the height of the audience row and the table header row
                WORKSHEET.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
                WORKSHEET.Row(currentRowIndex + 1).Height = ExportSharedLogic.ROW_HEIGHT;

                currentRowIndex += 2; //skip table header row

                //insert the necessary rows
                WORKSHEET.InsertRow(currentRowIndex + 1, firstTable.Rows.Count - 1);

                for (int rowIndex = 0; rowIndex < firstTable.Rows.Count; rowIndex++)
                {
                    if (rowIndex > 0)
                    {   //copy the previous row styles
                        WORKSHEET.Cells[currentRowIndex - 1, ExportSharedLogic.FIRST_COLUMNS_INDEX, currentRowIndex - 1, ExportSharedLogic.END_COLUMN_INDEX]
                                .Copy(WORKSHEET.Cells[currentRowIndex, ExportSharedLogic.FIRST_COLUMNS_INDEX]);
                    }

                    List<object> row =
                        _GetSecondaryAudienceRowObjects(firstTable.Rows[rowIndex], secondTable?.Rows[rowIndex], rowIndex);
                    _SetRowData(currentRowIndex, START_COLUMN, row);

                    currentRowIndex++;
                }

                List<object> totalRow =
                        _GetSecondaryAudienceTotalRowObjects(firstTable.TotalRow, secondTable?.TotalRow);
                _SetRowData(currentRowIndex, START_SECONDARY_AUDIENCE_COLUMN, totalRow);

                //if we have other secondary audiences to display, increment row index
                if (i + 2 < table.SecondaryAudiencesTables.Count)
                {
                    //next secondary tables start after 2 rows
                    currentRowIndex += 2;
                }
            }
        }

        private void _DeleteEmptyTable()
        {
            string rangeAddress = $"O{currentRowIndex}:U{currentRowIndex + ROWS_TO_COPY_SECONDARY_ONLY_TABLE}";
            WORKSHEET.Cells[rangeAddress].Style.Fill.PatternType = ExcelFillStyle.None;
            WORKSHEET.Cells[rangeAddress].Clear();
        }

        private void _DeleteSecondaryAudienceEmptyTemplateTable()
        {
            //because the template file has secondary audiences tables for all the quarters
            //we need to adjust the template dynamically if the current quarter does not have 
            //secondary audiences tables           
            WORKSHEET.DeleteRow(currentRowIndex + 1, ROWS_SECONDARY_TABLE_WITH_SEPARATOR);
        }

        private List<object> _GetSecondaryAudienceRowObjects(AudienceData firstTableRow
            , AudienceData secondTableRow, int rowIndex)
        {
            //We use "OddSA1 and EvenSA1" when there is a single secondary audience table
            //We use "OddSA2 and EvenSA2" when there are 2 secondary audience tables
            List<object> row = new List<object>
            {
                (rowIndex % 2 == 0) ? "OddSA1" : "EvenSA1",  //used for conditional formatting)
                ExportSharedLogic.EMPTY_CELL,
                ExportSharedLogic.EMPTY_CELL,
                ExportSharedLogic.EMPTY_CELL,
                ExportSharedLogic.EMPTY_CELL,
                ExportSharedLogic.EMPTY_CELL,
                ExportSharedLogic.EMPTY_CELL,
                firstTableRow.VPVH,
                firstTableRow.RatingPoints,
                firstTableRow.TotalRatingPoints,
                firstTableRow.Impressions,
                firstTableRow.TotalImpressions,
                firstTableRow.CPM,
                firstTableRow.CPP
            };
            if (secondTableRow != null)
            {
                row.Add(secondTableRow.VPVH);
                row.Add(secondTableRow.RatingPoints);
                row.Add(secondTableRow.TotalRatingPoints);
                row.Add(secondTableRow.Impressions);
                row.Add(secondTableRow.TotalImpressions);
                row.Add(secondTableRow.CPM);
                row.Add(secondTableRow.CPP);
                row[0] = (rowIndex % 2 == 0) ? "OddSA2" : "EvenSA2";   //used for conditional formatting)
            }
            
            return row;
        }

        private List<object> _GetSecondaryAudienceTotalRowObjects(AudienceData firstTableRow
            , AudienceData secondTableRow)
        {
            List<object> row = new List<object>
            {
                ExportSharedLogic.NO_VALUE_CELL,
                ExportSharedLogic.NO_VALUE_CELL,
                firstTableRow.TotalRatingPoints,
                ExportSharedLogic.NO_VALUE_CELL,
                firstTableRow.TotalImpressions,
                firstTableRow.CPM,
                firstTableRow.CPP
            };
            if (secondTableRow != null)
            {
                row.Add(ExportSharedLogic.NO_VALUE_CELL);
                row.Add(ExportSharedLogic.NO_VALUE_CELL);
                row.Add(secondTableRow.TotalRatingPoints);
                row.Add(ExportSharedLogic.NO_VALUE_CELL);
                row.Add(secondTableRow.TotalImpressions);
                row.Add(secondTableRow.CPM);
                row.Add(secondTableRow.CPP);
            }
            return row;
        }

        private void _InsertQuarterTableRows(List<string> guaranteedDemo, ProposalQuarterTableData table)
        {
            WORKSHEET.Cells[$"{PLAN_NAME_COLUMN}{quarterLabelRowIndex}"].Value = table.QuarterLabel;
            if (HasSecondaryAudiences)
            {
                WORKSHEET.Cells[$"{SECONDARY_AUDIENCE_LABEL_SECOND_COLUMN}{quarterLabelRowIndex}"].Value = string.Join(",", guaranteedDemo);
            }
            else
            {
                WORKSHEET.Cells[$"{GUARANTEED_DEMO_COLUMN}{quarterLabelRowIndex}"].Value = string.Join(",", guaranteedDemo);
            }

            //Set height for plan name row and audiences row
            WORKSHEET.Row(quarterLabelRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            WORKSHEET.Row(quarterLabelRowIndex + 1).Height = ExportSharedLogic.ROW_HEIGHT;

            //insert count - 1 rows because we already have 1 row in the template
            //insert at position currentRowIndex+1 because we want to insert after the first data row existing in the template
            WORKSHEET.InsertRow(currentRowIndex + 1, table.Rows.Count - 1);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                WORKSHEET.Cells[firstDataRowIndex, ExportSharedLogic.FIRST_COLUMNS_INDEX, firstDataRowIndex, ExportSharedLogic.END_COLUMN_INDEX]
                    .Copy(WORKSHEET.Cells[currentRowIndex, ExportSharedLogic.FIRST_COLUMNS_INDEX]);

                _SetRowData(currentRowIndex, START_COLUMN, _GetQuarterTableRowObjects(table.Rows[i], i));
                currentRowIndex++;
            }
        }

        private List<object> _GetQuarterTableRowObjects(ProposalQuarterTableRowData rowData, int index)
        {
            var row = new List<object>() {
                rowData.DaypartCode,
                rowData.SpotLengthLabel,
                rowData.Units,
                rowData.UnitsCost,
                rowData.TotalCost,
                rowData.HHData.RatingPoints,
                rowData.HHData.TotalRatingPoints,
                rowData.HHData.Impressions,
                rowData.HHData.TotalImpressions,
                rowData.HHData.CPM,
                rowData.HHData.CPP,
                rowData.GuaranteedData.VPVH,
                rowData.GuaranteedData.RatingPoints,
                rowData.GuaranteedData.TotalRatingPoints,
                rowData.GuaranteedData.Impressions,
                rowData.GuaranteedData.TotalImpressions,
                rowData.GuaranteedData.CPM,
                rowData.GuaranteedData.CPP,
            };
            if (HasSecondaryAudiences)
            {
                row.Insert(5, ExportSharedLogic.EMPTY_CELL);
            }
            row.Insert(0, (index % 2 == 0 ? "Odd" : "Even"));
            row.Insert(1, ExportSharedLogic.EMPTY_CELL); //column B is empty

            return row;
        }

        private List<object> _GetQuarterTableTotalRowObjects(ProposalQuarterTableRowData totalRow)
        {
            var row = new List<object>() {
                totalRow.Units,
                ExportSharedLogic.NO_VALUE_CELL,
                totalRow.TotalCost,
                ExportSharedLogic.NO_VALUE_CELL,
                totalRow.HHData.TotalRatingPoints,
                ExportSharedLogic.NO_VALUE_CELL,
                totalRow.HHData.TotalImpressions,
                totalRow.HHData.CPM,
                totalRow.HHData.CPP,
                ExportSharedLogic.NO_VALUE_CELL,
                ExportSharedLogic.NO_VALUE_CELL,
                totalRow.GuaranteedData.TotalRatingPoints,
                ExportSharedLogic.NO_VALUE_CELL,
                totalRow.GuaranteedData.TotalImpressions,
                totalRow.GuaranteedData.CPM,
                totalRow.GuaranteedData.CPP,
            };
            if (HasSecondaryAudiences)
            {
                row.Insert(3, ExportSharedLogic.EMPTY_CELL);
            }

            return row;
        }

        private void _SetRowData(int rowIndex, string columnIndex, List<object> totalRow)
        {
            WORKSHEET.Row(rowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            WORKSHEET.Cells[$"{columnIndex}{rowIndex}"]
                .LoadFromArrays(new List<object[]> { totalRow.ToArray() });
        }
    }
}
