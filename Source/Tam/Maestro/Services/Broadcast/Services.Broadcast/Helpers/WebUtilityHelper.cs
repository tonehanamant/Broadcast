using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Services.Broadcast.Helpers
{
    public static class WebUtilityHelper
    {
        public static void HtmlDecodeProgramNames(IEnumerable<ProgramMappingsFileRequestDto> programMappings)
        {
            foreach (var item in programMappings)
            {
                item.OriginalProgramName = WebUtility.HtmlDecode(item.OriginalProgramName).UnicodeDecodeString();
                item.OfficialProgramName = WebUtility.HtmlDecode(item.OfficialProgramName).UnicodeDecodeString();
            }
        }

        public static void HtmlDecodeProgramNames(InventoryFileBase inventoryFileBase)
        {
            foreach (var item in inventoryFileBase.GetAllManifests().SelectMany(x => x.ManifestDayparts))
            {
                item.ProgramName = WebUtility.HtmlDecode(item.ProgramName).UnicodeDecodeString();
            }
        }
    }
}
