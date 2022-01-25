using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface ILogoService : IApplicationService
    {
        /// <summary>
        /// Returns the application logo which is taken from the SMS service
        /// </summary>
        /// <returns>
        /// Image as a byte array
        /// </returns>
        byte[] GetLogoAsByteArray(string LogoFilePath);

        void SaveInventoryLogo(int inventorySourceId, FileRequest saveRequest, string userName, DateTime now);

        InventoryLogo GetInventoryLogo(int inventorySourceId);
    }

    public class LogoService : BroadcastBaseClass, ILogoService
    {
        private readonly IInventoryLogoRepository _InventoryLogoRepository;

        public LogoService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _InventoryLogoRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryLogoRepository>();
        }

        public byte[] GetLogoAsByteArray(string LogoFilePath)
        {
            string logoUrl = Path.Combine(LogoFilePath, BroadcastConstants.LOGO_FILENAME);
            return _ConvertFileToByteArray(logoUrl);
        }

        public void SaveInventoryLogo(int inventorySourceId, FileRequest saveRequest, string userName, DateTime now)
        {
            var allowedExtensions = new string[] { ".png", ".jpeg", ".jpg", ".gif" };
            var fileExtension = Path.GetExtension(saveRequest.FileName);

            if (allowedExtensions.All(x => !x.Equals(fileExtension, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception(
                    $"Invalid file format. Please provide a file with one of the following extensions: {string.Join(", ", allowedExtensions)}");
            }

            if (string.IsNullOrWhiteSpace(saveRequest.RawData))
            {
                throw new Exception("Please send logo image as RawData within a request");
            }

            var inventoryLogo = new InventoryLogo
            {
                InventorySource = new InventorySource { Id = inventorySourceId },
                CreatedBy = userName,
                CreatedDate = now,
                FileName = saveRequest.FileName,
                FileContent = Convert.FromBase64String(saveRequest.RawData)
            };

            _InventoryLogoRepository.SaveInventoryLogo(inventoryLogo);
        }

        public InventoryLogo GetInventoryLogo(int inventorySourceId)
        {
            return _InventoryLogoRepository.GetLatestInventoryLogo(inventorySourceId);
        }

        private byte[] _ConvertFileToByteArray(string logoUrl)
        {
            byte[] cadentLogo = System.IO.File.ReadAllBytes(logoUrl);
            return cadentLogo;
        }

    }
}