using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tam.Maestro.Data.Entities;

namespace Tam.Maestro.Services.ContractInterfaces.Common
{
    [DataContract]
    [Serializable]
    public class DisplayMediaMonth
    {
        [DataMember]
        public MediaMonth MediaMonth;
        [DataMember]
        public int WeeksInMonth;
        [DataMember]
        public int ActiveWeeksInMonth;
        [DataMember]
        private int[] _MediaWeekIds;

        public List<int> MediaWeekIds
        {
            get
            {
                List<int> lReturn = new List<int>();
                lReturn.AddRange(this._MediaWeekIds);
                return (lReturn);
            }
            set
            {
                this._MediaWeekIds = value.ToArray();
            }
        }

        public DisplayMediaMonth(MediaMonth pMediaMonth, int pWeeksInMonth)
        {
            this.MediaMonth = pMediaMonth;
            this.WeeksInMonth = pWeeksInMonth;
            this.ActiveWeeksInMonth = pWeeksInMonth;
        }
        public DisplayMediaMonth(MediaMonth pMediaMonth, int pWeeksInMonth, int pActiveWeeksInMonth)
        {
            this.MediaMonth = pMediaMonth;
            this.WeeksInMonth = pWeeksInMonth;
            this.ActiveWeeksInMonth = pActiveWeeksInMonth;
        }

        public override string ToString()
        {
            return this.MediaMonth.MediaMonthX;
        }
    }
}
