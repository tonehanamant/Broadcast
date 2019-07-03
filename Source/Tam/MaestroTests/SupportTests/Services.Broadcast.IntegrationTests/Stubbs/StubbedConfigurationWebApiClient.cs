using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigurationService.Client;

namespace Services.Broadcast.IntegrationTests.Stubbs
{
    public class StubbedConfigurationWebApiClient : IConfigurationWebApiClient
    {
        public string TAMEnvironment => throw new NotImplementedException();

        public bool ClearSystemComponentParameterCache(string componentId, string parameterId)
        {
            throw new NotImplementedException();
        }

        public bool ClearSystemComponentParameterCache()
        {
            throw new NotImplementedException();
        }

        public string GetResource(string pTamResource)
        {
            pTamResource = pTamResource.ToLower();
            //@todo, Temporary switch to check if the CI server is running the tests.
            if (System.Configuration.ConfigurationManager.AppSettings["Environment"] == "Development")
            {
                if (pTamResource == "broadcast")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_integration;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "broadcastforecast")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_forecast_integration;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }
            else if (System.Configuration.ConfigurationManager.AppSettings["Environment"] == "Staging")
            {
                if (pTamResource == "broadcast")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_integration_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "broadcastforecast")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_forecast_integration_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }
            else if (System.Configuration.ConfigurationManager.AppSettings["Environment"] == "Release")
            {
                if (pTamResource == "broadcast")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_integration_codefreeze;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "broadcastforecast")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_forecast_integration_codefreeze;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }
            else if (System.Configuration.ConfigurationManager.AppSettings["Environment"] == "Release_CodeFreeze")
            {
                if (pTamResource == "broadcast")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_integration_codefreeze_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
                else if (pTamResource == "broadcastforecast")
                {
                    return
                        @"Data Source=devsql.dev.crossmw.com\maestro2_dev;Initial Catalog=broadcast_forecast_integration_codefreeze_staging;  Persist Security Info=True;user id=tamservice;pwd=KFqUjr+SjgugpL7h7yeJCg==; Asynchronous Processing=true";
                }
            }

            throw new Exception("Un-coded resource: " + pTamResource);
        }

        public ConfigurationService.Interfaces.Dtos.SystemComponentParameter GetSystemComponentParameter(string componentId, string parameterId)
        {
            throw new NotImplementedException();
        }

        public string GetSystemComponentParameterValue(string componentId, string parameterId)
        {
            throw new NotImplementedException();
        }

        public void SaveSystemComponentParameters(List<ConfigurationService.Interfaces.Dtos.SystemComponentParameter> paramList)
        {
            throw new NotImplementedException();
        }

        public List<ConfigurationService.Interfaces.Dtos.SystemComponentParameter> SearchSystemComponentParameters(string componentId, string parameterId)
        {
            throw new NotImplementedException();
        }
    }
}
