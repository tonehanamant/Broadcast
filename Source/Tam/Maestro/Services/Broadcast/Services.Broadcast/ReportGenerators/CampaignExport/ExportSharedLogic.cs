﻿using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ReportGenerators.CampaignExport
{
    public static class ExportSharedLogic
    {
        public static readonly int ROW_HEIGHT = 24;
        public static readonly int ROW_HEIGHT_LARGE = 30;
        public static readonly int END_COLUMN_INDEX = 25;
        public static readonly string FONT_COLOR = "#3d5261";
        public static readonly int FIRST_COLUMNS_INDEX = 1;
        private static readonly string NOT_FOUND_WORKSHEET = "Could not find worksheet {0} in template file {1}";
        private static readonly Color fontColor = ColorTranslator.FromHtml(FONT_COLOR);
        public static readonly string NO_VALUE_CELL = "-";
        public static readonly string EMPTY_CELL = null;

        /// <summary>
        /// Gets a worksheet by name.
        /// </summary>
        /// <param name="templateFilePath">The template file path.</param>
        /// <param name="package">The package to find the worksheet in.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns>ExcelWorksheet object</returns>
        /// <exception cref="Exception">Throws "Could not find worksheet in template file" if the worksheet was not found</exception>
        public static ExcelWorksheet GetWorksheet(string templateFilePath, ExcelPackage package, string sheetName)
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name.Equals(sheetName));
            if (worksheet == null)
            {
                throw new Exception(string.Format(NOT_FOUND_WORKSHEET, sheetName, Path.GetFileName(templateFilePath)));
            }

            return worksheet;
        }


        /// <summary>
        /// Adds copies of empty tables based on a specific table.
        /// </summary>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="count">Number of tables to add</param>
        /// <param name="firstRowIndex">Row index of the first row from the source table</param>
        /// <param name="rowsToCopy">Number of rows to copy</param>
        public static void AddEmptyTables(ExcelWorksheet worksheet, int count, int firstRowIndex, int rowsToCopy)
        {
            //we are inserting (count -1) and starting the for at 1 because we already have 1 table
            worksheet.InsertRow(firstRowIndex + rowsToCopy, (count - 1) * (rowsToCopy + 1));

            for (int i = 1; i < count; i++)
            {
                worksheet.Cells[firstRowIndex, FIRST_COLUMNS_INDEX, firstRowIndex + rowsToCopy, END_COLUMN_INDEX]
                    .Copy(worksheet.Cells[firstRowIndex + (i * rowsToCopy) + i, FIRST_COLUMNS_INDEX]);
            }
        }

        public static void ExtendTable(
            ExcelWorksheet worksheet,
            (int Row, int Column) fromSourceCell,
            (int Row, int Column) toSourceCell,
            int rowsToCopy)
        {
            if (rowsToCopy == 0)
            {
                return;
            }

            var rowsBatchSize = toSourceCell.Row - fromSourceCell.Row + 1;
            var rowsBatchToCopy = worksheet
                .Cells[fromSourceCell.Row, fromSourceCell.Column, toSourceCell.Row, toSourceCell.Column];

            var firstRow = fromSourceCell.Row + 1;
            var lastRow = fromSourceCell.Row + rowsToCopy * rowsBatchSize;

            for (var i = firstRow; i <= lastRow; i += rowsBatchSize)
            {
                var destinationCell = worksheet.Cells[i, fromSourceCell.Column];

                rowsBatchToCopy.Copy(destinationCell);
            }
        }

        public static void FormatTableRows(
            ExcelWorksheet worksheet,
            (int Row, int Column) topLeftCell,
            (int Row, int Column) bottomRightCell)
        {
            //if there is only 1 row in the table we do nothing
            if (topLeftCell.Row == bottomRightCell.Row)
            {
                return;
            }

            // remove bottom borders from all rows except the last one
            worksheet.Cells[
                topLeftCell.Row, topLeftCell.Column,
                bottomRightCell.Row - 1, bottomRightCell.Column].Style.Border.Bottom.Style = ExcelBorderStyle.None;

            // remove top borders from all rows except the first one
            worksheet.Cells[
                topLeftCell.Row + 1, topLeftCell.Column,
                bottomRightCell.Row, bottomRightCell.Column].Style.Border.Top.Style = ExcelBorderStyle.None;

            for (int row = topLeftCell.Row, count = 1; row <= bottomRightCell.Row; row++, count++)
            {
                worksheet.Row(row).Height = ROW_HEIGHT;

                if (count % 2 == 0)
                {
                    worksheet.Cells[row, FIRST_COLUMNS_INDEX].Value = "Even";
                }
            }
        }

        public static void PopulateContentRestrictions(ExcelWorksheet worksheet, List<DaypartData> daypartsData
            , string cellAddress)
        {
            if (daypartsData.Any())
            {
                ExcelRichText richText = null;
                foreach (var daypart in daypartsData
                    .Where(x => x.GenreRestrictions.Any() || x.ProgramRestrictions.Any()).ToList())
                {
                    if (worksheet.Cells[cellAddress].Value == null)
                    {
                        richText = worksheet.Cells[cellAddress].RichText.Add($"{daypart.DaypartCode}: ");
                    }
                    else
                    {
                        richText = worksheet.Cells[cellAddress].RichText.Add($" {daypart.DaypartCode}: ");
                    }
                    _SetRichTextStype(richText, true);

                    string contentRestrictionsRowText = string.Empty;
                    string genreRestrictions = _GetRestrictions(daypart.GenreRestrictions, "Genre ");
                    if (!string.IsNullOrWhiteSpace(genreRestrictions))
                    {
                        contentRestrictionsRowText += genreRestrictions;
                    }
                    string programRestrictions = _GetRestrictions(daypart.ProgramRestrictions, "Program ");
                    if (!string.IsNullOrWhiteSpace(programRestrictions))
                    {
                        //if there are genre restrictions, we need to add the separator
                        if (!string.IsNullOrWhiteSpace(genreRestrictions))
                        {
                            contentRestrictionsRowText += " | ";
                        }
                        contentRestrictionsRowText += programRestrictions;
                    }
                    richText = worksheet.Cells[cellAddress].RichText.Add(contentRestrictionsRowText);
                    _SetRichTextStype(richText, false);
                }
            }
        }

        //Need to manually set the font color because the rich text is not inheriting the cell color
        private static void _SetRichTextStype(ExcelRichText richText, bool isBold)
        {
            richText.Bold = isBold;
            richText.Color = fontColor;
        }

        private static string _GetRestrictions(List<DaypartRestrictionsData> daypartRestrictions, string restrictionTypeLabel)
        {
            string restrictionsText = string.Empty;
            if (daypartRestrictions.Any())
            {
                restrictionsText += restrictionTypeLabel;
                var groupByContainType = daypartRestrictions.GroupBy(x => x.ContainType);
                foreach (var groupByIncludeType in groupByContainType)
                {
                    var items = groupByIncludeType.ToList();
                    restrictionsText += $"{(groupByIncludeType.Key.Equals(ContainTypeEnum.Include) ? "Includes " : "Excludes ")}";
                    for (int i = 0; i < items.Count(); i++)
                    {
                        restrictionsText += string.Join(", ", items[i].Restrictions.OrderBy(x=>x));
                        restrictionsText += $" (Plan ID {items[i].PlanId})";
                        if (i < items.Count - 1)
                        {
                            restrictionsText += " ";
                        }
                    }
                    if (groupByContainType.Count() == 2)
                    {
                        restrictionsText += " ";
                    }
                }
            }
            return restrictionsText;
        }
    }
}