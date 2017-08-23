using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class ProgramRating : Program
    {
        public readonly double? Rating;

        public ProgramRating(short stationCode, DisplayDaypart daypart, double? rating) : base(stationCode, daypart)
        {
            Rating = rating;
        }

        public override string ToString()
        {
            return StationCode + " " + Rating;
        }
    }
}