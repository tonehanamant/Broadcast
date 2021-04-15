using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// Time the steps in a process for reporting.
    /// </summary>
    public class ProcessWorkflowTimers
    {
        private ConcurrentDictionary<string, Stopwatch> StopWatchDict { get; } = new ConcurrentDictionary<string, Stopwatch>();
        private StringBuilder DiagnosticMessage { get; set; } = new StringBuilder();

        public void Start(string key)
        {
            _StartTimer(key);
        }

        public void End(string key)
        {
            _StopTimer(key);
            DiagnosticMessage.AppendLine(_GetDurationString(key));
        }

        private void _StartTimer(string timerKey)
        {
            if (StopWatchDict.ContainsKey(timerKey) == false)
            {
                StopWatchDict[timerKey] = new Stopwatch();
            }

            StopWatchDict[timerKey].Start();
        }

        private void _StopTimer(string timerKey)
        {
            if (StopWatchDict.ContainsKey(timerKey) == false)
            {
                StopWatchDict[timerKey] = new Stopwatch();
            }

            StopWatchDict[timerKey].Stop();
        }

        private string _GetDurationString(string key)
        {
            if (StopWatchDict.ContainsKey(key) == false)
            {
                StopWatchDict[key] = new Stopwatch();
            }

            var sw = StopWatchDict[key];

            return $"{key} = {sw.Elapsed.Hours}h {sw.Elapsed.Minutes}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms";
        }

        public override string ToString()
        {
            return DiagnosticMessage.ToString();
        }
    }
}