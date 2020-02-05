using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Services.Broadcast.Entities
{
    public abstract class InventoryProgramsProcessingJobDiagnostics
    {
        public delegate void OnMessageUpdatedDelegate(int jobId, string message);

        private readonly OnMessageUpdatedDelegate _OnMessageUpdated;

        public InventoryProgramsProcessingJobDiagnostics(OnMessageUpdatedDelegate onMessageUpdated)
        {
            _OnMessageUpdated = onMessageUpdated;
        }

        public int JobId { get; set; }

        public int RequestChunkSize { get; set; }
        public int SaveChunkSize { get; set; }

        public InventorySource InventorySource { get; set; }

        public int TotalManifestCount { get; set; }
        public int TotalWeekCount { get; set; }
        public int TotalDaypartCount { get; set; }
        public int TotalRequestCount { get; set; }
        public int TotalApiCallCount { get; set; }
        public int TotalResponseCount { get; set; }
        public int TotalProgramsSaved { get; set; }
        public int TotalSavesCount { get; set; }

        private const string SW_KEY_TOTAL_DURATION = "TotalDuration";
        private const string SW_KEY_TOTAL_DURATION_GATHER_INVENTORY = "TotalDurationGatherInventory";
        private const string SW_KEY_TOTAL_DURATION_TRANSFORM_TO_INPUT = "TotalDurationTransformToInput";
        private const string SW_KEY_TOTAL_DURATION_CALL_TO_API = "TotalDurationCallToApi";
        private const string SW_KEY_TOTAL_DURATION_APPLY_API_RESPONSE = "TotalDurationApplyApiResponse";
        private const string SW_KEY_TOTAL_DURATION_SAVE_PROGRAMS = "TotalDurationSavePrograms";

        private const string SW_KEY_ITERATION_TRANSFORM_TO_INPUT = "IterationTransformToInput";
        private const string SW_KEY_ITERATION_CALL_TO_API = "IterationCallToApi";
        private const string SW_KEY_ITERATION_APPLY_API_RESPONSE = "IterationApplyApiResponse";
        private const string SW_KEY_ITERATION_SAVE_PROGRAMS = "IterationSavePrograms";

        public Dictionary<string, Stopwatch> StopWatchDict { get; } = new Dictionary<string, Stopwatch>();

        public void RecordStart()
        {
            _ReportToConsole("**************************************");
            _ReportToConsole($"Starting jobId {JobId}.");
            _ReportToConsoleAndJobNotes($"Config values : RequestChunkSize = {RequestChunkSize}");
            _ReportToConsoleAndJobNotes($"Config values : SaveChunkSize = {SaveChunkSize}");

            _StartTimer(SW_KEY_TOTAL_DURATION);
        }

        public void RecordInventorySource(InventorySource inventorySource)
        {
            InventorySource = inventorySource;

            _ReportToConsoleAndJobNotes($"InventorySource : {InventorySource.Name}");
            _ReportToConsoleAndJobNotes($"InventorySourceType : {InventorySource.InventoryType}");
        }

        public void RecordGatherInventoryStart()
        {
            _StartTimer(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY);
        }

        public void RecordGatherInventoryStop()
        {
            _StopTimer(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY);
            _ReportToConsoleAndJobNotes(GetDurationString(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY));
        }

        public void RecordManifestDetails(int manifestCount, int weekCount, int daypartCount)
        {
            TotalManifestCount += manifestCount;
            TotalWeekCount += weekCount;
            TotalDaypartCount += daypartCount;

            _ReportToConsoleAndJobNotes($"Adding Iteration counts : Inventory manifest count {manifestCount} translated to {weekCount} distinct weeks for {daypartCount} dayparts.");
        }

        public void RecordIterationStart(int iterationNumber, int iterationTotalNumber)
        {
            _ReportToConsoleAndJobNotes($"Beginning manifest processing iteration {iterationNumber} of {iterationTotalNumber}.");
        }
        
        public void RecordTransformToInputStart()
        {
            _StartTimer(SW_KEY_TOTAL_DURATION_TRANSFORM_TO_INPUT);
            _RestartTimer(SW_KEY_ITERATION_TRANSFORM_TO_INPUT);
        }

        public void RecordTransformToInputStop(int requestElementCount)
        {
            TotalRequestCount += requestElementCount;

            _StopTimer(SW_KEY_TOTAL_DURATION_TRANSFORM_TO_INPUT);
            _StopTimer(SW_KEY_ITERATION_TRANSFORM_TO_INPUT);
            _ReportToConsoleAndJobNotes(GetDurationString(SW_KEY_ITERATION_TRANSFORM_TO_INPUT));
            _ReportToConsoleAndJobNotes($"Transformed to {requestElementCount} requestElements.");
        }

        public void RecordIterationStartCallToApi(int iterationNumber, int iterationTotalNumber)
        {
            TotalApiCallCount++;
            _ReportToConsoleAndJobNotes($"Beginning call to api iteration {iterationNumber} of {iterationTotalNumber}.");
            _StartTimer(SW_KEY_TOTAL_DURATION_CALL_TO_API);
            _RestartTimer(SW_KEY_ITERATION_CALL_TO_API);
        }

        public void RecordIterationStopCallToApi(int responseCount)
        {
            _ReportToConsoleAndJobNotes($"Response contained {responseCount} response elements.");
            TotalResponseCount += responseCount;

            _StopTimer(SW_KEY_TOTAL_DURATION_CALL_TO_API);
            _StopTimer(SW_KEY_ITERATION_CALL_TO_API);

            _ReportToConsoleAndJobNotes(GetDurationString(SW_KEY_ITERATION_CALL_TO_API));
        }

        public void RecordIterationStartApplyApiResponse()
        {
            _StartTimer(SW_KEY_TOTAL_DURATION_APPLY_API_RESPONSE);
            _RestartTimer(SW_KEY_ITERATION_APPLY_API_RESPONSE);
        }

        public void RecordIterationStopApplyApiResponse(int programCount)
        {
            TotalProgramsSaved += programCount;

            _StopTimer(SW_KEY_TOTAL_DURATION_APPLY_API_RESPONSE);
            _StopTimer(SW_KEY_ITERATION_APPLY_API_RESPONSE);

            _ReportToConsoleAndJobNotes($"Resulted in {programCount} Programs to save.");
            _ReportToConsoleAndJobNotes(GetDurationString(SW_KEY_ITERATION_APPLY_API_RESPONSE));
        }

        public void RecordIterationStartSavePrograms()
        {
            _StartTimer(SW_KEY_TOTAL_DURATION_SAVE_PROGRAMS);
            _RestartTimer(SW_KEY_ITERATION_SAVE_PROGRAMS);
        }

        public void RecordIterationStopSavePrograms(int saveChunkCount)
        {
            TotalSavesCount += saveChunkCount;

            _StopTimer(SW_KEY_TOTAL_DURATION_SAVE_PROGRAMS);
            _StopTimer(SW_KEY_ITERATION_SAVE_PROGRAMS);
            _ReportToConsoleAndJobNotes($"Resulted in {saveChunkCount} programs save chunks.");
            _ReportToConsoleAndJobNotes(GetDurationString(SW_KEY_ITERATION_SAVE_PROGRAMS));
        }

        public void RecordStop()
        {
            _StopTimer(SW_KEY_TOTAL_DURATION);
            _ReportToConsoleAndJobNotes("Process completed.");
            _ReportToConsole("");
            _ReportToConsole(ToString());
            _ReportToConsole("**************************************");
        }

        protected abstract string OnToString();

        public override string ToString()
        {
            var inventorySourceName = InventorySource == null ? "unknown" : InventorySource.Name;
            var inventorySourceType = InventorySource == null ? "unknown" : InventorySource.InventoryType.ToString();

            var sb = new StringBuilder();
            sb.AppendLine($"JobId : {JobId}");
            sb.AppendLine();
            sb.AppendLine(OnToString());
            sb.AppendLine();
            sb.AppendLine($"InventorySource : {inventorySourceName}");
            sb.AppendLine($"InventorySourceType : {inventorySourceType}");
            sb.AppendLine();
            sb.AppendLine($"RequestChunkSize : {RequestChunkSize}");
            sb.AppendLine($"SaveChunkSize : {SaveChunkSize}");
            sb.AppendLine();
            sb.AppendLine($"Total Manifest Count : {TotalManifestCount}");
            sb.AppendLine($"Total Week Count : {TotalWeekCount}");
            sb.AppendLine($"Total Daypart Count : {TotalDaypartCount}");
            sb.AppendLine($"Total Request Count : {TotalRequestCount}");
            sb.AppendLine($"Total ApiCall Count : {TotalApiCallCount}");
            sb.AppendLine($"Total Response Count : {TotalResponseCount}");
            sb.AppendLine($"Total Programs Count : {TotalProgramsSaved}");
            sb.AppendLine($"Total SaveCalls Count : {TotalSavesCount}");
            sb.AppendLine();
            sb.AppendLine(GetDurationString(SW_KEY_TOTAL_DURATION));
            sb.AppendLine(GetDurationString(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY));
            sb.AppendLine(GetDurationString(SW_KEY_TOTAL_DURATION_TRANSFORM_TO_INPUT));
            sb.AppendLine(GetDurationString(SW_KEY_TOTAL_DURATION_CALL_TO_API));
            sb.AppendLine(GetDurationString(SW_KEY_TOTAL_DURATION_APPLY_API_RESPONSE));
            sb.AppendLine(GetDurationString(SW_KEY_TOTAL_DURATION_SAVE_PROGRAMS));

            return sb.ToString();
        }

        private void _StartTimer(string timerKey)
        {
            if (StopWatchDict.ContainsKey(timerKey) == false)
            {
                StopWatchDict[timerKey] = new Stopwatch();
            }

            StopWatchDict[timerKey].Start();
        }

        private void _RestartTimer(string timerKey)
        {
            if (StopWatchDict.ContainsKey(timerKey) == false)
            {
                StopWatchDict[timerKey] = new Stopwatch();
            }

            StopWatchDict[timerKey].Restart();
        }

        private void _StopTimer(string timerKey)
        {
            if (StopWatchDict.ContainsKey(timerKey) == false)
            {
                StopWatchDict[timerKey] = new Stopwatch();
            }
            StopWatchDict[timerKey].Stop();
        }

        private string GetDurationString(string key)
        {
            if (StopWatchDict.ContainsKey(key) == false)
            {
                StopWatchDict[key] = new Stopwatch();
            }
            var sw = StopWatchDict[key];
            return $"{key} = {sw.Elapsed.Hours}:{sw.Elapsed.Minutes}:{sw.Elapsed.Seconds}.{sw.Elapsed.Milliseconds}";
        }

        protected void _ReportToConsole(string message)
        {
            Debug.WriteLine(message);
        }

        protected void _ReportToConsoleAndJobNotes(string message)
        {
            _ReportToConsole(message);
            _OnMessageUpdated?.Invoke(JobId, message);
        }
    }
}