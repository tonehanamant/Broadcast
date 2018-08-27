using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Extensions
{
    public static class AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsExtensions
    {
        public static List<AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod> GetAllPeriods(this AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriods periods)
        {
            var result = new List<AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod>();

            if (periods != null)
            {
                if (periods.DetailedPeriod != null && periods.DetailedPeriod.Count() > 0)
                {
                    result.AddRange(periods.DetailedPeriod);
                }

                if (periods.DayDetailedPeriod != null && periods.DayDetailedPeriod.Count() > 0)
                {
                    var dayDetailedPeriods = periods.DayDetailedPeriod.Select(x => new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod
                    {
                        Rate = x.Rate,
                        endDate = x.endDate,
                        startDate = x.startDate,
                        DemoValues = x.DemoValues.Select(d => new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue
                        {
                            demoRef = d.demoRef,
                            Value = d.Value
                        }).ToArray()
                    });

                    result.AddRange(dayDetailedPeriods);
                }
            }
            
            return result;
        }
    }
}