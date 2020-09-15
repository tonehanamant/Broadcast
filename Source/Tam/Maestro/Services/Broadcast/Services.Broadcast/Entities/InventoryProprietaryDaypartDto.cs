namespace Services.Broadcast.Entities
{
    public class InventoryProprietaryDaypartDto
    {
        public int InventorySourceId { get; set; }
        public int DefaultDaypartId { get; set; }
        public string ProgramName { get; set; }
        public int GenreId { get; set; }
        public int ShowTypeId { get; set; }
    }
}