using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Common.Services;
using Services.Broadcast.Converters;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitPickupFileValidation
    {
        List<string> ErrorMessages { get; set; }

        void ValidateFileStruct();

        void HasMissingData();

        void ValidateHeaders();
    }

    public abstract class AffidavitPickupFileValidation : IAffidavitPickupFileValidation
    {
        protected FileInfo _fileInfo;
        protected OutboundAffidavitFileValidationResultDto _currentFile;

        protected AffidavitPickupFileValidation(FileInfo fileInfo, OutboundAffidavitFileValidationResultDto currentFile)
        {
            _fileInfo = fileInfo;
            _currentFile = currentFile;
        }

        public List<string> ErrorMessages { get; set; } = new List<string>();


        public abstract void HasMissingData();

        public abstract void ValidateHeaders();

        public abstract void ValidateFileStruct();


        public static IAffidavitPickupFileValidation GetAffidavitValidationService(FileInfo fileInfo,OutboundAffidavitFileValidationResultDto currentFile)
        {
            if (fileInfo.Extension.Equals(".xlsx", StringComparison.InvariantCultureIgnoreCase))
            {
                return new AffidavitPickupValidationStrata(fileInfo,currentFile);
            }
            else if (fileInfo.Extension.Equals(".csv", StringComparison.InvariantCultureIgnoreCase))
            {
                return new AffidavitPickupValidationKeepingTrac(fileInfo, currentFile);
            }


            currentFile.ErrorMessages.Add($"Unknown extension type for file {currentFile.FilePath}");
            currentFile.Status = AffidaviteFileProcessingStatus.Invalid;
            return null;
        }
    }

    class AffidavitPickupValidationKeepingTrac : AffidavitPickupFileValidation
    {
        protected List<string> AffidavitFileHeaders = new List<string>() { "Estimate", "Station", "Air Date", "Air Time", "Air ISCI", "Demographic" ,"Act Ratings","Act Impression"};
        private CsvFileReader _csvReader;

        public AffidavitPickupValidationKeepingTrac(FileInfo fileInfo, OutboundAffidavitFileValidationResultDto currentFile) : base(fileInfo, currentFile)
        {
            currentFile.SourceId = (int) AffidaviteFileSourceEnum.KeepingTrac;
        }

        private List<string> _MissingHeaders = new List<string>();

        private bool _OnMissingHeader(string headerName)
        {
            _MissingHeaders.Add(headerName);
            return true;
        }
        public override void ValidateFileStruct()
        {
            var stream = File.OpenRead(_fileInfo.FullName);

            _csvReader = new CsvFileReader(AffidavitFileHeaders);
            _csvReader.OnMissingHeader = _OnMissingHeader;
            _csvReader.Initialize(stream);
        }

        public override void HasMissingData()
        {
            int row = 1;
            while (!_csvReader.IsEOF())
            {
                _csvReader.NextRow();

                if (_csvReader.IsEmptyRow())
                    break;

                foreach (string header in AffidavitFileHeaders)
                {
                    if (string.IsNullOrEmpty(_csvReader.GetCellValue(header)))
                    {
                        _currentFile.ErrorMessages.Add($"Missing '{header}' on row {row}");
                    }
                }
                row++;
            }
        }

        public override void ValidateHeaders()
        {
            _MissingHeaders.ForEach(header =>
                _currentFile.ErrorMessages.Add(string.Format("Could not find header for column '{0}' in file {1}", header,_currentFile.FilePath)));
        }
    }

    class AffidavitPickupValidationStrata : AffidavitPickupFileValidation
    {
        internal List<string> AffidavitFileHeaders = new List<string>() { "ESTIMATE_ID", "STATION_NAME", "DATE_RANGE", "SPOT_TIME", "SPOT_DESCRIPTOR", "COST" };
        public const string _ValidStrataTabName = "PostAnalRep_ExportDetail";
        private Dictionary<string, int> _FoundHeaders;

        private ExcelWorksheet _tab;

        public AffidavitPickupValidationStrata(FileInfo fileInfo, OutboundAffidavitFileValidationResultDto currentFile) 
            : base(fileInfo, currentFile)
        {
            currentFile.SourceId = (int) AffidaviteFileSourceEnum.Strata;
        }


        public override void HasMissingData()
        {
            var hasMissingData = false;
            for (var row = 2; row <= _tab.Dimension.End.Row; row++)
            {
                if (_IsEmptyRow(row, _tab))
                {
                    continue;
                }
                foreach (string name in AffidavitFileHeaders)
                {
                    if (string.IsNullOrWhiteSpace(_tab.Cells[row, _FoundHeaders[name]].Value?.ToString()))
                    {
                        _currentFile.ErrorMessages.Add($"Missing '{name}' on row {row}");
                        hasMissingData = true;
                    }
                }
            }
            if (hasMissingData)
            {
                _currentFile.Status = AffidaviteFileProcessingStatus.Invalid;
            }
        }

        public override void ValidateFileStruct()
        {
            var package = new ExcelPackage(_fileInfo, true);
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                if (worksheet.Name.Equals(_ValidStrataTabName))
                {
                    _tab = worksheet;
                }
            }
            if (_tab == null)
            {
                _currentFile.ErrorMessages.Add(string.Format("Could not find the tab {0} in file {1}", _ValidStrataTabName, _currentFile.FilePath));
                _currentFile.Status = AffidaviteFileProcessingStatus.Invalid;
            }
        }

        public override void ValidateHeaders()
        {
            _FoundHeaders = new Dictionary<string, int>();
            foreach (var header in AffidavitFileHeaders)
            {
                for (var column = 1; column <= _tab.Dimension.End.Column; column++)
                {
                    var cellValue = (string)_tab.Cells[1, column].Value;

                    if (!cellValue.Trim().Equals(header, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    _FoundHeaders.Add(header, column);
                    break;
                }

                if (!_FoundHeaders.ContainsKey(header))
                {
                    _currentFile.ErrorMessages.Add(string.Format("Could not find header for column {0} in file {1}", header, _currentFile.FilePath));
                }
            }
            if (_FoundHeaders.Count != AffidavitFileHeaders.Count)
            {
                _currentFile.Status = AffidaviteFileProcessingStatus.Invalid;
            }
        }

        private bool _IsEmptyRow(int row, ExcelWorksheet excelWorksheet)
        {
            for (var c = 1; c < excelWorksheet.Dimension.End.Column; c++)
                if (!string.IsNullOrWhiteSpace(excelWorksheet.Cells[row, c].Text))
                    return false;

            return true;
        }

    }
}
