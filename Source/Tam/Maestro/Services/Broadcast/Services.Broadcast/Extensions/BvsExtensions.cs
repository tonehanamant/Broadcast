using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Extensions
{
    public static class BvsExtensions
    {
        public static bool IsInSpec(this bvs_file_details bvsDetails)
        {
            return bvsDetails.status == (int)TrackingStatus.InSpec;
        }
        public static bool IsOutOfSpec(this bvs_file_details bvsDetails)
        {
            return !bvsDetails.IsInSpec();
        }
        public static IEnumerable<bvs_file_details> InSpec(this IEnumerable<bvs_file_details> bvsDetails)
        {
            return bvsDetails.Where(bfd => bfd.IsInSpec());
        }
        public static IEnumerable<bvs_file_details> OutOfSpec(this IEnumerable<bvs_file_details> bvsDetails)
        {
            return bvsDetails.Where(bfd => bfd.IsOutOfSpec());
        }

        public static void ClearStatus(this bvs_file_details bvsFileDetails)
        {
             bvsFileDetails.status = (bvsFileDetails.status != (int)TrackingStatus.OfficialOutOfSpec)
                                                ? (int)TrackingStatus.UnTracked : (int)TrackingStatus.OfficialOutOfSpec;
        }
    }
}
