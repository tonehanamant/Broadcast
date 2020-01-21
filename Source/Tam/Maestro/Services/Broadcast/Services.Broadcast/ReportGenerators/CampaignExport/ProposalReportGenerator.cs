using OfficeOpenXml;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public class ProposalReportGenerator
    {
        #region Cells addresses
        private readonly string CREATED_DATE_CELL = "T2";
        private readonly string CAMPAIGN_NAME_CELL = "F2";
        private readonly string AGENCY_NAME_CELL = "C5";
        private readonly string CLIENT_NAME_CELL = "E5";
        private readonly string CAMPAIGN_FLIGHT_CELL = "H5";
        private readonly string GUARANTEED_DEMO_CELL = "J5";
        private readonly string CAMPAIGN_SPOT_LENGTH_CELL = "L5";
        private readonly string POSTING_TYPE_CELL = "N5";
        private readonly string STATUS_CELL = "T5";
        private readonly string DAYPART_CODE_COLUMN = "C";
        private readonly string PLAN_NAME_COLUMN = "C";
        private readonly string SPOT_LENGTH_COLUMN = "D";
        private readonly string UNITS_COLUMN = "E";
        private readonly string UNIT_COST_COLUMN = "F";
        private readonly string TOTAL_COST_COLUMN = "G";
        private readonly string HH_GRP_COLUMN = "H";
        private readonly string TOTAL_HH_GRP_COLUMN = "I";
        private readonly string HH_IMPRESSIONS_COLUMN = "J";
        private readonly string TOTAL_HH_IMPRESSIONS_COLUMN = "K";
        private readonly string HH_CPM_COLUMN = "L";
        private readonly string HH_CPP_COLUMN = "M";
        private readonly string VPVH_COLUMN = "N";
        private readonly string GUARANTEED_DEMO_COLUMN = "N";
        private readonly string GRP_COLUMN = "O";
        private readonly string TOTAL_GRP_COLUMN = "P";
        private readonly string IMPRESSIONS_COLUMN = "Q";
        private readonly string TOTAL_IMPRESSIONS_COLUMN = "R";
        private readonly string CPM_COLUMN = "S";
        private readonly string CPP_COLUMN = "T";
        private readonly string CONDITIONAL_FORMAT_COLUMN = "A";
        private readonly string FOOTER_INFO_COLUMN_INDEX = "F";
        #endregion

        private readonly int EMPTY_TABLE_ROWS_NUMBER = 4;
        private readonly int ROWS_TO_COPY = 4;
        private int planNameRowIndex = 7;
        private int tableLastRowIndex = 10;
        private int firstDataRowIndex = 9;
        private int currentRowIndex = 0;

        public void PopulateProposalTab(CampaignReportData campaignReportData, ExcelWorksheet proposalWorksheet)
        {
            _PopulateProposalWorksheetHeader(proposalWorksheet, campaignReportData);
            _PopulateProposalWorksheetQuarterTables(proposalWorksheet, campaignReportData);
            _PopulateProposalTabTotalsTable(proposalWorksheet, campaignReportData.GuaranteedDemo, campaignReportData.ProposalCampaignTotalsTable);
            _PopulateMarketCoverage(proposalWorksheet, campaignReportData.MarketCoverageData);
            _PopulateDayparts(proposalWorksheet, campaignReportData.DaypartsData);

            //content restrictions row is the second row after current index
            currentRowIndex += 2;
            ExportSharedLogic.PopulateContentRestrictions(proposalWorksheet, campaignReportData.DaypartsData
                , $"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}");

            _PopulateFlightHiatuses(proposalWorksheet, campaignReportData.FlightHiatuses);
            _PopulateNotes(proposalWorksheet, campaignReportData.Notes);
        }

        private void _PopulateNotes(ExcelWorksheet proposalWorksheet, string notes)
        {
            //notes row is the second row after current index
            currentRowIndex += 2;
            if (!string.IsNullOrWhiteSpace(notes))
            {
                proposalWorksheet.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = notes;
            }
        }

        private void _PopulateFlightHiatuses(ExcelWorksheet proposalWorksheet, List<string> flightHiatuses)
        {
            //flight hiatuses row is the second row after current index
            currentRowIndex += 2;
            if (flightHiatuses.Any())
            {
                proposalWorksheet.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = string.Join(", ", flightHiatuses);
            }
        }
        

        private void _PopulateDayparts(ExcelWorksheet proposalWorksheet, List<DaypartData> daypartsData)
        {
            //dayparts row is the second row after current index
            currentRowIndex += 2;
            if (daypartsData.Any())
            {
                string daypartsRowData = string.Join(", ", daypartsData.Select(x => $"{x.DaypartCode} - {x.StartTime} - {x.EndTime}").ToList());
                proposalWorksheet.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = daypartsRowData;
            }
        }

        private void _PopulateMarketCoverage(ExcelWorksheet proposalWorksheet, MarketCoverageData data)
        {
            //markets row is the second row after current index
            currentRowIndex += 2;
            string marketCoverageValue = string.Format("~{0}% Minimum TV HH Coverage{1}{2}"
                , data.CoveragePercentage
                , data.BlackoutMarketsName.Any() ? $" | Blackout Markets: {string.Join(", ", data.BlackoutMarketsName)}" : string.Empty
                , data.PreferentialMarketsName.Any() ? $" | Preferential Markets: {string.Join(", ", data.PreferentialMarketsName)}" : string.Empty
                );
            proposalWorksheet.Cells[$"{FOOTER_INFO_COLUMN_INDEX}{currentRowIndex}"].Value = marketCoverageValue;
        }

        private void _PopulateProposalWorksheetQuarterTables(ExcelWorksheet proposalWorksheet, CampaignReportData campaignReportData)
        {
            ExportSharedLogic.AddEmptyTables(proposalWorksheet
                , campaignReportData.ProposalQuarterTables.Count
                , planNameRowIndex
                , tableLastRowIndex
                , ROWS_TO_COPY);
            _PutDataIntoPlanQuarterTables(proposalWorksheet, campaignReportData);
        }

        private void _PopulateProposalTabTotalsTable(ExcelWorksheet proposalWorksheet, List<string> guaranteedDemo
            , ProposalQuarterTableData campaignTotalsTable)
        {
            _InsertTableRowsData(proposalWorksheet, guaranteedDemo, campaignTotalsTable, false);

            _SetTableTotals(proposalWorksheet, currentRowIndex, campaignTotalsTable);
        }

        private void _PopulateProposalWorksheetHeader(ExcelWorksheet worksheet, CampaignReportData data)
        {
            worksheet.Cells[CREATED_DATE_CELL].Value += data.CreatedDate;
            worksheet.Cells[CAMPAIGN_NAME_CELL].Value += data.CampaignName;
            worksheet.Cells[AGENCY_NAME_CELL].Value = data.AgencyName;
            worksheet.Cells[CLIENT_NAME_CELL].Value = data.ClientName;
            worksheet.Cells[CAMPAIGN_FLIGHT_CELL].Value = $"{data.CampaignFlightStartDate} - {data.CampaignFlightEndDate}";
            worksheet.Cells[GUARANTEED_DEMO_CELL].Value = string.Join(",", data.GuaranteedDemo);
            worksheet.Cells[CAMPAIGN_SPOT_LENGTH_CELL].Value = string.Join(", ", data.SpotLengths);
            worksheet.Cells[POSTING_TYPE_CELL].Value = data.PostingType;
            worksheet.Cells[STATUS_CELL].Value = data.Status;
        }

        private void _PopulateContractWorksheet(ExcelWorksheet contractWorksheet, CampaignReportData dataObject)
        {
            throw new NotImplementedException();
        }

        private void _PutDataIntoPlanQuarterTables(ExcelWorksheet worksheet, CampaignReportData campaignReportData)
        {
            currentRowIndex = firstDataRowIndex;
            for (int j = 0; j < campaignReportData.ProposalQuarterTables.Count; j++)
            {
                var table = campaignReportData.ProposalQuarterTables[j];
                _InsertTableRowsData(worksheet, campaignReportData.GuaranteedDemo, table);

                _SetTableTotals(worksheet, currentRowIndex, table);

                if (j < campaignReportData.ProposalQuarterTables.Count)
                {
                    currentRowIndex += EMPTY_TABLE_ROWS_NUMBER;
                    planNameRowIndex = currentRowIndex - 2; //plan name row is 2 rows before the data row
                    firstDataRowIndex = currentRowIndex;
                }
            }
        }

        private void _InsertTableRowsData(ExcelWorksheet worksheet, List<string> guaranteedDemo
            , ProposalQuarterTableData table, bool setTableName = true)
        {
            //we only set table name for quarter tables and not for campaign totals table
            if (setTableName)
            {
                worksheet.Cells[$"{PLAN_NAME_COLUMN}{planNameRowIndex}"].Value = table.QuarterLabel;
            }
            worksheet.Cells[$"{GUARANTEED_DEMO_COLUMN}{planNameRowIndex}"].Value = string.Join(",", guaranteedDemo);

            //Set height for plan name row and audiences row
            worksheet.Row(planNameRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
            worksheet.Row(planNameRowIndex + 1).Height = ExportSharedLogic.ROW_HEIGHT;

            //insert count - 1 rows because we already have 1 row in the template
            //insert at position currentRowIndex+1 because we want to insert after the first data row existing in the template
            worksheet.InsertRow(currentRowIndex + 1, table.Rows.Count - 1);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                worksheet.Cells[firstDataRowIndex, ExportSharedLogic.FIRST_COLUMNS_INDEX, firstDataRowIndex, ExportSharedLogic.END_COLUMN_INDEX]
                    .Copy(worksheet.Cells[currentRowIndex, ExportSharedLogic.FIRST_COLUMNS_INDEX]);

                var row = table.Rows[i];
                worksheet.Cells[$"{CONDITIONAL_FORMAT_COLUMN}{currentRowIndex}"].Value = (i % 2 == 0 ? "Odd" : "Even");

                _PopulateRowData(worksheet, currentRowIndex, row);

                worksheet.Row(currentRowIndex).Height = ExportSharedLogic.ROW_HEIGHT;
                currentRowIndex++;
            }
        }

        private void _SetTableTotals(ExcelWorksheet worksheet, int currentRowIndex, ProposalQuarterTableData table)
        {
            worksheet.Cells[$"{UNITS_COLUMN}{currentRowIndex}"].Value = table.TotalUnits;
            worksheet.Cells[$"{TOTAL_COST_COLUMN}{currentRowIndex}"].Value = table.TotalCost;
            worksheet.Cells[$"{TOTAL_HH_GRP_COLUMN}{currentRowIndex}"].Value = table.TotalHHData.TotalRatingPoints;
            worksheet.Cells[$"{TOTAL_HH_IMPRESSIONS_COLUMN}{currentRowIndex}"].Value = table.TotalHHData.TotalImpressions;
            worksheet.Cells[$"{HH_CPM_COLUMN}{currentRowIndex}"].Value = table.TotalHHData.TotalCPM;
            worksheet.Cells[$"{HH_CPP_COLUMN}{currentRowIndex}"].Value = table.TotalHHData.TotalCPP;
            worksheet.Cells[$"{TOTAL_GRP_COLUMN}{currentRowIndex}"].Value = table.TotalGuaranteeedData.TotalRatingPoints;
            worksheet.Cells[$"{TOTAL_IMPRESSIONS_COLUMN}{currentRowIndex}"].Value = table.TotalGuaranteeedData.TotalImpressions;
            worksheet.Cells[$"{CPM_COLUMN}{currentRowIndex}"].Value = table.TotalGuaranteeedData.TotalCPM;
            worksheet.Cells[$"{CPP_COLUMN}{currentRowIndex}"].Value = table.TotalGuaranteeedData.TotalCPP;

            worksheet.Row(currentRowIndex).Height = 24;
        }

        private void _PopulateRowData(ExcelWorksheet worksheet, int currentRowIndex, ProposalQuarterTableRowData row, bool campaignTotals = false)
        {
            worksheet.Cells[$"{DAYPART_CODE_COLUMN}{currentRowIndex}"].Value = row.DaypartCode;
            worksheet.Cells[$"{SPOT_LENGTH_COLUMN}{currentRowIndex}"].Value = row.SpotLength;
            worksheet.Cells[$"{UNITS_COLUMN}{currentRowIndex}"].Value = row.Units;
            worksheet.Cells[$"{UNIT_COST_COLUMN}{currentRowIndex}"].Value = campaignTotals ? "-" : row.UnitCost.ToString();
            worksheet.Cells[$"{TOTAL_COST_COLUMN}{currentRowIndex}"].Value = row.TotalCost;

            worksheet.Cells[$"{HH_GRP_COLUMN}{currentRowIndex}"].Value = row.HHAudienceData.RatingPoints;
            worksheet.Cells[$"{TOTAL_HH_GRP_COLUMN}{currentRowIndex}"].Value = row.HHAudienceData.TotalRatingPoints;
            worksheet.Cells[$"{HH_IMPRESSIONS_COLUMN}{currentRowIndex}"].Value = row.HHAudienceData.Impressions;
            worksheet.Cells[$"{TOTAL_HH_IMPRESSIONS_COLUMN}{currentRowIndex}"].Value = row.HHAudienceData.TotalImpressions;
            worksheet.Cells[$"{HH_CPM_COLUMN}{currentRowIndex}"].Value = row.HHAudienceData.CPM;
            worksheet.Cells[$"{HH_CPP_COLUMN}{currentRowIndex}"].Value = row.HHAudienceData.CPP;

            worksheet.Cells[$"{VPVH_COLUMN}{currentRowIndex}"].Value = row.GuaranteedAudienceData.VPVH;
            worksheet.Cells[$"{GRP_COLUMN}{currentRowIndex}"].Value = row.GuaranteedAudienceData.RatingPoints;
            worksheet.Cells[$"{TOTAL_GRP_COLUMN}{currentRowIndex}"].Value = row.GuaranteedAudienceData.TotalRatingPoints;
            worksheet.Cells[$"{IMPRESSIONS_COLUMN}{currentRowIndex}"].Value = row.GuaranteedAudienceData.Impressions;
            worksheet.Cells[$"{TOTAL_IMPRESSIONS_COLUMN}{currentRowIndex}"].Value = row.GuaranteedAudienceData.TotalImpressions;
            worksheet.Cells[$"{CPM_COLUMN}{currentRowIndex}"].Value = row.GuaranteedAudienceData.CPM;
            worksheet.Cells[$"{CPP_COLUMN}{currentRowIndex}"].Value = row.GuaranteedAudienceData.CPP;
        }
    }
}
