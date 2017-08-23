using System;
using System.Runtime.Serialization;


namespace Tam.Maestro.Services.ContractInterfaces.Common
{
    [DataContract]
    [Serializable]
    public class Flight
    {
        [DataMember]
        public DateTime? StartDate;
        [DataMember]
        public DateTime? EndDate;

        public Flight()
        {
            this.StartDate = null;
            this.EndDate = null;
        }
        public Flight(DateTime? pStartDate, DateTime? pEndDate)
        {
            this.StartDate = pStartDate;
            this.EndDate = pEndDate;
        }

        public override string ToString()
        {
            if (this.StartDate != null && this.EndDate != null)
                return (this.StartDate.Value.ToString("MM/dd/yyyy") + " - " + this.EndDate.Value.ToString("MM/dd/yyyy"));
            else
                return ("");
        }
        public string ToAbbreviatedString()
        {
            if (this.StartDate != null && this.EndDate != null)
                return (this.StartDate.Value.ToString("MM/dd") + " - " + this.EndDate.Value.ToString("MM/dd"));
            else
                return ("");
        }

        public override bool Equals(object obj)
        {
            Flight lFlight = (Flight)obj;
            return (this.StartDate == lFlight.StartDate && this.EndDate == lFlight.EndDate);
        }
        public override int GetHashCode()
        {
            string lText = "";
            if (this.StartDate.HasValue)
                lText = this.StartDate.Value.ToShortDateString();
            if (this.EndDate.HasValue)
                lText += (this.StartDate.HasValue ? "_" : "") + this.EndDate.Value.ToShortDateString();
            return lText.GetHashCode();
        }
        public void Dispose()
        {
            this.StartDate = null;
            this.EndDate = null;
        }
    }
}
