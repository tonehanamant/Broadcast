using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigurationService.Client;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Helpers
{
    public static class SystemComponentParameterHelper
    {
        public static IConfigurationWebApiClient _configurationClient;

        public static void SetConfigurationClient(IConfigurationWebApiClient client)
        {
            _configurationClient = client;
        }

        public static IConfigurationWebApiClient GetConfigurationClient()
        {
            if (_configurationClient != null)
                return _configurationClient;
            throw new Exception("The Configuration Client has not been set up.");
        }

        public static T GetPropertyValue<T>(
            string pSystemComponentID,
            string pSystemParameterID)
            where T : IConvertible
        {
            string componentParameterValue = GetConfigurationClient().GetSystemComponentParameterValue(pSystemComponentID, pSystemParameterID);
            try
            {
                return (T)Convert.ChangeType((object)componentParameterValue, typeof(T), (IFormatProvider)CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new ApplicationException(string.Format("System parameter has invalid value.Failed to convert value {0} to {1} type for system component {2} & parameter {3}.", (object)componentParameterValue, (object)typeof(T).ToString(), (object)pSystemComponentID, (object)pSystemParameterID));
            }
        }
    }

}
