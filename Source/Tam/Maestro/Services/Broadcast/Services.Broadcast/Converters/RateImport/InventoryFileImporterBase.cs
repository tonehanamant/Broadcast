using Common.Services;
using Microsoft.Practices.Unity.InterceptionExtension;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Converters.RateImport
{
    public abstract class InventoryFileImporterBase
    {
        private readonly string _validTimeExpression =
            @"(^([0-9]|[0-1][0-9]|[2][0-3])(:)?([0-5][0-9])(\s{0,1})(A|AM|P|PM|a|am|p|pm|aM|Am|pM|Pm{2,2})$)|(^([0-9]|[1][0-9]|[2][0-3])(\s{0,1})(A|AM|P|PM|a|am|p|pm|aM|Am|pM|Pm{2,2})$)";

        protected BroadcastDataDataRepositoryFactory _BroadcastDataRepositoryFactory;
        protected IDaypartCache _DaypartCache;
        protected IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        protected IBroadcastAudiencesCache _AudiencesCache;
        protected Dictionary<int, double> _SpotLengthMultipliers;

        private Dictionary<int, int> _SpotLengthIdsByLength;
        private Dictionary<int, int> _SpotLengthsById;
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

        public Dictionary<int, int> SpotLengthIdsByLength
        {
            get
            {
                if (_SpotLengthIdsByLength == null)
                {
                    _SpotLengthIdsByLength = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
                }
                return _SpotLengthIdsByLength;
            }
        }

        public Dictionary<int, int> SpotLengthsById
        {
            get
            {
                if (_SpotLengthsById == null)
                {
                    _SpotLengthsById = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthsById();
                }
                return _SpotLengthsById;
            }
        }

        public void CheckFileHash()
        {

            //check if file has already been loaded
            if (_BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>()
                    .GetInventoryFileIdByHash(_fileHash) > 0)
            {
                throw new BroadcastDuplicateInventoryFileException(
                    "Unable to load rate file. The selected rate file has already been loaded or is already loading.");
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
            get { return _fileHash; }

        }

        public abstract InventoryFile.InventorySource InventoryFileSource { get; }
        public abstract InventorySource InventorySource { get; set; }

        public virtual InventoryFile GetPendingInventoryFile()
        {
            var result = new InventoryFile();
            return HydrateInventoryFile(result);
        }

        protected InventoryFile HydrateInventoryFile(InventoryFile inventoryFileToHydrate)
        {
            inventoryFileToHydrate.FileName = Request.FileName == null ? "unknown" : Request.FileName;
            inventoryFileToHydrate.FileStatus = InventoryFile.FileStatusEnum.Pending;
            inventoryFileToHydrate.Hash = _fileHash;
            inventoryFileToHydrate.InventorySourceId = inventoryFileToHydrate.InventorySourceId;
            inventoryFileToHydrate.RatingBook = Request.RatingBook;
            inventoryFileToHydrate.PlaybackType = Request.PlaybackType;
            inventoryFileToHydrate.Source = this.InventoryFileSource;
            return inventoryFileToHydrate;
        }

        public abstract void ExtractFileData(
            System.IO.Stream stream,
            InventoryFile inventoryFile,
            DateTime effectiveDate,
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

        protected List<StationInventoryManifestRate> _GetManifestRatesFromMultipliers(decimal periodRate)
        {
            var manifestRates = new List<StationInventoryManifestRate>();

            manifestRates.Add(new StationInventoryManifestRate()
            {
                Rate = periodRate * (decimal) _SpotLengthMultipliers[15],
                SpotLengthId = _SpotLengthIdsByLength[15]
            });

            manifestRates.Add(new StationInventoryManifestRate()
            {
                Rate = periodRate,
                SpotLengthId = _SpotLengthIdsByLength[30]
            });

            manifestRates.Add(new StationInventoryManifestRate()
            {
                Rate = periodRate * (decimal)_SpotLengthMultipliers[60],
                SpotLengthId = _SpotLengthIdsByLength[60]
            });

            manifestRates.Add(new StationInventoryManifestRate()
            {
                Rate = periodRate * (decimal)_SpotLengthMultipliers[90],
                SpotLengthId = _SpotLengthIdsByLength[90]
            });

            manifestRates.Add(new StationInventoryManifestRate()
            {
                Rate = periodRate * (decimal)_SpotLengthMultipliers[120],
                SpotLengthId = _SpotLengthIdsByLength[120]
            });

            return manifestRates;
        }

        protected DisplayDaypart ParseStringToDaypart(string daypartText, string station)
        {
            DisplayDaypart displayDaypart;

            if (!TryParse(daypartText, out displayDaypart))
                throw new Exception(string.Format("Invalid daypart '{0}' on Station {1}.", daypartText, station));
            if (displayDaypart == null || (displayDaypart != null && !displayDaypart.IsValid))
                throw new Exception(string.Format("Invalid daypart '{0}' on Station {1}.", daypartText, station));

            return displayDaypart;
        }

        private bool TryParse(string value, out DisplayDaypart result)
        {
            result = null;
            value = value.Trim();

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

                string startMeridiem = null;
                string endMeridiem = null;
                if (lTimesSplit[0].EndsWith("am", StringComparison.InvariantCultureIgnoreCase)
                    || lTimesSplit[0].EndsWith("a", StringComparison.InvariantCultureIgnoreCase))
                {
                    startMeridiem = "am";
                }
                else if (lTimesSplit[0].EndsWith("pm", StringComparison.InvariantCultureIgnoreCase)
                   || lTimesSplit[0].EndsWith("p", StringComparison.InvariantCultureIgnoreCase))
                {
                    startMeridiem = "pm";
                }

                if (lTimesSplit[1].EndsWith("am", StringComparison.InvariantCultureIgnoreCase)
                    || lTimesSplit[1].EndsWith("a", StringComparison.InvariantCultureIgnoreCase))
                {
                    endMeridiem = "am";
                }
                else if (lTimesSplit[1].EndsWith("pm", StringComparison.InvariantCultureIgnoreCase)
                   || lTimesSplit[1].EndsWith("p", StringComparison.InvariantCultureIgnoreCase))
                {
                    endMeridiem = "pm";
                }

                if (startMeridiem == null && endMeridiem == null)
                {
                    return false;
                }

                if (startMeridiem == null)
                {
                    lTimesSplit[0] = lTimesSplit[0] + endMeridiem;
                }

                if (endMeridiem == null)
                {
                    lTimesSplit[1] = lTimesSplit[1] + startMeridiem;
                }

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

                    for (int i = 0; i < 7; i++) //7 -> days of the week
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

                bool AM = pTimeString.EndsWith("A", StringComparison.InvariantCultureIgnoreCase) || pTimeString.EndsWith("AM", StringComparison.InvariantCultureIgnoreCase);
                bool PM = pTimeString.EndsWith("P", StringComparison.InvariantCultureIgnoreCase) || pTimeString.EndsWith("PM", StringComparison.InvariantCultureIgnoreCase);

                pTimeString =
                    pTimeString.Replace("A", "")
                        .Replace("a", "")
                        .Replace("P", "")
                        .Replace("p", "")
                        .Replace("M", "")
                        .Replace("m", "");

                //ad colon separating minutes and hours if missing
                if (!pTimeString.Contains(":") && pTimeString.Length > 2)
                {
                    pTimeString = pTimeString.Substring(0, pTimeString.Length - 2) + ":" + pTimeString.Substring(pTimeString.Length - 2);
                }

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
