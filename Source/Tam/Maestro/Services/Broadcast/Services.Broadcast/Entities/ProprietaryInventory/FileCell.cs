namespace Services.Broadcast.Entities.ProprietaryInventory
{
    public class FileCell
    {
        public string ColumnLetter { get; set; }
        public int RowIndex { get; set; }

        public override string ToString()
        {
            return $"{ColumnLetter}{RowIndex}";
        }
    }
}
