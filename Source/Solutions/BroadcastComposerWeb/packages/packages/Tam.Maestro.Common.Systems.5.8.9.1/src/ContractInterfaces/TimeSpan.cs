using System;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Tam.Maestro.Services.ContractInterfaces.InventoryBusinessObjects
{
    [Serializable]
    public struct TimeSpan : IEquatable<TimeSpan>
    {
        public int StartTime;
        public int EndTime;

        public int StartHour { get { return StartTime / 3600; } }
        public int EndHour
        {
            get
            {
                var roundedEndTime = EndTime % 60 != 0 ? EndTime + 1 : EndTime;
                return roundedEndTime / 3600;
            }
        }

        public TimeSpan(DisplayDaypart daypart)
        {
            StartTime = daypart.StartTime;
            EndTime = daypart.EndTime;
        }

        public TimeSpan(int pStartTime, int pEndTime)
        {
            StartTime = pStartTime;
            EndTime = pEndTime;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(TimeSpan other)
        {
            return StartTime == other.StartTime && EndTime == other.EndTime;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StartTime * 397) ^ EndTime;
            }
        }

        public override string ToString()
        {
            return StartTime + "-" + EndTime;
        }
    }
}