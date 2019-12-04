using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public List<string> QuartersCovered { get; set; } = new List<string>();
        public int QuartersCoveredCount => QuartersCovered.Count;

        public List<QuarterPostingBook> PostingBooksPerQuarter { get; } = new List<QuarterPostingBook>();

        public Dictionary<string, Stopwatch> StopWatchDict { get; } = new Dictionary<string, Stopwatch>();

        private const string SW_KEY_TOTAL_DURATION = "TotalDuration";
        private const string SW_KEY_GATHER_INVENTORY = "GatherInventory";
        private const string SW_KEY_ORGANIZE_INVENTORY = "OrganizeInventory";
        private const string SW_KEY_PROCESS_IMPRESSIONS_TOTAL = "ProcessImpressionsTotal";
        private const string SW_KEY_DETERMINE_POSTING_BOOK = "DeterminePostingBook";
        private const string SW_KEY_ADD_IMPORESSIONS = "AddImpressions";
        private const string SW_KEY_SAVE_MANIFESTS = "SaveManifests";

        public void RecordJobStart(int jobId, int fileId, InventorySource inventorySource, DateTime startDateTime)
        {
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
        }

        public void RecordGatherInventoryStart()
        {
            _StartTimer(SW_KEY_GATHER_INVENTORY);
        }

        public void RecordGatherInventoryStop()
        {
            _StopTimer(SW_KEY_GATHER_INVENTORY);
        }

        public void RecordOrganizeInventoryStart()
        {
            _StartTimer(SW_KEY_ORGANIZE_INVENTORY);
        }

        public void RecordOrganizeInventoryStop()
        {
            _StopTimer(SW_KEY_ORGANIZE_INVENTORY);
        }

        public void RecordProcessImpressionsTotalStart()
        {
            _StartTimer(SW_KEY_PROCESS_IMPRESSIONS_TOTAL);
        }

        public void RecordProcessImpressionsTotalStop()
        {
            _StopTimer(SW_KEY_PROCESS_IMPRESSIONS_TOTAL);
        }

        public void RecordDeterminePostingBookStart()
        {
            _RestartTimer(SW_KEY_DETERMINE_POSTING_BOOK);
        }

        public void RecordDeterminePostingBookStop(string quarterText, int postingBookId)
        {
            PostingBooksPerQuarter.Add(new QuarterPostingBook {QuarterText = quarterText, PostingBookId = postingBookId});
            _StopTimer(SW_KEY_DETERMINE_POSTING_BOOK);
        }

        public void RecordAddImpressionsStart()
        {
            _RestartTimer(SW_KEY_ADD_IMPORESSIONS);
        }

        public void RecordAddImpressionsStop()
        {
            _StopTimer(SW_KEY_ADD_IMPORESSIONS);
        }

        public void RecordSaveManifestsStart()
        {
            _StartTimer(SW_KEY_SAVE_MANIFESTS);
        }

        public void RecordSaveManifestsStop()
        {
            _StopTimer(SW_KEY_SAVE_MANIFESTS);
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