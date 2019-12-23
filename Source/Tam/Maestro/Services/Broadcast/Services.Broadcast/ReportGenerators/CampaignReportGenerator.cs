using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ReportGenerators
{
    public class CampaignReportGenerator : IReportGenerator<CampaignReportData>
    {
        private readonly string PROPOSAL_WORKSHEET_NAME = "Proposal";
        private readonly string CONTRACT_WORKSHEET_NAME = "Contract";
        private readonly string TERMS_WORKSHEET_NAME = "Terms & Conditions";
        private readonly string FLOW_CHART_WORKSHEET_NAME = "Flow Chart";

        private readonly string NOT_FOUND_WORKSHEET = "Could not find worksheet {0} in template file {1}";

        private readonly string TEMPLATE_FILENAME = "Template - Campaign Export.xlsx";

        private readonly int ROW_HEIGHT = 24;
        private readonly int EMPTY_TABLE_ROWS_NUMBER = 4;
        private readonly int END_COLUMN_INDEX = 25;
        private readonly int FIRST_COLUMNS_INDEX = 1;
        private readonly int ROWS_TO_COPY = 4;
        private int planNameRowIndex = 7;        
        private int firstDataRowIndex = 9;
        

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
        #endregion

        /// <summary>
        /// Generates a report of type CampaignReport
        /// </summary>
        /// <param name="dataObject">Data object used to generate the file</param>
        /// <returns>
        /// ReportOutput object containing the generated file stream
        /// </returns>
        public ReportOutput Generate(CampaignReportData dataObject)
        {
            var output = new ReportOutput(filename: dataObject.CampaignExportFileName);

            ExcelPackage package = _GetFileWithData(dataObject);

            package.SaveAs(output.Stream);
            package.Dispose();
            output.Stream.Position = 0;

            return output;
        }

        private ExcelPackage _GetFileWithData(CampaignReportData campaignReportData)
        {
            string templateFilePath = $@"{BroadcastServiceSystemParameter.BroadcastExcelTemplatesPath}\{TEMPLATE_FILENAME}";
            var package = new ExcelPackage(new FileInfo(templateFilePath), useStream: true);

            ExcelWorksheet proposalWorksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name.Equals(PROPOSAL_WORKSHEET_NAME));
            if (proposalWorksheet == null)
            {
                throw new Exception(string.Format(NOT_FOUND_WORKSHEET, PROPOSAL_WORKSHEET_NAME, Path.GetFileName(templateFilePath)));
            }
            _PopulateProposalWorksheetHeader(proposalWorksheet, campaignReportData);
            int lastDataRowIndex = _PopulateProposalWorksheetQuarterTables(proposalWorksheet, campaignReportData);
            _PopulateProposaTabTotalsTable(proposalWorksheet, campaignReportData.GuaranteedDemo, campaignReportData.CampaignTotalsTable, lastDataRowIndex);

            if (campaignReportData.Status.Equals("Proposal"))
            {
                package.Workbook.Worksheets.Delete(CONTRACT_WORKSHEET_NAME);
                package.Workbook.Worksheets.Delete(TERMS_WORKSHEET_NAME);
                package.Workbook.Worksheets.MoveToStart(PROPOSAL_WORKSHEET_NAME);
            }
            else
            {
                ExcelWorksheet contractWorksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name.Equals(PROPOSAL_WORKSHEET_NAME));
                if (contractWorksheet == null)
                {
                    throw new Exception(string.Format(NOT_FOUND_WORKSHEET, PROPOSAL_WORKSHEET_NAME, Path.GetFileName(templateFilePath)));
                }
               // _PopulateContractWorksheet(contractWorksheet, dataObject);
            }

            //set the first tab as the active tab in the file
            package.Workbook.Worksheets.First().Select();

            //force calculation 
            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            return package;
        }

        private int _PopulateProposalWorksheetQuarterTables(ExcelWorksheet proposalWorksheet, CampaignReportData campaignReportData)
        {
            _AddQuarterPlanEmptyTables(proposalWorksheet, campaignReportData.QuarterTables.Count);
            int lastDataRowIndex = _PutDataIntoPlanQuarterTables(proposalWorksheet, campaignReportData);
            return lastDataRowIndex;
        }

        private void _PopulateProposaTabTotalsTable(ExcelWorksheet proposalWorksheet, List<string> guaranteedDemo
            , ProposalQuarterTableData campaignTotalsTable, int lastDataRowIndex)
        {
            int currentRowIndex = lastDataRowIndex;

            currentRowIndex = _InsertTableRowsData(proposalWorksheet, guaranteedDemo, null, lastDataRowIndex
                    , END_COLUMN_INDEX, FIRST_COLUMNS_INDEX, currentRowIndex, campaignTotalsTable);

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

        private void _AddQuarterPlanEmptyTables(ExcelWorksheet worksheet, int count)
        {
            int planEndRowIndex = 10;
            worksheet.InsertRow(planNameRowIndex + 4, (count - 1) * 5);

            for (int i = 1; i < count; i++)
            {
                worksheet.Cells[planNameRowIndex, FIRST_COLUMNS_INDEX, planEndRowIndex, END_COLUMN_INDEX].Copy(worksheet.Cells[planNameRowIndex + (i * ROWS_TO_COPY) + i, FIRST_COLUMNS_INDEX]);
            }
        }

        private int _PutDataIntoPlanQuarterTables(ExcelWorksheet worksheet, CampaignReportData campaignReportData)
        {
            int currentRowIndex = firstDataRowIndex;
            for (int j = 0; j < campaignReportData.QuarterTables.Count; j++)
            {
                var table = campaignReportData.QuarterTables[j];
                currentRowIndex = _InsertTableRowsData(worksheet, campaignReportData.GuaranteedDemo, planNameRowIndex, firstDataRowIndex
                    , END_COLUMN_INDEX, FIRST_COLUMNS_INDEX, currentRowIndex, table);

                _SetTableTotals(worksheet, currentRowIndex, table);

                if (j < campaignReportData.QuarterTables.Count)
                {
                    currentRowIndex += EMPTY_TABLE_ROWS_NUMBER;
                    planNameRowIndex = currentRowIndex - 2;
                    firstDataRowIndex = currentRowIndex;
                }
            }
            return currentRowIndex;
        }

        private int _InsertTableRowsData(ExcelWorksheet worksheet, List<string> guaranteedDemo
            , int? planNameRowIndex, int firstDataRowIndex, int planEndColumnIndex, int firstColumnIndex
            , int currentRowIndex, ProposalQuarterTableData table)
        {
            //we only set table name for quarter tables and not for campaign totals table
            if(planNameRowIndex != null)
            {
                worksheet.Cells[$"{PLAN_NAME_COLUMN}{planNameRowIndex}"].Value = table.QuarterLabel;
                worksheet.Cells[$"{GUARANTEED_DEMO_COLUMN}{planNameRowIndex}"].Value = guaranteedDemo;
            }
            
            //insert count - 1 rows because we already have 1 row in the template
            //insert at position currentRowIndex+1 because we want to insert after the first data row existing in the template
            worksheet.InsertRow(currentRowIndex + 1, table.Rows.Count - 1);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                worksheet.Cells[firstDataRowIndex, firstColumnIndex, firstDataRowIndex, planEndColumnIndex].Copy(worksheet.Cells[currentRowIndex, firstColumnIndex]);

                var row = table.Rows[i];
                worksheet.Cells[$"{CONDITIONAL_FORMAT_COLUMN}{currentRowIndex}"].Value = (i % 2 == 0 ? "Odd" : "Even");

                _PopulateRowData(worksheet, currentRowIndex, row);

                worksheet.Row(currentRowIndex).Height = ROW_HEIGHT;
                currentRowIndex++;
            }

            return currentRowIndex;
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
