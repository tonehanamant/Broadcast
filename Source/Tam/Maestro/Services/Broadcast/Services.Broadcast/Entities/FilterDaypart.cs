using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class FilterDaypart
    {
        public DisplayDaypart DisplayDaypart { get; set; }
        public ContainTypeEnum Contain { get; set; }
    }
}
