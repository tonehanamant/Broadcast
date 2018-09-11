using System;

namespace Services.Broadcast.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class ExcelColumnAttribute : Attribute
    {
        public int ColumnIndex { get; set; }
        
        public ExcelColumnAttribute(int column)
        {
            ColumnIndex = column;
        }
    }
}
