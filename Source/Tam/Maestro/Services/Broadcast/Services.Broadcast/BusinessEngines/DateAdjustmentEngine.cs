using Common.Services.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.BusinessEngines
{

    public interface IDateAdjustmentEngine : IApplicationService
    {
        /// <summary>
        /// 3a-3a rule
        /// </summary>
        /// <param name="date"></param>
        /// <param name="airTime"></param>
        /// <returns></returns>
        DateTime ConvertToNSITime(DateTime date, TimeSpan airTime);

        /// <summary>
        /// Uses ConvertToNSITime
        /// </summary>
        /// <param name="date"></param>
        /// <param name="airTime"></param>
        /// <returns></returns>
        DateTime ConvertToNTITime(DateTime date, TimeSpan airTime);
    }

    public class DateAdjustmentEngine : IDateAdjustmentEngine
    {
        /// <summary>
        /// 3a-3a rule
        /// </summary>
        /// <param name="date"></param>
        /// <param name="airTime"></param>
        /// <returns></returns>
        public DateTime ConvertToNSITime(DateTime date, TimeSpan airTime)
        {
            if (airTime.TotalSeconds <= new TimeSpan(3, 0, 0).TotalSeconds)
            {
                return date.AddDays(-1);
            }

            return date;
        }

        /// <summary>
        /// Uses ConvertToNSITime
        /// </summary>
        /// <param name="date"></param>
        /// <param name="airTime"></param>
        /// <returns></returns>
        public DateTime ConvertToNTITime(DateTime date, TimeSpan airTime)
        {
            return ConvertToNSITime(date, airTime);
        }
    }
}
