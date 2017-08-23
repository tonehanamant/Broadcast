using System;
using System.Runtime.Serialization;
using System.Text;

namespace Tam.Maestro.Services.ContractInterfaces.Common
{
    [DataContract]
    [Serializable]
    public class MaestroServiceRuntime
    {
        [DataMember]
        public TAMService Service;
        [DataMember]
        public string AssemblyVersion;
        [DataMember]
        public string MachineName;
        [DataMember]
        private long _ProcessTotalProcessorTimeTicks;
        [DataMember]
        public int ProcessThreadCount;
        [DataMember]
        public long ProcessNonpagedSystemMemorySize;
        [DataMember]
        public long ProcessPagedMemorySize;
        [DataMember]
        public long ProcessPagedSystemMemorySize;
        [DataMember]
        public long ProcessPeakPagedMemorySize;
        [DataMember]
        public long ProcessPeakVirtualMemorySize;
        [DataMember]
        public long ProcessPeakWorkingSet;
        [DataMember]
        public long ProcessPrivateMemorySize64;
        [DataMember]
        public long ProcessVirtualMemorySize;
        [DataMember]
        public long ProcessWorkingSet;
        [DataMember]
        public DateTime ProcessStartTime;

        public TimeSpan ProcessTotalProcessorTime
        {
            get { return (new TimeSpan(this._ProcessTotalProcessorTimeTicks)); }
            set { this._ProcessTotalProcessorTimeTicks = value.Ticks; }
        }
        public TimeSpan ProcessRunningTime
        {
            get { return ((TimeSpan)DateTime.Now.Subtract(this.ProcessStartTime)); }
        }

        public MaestroServiceRuntime(TAMService pType, string pAssemblyVersion, System.Diagnostics.Process pProcess)
        {
            this.Service = pType;
            this.AssemblyVersion = pAssemblyVersion;
            this.MachineName = Environment.MachineName;
            this._ProcessTotalProcessorTimeTicks = pProcess.TotalProcessorTime.Ticks;
            this.ProcessNonpagedSystemMemorySize = pProcess.NonpagedSystemMemorySize64;
            this.ProcessPagedMemorySize = pProcess.PagedMemorySize64;
            this.ProcessPagedSystemMemorySize = pProcess.PagedSystemMemorySize64;
            this.ProcessPeakPagedMemorySize = pProcess.PeakPagedMemorySize64;
            this.ProcessPeakVirtualMemorySize = pProcess.PeakVirtualMemorySize64;
            this.ProcessPeakWorkingSet = pProcess.PeakWorkingSet64;
            this.ProcessPrivateMemorySize64 = pProcess.PrivateMemorySize64;
            this.ProcessStartTime = pProcess.StartTime;
            this.ProcessThreadCount = pProcess.Threads.Count;
            this.ProcessVirtualMemorySize = pProcess.VirtualMemorySize64;
            this.ProcessWorkingSet = pProcess.WorkingSet64;
        }

        public static string FormatMB(long pValue)
        {
            string lReturn = "";
            lReturn = ((double)pValue / (1024D * 1024D)).ToString("N0") + "MB";
            return (lReturn);
        }

        public override string ToString()
        {
            StringBuilder lReturn = new StringBuilder();
            lReturn.Append(this.Service.ToString().Replace("_", " "));
            return (lReturn.ToString());
        }
    }
}
