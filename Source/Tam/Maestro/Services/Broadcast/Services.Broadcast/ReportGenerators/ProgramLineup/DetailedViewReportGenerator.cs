﻿using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.ReportGenerators.CampaignExport;
using System;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.ProgramLineup
{
    public class DetailedViewReportGenerator
    {
        #region Cells addresses
        private readonly string PLAN_HEADER_NAME_CELL = "E2";
        private readonly string REPORT_GENERATED_AND_ACCURACY_ESTIMATE_DATES_CELL = "J2";
        private readonly string AGENCY_CELL = "C6";
        private readonly string CLIENT_CELL = "E6";
        private readonly string FLIGHT_CELL = "F6";
        private readonly string GUARANTEED_DEMO_CELL = "H6";
        private readonly string SPOT_LENGTH_CELL = "I6";
        private readonly string POSTING_TYPE_CELL = "J6";
        private readonly string ACCOUNT_EXECUTIVE_CELL = "K6";
        private readonly string CLIENT_CONTRACT_CELL = "L6";

        private (int Row, int Column) TableTopLeftCell = (Row : 9, Column : 3 );
        private (int Row, int Column) TableTopRightCell = (Row: 9, Column: 11);
        #endregion

        internal void PopulateTab(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            _AddHeaderInformation(worksheet, reportData);
            _PopulateTable(worksheet, reportData);
        }

        private void _AddHeaderInformation(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            worksheet.Cells[PLAN_HEADER_NAME_CELL].Value = reportData.PlanHeaderName;
            worksheet.Cells[REPORT_GENERATED_AND_ACCURACY_ESTIMATE_DATES_CELL].Value = 
                $"Report generated on {reportData.ReportGeneratedDate}" +
                Environment.NewLine +
                $"Estimate as of {reportData.AccuracyEstimateDate}";
            
            worksheet.Cells[AGENCY_CELL].Value = reportData.Agency;
            worksheet.Cells[CLIENT_CELL].Value = reportData.Client;
            worksheet.Cells[FLIGHT_CELL].Value = $"{reportData.FlightStartDate} - {reportData.FlightEndDate}";
            worksheet.Cells[GUARANTEED_DEMO_CELL].Value = reportData.GuaranteedDemo;
            worksheet.Cells[SPOT_LENGTH_CELL].Value = reportData.SpotLengths;
            worksheet.Cells[POSTING_TYPE_CELL].Value = reportData.PostingType;
            worksheet.Cells[ACCOUNT_EXECUTIVE_CELL].Value = reportData.AccountExecutive;
            worksheet.Cells[CLIENT_CONTRACT_CELL].Value = reportData.ClientContact;
        }

        private void _PopulateTable(ExcelWorksheet worksheet, ProgramLineupReportData reportData)
        {
            (int Row, int Column) tableBottomRightCell = 
            (
                Row: TableTopLeftCell.Row + reportData.DetailedViewRows.Count - 1
                , TableTopRightCell.Column
            );

            ExportSharedLogic.ExtendTable(
                worksheet,
                TableTopLeftCell,
                TableTopRightCell,
                reportData.DetailedViewRows.Count - 1);

            ExportSharedLogic.FormatTableRows(
                worksheet,
                TableTopLeftCell,
                tableBottomRightCell);

            var tableData = reportData.DetailedViewRows
                .Select(x => new object[]
                {
                    x.Rank,
                    x.DMA,
                    x.Station,
                    x.NetworkAffiliation,
                    x.Days,
                    x.TimePeriods,
                    x.Program,
                    x.Genre,
                    x.Daypart
                })
                .ToList();

            worksheet.Cells[TableTopLeftCell.Row, TableTopLeftCell.Column].LoadFromArrays(tableData);
        }
    }
}
