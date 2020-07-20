using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Converters.Post
{
    public interface IPostFileParser : IApplicationService
    {
        List<post_file_details> ParseExcel(ExcelPackage stream);
        List<post_file_details> Parse(ExcelPackage package);
    }

    public abstract class BasePostFileParser : IPostFileParser
    {
        public static readonly string ColumnRequiredErrorMessage = "\t'{0}' column is required\n";
        public static readonly string InvalidDateErrorMessage = "\t'{0}' column is not a valid date\n";
        public static readonly string InvalidNumberErrorMessage = "\t'{0}' field has invalid number '{1}'\n";
        public static readonly string ErrorInColumn = "\tError in Column {0}\n";

        protected readonly List<string> WeekDays = new List<string> { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
        protected readonly DateTime MagicBaseDate = DateTime.Parse("12/30/1899");
        protected readonly IDataRepositoryFactory DataRepositoryFactory;

        protected BasePostFileParser(IDataRepositoryFactory dataRepositoryFactory)
        {
            DataRepositoryFactory = dataRepositoryFactory;
        }

        protected string GetCellValue(int row, int column, ExcelWorksheet excelWorksheet)
        {
            var value = excelWorksheet.Cells[row, column].Value ?? "";
            return value.ToString().Trim();
        }

        protected DateTime GetDateValue(int row, int column, ExcelWorksheet excelWorksheet)
        {
            // EPP beyond 4.1.0 doesn't accept empty inline.
            var value = excelWorksheet.Cells[row, column].GetValue<DateTime?>();
            return value ?? DateTime.MinValue;
        }

        protected bool IsEmptyRow(int row, ExcelWorksheet excelWorksheet)
        {
            for (var c = 1; c < excelWorksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrEmpty(excelWorksheet.Cells[row, c].Text))
                    return false;

            return true;
        }

        public List<post_file_details> ParseExcel(ExcelPackage excelPackage)
        {
            return Parse(excelPackage);
        }

        public abstract List<post_file_details> Parse(ExcelPackage package);
    }
}
