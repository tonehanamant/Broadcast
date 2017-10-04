using Common.Services;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public abstract class InventoryFileImporterBase
    {
        private readonly string _validTimeExpression =
   @"(^([0-9]|[0-1][0-9]|[2][0-3])(:|\-)([0-5][0-9])(\s{0,1})(AM|PM|am|pm|aM|Am|pM|Pm{2,2})$)|(^([0-9]|[1][0-9]|[2][0-3])(\s{0,1})(AM|PM|am|pm|aM|Am|pM|Pm{2,2})$)";

        protected BroadcastDataDataRepositoryFactory _BroadcastDataRepositoryFactory;
        protected IDaypartCache _DaypartCache;
        protected IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        protected IBroadcastAudiencesCache _AudiencesCache;
        protected Dictionary<int, double> _SpotLengthMultipliers;

        private string _fileHash;

        public InventoryFileSaveRequest Request { get; private set; }

        public BroadcastDataDataRepositoryFactory BroadcastDataDataRepository
        {
            set { _BroadcastDataRepositoryFactory = value; }
        }

        public IDaypartCache DaypartCache
        {
            set { _DaypartCache = value; }
        }

        public IMediaMonthAndWeekAggregateCache MediaMonthAndWeekAggregateCache
        {
            set { _MediaMonthAndWeekAggregateCache = value; }
        }

        public IBroadcastAudiencesCache AudiencesCache
        {
            set { _AudiencesCache = value; }
        }

        public void CheckFileHash()
        {

            //check if file has already been loaded
            if (_BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>().GetInventoryFileIdByHash(_fileHash) > 0)
            {
                throw new BroadcastDuplicateInventoryFileException("Unable to load rate file. The selected rate file has already been loaded or is already loading.");
            }
        }

        public void LoadFromSaveRequest(InventoryFileSaveRequest request)
        {
            Request = request;
            _fileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(request.RatesStream));
            _SpotLengthMultipliers = GetSpotLengthAndMultipliers();
        }

        public string FileHash
        {
            get
            {
                return _fileHash;
                
            }

        }

        public abstract InventoryFile.InventorySourceType InventorySource { get; }

        public InventoryFile GetPendingRatesFile()
        {
            var result = new InventoryFile
            {
                FileName = Request.FileName == null ? "unknown" : Request.FileName,
                FileStatus = InventoryFile.FileStatusEnum.Pending,
                Hash = _fileHash,
                InventorySource = InventorySource,
                RatingBook = Request.RatingBook,
                PlaybackType = Request.PlaybackType
            };
            return result;
        }

        public abstract void ExtractFileData(
            System.IO.Stream stream,
            InventoryFile inventoryFile,
            System.Collections.Generic.List<InventoryFileProblem> fileProblems);

        private Dictionary<int, double> GetSpotLengthAndMultipliers()
        {
            // load the list of spots and ids
            var spotLengthIds = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

            // load spot lenght ids and multipliers
            var spotMultipliers = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthMultiplierRepository>().GetSpotLengthIdsAndCostMultipliers();

            return (from c in spotLengthIds
                    join d in spotMultipliers on c.Value equals d.Key
                    select new { c.Key, d.Value }).ToDictionary(x => x.Key, y => y.Value);

        }

        //protected void _ApplySpotLengthRateMultipliers(StationProgramFlightWeek flightWeek, decimal periodRate)
        //{
        //    flightWeek.Rate15s = periodRate * (decimal) _SpotLengthMultipliers[15];
        //    flightWeek.Rate30s = periodRate;
        //    flightWeek.Rate60s = periodRate * (decimal) _SpotLengthMultipliers[60];
        //    flightWeek.Rate90s = periodRate * (decimal) _SpotLengthMultipliers[90];
        //    flightWeek.Rate120s = periodRate * (decimal) _SpotLengthMultipliers[120];
        //}

        protected DisplayDaypart ParseStringToDaypart(string dayPartText, string station)
        {
            DisplayDaypart displayDaypart;

            if (!TryParse(dayPartText, out displayDaypart))
                throw new Exception(string.Format("Invalid daypart '{0}' for program '{1}' on Station {2}.", dayPartText, this.Request.BlockName, station));
            if (displayDaypart == null || (displayDaypart != null && !displayDaypart.IsValid))
                throw new Exception(string.Format("Invalid daypart '{0}' for program '{1}' on Station {2}.", dayPartText, this.Request.BlockName, station));

            return displayDaypart;
        }

        private bool TryParse(string value, out DisplayDaypart result)
        {
            result = null;

            if (!string.IsNullOrEmpty(value))
            {
                string lDays;
                string lTimes;
                string[] lTimesSplit;

                DisplayDaypart lDisplayDaypart = new DisplayDaypart();
                lDisplayDaypart.Code = "CUS";
                lDisplayDaypart.Name = "Custom";

                if (value.Split(new char[] { ' ' }).Length != 2)
                    return (false);

                lDays = value.Split(new char[] { ' ' })[0].Trim();
                lTimes = value.Split(new char[] { ' ' })[1].Trim();

                lTimesSplit = lTimes.Split(new char[] { '-' });
                if (lTimesSplit.Length != 2)
                    return (false);

                #region Days
                if (lDays.Contains(","))
                {
                    string[] lDayGroups = lDays.Split(new char[] { ',' });
                    ParseDays(lDisplayDaypart, lDayGroups);
                }
                else
                {
                    ParseDays(lDisplayDaypart, new string[] { lDays });
                }
                #endregion
                #region Times
                #region Start Time
                int? lStartTime = ParseTime(lTimesSplit[0]);
                if (!lStartTime.HasValue)
                    return false;
                lDisplayDaypart.StartTime = lStartTime.Value;
                #endregion
                #region End Time
                int? lEndTime = ParseTime(lTimesSplit[1]);
                if (!lEndTime.HasValue)
                    return false;
                lDisplayDaypart.EndTime = lEndTime.Value - 1;
                #endregion
                #endregion

                if (lDisplayDaypart.StartTime == 86400)
                {
                    lDisplayDaypart.StartTime = 0;
                }

                result = lDisplayDaypart;
            }

            return (result != null);
        }
        private void ParseDays(DisplayDaypart pDaypart, string[] pDayGroups)
        {
            string[] lDayConstants1 = new string[7] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
            string[] lDayConstants2 = new string[7] { "M", "TU", "W", "TH", "F", "SA", "SU" };
            string[] lDayConstants3 = new string[7] { "MO", "TU", "WE", "TH", "FR", "SA", "SU" };
            string[] lDayConstants4 = new string[7] { "M", "T", "W", "R", "F", "S", "SU" };

            foreach (string lDayGroup in pDayGroups)
            {
                if (lDayGroup.Trim() == "")
                    continue;

                if (lDayGroup.Contains("-"))
                {
                    int lEndIndex = -1;
                    int lStartIndex = -1;
                    string[] lSplitDays = lDayGroup.Split(new char[] { '-' });
                    for (int i = 0; i < lSplitDays.Length; i++)
                        lSplitDays[i] = lSplitDays[i].Trim();

                    for (int i = 0; i < 7; i++)
                    {
                        if (lDayConstants1[i] == lSplitDays[0].ToUpper() ||
                            lDayConstants2[i] == lSplitDays[0].ToUpper() ||
                            lDayConstants3[i] == lSplitDays[0].ToUpper() ||
                            lDayConstants4[i] == lSplitDays[0].ToUpper())
                        {
                            lStartIndex = i;
                        }
                        else if (lDayConstants1[i] == lSplitDays[1].ToUpper() ||
                                 lDayConstants2[i] == lSplitDays[1].ToUpper() ||
                                 lDayConstants3[i] == lSplitDays[1].ToUpper() ||
                                 lDayConstants4[i] == lSplitDays[1].ToUpper())
                        {
                            lEndIndex = i;
                        }
                    }
                    if (lStartIndex >= 0 && lEndIndex >= 0)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            if (i >= lStartIndex && i <= lEndIndex)
                            {
                                switch (i)
                                {
                                    case (0):
                                        pDaypart.Monday = true;
                                        break;
                                    case (1):
                                        pDaypart.Tuesday = true;
                                        break;
                                    case (2):
                                        pDaypart.Wednesday = true;
                                        break;
                                    case (3):
                                        pDaypart.Thursday = true;
                                        break;
                                    case (4):
                                        pDaypart.Friday = true;
                                        break;
                                    case (5):
                                        pDaypart.Saturday = true;
                                        break;
                                    case (6):
                                        pDaypart.Sunday = true;
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (lDayConstants1[i] == lDayGroup.ToUpper() ||
                            lDayConstants2[i] == lDayGroup.ToUpper() ||
                            lDayConstants3[i] == lDayGroup.ToUpper() ||
                            lDayConstants4[i] == lDayGroup.ToUpper())
                        {
                            switch (i)
                            {
                                case (0):
                                    pDaypart.Monday = true;
                                    break;
                                case (1):
                                    pDaypart.Tuesday = true;
                                    break;
                                case (2):
                                    pDaypart.Wednesday = true;
                                    break;
                                case (3):
                                    pDaypart.Thursday = true;
                                    break;
                                case (4):
                                    pDaypart.Friday = true;
                                    break;
                                case (5):
                                    pDaypart.Saturday = true;
                                    break;
                                case (6):
                                    pDaypart.Sunday = true;
                                    break;
                            }
                        }
                    }
                }
            }
        }
        private int? ParseTime(string pTimeString)
        {
            int lReturn = -1;

            var checkTime = new Regex(_validTimeExpression);

            if (checkTime.IsMatch(pTimeString))
            {
                int lHour;
                int lMinute;
                int lSecond;

                bool AM = pTimeString.Contains("A") || pTimeString.Contains("AM");
                bool PM = pTimeString.Contains("P") || pTimeString.Contains("PM");

                pTimeString =
                    pTimeString.Replace("A", "")
                        .Replace("a", "")
                        .Replace("P", "")
                        .Replace("p", "")
                        .Replace("M", "")
                        .Replace("m", "");

                if (pTimeString.Contains(":"))
                {
                    string[] lTimePieces = pTimeString.Split(new char[] {':'});
                    if (lTimePieces.Length == 2)
                    {
                        if (!int.TryParse(lTimePieces[0], out lHour))
                            return (null);
                        if (!int.TryParse(lTimePieces[1], out lMinute))
                            return (null);

                        if (PM && lHour < 12)
                            lHour += 12;
                        if (AM && lHour >= 12)
                            lHour -= 12;

                        lReturn = (lHour*3600) + (lMinute*60);
                    }
                    else if (lTimePieces.Length == 3)
                    {
                        if (!int.TryParse(lTimePieces[0], out lHour))
                            return (null);
                        if (!int.TryParse(lTimePieces[1], out lMinute))
                            return (null);
                        if (!int.TryParse(lTimePieces[2], out lSecond))
                            return (null);

                        if (PM && lHour < 12)
                            lHour += 12;
                        if (AM && lHour >= 12)
                            lHour -= 12;

                        lReturn = (lHour*3600) + (lMinute*60) + lSecond;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (!int.TryParse(pTimeString, out lHour))
                        return (null);

                    if (PM && lHour < 12)
                        lHour += 12;
                    if (AM && lHour >= 12)
                        lHour -= 12;

                    if (lHour == 0)
                        lHour = 24;

                    lReturn = (lHour*3600);
                }
            }

            return lReturn == -1 ? null : (int?)lReturn;
        }
    }
}
