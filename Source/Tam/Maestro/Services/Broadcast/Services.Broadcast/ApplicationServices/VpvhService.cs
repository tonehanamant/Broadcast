using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
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
    }

    public class VpvhService : IVpvhService
    {
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IVpvhFileImporter _VpvhFileImporter;
        private readonly IVpvhRepository _VpvhRepository;

        public VpvhService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IVpvhFileImporter vpvhFileImporter,
            IBroadcastAudiencesCache broadcastAudiencesCache)
        {
            _VpvhRepository = broadcastDataRepositoryFactory.GetDataRepository<IVpvhRepository>();
            _VpvhFileImporter = vpvhFileImporter;
            _BroadcastAudiencesCache = broadcastAudiencesCache;
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
            foreach(var quarter in quarters)
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
    }
}
