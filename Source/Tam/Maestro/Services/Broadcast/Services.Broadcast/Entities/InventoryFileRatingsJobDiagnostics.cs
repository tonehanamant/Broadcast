using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Services.Broadcast.Entities
{
    public class QuarterPostingBook
    {
        public string QuarterText { get; set; }
        public int PostingBookId { get; set; }
    }

    public class InventoryFileRatingsJobDiagnostics
    {
        public int JobId { get; set; }
        public int FileId { get; set; }
        public InventorySource InventorySource { get; set; }

        public DateTime StartedDateTime { get; set; }
        public DateTime CompletedDateTime { get; set; }

        public int ManifestCount { get; set; }
        public int AudienceCount { get; set; }
        public int WeekCount { get; set; }
        public int DaypartCount { get; set; }
        public int StationCount { get; set; }

        public bool IsChunking { get; set; }
        public int SaveChunkSize { get; set; }
        public int SaveChunkCount { get; set; }

        public List<string> QuartersCovered { get; set; } = new List<string>();
        public int QuartersCoveredCount => QuartersCovered.Count;

        public List<QuarterPostingBook> PostingBooksPerQuarter { get; } = new List<QuarterPostingBook>();

        public Dictionary<string, Stopwatch> StopWatchDict { get; } = new Dictionary<string, Stopwatch>();

        private const string SW_KEY_TOTAL_DURATION = "TotalDuration";
        private const string SW_KEY_GATHER_INVENTORY = "GatherInventory";
        private const string SW_KEY_ORGANIZE_INVENTORY = "OrganizeInventory";
        private const string SW_KEY_PROCESS_IMPRESSIONS_TOTAL = "ProcessImpressionsTotal";
        private const string SW_KEY_DETERMINE_POSTING_BOOK = "DeterminePostingBook";
        private const string SW_KEY_ADD_IMPRESSIONS = "AddImpressions";
        private const string SW_KEY_SAVE_MANIFESTS = "SaveManifests";

        public void RecordJobStart(int jobId, int fileId, InventorySource inventorySource, DateTime startDateTime)
        {
            ReportHeaderFooter();
            Report($"Starting JobId {jobId} at {startDateTime}");
            Report($"FileID = {fileId}");
            Report($"InventorySource = {inventorySource.Name} ({inventorySource.Id} : {inventorySource.InventoryType})");

            _StartTimer(SW_KEY_TOTAL_DURATION);

            JobId = jobId;
            FileId = fileId;
            InventorySource = inventorySource;
            StartedDateTime = startDateTime;
        }

        public void RecordJobCompleted(DateTime completedDateTime)
        {
            _StopTimer(SW_KEY_TOTAL_DURATION);
            CompletedDateTime = completedDateTime;

            Report("Process completed");
            Report("");
            Report(ToString());
            ReportHeaderFooter();
        }

        public void RecordGatherInventoryStart()
        {
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
            _StartTimer(SW_KEY_GATHER_INVENTORY);
        }

        public void RecordGatherInventoryStop()
        {
            _StopTimer(SW_KEY_GATHER_INVENTORY);
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void RecordOrganizeInventoryStart()
        {
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
            _StartTimer(SW_KEY_ORGANIZE_INVENTORY);
        }

        public void RecordOrganizeInventoryStop()
        {
            _StopTimer(SW_KEY_ORGANIZE_INVENTORY);
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void RecordProcessImpressionsTotalStart()
        {
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
            _StartTimer(SW_KEY_PROCESS_IMPRESSIONS_TOTAL);
        }

        public void RecordProcessImpressionsTotalStop()
        {
            _StopTimer(SW_KEY_PROCESS_IMPRESSIONS_TOTAL);
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void RecordDeterminePostingBookStart()
        {
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
            _RestartTimer(SW_KEY_DETERMINE_POSTING_BOOK);
        }

        public void RecordDeterminePostingBookStop(string quarterText, int postingBookId)
        {
            PostingBooksPerQuarter.Add(new QuarterPostingBook {QuarterText = quarterText, PostingBookId = postingBookId});
            _StopTimer(SW_KEY_DETERMINE_POSTING_BOOK);
            Report($"{System.Reflection.MethodBase.GetCurrentMethod().Name} : {quarterText} : {postingBookId}");
        }

        public void RecordAddImpressionsStart()
        {
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
            _RestartTimer(SW_KEY_ADD_IMPRESSIONS);
        }

        public void RecordAddImpressionsStop()
        {
            _StopTimer(SW_KEY_ADD_IMPRESSIONS);
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void RecordSaveManifestsStart()
        {
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
            _StartTimer(SW_KEY_SAVE_MANIFESTS);
        }

        public void RecordSaveManifestsStop()
        {
            _StopTimer(SW_KEY_SAVE_MANIFESTS);
            Report(System.Reflection.MethodBase.GetCurrentMethod().Name);
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

        public override string ToString()
        {
            var inventorySourceName = InventorySource == null ? "unknown" : InventorySource.Name;
            var inventorySourceType = InventorySource == null ? "unknown" : InventorySource.InventoryType.ToString();

            var sb = new StringBuilder();
            sb.AppendLine($"JobId : {JobId}");
            sb.AppendLine($"FileId : {FileId}");
            sb.AppendLine($"InventorySource : {inventorySourceName}");
            sb.AppendLine($"InventorySourceType : {inventorySourceType}");
            sb.AppendLine($"StartedDateTime : {StartedDateTime}");
            sb.AppendLine($"CompletedDateTime : {CompletedDateTime}");
            sb.AppendLine();
            sb.AppendLine($"QuartersCoveredCount : {QuartersCoveredCount}");
            sb.AppendLine($"QuartersCovered : {string.Join(",", QuartersCovered)}");
            sb.AppendLine($"PostingBooksPerQuarter : {string.Join(",", PostingBooksPerQuarter.Select(s => $"{s.QuarterText} : {s.PostingBookId}"))}");
            sb.AppendLine();
            sb.AppendLine($"IsChunking : {IsChunking}");
            sb.AppendLine($"SaveChunkSize : {SaveChunkSize}");
            sb.AppendLine();
            sb.AppendLine($"Manifest Count : {ManifestCount}");
            sb.AppendLine($"Audience Count : {AudienceCount}");
            sb.AppendLine($"Week Count : {WeekCount}");
            sb.AppendLine($"Daypart Count : {DaypartCount}");
            sb.AppendLine($"Station Count : {StationCount}");
            sb.AppendLine($"Station Count : {StationCount}");
            sb.AppendLine($"Save Chunk Count : {SaveChunkCount}");
            
            sb.AppendLine();
            sb.AppendLine("Durations:");
            sb.AppendLine(GetDurationString(SW_KEY_TOTAL_DURATION));
            sb.AppendLine(GetDurationString(SW_KEY_GATHER_INVENTORY));
            sb.AppendLine(GetDurationString(SW_KEY_ORGANIZE_INVENTORY));
            sb.AppendLine(GetDurationString(SW_KEY_PROCESS_IMPRESSIONS_TOTAL));
            sb.AppendLine(GetDurationString(SW_KEY_DETERMINE_POSTING_BOOK));
            sb.AppendLine(GetDurationString(SW_KEY_ADD_IMPRESSIONS));
            sb.AppendLine(GetDurationString(SW_KEY_SAVE_MANIFESTS));

            return sb.ToString();
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

        private void ReportHeaderFooter()
        {
            Report("**************************************");
        }

        private void Report(string message)
        {
            Debug.WriteLine($"{DateTime.Now} : {message}");
        }
    }
}