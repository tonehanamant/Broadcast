using OfficeOpenXml;
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
    }
}
