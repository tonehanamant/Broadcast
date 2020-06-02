using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Entities.Vpvh
{
    public class VpvhQuarter
    {
        public int Id { get; set; }
        public DisplayAudience Audience { get; set; }
        public int Quarter { get; set; }
        public int Year { get; set; }
        public double AMNews { get; set; }
        public double PMNews { get; set; }
        public double SynAll { get; set; }
        public double Tdn { get; set; }
        public double Tdns { get; set; }
    }
}
