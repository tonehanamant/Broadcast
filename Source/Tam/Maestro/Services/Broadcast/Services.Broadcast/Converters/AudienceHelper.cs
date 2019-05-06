using System;

namespace Services.Broadcast.Converters
{
    public static class AudienceHelper
    {
        public static bool TryMapToSupportedFormat(string audienceCode, out string result)
        {
            result = null;

            if (audienceCode.StartsWith("P", StringComparison.OrdinalIgnoreCase))
            {
                result = 'A' + audienceCode.Substring(1);
                return true;
            }

            return false;
        }
    }
}
