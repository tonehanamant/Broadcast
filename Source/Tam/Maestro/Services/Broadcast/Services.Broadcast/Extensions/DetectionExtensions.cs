using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Extensions
{
    public static class DetectionExtensions
    {
        public static bool IsInSpec(this detection_file_details bvsDetails)
        {
            return bvsDetails.status == (int)TrackingStatus.InSpec;
        }
        public static bool IsOutOfSpec(this detection_file_details bvsDetails)
        {
            return !bvsDetails.IsInSpec();
        }
        public static IEnumerable<detection_file_details> InSpec(this IEnumerable<detection_file_details> bvsDetails)
        {
            return bvsDetails.Where(bfd => bfd.IsInSpec());
        }
        public static IEnumerable<detection_file_details> OutOfSpec(this IEnumerable<detection_file_details> bvsDetails)
        {
            return bvsDetails.Where(bfd => bfd.IsOutOfSpec());
        }

        public static void ClearStatus(this detection_file_details bvsFileDetails)
        {
             bvsFileDetails.status = (bvsFileDetails.status != (int)TrackingStatus.OfficialOutOfSpec)
                                                ? (int)TrackingStatus.UnTracked : (int)TrackingStatus.OfficialOutOfSpec;
        }
    }
}
