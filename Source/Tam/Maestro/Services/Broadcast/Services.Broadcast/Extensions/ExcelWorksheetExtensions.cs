﻿using OfficeOpenXml;
using Services.Broadcast.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Services.Broadcast.Extensions
{
    public static class ExcelWorksheetExtensions
    {
        /// <summary>
        /// Converts excel table into a list of objects with defined type.
        /// The defined type should use ExcelColumnAttribute to identify which column of the table 
        /// should be mapped to particular column
        /// </summary>
        /// <param name="worksheet">Worksheet that represents the table with data for converting</param>
        /// <returns>List of objects with defined type</returns>
        public static IEnumerable<T> ConvertSheetToObjects<T>(this ExcelWorksheet worksheet) where T : new()
        {
            bool columnOnly(CustomAttributeData y) => y.AttributeType == typeof(ExcelColumnAttribute);

            var columns = typeof(T)
                    .GetProperties()
                    .Where(x => x.CustomAttributes.Any(columnOnly))
                    .Select(p => new
                    {
                        Property = p,
                        Column = p.GetCustomAttributes<ExcelColumnAttribute>().First().ColumnIndex //safe because if where above
                    })
                    .ToList();
            
            var rows = worksheet.Cells
                .Select(cell => cell.Start.Row)
                .Distinct()
                .OrderBy(x => x);
            
            var collection = rows.Skip(1)
                .Select(row =>
                {
                    var tnew = new T();

                    columns.ForEach(column =>
                    {
                        //This is the real wrinkle to using reflection - Excel stores all numbers as double including int
                        var val = worksheet.Cells[row, column.Column];

                        //If it is numeric it is a double since that is how excel stores all numbers
                        if (val.Value == null)
                        {
                            column.Property.SetValue(tnew, null);
                            return;
                        }

                        if (column.Property.PropertyType == typeof(int) || column.Property.PropertyType == typeof(int?))
                        {
                            column.Property.SetValue(tnew, val.GetValue<int>());
                            return;
                        }

                        if (column.Property.PropertyType == typeof(double))
                        {
                            column.Property.SetValue(tnew, val.GetValue<double>());
                            return;
                        }

                        if (column.Property.PropertyType == typeof(DateTime))
                        {
                            column.Property.SetValue(tnew, val.GetValue<DateTime>());
                            return;
                        }

                        //Its a string
                        column.Property.SetValue(tnew, val.GetValue<string>());
                    });

                    return tnew;
                });
            
            return collection;
        }

        /// <summary>
        /// Gets the string value from a cell handling null reference
        /// </summary>
        /// <param name="cell">Excel cell object</param>
        /// <returns>String value or null</returns>
        public static string GetStringValue(this ExcelRange cell)
        {
            return cell.Value?.ToString()?.Trim();
        }

        /// <summary>
        /// Gets the text value from a cell handling null reference
        /// </summary>
        /// <param name="cell">Excel cell object</param>
        /// <returns>String value or null</returns>
        public static string GetTextValue(this ExcelRange cell)
        {
            return cell.Text?.ToString()?.Trim();
        }

        /// <summary>
        /// Gets int value or null from the excel cell
        /// </summary>
        /// <param name="cell">Excel cell object</param>
        /// <returns>Int value or null</returns>
        public static int? GetIntValue(this ExcelRange cell)
        {
            return int.TryParse(GetStringValue(cell), out var result) ? result : (int?)null;
        }

        /// <summary>
        /// Gets double value or null from the excel cell
        /// </summary>
        /// <param name="cell">Excel cell object</param>
        /// <returns>Double value or null</returns>
        public static double? GetDoubleValue(this ExcelRange cell)
        {
            return double.TryParse(GetStringValue(cell), out var result) ? result : (double?)null;
        }

        /// <summary>
        /// Gets decimal value or null from the excel cell
        /// </summary>
        /// <param name="cell">Excel cell object</param>
        /// <returns>Decimal value or null</returns>
        public static decimal? GetDecimalValue(this ExcelRange cell)
        {
            return decimal.TryParse(GetStringValue(cell), out var result) ? result : (decimal?)null;
        }

        /// <summary>
        /// Gets the column letter based on column index
        /// </summary>
        /// <param name="columnIndex">Column index</param>
        /// <returns>Column letter</returns>
        public static string GetColumnAdress(this int columnIndex)
        {
            if (columnIndex <= 26)
            {
                return Convert.ToChar(columnIndex + 64).ToString();
            }
            int div = columnIndex / 26;
            int mod = columnIndex % 26;
            if (mod == 0) { mod = 26; div--; }
            return GetColumnAdress(div) + GetColumnAdress(mod);
        }
    }
}
