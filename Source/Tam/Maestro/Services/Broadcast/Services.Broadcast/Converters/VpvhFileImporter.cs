using OfficeOpenXml;
using Services.Broadcast.Entities.Vpvh;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public interface IVpvhFileImporter
    {
        List<VpvhExcelRecord> ReadVpvhs(Stream stream);
    }

    public class VpvhFileImporter : IVpvhFileImporter
    {
        public List<VpvhExcelRecord> ReadVpvhs(Stream stream)
        {
            try
            {
                var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();

                return worksheet.ConvertSheetToObjects<VpvhExcelRecord>().ToList();
            }
            catch
            {
                throw new Exception("Invalid VPVH file.");
            }
        }
    }
}
