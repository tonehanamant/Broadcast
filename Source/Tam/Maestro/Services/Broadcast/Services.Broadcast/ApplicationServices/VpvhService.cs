using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Vpvh;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IVpvhService : IApplicationService
    {
        /// <summary>
        /// Loads VPVHs from an xlsx file to the broadcast database
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="fileName">File name</param>
        /// <param name="userName">The name of the current user</param>
        /// <param name="createdDate">Created date</param>
        void LoadVpvhs(Stream fileStream, string fileName, string userName, DateTime createdDate);

        /// <summary>
        /// Gets a list of VPVHs quarters from a quarter
        /// </summary>
        /// <param name="quarter">Quarter</param>
        /// <returns>VPVHs quarter list</returns>
        List<VpvhQuarter> GetQuarters(QuarterDto quarter);

        /// <summary>
        /// Gets a VPVH quarters
        /// </summary>
        /// <param name="quarter">Quarter</param>
        /// <param name="audienceId">Audience</param>
        /// <returns>VPVH quarter</returns>
        VpvhQuarter GetQuarter(QuarterDto quarter, int audienceId);

        /// <summary>
        /// Export all VPVH quarters
        /// </summary>
        /// <returns>Excel file stream</returns>
        Stream Export();

        List<VpvhDefaultResponse> GetVpvhDefaults(VpvhDefaultsRequest request);

        /// <summary>
        /// Gets the four books, previous quarter and last year vpvh
        /// </summary>
        List<VpvhResponse> GetVpvhs(VpvhRequest request);
    }

    public class VpvhService : BroadcastBaseClass, IVpvhService
    {
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IVpvhFileImporter _VpvhFileImporter;
        private readonly IVpvhRepository _VpvhRepository;
        private readonly IVpvhForecastRepository _VpvhForecastRepository;
        private readonly IVpvhExportEngine _VpvhExportEngine;
        private readonly IStandardDaypartRepository _StandardDaypartRepository;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly Lazy<bool> _IsPlanningVpvhSourceV2Enabled;        
        private readonly Dictionary<VpvhCalculationSourceTypeEnum, Func<VpvhQuarter, double>> _VpvhValueExtractors = new Dictionary<VpvhCalculationSourceTypeEnum, Func<VpvhQuarter, double>>
            {
                { VpvhCalculationSourceTypeEnum.AM_NEWS, x => x.AMNews },
                { VpvhCalculationSourceTypeEnum.PM_NEWS, x => x.PMNews },
                { VpvhCalculationSourceTypeEnum.SYN_All, x => x.SynAll },
                { VpvhCalculationSourceTypeEnum.AVG_OF_AM_NEWS_AND_PM_NEWS, x => x.Tdn },
                { VpvhCalculationSourceTypeEnum.AVG_OF_AM_NEWS_AND_PM_NEWS_AND_SYN_ALL, x => x.Tdns }
            };

        public VpvhService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IVpvhFileImporter vpvhFileImporter,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IVpvhExportEngine vpvhExportEngine,
            IDateTimeEngine dateTimeEngine,
            IFeatureToggleHelper featureToggleHelper,             
            IConfigurationSettingsHelper configurationSettingsHelper,
            IQuarterCalculationEngine quarterCalculationEngine) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _VpvhForecastRepository = broadcastDataRepositoryFactory.GetDataRepository<IVpvhForecastRepository>();
            _VpvhRepository = broadcastDataRepositoryFactory.GetDataRepository<IVpvhRepository>();
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _VpvhFileImporter = vpvhFileImporter;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
            _VpvhExportEngine = vpvhExportEngine;
            _DateTimeEngine = dateTimeEngine;
            _QuarterCalculationEngine = quarterCalculationEngine;            
            _IsPlanningVpvhSourceV2Enabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_STATION_SECONDARY_AFFILIATIONS));
        }

        public List<VpvhDefaultResponse> GetVpvhDefaults(VpvhDefaultsRequest request)
        {
            var standardDayparts = _StandardDaypartRepository.GetAllStandardDayparts();
            return _GetFourBookAverage(request.AudienceIds, standardDayparts);
        }

        private List<VpvhQuarter> _GetLastFourQuartersVpvhData()
        {
            var quarterWithVpvhData = _VpvhRepository.GetQuartersWithVpvhData();
            var lastFourQuarters = quarterWithVpvhData.OrderByDescending(x => x.Year).ThenByDescending(x => x.Quarter).Take(4).ToList();

            if (!lastFourQuarters.Any())
                return new List<VpvhQuarter>();

            var years = lastFourQuarters.Select(x => x.Year).Distinct();
            var quartersVpvhData = _VpvhRepository.GetQuartersByYears(years);
            var lastFourQuartersVpvhData = quartersVpvhData.Where(x => lastFourQuarters.Any(q => x.Year == q.Year && x.Quarter == q.Quarter)).ToList();

            return lastFourQuartersVpvhData;
        }

        private List<VpvhDefaultResponse> _CalculateFourBookAverageVpvhPerDaypartPerAudience(
            List<VpvhQuarter> vpvhQuarters,
            List<StandardDaypartDto> standardDayparts,
            List<int> audienceIds)
        {
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            var result = new List<VpvhDefaultResponse>();

            foreach (var audienceId in audienceIds)
            {
                if (audienceId == BroadcastConstants.HouseholdAudienceId)
                {
                    result.AddRange(standardDayparts.Select(x => new VpvhDefaultResponse
                    {
                        StandardDaypartId = x.Id,
                        AudienceId = audienceId,
                        Vpvh = 1, // VPVH for HH is always 1
                        VpvhType = VpvhTypeEnum.FourBookAverage,
                        StartingPoint = currentDate
                    }));

                    continue;
                }

                var quartersForAudience = vpvhQuarters.Where(x => x.Audience.Id == audienceId).ToList();

                foreach (var standardDaypart in standardDayparts)
                {
                    if (!quartersForAudience.Any())
                    {
                        // if VPVH for audience has never been uploaded, the user has to enter VPVH manually
                        result.Add(new VpvhDefaultResponse
                        {
                            StandardDaypartId = standardDaypart.Id,
                            AudienceId = audienceId,
                            Vpvh = null,
                            VpvhType = VpvhTypeEnum.Custom,
                            StartingPoint = currentDate
                        });
                    }
                    else if (_VpvhValueExtractors.TryGetValue(standardDaypart.VpvhCalculationSourceType, out var vpvhValueExtractor))
                    {
                        // if there is data only for 3 quarters, we calculate an avarage of 3 quarters
                        var averageVpvh = quartersForAudience.Select(vpvhValueExtractor).Average();

                        result.Add(new VpvhDefaultResponse
                        {
                            StandardDaypartId = standardDaypart.Id,
                            AudienceId = audienceId,
                            Vpvh = averageVpvh,
                            VpvhType = VpvhTypeEnum.FourBookAverage,
                            StartingPoint = currentDate
                        });
                    }
                    else
                    {
                        throw new Exception("Unknown VpvhCalculationSourceTypeEnum was discovered");
                    }
                }
            }

            return result;
        }

        public List<VpvhQuarter> GetQuarters(QuarterDto quarter)
        {
            return _VpvhRepository.GetQuarters(quarter);
        }

        public VpvhQuarter GetQuarter(QuarterDto quarter, int audienceId)
        {
            return _VpvhRepository.GetQuarter(audienceId, quarter.Year, quarter.Quarter);
        }

        public void LoadVpvhs(Stream fileStream, string fileName, string userName, DateTime createdDate)
        {
            var vpvhFile = new VpvhFile
            {
                FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(fileStream)),
                FileName = fileName,
                CreatedBy = userName,
                CreatedDate = createdDate
            };

            _CheckIfFileAlreadyUploaded(vpvhFile.FileHash);

            try
            {
                var vpvhs = _VpvhFileImporter.ReadVpvhs(fileStream);
                vpvhFile.Items.AddRange(_ProcessItems(vpvhs));
                vpvhFile.Success = true;
                _VpvhRepository.SaveFile(vpvhFile);

                _CalculateQuarters(vpvhFile.Items);
            }
            catch (Exception ex)
            {
                vpvhFile.Success = false;
                vpvhFile.ErrorMessage = ex.Message;
                _VpvhRepository.SaveFile(vpvhFile);

                throw;
            }
        }

        private void _CalculateQuarters(List<VpvhFileItem> vpvhItems)
        {
            _CalculateBlockQuarters(vpvhItems);

            var quarters = vpvhItems.Select(v => new { v.Quarter, v.Year }).Distinct().Select(v => new QuarterDto { Quarter = v.Quarter, Year = v.Year }).ToList();

            _CalculateDerivedQuarters(quarters);
        }

        private void _CalculateDerivedQuarters(List<QuarterDto> quarters)
        {
            var vpvhMappings = _VpvhRepository.GetVpvhMappings();
            foreach (var quarter in quarters)
            {
                var vpvhQuarters = _VpvhRepository.GetQuarters(quarter);

                foreach (var audienceId in vpvhMappings.Select(v => v.Audience.Id).Distinct())
                {
                    double totalAMNews = 0, totalPMNews = 0, totalSynAll = 0;

                    foreach (var mappingAudience in vpvhMappings.Where(v => v.Audience.Id == audienceId))
                    {
                        var blockQuarter = vpvhQuarters.FirstOrDefault(v => v.Audience.Id == mappingAudience.ComposeAudience.Id);
                        switch (mappingAudience.Operation)
                        {
                            case VpvhOperationEnum.Sum:
                                totalAMNews += blockQuarter?.AMNews ?? 0;
                                totalPMNews += blockQuarter?.PMNews ?? 0;
                                totalSynAll += blockQuarter?.SynAll ?? 0;
                                break;
                            case VpvhOperationEnum.Subtraction:
                                totalAMNews -= blockQuarter?.AMNews ?? 0;
                                totalPMNews -= blockQuarter?.PMNews ?? 0;
                                totalSynAll -= blockQuarter?.SynAll ?? 0;
                                break;
                        }
                    }

                    var vpvhQuarter = vpvhQuarters.FirstOrDefault(v => v.Audience.Id == audienceId);

                    _SaveVpvhQuarter(vpvhQuarter, new DisplayAudience() { Id = audienceId }, quarter, totalAMNews, totalPMNews, totalSynAll);
                }
            }
        }

        private void _CalculateBlockQuarters(List<VpvhFileItem> vpvhItems)
        {
            foreach (var item in vpvhItems)
            {
                var vpvhQuarter = _VpvhRepository.GetQuarter(item.Audience.Id, item.Year, item.Quarter);

                _SaveVpvhQuarter(vpvhQuarter, item.Audience, new QuarterDto { Quarter = item.Quarter, Year = item.Year }, item.AMNews, item.PMNews, item.SynAll);
            }
        }

        private void _SaveVpvhQuarter(VpvhQuarter vpvhQuarter, DisplayAudience audience, QuarterDto quarter, double amNews, double pmNews, double synAll)
        {
            if (vpvhQuarter == null)
            {
                vpvhQuarter = new VpvhQuarter
                {
                    Audience = audience,
                    Quarter = quarter.Quarter,
                    Year = quarter.Year
                };
            }

            vpvhQuarter.AMNews = amNews;
            vpvhQuarter.PMNews = pmNews;
            vpvhQuarter.SynAll = synAll;
            vpvhQuarter.Tdn = _CalculateAverage(amNews, pmNews);
            vpvhQuarter.Tdns = _CalculateAverage(amNews, pmNews, synAll);

            if (vpvhQuarter.Id == 0)
                _VpvhRepository.SaveNewQuarter(vpvhQuarter);
            else
                _VpvhRepository.SaveQuarter(vpvhQuarter);
        }

        private double _CalculateAverage(params double[] values) =>
           Math.Round(values.Sum() / values.Length, 3);

        private void _CheckIfFileAlreadyUploaded(string fileHash)
        {
            if (_VpvhRepository.HasFile(fileHash))
                throw new Exception("VPVH file already uploaded to the system.");
        }

        private List<VpvhFileItem> _ProcessItems(List<VpvhExcelRecord> vpvhs)
        {
            const string quarterPattern = @"[1-4]Q\d{2}";
            var items = new List<VpvhFileItem>();
            foreach (var vpvh in vpvhs)
            {
                var audience = _BroadcastAudiencesCache.GetDisplayAudienceByCode(vpvh.Audience);
                if (audience == null)
                    throw new Exception("Invalid audience.");

                if (!Regex.IsMatch(vpvh.Quarter, quarterPattern, RegexOptions.Compiled))
                    throw new Exception("Invalid quarter.");

                var quarter = (int)char.GetNumericValue(vpvh.Quarter[0]);
                var year = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(vpvh.Quarter.Substring(2, 2)));

                _ValidateVpvh(vpvh.AMNews);
                _ValidateVpvh(vpvh.SynAll);
                _ValidateVpvh(vpvh.PMNews);

                var item = new VpvhFileItem
                {
                    Audience = audience,
                    Quarter = quarter,
                    Year = year,
                    AMNews = vpvh.AMNews,
                    PMNews = vpvh.PMNews,
                    SynAll = vpvh.SynAll
                };

                if (items.Any(i => i.Quarter == item.Quarter && i.Year == item.Year && i.Audience.Id == audience.Id))
                    throw new Exception($"Duplicate values to {vpvh.Audience} audience.");

                items.Add(item);
            }
            return items;
        }

        private void _ValidateVpvh(double vpvh)
        {
            if (vpvh > 10 || vpvh < 0.001)
                throw new Exception("VPVH must be between 0.01 and 10.");
        }

        public Stream Export()
        {
            var vpvhQuarters = _VpvhRepository.GetAllQuarters();

            var excel = _VpvhExportEngine.ExportQuarters(vpvhQuarters);

            return new MemoryStream(excel.GetAsByteArray());
        }

        public List<VpvhResponse> GetVpvhs(VpvhRequest request)
        {
            var standardDaypart = _StandardDaypartRepository.GetStandardDaypartById(request.StandardDaypartId);
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            var previousQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate.AddMonths(-3));
            var lastYearQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate.AddYears(-1));

            var previousVpvhQuarters = _VpvhRepository.GetQuarters(previousQuarter.Year, previousQuarter.Quarter, request.AudienceIds);
            var lastYearVpvhQuarters = _VpvhRepository.GetQuarters(lastYearQuarter.Year, lastYearQuarter.Quarter, request.AudienceIds);
            var fourBookAverages = _GetFourBookAverage(request.AudienceIds, new List<StandardDaypartDto> { standardDaypart });

            var response = new List<VpvhResponse>();

            foreach (var audienceId in request.AudienceIds)
            {
                // VPVH for HH is always 1
                if (audienceId == BroadcastConstants.HouseholdAudienceId)
                {
                    response.Add(new VpvhResponse
                    {
                        AudienceId = audienceId,
                        StartingPoint = currentDate,
                        StandardDaypartId = standardDaypart.Id,
                        LastYearVpvh = 1,
                        PreviousQuarterVpvh = 1,
                        FourBookAverageVpvh = 1,
                        VpvhDefaultValue = VpvhTypeEnum.FourBookAverage,
                        PreviousQuarter = new QuarterDto(previousQuarter.Quarter, previousQuarter.Year),
                        LastYearQuarter = new QuarterDto(lastYearQuarter.Quarter, lastYearQuarter.Year)
                    });

                    continue;
                }

                var previousVpvhQuarter = previousVpvhQuarters.SingleOrDefault(v => v.Year == previousQuarter.Year && v.Quarter == previousQuarter.Quarter && v.Audience.Id == audienceId);
                var lastYearVpvhQuarter = lastYearVpvhQuarters.SingleOrDefault(v => v.Year == lastYearQuarter.Year && v.Quarter == lastYearQuarter.Quarter && v.Audience.Id == audienceId);
                double? lastYearVpvh = 0;
                double? previousQuarterVpvh = 0;

                if (_IsPlanningVpvhSourceV2Enabled.Value)
                {
                    lastYearVpvh = _GetVpvhValueFromForecastDb(standardDaypart.Id, audienceId, lastYearQuarter.Quarter, lastYearQuarter.Year);
                    previousQuarterVpvh = _GetVpvhValueFromForecastDb(standardDaypart.Id, audienceId, previousQuarter.Quarter, previousQuarter.Year);
                    _LogInfo($"Last Year VPVH and Previous Year VPVH is populated from BroadcastForecastDB.");
                }
                else
                {
                    lastYearVpvh = _GetVpvhValue(lastYearVpvhQuarter, standardDaypart.VpvhCalculationSourceType);
                    previousQuarterVpvh = _GetVpvhValue(previousVpvhQuarter, standardDaypart.VpvhCalculationSourceType);
                    _LogInfo($"Last Year VPVH and Previous Year VPVH is populated from BroadcastDB.");
                }

                var vpvhResponseForAudience = new VpvhResponse
                {
                    AudienceId = audienceId,
                    StartingPoint = currentDate,
                    StandardDaypartId = standardDaypart.Id,
                    LastYearVpvh = lastYearVpvh,
                    PreviousQuarterVpvh = previousQuarterVpvh,
                    FourBookAverageVpvh = fourBookAverages.SingleOrDefault(v => v.AudienceId == audienceId)?.Vpvh,
                    PreviousQuarter = new QuarterDto(previousQuarter.Quarter, previousQuarter.Year),
                    LastYearQuarter = new QuarterDto(lastYearQuarter.Quarter, lastYearQuarter.Year)
                };

                // if VPVH for audience has never been uploaded, the user has to enter VPVH manually
                vpvhResponseForAudience.VpvhDefaultValue = vpvhResponseForAudience.FourBookAverageVpvh.HasValue ?
                    VpvhTypeEnum.FourBookAverage :
                    VpvhTypeEnum.Custom;

                response.Add(vpvhResponseForAudience);
            }

            return response;
        }

        private List<VpvhDefaultResponse> _GetFourBookAverage(List<int> audienceIds, List<StandardDaypartDto> standardDayparts)
        {
            // last 4 available quarter average (we use all available quarters if there are less than 4 available), including future quarters
            var lastFourQuartersVpvhData = _GetLastFourQuartersVpvhData();
            var result = _CalculateFourBookAverageVpvhPerDaypartPerAudience(lastFourQuartersVpvhData, standardDayparts, audienceIds);
            return result;
        }

        private double? _GetVpvhValue(VpvhQuarter vpvhQuarter, VpvhCalculationSourceTypeEnum vpvhCalculationSourceType)
        {
            if (vpvhQuarter == null)
                return null;

            _VpvhValueExtractors.TryGetValue(vpvhCalculationSourceType, out var vpvhValueExtractor);

            return vpvhValueExtractor.Invoke(vpvhQuarter);
        }
        private double _GetVpvhValueFromForecastDb(int stadardDaypartId, int audienceId, int VpvhQuarter, int year)
        { 
            var result=_VpvhForecastRepository.GetVpvhValueFromForecastDb(stadardDaypartId, audienceId, VpvhQuarter,year);
            return result;
        }
    }
}
