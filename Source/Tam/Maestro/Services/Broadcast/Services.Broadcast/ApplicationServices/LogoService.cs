using Common.Services.ApplicationServices;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;

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
        byte[] GetLogoAsByteArray();
    }

    public class LogoService : ILogoService
    {
        private readonly ISMSClient _smsClient;

        public LogoService(ISMSClient smsClient)
        {
            _smsClient = smsClient;
        }

        public byte[] GetLogoAsByteArray()
        {
            return _smsClient.GetLogoImage(CMWImageEnums.CMW_CADENT_LOGO).ImageData;
        }
    }
}
