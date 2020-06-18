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
                item.OriginalProgramName = HtmlDecodeProgramName(item.OriginalProgramName);
                item.OfficialProgramName = HtmlDecodeProgramName(item.OfficialProgramName);
            }
        }

        public static void HtmlDecodeProgramNames(InventoryFileBase inventoryFileBase)
        {
            foreach (var item in inventoryFileBase.GetAllManifests().SelectMany(x => x.ManifestDayparts))
            {
                item.ProgramName = HtmlDecodeProgramName(item.ProgramName);
            }
        }

        public static string HtmlDecodeProgramName(string programName)
        {
            var result = WebUtility.HtmlDecode(programName).UnicodeDecodeString();
            return result;
        }
    }
}
