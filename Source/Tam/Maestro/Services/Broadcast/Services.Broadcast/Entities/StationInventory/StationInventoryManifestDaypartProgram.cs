using System;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifestDaypartProgram
    {
        public int StationInventoryManifestDaypartId { get; set; }
        public string ProgramName { get; set; }
        public string ShowType { get; set; }
        public string Genre { get; set; }
        public int GenreSourceId { get; set; }
        public int GenreId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}