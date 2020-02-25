using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Services.Broadcast.Entities.Plan.Pricing
{
    public class PlanPricingJobDiagnostic
    {
        public int JobId { get; set; }
        private Dictionary<string, Stopwatch> StopWatchDict { get; } = new Dictionary<string, Stopwatch>();
        private StringBuilder DiagnosticMessage { get; set; } = new StringBuilder();
        private const string SW_KEY_TOTAL_DURATION = "TotalDuration";
        private const string SW_KEY_TOTAL_DURATION_GATHER_INVENTORY = "TotalDurationGatherInventory";
        private const string SW_KEY_TOTAL_DURATION_CALL_TO_API = "TotalDurationCallToApi";
        private const string SW_KEY_TOTAL_DURATION_INVENTORY_SOURCE_ESTIMATES_CALCULATION = "TotalDurationInventorySourceEstimatesCalculation";

        public void RecordStart()
        {
            DiagnosticMessage.AppendLine($"Starting job with id {JobId}.");
            _StartTimer(SW_KEY_TOTAL_DURATION);
        }

        public void RecordEnd()
        {
            _StopTimer(SW_KEY_TOTAL_DURATION);
            DiagnosticMessage.AppendLine(_GetDurationString(SW_KEY_TOTAL_DURATION));
        }

        public void RecordInventorySourceEstimatesCalculationStart()
        {
            _StartTimer(SW_KEY_TOTAL_DURATION_INVENTORY_SOURCE_ESTIMATES_CALCULATION);
        }

        public void RecordInventorySourceEstimatesCalculationEnd()
        {
            _StopTimer(SW_KEY_TOTAL_DURATION_INVENTORY_SOURCE_ESTIMATES_CALCULATION);
            DiagnosticMessage.AppendLine(_GetDurationString(SW_KEY_TOTAL_DURATION_INVENTORY_SOURCE_ESTIMATES_CALCULATION));
        }

        public void RecordGatherInventoryStart()
        {
            _StartTimer(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY);
        }

        public void RecordGatherInventoryEnd()
        {
            _StopTimer(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY);
            DiagnosticMessage.AppendLine(_GetDurationString(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY));
        }

        public void RecordApiCallStart()
        {
            _StartTimer(SW_KEY_TOTAL_DURATION_CALL_TO_API);
        }

        public void RecordApiCallEnd()
        {
            _StopTimer(SW_KEY_TOTAL_DURATION_CALL_TO_API);
            DiagnosticMessage.AppendLine(_GetDurationString(SW_KEY_TOTAL_DURATION_CALL_TO_API));
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
            return $"{key} = {sw.Elapsed.Hours}:{sw.Elapsed.Minutes}:{sw.Elapsed.Seconds}.{sw.Elapsed.Milliseconds}";
        }

        public override string ToString()
        {
            return DiagnosticMessage.ToString();
        }
    }
}
