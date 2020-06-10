using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Vpvh;
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
    }

    public class VpvhService : IVpvhService
    {
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IVpvhFileImporter _VpvhFileImporter;
        private readonly IVpvhRepository _VpvhRepository;
        private readonly IVpvhExportEngine _VpvhExportEngine;
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;
        private readonly IDateTimeEngine _DateTimeEngine;

        public VpvhService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IVpvhFileImporter vpvhFileImporter,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IVpvhExportEngine vpvhExportEngine,
            IDateTimeEngine dateTimeEngine)
        {
            _VpvhRepository = broadcastDataRepositoryFactory.GetDataRepository<IVpvhRepository>();
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
            _VpvhFileImporter = vpvhFileImporter;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
            _VpvhExportEngine = vpvhExportEngine;
            _DateTimeEngine = dateTimeEngine;
        }

        public List<VpvhDefaultResponse> GetVpvhDefaults(VpvhDefaultsRequest request)
        {
            var standardDayparts = _DaypartDefaultRepository.GetAllDaypartDefaults();

            // default VPVH type is FourBookAverage (last 4 available quarter average, including future quarters)
            var lastFourQuartersVpvhData = _GetLastFourQuartersVpvhData(request.AudienceIds);
            var result = _CalculateFourBookAverageVpvhPerDaypartPerAudience(lastFourQuartersVpvhData, standardDayparts, request.AudienceIds);

            return result;
        }

        private List<VpvhQuarter> _GetLastFourQuartersVpvhData(List<int> audienceIds)
        {
            var quarterWithVpvhData = _VpvhRepository.GetQuartersWithVpvhData();

            if (quarterWithVpvhData.Count < 4)
            {
                throw new Exception("There must VPVH data for at least 4 quarters");
            }

            var lastFourQuarters = quarterWithVpvhData.OrderByDescending(x => x.Year).ThenByDescending(x => x.Quarter).Take(4).ToList();
            var years = lastFourQuarters.Select(x => x.Year).Distinct();
            var quartersVpvhData = _VpvhRepository.GetQuartersByYears(years);
            var lastFourQuartersVpvhData = quartersVpvhData.Where(x => lastFourQuarters.Any(q => x.Year == q.Year && x.Quarter == q.Quarter)).ToList();

            _EnsureVpvhDataExistsForQuartersAndAudiences(lastFourQuartersVpvhData, lastFourQuarters, audienceIds);

            return lastFourQuartersVpvhData;
        }

        private List<VpvhDefaultResponse> _CalculateFourBookAverageVpvhPerDaypartPerAudience(
            List<VpvhQuarter> vpvhQuarters,
            List<DaypartDefaultDto> standardDayparts,
            List<int> audienceIds)
        {
            var currentDate = _DateTimeEngine.GetCurrentMoment();
            var vpvhValueExtractors = new Dictionary<VpvhCalculationSourceTypeEnum, Func<VpvhQuarter, double>>
            {
                { VpvhCalculationSourceTypeEnum.AM_NEWS, x => x.AMNews },
                { VpvhCalculationSourceTypeEnum.PM_NEWS, x => x.PMNews },
                { VpvhCalculationSourceTypeEnum.SYN_All, x => x.SynAll },
                { VpvhCalculationSourceTypeEnum.AVG_OF_AM_NEWS_AND_PM_NEWS, x => x.Tdn },
                { VpvhCalculationSourceTypeEnum.AVG_OF_AM_NEWS_AND_PM_NEWS_AND_SYN_ALL, x => x.Tdns }
            };

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
                    if (vpvhValueExtractors.TryGetValue(standardDaypart.VpvhCalculationSourceType, out var vpvhValueExtractor))
                    {
                        var averageVpvh = quartersForAudience.Select(vpvhValueExtractor).Sum() / 4;

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

        private void _EnsureVpvhDataExistsForQuartersAndAudiences(
            List<VpvhQuarter> vpvhQuarters,
            List<QuarterDto> targetQuarters,
            List<int> audienceIds)
        {
            foreach (var audienceId in audienceIds)
            {
                // there is no need to validate VPVH for HH, it`s always 1
                if (audienceId == BroadcastConstants.HouseholdAudienceId)
                    continue;

                var vpvhDataForAudience = vpvhQuarters.Where(x => x.Audience.Id == audienceId).ToList();

                foreach (var targetQuarter in targetQuarters)
                {
                    var numberOfRecordsForQuarter = vpvhDataForAudience.Count(x => x.Quarter == targetQuarter.Quarter && x.Year == targetQuarter.Year);

                    if (numberOfRecordsForQuarter == 0)
                        throw new Exception($"There is no VPVH data. Audience id: {audienceId}, quarter: Q{targetQuarter.Quarter} {targetQuarter.Year}");

                    if (numberOfRecordsForQuarter > 1)
                        throw new Exception($"More than one VPVH record exists. Audience id: {audienceId}, quarter: Q{targetQuarter.Quarter} {targetQuarter.Year}");
                }
            }
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
                            case Entities.Enums.VpvhOperationEnum.Sum:
                                totalAMNews += blockQuarter?.AMNews ?? 0;
                                totalPMNews += blockQuarter?.PMNews ?? 0;
                                totalSynAll += blockQuarter?.SynAll ?? 0;
                                break;
                            case Entities.Enums.VpvhOperationEnum.Subtraction:
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
    }
}
