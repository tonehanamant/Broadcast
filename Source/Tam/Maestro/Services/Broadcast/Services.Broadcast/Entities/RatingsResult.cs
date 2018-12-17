using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class RatingsResult
    {
        public string LegacyCallLetters { get; set; }

        public DisplayDaypart Daypart { get; set; }

        public double? Rating { get; set; }

        public override string ToString()
        {
            return LegacyCallLetters + ":" + Rating;
        }
    }
}