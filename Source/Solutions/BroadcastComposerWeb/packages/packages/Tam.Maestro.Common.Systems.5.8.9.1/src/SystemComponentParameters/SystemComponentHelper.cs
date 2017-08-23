using System;
using Tam.Maestro.Services.Clients;

namespace Tam.Maestro.Services.Cable.SystemComponentParameters
{
    public static class SystemComponentHelper
    {
        private static ISMSClient _smsClient;

        public static void SetSmsClient(ISMSClient smsClient)
        {
            _smsClient = smsClient;
        }

        public static ISMSClient GetSmsClient()
        {
            if (_smsClient != null)
            {
                return _smsClient;
            }
            else
            {
                throw new Exception("The SMS Client has not been setup.");
            }
        }

        public static T GetPropertyValue<T>(string pSystemComponentID, string pSystemParameterID) where T : IConvertible
        {
            T lReturnValue;
            int lRetryCount = 3;
            while (true)
            {
                try
                {
                    lReturnValue = GetPropertyValueInternal<T>(pSystemComponentID, pSystemParameterID);
                    break;
                }
                catch (ApplicationException)
                {
                    throw;
                }
                catch
                {
                    if (--lRetryCount < 0)
                    {
                        throw;
                    }
                    //Wait for 3 seconds and retry
                    System.Threading.Thread.Sleep(3000);
                }
            }

            return lReturnValue;
        }
        
        public static T GetPropertyValueInternal<T>(string pSystemComponentID, string pSystemParameterID) where T : IConvertible
        {
            T lReturnValue;

            string parameterValue = null;

            parameterValue = GetSmsClient().GetSystemComponentParameterValue(pSystemComponentID, pSystemParameterID);

            try
            {
                lReturnValue = (T)Convert.ChangeType(parameterValue, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                string errMsg = string.Format("System parameter has invalid value.Failed to convert value {0} to {1} type for system component {2} & parameter {3}.",
                    parameterValue, typeof(T).ToString(), pSystemComponentID, pSystemParameterID);
                throw new ApplicationException(errMsg);
            }
            return lReturnValue;
        }
    }
}
