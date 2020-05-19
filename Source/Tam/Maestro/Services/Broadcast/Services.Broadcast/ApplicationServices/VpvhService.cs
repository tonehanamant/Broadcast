using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities.Vpvh;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tam.Maestro.Common;

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
            }
            catch (Exception ex)
            {
                vpvhFile.Success = false;
                vpvhFile.ErrorMessage = ex.Message;
                _VpvhRepository.SaveFile(vpvhFile);

                throw;
            }
        }

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

                var quarter = Convert.ToInt32(vpvh.Quarter[0]);
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
