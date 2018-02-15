using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.VisualBasic.FileIO;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters
{

    public abstract class TextFileLineReader
    {
        protected bool IsInitilized;
        protected List<string> _Headers;
        protected Dictionary<string, int> _HeaderDict;
        protected int DataStartRow;
        protected int CurrentRowNumber;

        public TextFileLineReader()
        {
            CurrentRowNumber = 0;
            DataStartRow = 0;
            IsInitilized = false;
        }
        public TextFileLineReader(List<string> headers) : this()
        {
            _Headers = headers;
        }

        public abstract IDisposable Initialize(Stream rawStream);
        public abstract bool IsEOF();
        public abstract bool IsEOFOrEmptyRow();

        public abstract bool IsEndOfRow(int column);
        public abstract bool IsEmptyRow();
        public virtual void NextRow()
        {
            CurrentRowNumber++;
        }
        public abstract string GetCellValue(string columnName);
        public abstract string GetCellValue(int col);

        protected virtual void _SetupHeadersValidateSheet()
        {
            var validationErrors = new List<string>();

            var headerRow = 0;

            // find starting row which is first row with values containing header stuff
            while (IsEmptyRow() && !IsEOF()) NextRow();

            if (IsEOF())
                throw new Exception("Empty file.");

            DataStartRow = headerRow + 1;

            int column = 0;
            _HeaderDict = new Dictionary<string, int>();
            while (!IsEndOfRow(column))
            {
                var header = GetCellValue(column);
                _HeaderDict.Add(header, column++);
            }

            foreach (var header in _Headers)
            {
                if (!_HeaderDict.ContainsKey(header))
                {
                    validationErrors.Add(string.Format("Could not find required column {0}.<br />", header));
                }
            }

            if (validationErrors.Any())
            {
                string message = "";
                validationErrors.ForEach(err => message += err + Environment.NewLine);
                throw new Exception(message);
            }
        }
    }
    public class CsvFileReader : TextFileLineReader
    {
        private TextFieldParser _Parser;
        private string[] _CurrentRow;

        public CsvFileReader(List<string> headers) : base()
        {
            _Headers = headers;
        }

        public override IDisposable Initialize(Stream rawStream)
        {
            _Parser = new TextFieldParser(rawStream);
            if (_Parser.EndOfData)
            {
                throw new Exception("Empty file not supported.");
            }
            _Parser.SetDelimiters(new string[] { "," });

            NextRow();  // point to first row
            _SetupHeadersValidateSheet();

            return _Parser;
        }

        public override bool IsEOF()
        {
            return _Parser.EndOfData;
        }
        public override bool IsEOFOrEmptyRow()
        {
            return _Parser.EndOfData || IsEmptyRow(); 
        }
        public override bool IsEndOfRow(int column)
        {
            if (column >= _CurrentRow.Length)
                return true;
            return false;
        }

        public override void NextRow()
        {
            _CurrentRow = _Parser.ReadFields();
            base.NextRow();
        }

        public override bool IsEmptyRow()
        {
            return _CurrentRow == null || _CurrentRow.Length == 0 || _CurrentRow.All(i => string.IsNullOrWhiteSpace(i));

        }
        public override string GetCellValue(int col)
        {
            if (col >= _CurrentRow.Length)
                return string.Empty;

            return _CurrentRow[col].Trim();
        }

        public override string GetCellValue(string columnName)
        {
            return _CurrentRow[_HeaderDict[columnName]].Trim();
        }
    }
    //public class ExcelFileReader : TextFileLineReader
    //{
    //    private OfficeOpenXml.ExcelPackage _ExcelPackage;
    //    private OfficeOpenXml.ExcelWorksheet _Worksheet;

    //    public ExcelFileReader(List<string> headers) : base(headers)
    //    {

    //    }

    //    public override IDisposable Initialize(Stream rawStream)
    //    {
    //        _ExcelPackage = new OfficeOpenXml.ExcelPackage(rawStream);
    //        _Worksheet = _ExcelPackage.Workbook.Worksheets.First();
    //        NextRow();
    //        _SetupHeadersValidateSheet();

    //        return _ExcelPackage;
    //    }

    //    public override bool IsEOF()
    //    {
    //        if (CurrentRowNumber > _Worksheet.Dimension.End.Row)
    //            return true;
    //        return false;
    //    }

    //    public override bool IsEndOfRow(int column)
    //    {
    //        if (column > _Worksheet.Dimension.End.Column)
    //            return true;
    //        return false;
    //    }

    //    public override bool IsEmptyRow()
    //    {
    //        for (int column = 1; column < _Worksheet.Dimension.End.Column; column++)
    //            if (!string.IsNullOrEmpty(_Worksheet.Cells[CurrentRowNumber, column].Text))
    //                return false;
    //        return true;
    //    }

    //    public override string GetCellValue(int col)
    //    {
    //        var value = _Worksheet.Cells[CurrentRowNumber, col].Value ?? "";
    //        return value.ToString().Trim();
    //    }

    //    public override string GetCellValue(string columnName)
    //    {
    //        var value = _Worksheet.Cells[CurrentRowNumber, _HeaderDict[columnName]].Value ?? "";
    //        return value.ToString().Trim();
    //    }
    //}

}
