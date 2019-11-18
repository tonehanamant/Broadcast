using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Services.Broadcast.Entities
{
    public class InventoryFileProgramEnrichmentJobDiagnostics
    {
        public int JobId { get; set; }
        public int FileId { get; set; }
        public InventorySource InventorySource { get; set; }

        public int RequestChunkSize { get; set; }
        public int SaveChunkSize { get; set; }
        
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
            Report("**************************************");
            Report($"Starting jobId {JobId}.");
            Report($"Config values : RequestChunkSize = {RequestChunkSize}");
            Report($"Config values : SaveChunkSize = {SaveChunkSize}");

            _StartTimer(SW_KEY_TOTAL_DURATION);
        }

        public void RecordFileInfo(int fileId, InventorySource inventorySource)
        {
            FileId = fileId;
            InventorySource = inventorySource;
            Report($"JobId {JobId} processes fileId {FileId}.");
        }

        public void RecordGatherInventoryStart()
        {
            _StartTimer(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY);
        }

        public void RecordGatherInventoryStop()
        {
            _StopTimer(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY);
            Report(GetDurationString(SW_KEY_TOTAL_DURATION_GATHER_INVENTORY));
        }

        public void RecordManifestDetails(int manifestCount, int weekCount, int daypartCount)
        {
            TotalManifestCount = manifestCount;
            TotalWeekCount = weekCount;
            TotalDaypartCount = daypartCount;

            Report($"Inventory manifest count {TotalManifestCount} translated to {TotalWeekCount} distinct weeks for {TotalDaypartCount} dayparts.");
        }

        public void RecordIterationStart(int iterationNumber, int iterationTotalNumber)
        {
            Report($"Beginning manifest processing iteration {iterationNumber} of {iterationTotalNumber}.");
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
            Report(GetDurationString(SW_KEY_ITERATION_TRANSFORM_TO_INPUT));
            Report($"Transformed to {requestElementCount} requestElements.");
        }

        public void RecordIterationStartCallToApi(int iterationNumber, int iterationTotalNumber)
        {
            TotalApiCallCount++;
            Report($"Beginning call to api iteration {iterationNumber} of {iterationTotalNumber}.");
            _StartTimer(SW_KEY_TOTAL_DURATION_CALL_TO_API);
            _RestartTimer(SW_KEY_ITERATION_CALL_TO_API);
        }

        public void RecordIterationStopCallToApi(int responseCount)
        {
            Report($"Response contained {responseCount} response elements.");
            TotalResponseCount += responseCount;

            _StopTimer(SW_KEY_TOTAL_DURATION_CALL_TO_API);
            _StopTimer(SW_KEY_ITERATION_CALL_TO_API);

            Report(GetDurationString(SW_KEY_ITERATION_CALL_TO_API));
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

            Report($"Resulted in {programCount} Programs to save.");
            Report(GetDurationString(SW_KEY_ITERATION_APPLY_API_RESPONSE));
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
            Report($"Resulted in {saveChunkCount} programs save chunks.");
            Report(GetDurationString(SW_KEY_ITERATION_SAVE_PROGRAMS));
        }

        public void RecordStop()
        {
            _StopTimer(SW_KEY_TOTAL_DURATION);
            Report("Process completed.");
            Report("");
            Report(ToString());
            Report("**************************************");
        }

        public override string ToString()
        {
            var inventorySourceName = InventorySource == null ? "unknown" : InventorySource.Name;
            var inventorySourceType = InventorySource == null ? "unknown" : InventorySource.InventoryType.ToString();

            var sb = new StringBuilder();
            sb.AppendLine($"JobId : {JobId}");
            sb.AppendLine($"FileId : {FileId}");
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

        private void Report(string message)
        {
            Debug.WriteLine(message);
        }
    }
}