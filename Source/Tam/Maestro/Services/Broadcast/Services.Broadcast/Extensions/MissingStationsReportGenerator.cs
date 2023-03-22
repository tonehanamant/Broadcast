using OfficeOpenXml;
using OfficeOpenXml.Style;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Extensions
{
    public static class MissingStationsReportGenerator
    {
        
        /// <summary>
        /// Gets the file with data.
        /// </summary>
        /// <param name="missingStationsData">The missing station report data.</param>
        /// <returns></returns>
        public static MemoryStream Generate(List<StationsGapDto> missingStationsData)
        {
            ExcelPackage package = new ExcelPackage();
            MemoryStream stream = new MemoryStream();
            var workSheet = package.Workbook.Worksheets.Add("Missing-Stations-Info");
            workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;
            //Header of table 

            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "LegacyCallLetters";
            workSheet.Cells[1, 2].Value = "MarketCode";
            workSheet.Cells[1, 3].Value = "MarketName";
            workSheet.Cells[1, 4].Value = "Affiliation";

            //Body of table  
            int recordIndex = 2;
            foreach (var stn in missingStationsData)
            {
                workSheet.Cells[recordIndex, 1].Value = stn.LegacyCallLetters;
                workSheet.Cells[recordIndex, 2].Value = stn.MarketCode;
                workSheet.Cells[recordIndex, 3].Value = stn.MarketName;
                workSheet.Cells[recordIndex, 4].Value = stn.Affiliation;
                recordIndex++;
            }
            workSheet.Column(1).AutoFit();
            workSheet.Column(2).AutoFit();
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).AutoFit();          


            package.Workbook.Worksheets.First().Select();

            package.Workbook.Calculate();
            package.Workbook.CalcMode = ExcelCalcMode.Automatic;
            package.SaveAs(stream);
            package.Dispose();
            stream.Position = 0;

            return stream;          
        }
    }
}
