using System.Collections;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class QuarterDto
    {
        public QuarterDto()
        {
        }

        public QuarterDto(int quarter, int year)
        {
            Quarter = quarter;
            Year = year;
        }

        public int Quarter { get; set; }
        public int Year { get; set; }
    }

    public class QuarterDtoComparer : IEqualityComparer<QuarterDto>
    {
        public bool Equals(QuarterDto x, QuarterDto y)
        {
            return x.Year == y.Year && x.Quarter == y.Quarter;
        }

        public int GetHashCode(QuarterDto obj)
        {
            return obj.GetHashCode();
        }
    }
}
