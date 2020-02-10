using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using SecondaryDemoTable = Services.Broadcast.Entities.Campaign.ProposalQuarterTableData.SecondayAudienceTable;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class ProposalReportGenerator
    {
        #region Cells addresses
        private readonly string START_COLUMN = "A";
        private readonly string START_TOTAL_COLUMN = "E";
        private readonly string START_SECONDARY_AUDIENCE_COLUMN = "H";

        private readonly string CREATED_DATE_CELL = "T2";
        private readonly string CAMPAIGN_NAME_CELL = "F2";
        private readonly string AGENCY_NAME_CELL = "C5";
        private readonly string CLIENT_NAME_CELL = "E5";
        private readonly string CAMPAIGN_FLIGHT_CELL = "H5";
        private readonly string GUARANTEED_DEMO_CELL = "J5";
        private readonly string CAMPAIGN_SPOT_LENGTH_CELL = "L5";
        private readonly string POSTING_TYPE_CELL = "N5";
        private readonly string STATUS_CELL = "T5";
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

        public void PopulateProposalTab(CampaignReportData campaignReportData, ExcelWorksheet proposalWorksheet)
        {
            WORKSHEET = proposalWorksheet;
            _PopulateHeader(campaignReportData);
            _PopulateQuarterTables(campaignReportData);
            _PopulateTotalsTable(campaignReportData);
            _PopulateMarketCoverage(campaignReportData.MarketCoverageData);
            _PopulateDayparts(campaignReportData.DaypartsData);

            //content restrictions row is the second row after current index
            currentRowIndex += 2;
            ExportSharedLogic.PopulateContentRestrictions(WORKSHEET, campaignReportData.DaypartsData
                , $"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}");

            _PopulateFlightHiatuses(campaignReportData.FlightHiatuses);
            _PopulateNotes(campaignReportData.Notes);
        }

        private void _PopulateNotes(string notes)
        {
            //notes row is the second row after current index
            currentRowIndex += 2;
            if (!string.IsNullOrWhiteSpace(notes))
            {
                WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = notes;
            }
        }

        private void _PopulateFlightHiatuses(List<string> flightHiatuses)
        {
            //flight hiatuses row is the second row after current index
            currentRowIndex += 2;
            if (flightHiatuses.Any())
            {
                WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = string.Join(", ", flightHiatuses);
            }
        }

        private void _PopulateDayparts(List<DaypartData> daypartsData)
        {
            //dayparts row is the second row after current index
            currentRowIndex += 2;
            if (daypartsData.Any())
            {
                string daypartsRowData = string.Join(", ", daypartsData.Select(x => $"{x.DaypartCode} - {x.StartTime} - {x.EndTime}").ToList());
                WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = daypartsRowData;
            }
        }

        private void _PopulateMarketCoverage(MarketCoverageData data)
        {
            //markets row is the second row after current index
            currentRowIndex += 2;
            string marketCoverageValue = string.Format("~{0}% Minimum TV HH Coverage{1}{2}"
                , data.CoveragePercentage
                , data.BlackoutMarketsName.Any() ? $" | Blackout Markets: {string.Join(", ", data.BlackoutMarketsName)}" : string.Empty
                , data.PreferentialMarketsName.Any() ? $" | Preferential Markets: {string.Join(", ", data.PreferentialMarketsName)}" : string.Empty
                );
            WORKSHEET.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = marketCoverageValue;
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

            _InsertQuarterMainTableRowsData(campaignReportData.GuaranteedDemo, 
                campaignReportData.ProposalCampaignTotalsTable, campaignReportData.HasSecondaryAudiences);

            //add total row data
            _SetRowData(currentRowIndex, START_TOTAL_COLUMN, campaignReportData.ProposalCampaignTotalsTable.TotalRow);
            
            //this logic needs adjusting when populating secondary audiences tables for campaign totals
            if (!campaignReportData.ProposalCampaignTotalsTable.HasSecondaryAudiences)
            {                
                _DeleteSecondaryAudienceEmptyTemplateTable();
            }
        }

        private void _PopulateHeader(CampaignReportData data)
        {
            WORKSHEET.Cells[CREATED_DATE_CELL].Value += data.CreatedDate;
            WORKSHEET.Cells[CAMPAIGN_NAME_CELL].Value += data.CampaignName;
            WORKSHEET.Cells[AGENCY_NAME_CELL].Value = data.AgencyName;
            WORKSHEET.Cells[CLIENT_NAME_CELL].Value = data.ClientName;
            WORKSHEET.Cells[CAMPAIGN_FLIGHT_CELL].Value = $"{data.CampaignFlightStartDate} - {data.CampaignFlightEndDate}";
            WORKSHEET.Cells[GUARANTEED_DEMO_CELL].Value = string.Join(",", data.GuaranteedDemo);
            WORKSHEET.Cells[CAMPAIGN_SPOT_LENGTH_CELL].Value = string.Join(", ", data.SpotLengths);
            WORKSHEET.Cells[POSTING_TYPE_CELL].Value = data.PostingType;
            WORKSHEET.Cells[STATUS_CELL].Value = data.Status;
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

                _InsertQuarterMainTableRowsData(campaignReportData.GuaranteedDemo, table, campaignReportData.HasSecondaryAudiences);

                _SetRowData(currentRowIndex, START_TOTAL_COLUMN, table.TotalRow);
                if (table.HasSecondaryAudiences)
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
                            //add this for conditional formating
                            WORKSHEET.Cells[$"{START_COLUMN}{currentRowIndex}"].Value = (rowIndex % 2 == 0) ? "Even" : "Odd";

                            _AddSecondaryAudienceRowData(firstTable.Rows[rowIndex], secondTable?.Rows[rowIndex]);

                            currentRowIndex++;
                        }
                        _AddSecondaryAudienceRowData(firstTable.TotalRow, secondTable?.TotalRow);

                        //if we have other secondary audiences to display, increment row index
                        if (i + 2 < table.SecondaryAudiencesTables.Count)
                        {
                            //next secondary tables start after 2 rows
                            currentRowIndex += 2;
                        }
                    }
                }
                else
                {
                    _DeleteSecondaryAudienceEmptyTemplateTable();
                }
            }
        }

        private void _DeleteSecondaryAudienceEmptyTemplateTable()
        {
            //because the template file has secondary audiences tables for all the quarters
            //we need to adjust the template dynamically if the current quarter does not have 
            //secondary audiences tables
            for(int i=0;i< ROWS_SECONDARY_TABLE_WITH_SEPARATOR; i++)
            {   //always delete the currentRowIndex + 1 row because once the row gets deleted
                //the next iteration will skip rows if we use "i" instead of "1"
                WORKSHEET.DeleteRow(currentRowIndex + 1);
            }
        }

        private void _AddSecondaryAudienceRowData(List<object> firstTableRow,
            List<object> secondTableRow)
        {
            var completeRow = new List<object>();
            completeRow.AddRange(firstTableRow);
            if (secondTableRow != null)
            {
                completeRow.AddRange(secondTableRow);
            }

            _SetRowData(currentRowIndex, START_SECONDARY_AUDIENCE_COLUMN, completeRow);
        }

        private void _InsertQuarterMainTableRowsData(List<string> guaranteedDemo, ProposalQuarterTableData table, bool hasSecondaryAudiences)
        {
            WORKSHEET.Cells[$"{PLAN_NAME_COLUMN}{quarterLabelRowIndex}"].Value = table.QuarterLabel;
            if (hasSecondaryAudiences)
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

                var row = table.Rows[i];
                row.Insert(0, (i % 2 == 0 ? "Odd" : "Even"));
                row.Insert(1, null); //there is an empty cell

                _SetRowData(currentRowIndex, START_COLUMN, row);
                currentRowIndex++;
            }
        }

        private void _SetRowData(int rowIndex, string columnIndex, List<object> totalRow)
        {
            WORKSHEET.Row(rowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            WORKSHEET.Cells[$"{columnIndex}{rowIndex}"]
                .LoadFromArrays(new List<object[]> { totalRow.ToArray() });
        }
    }
}
