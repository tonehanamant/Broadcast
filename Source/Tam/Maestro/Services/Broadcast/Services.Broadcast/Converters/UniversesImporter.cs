using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public interface IUniversesFileImporter
    {
        List<NtiUniverseExcelRecord> ReadUniverses(Stream stream);
    }

    public class UniversesFileImporter : IUniversesFileImporter
    {
        public List<NtiUniverseExcelRecord> ReadUniverses(Stream stream)
        {
            var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.First();

            return worksheet.ConvertSheetToObjects<NtiUniverseExcelRecord>().ToList();
        }
    }
}
