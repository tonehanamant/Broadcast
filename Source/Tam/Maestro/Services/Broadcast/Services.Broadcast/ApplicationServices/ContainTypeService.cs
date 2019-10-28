using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IContainTypeService : IApplicationService
    {
        List<LookupDto> GetContainTypes();
    }

    public class ContainTypeService : IContainTypeService
    {
        public List<LookupDto> GetContainTypes()
        {
            return EnumExtensions.ToLookupDtoList<ContainTypeEnum>();
        }
    }
}
