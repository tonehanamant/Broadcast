using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using ConfigurationService.Client;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class StubbedConfigurationWebApiClient : IConfigurationWebApiClient
    {
        public string TAMEnvironment => "Local";
        public static Dictionary<string, string> RunTimeParameters = new Dictionary<string, string>();

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
            // Note: The SMS Service and the Configuration API take slightly different strings as input
            pTamResource = pTamResource == TAMResource.BroadcastConnectionString.ToString()
                ? "broadcast"
                : "broadcastforecast";

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
            if (RunTimeParameters.TryGetValue(parameterId, out var result))
            {
                return result;
            }
            else
            {
                result = string.Empty;
            }
            
            switch (parameterId)
            {
                case "BroadcastSharedFolder":
                    result = @"\\cadfs11\Broadcast\IntegrationTests";
                    break;
                case "CampaignExportReportsFolder":
                    result = "CampaignExportReports";
                    break;
                case "BroadcastMatchingBuffer":
                    result = "120";
                    break;
                case "DaypartCacheSlidingExpirationSeconds":
                    result = "1800";
                    break;
                case "WWTV_FtpUsername":
                    result = "broadcast";
                    break;
                case "WWTV_FtpPassword":
                    result = "password";
                    break;
                case "WWTV_FtpHost":
                    result = "cad-phl-mhohens.crossmw.com";
                    break;
                case "WWTV_FtpOutboundFolder":
                    result = "uploads";
                    break;
                case "WWTV_FtpErrorFolder":
                    result = "Errors";
                    break;
                case "WWTV_FailedFolder":
                    result = @"E:\temp";
                    break;
                case "WWTV_FtpInboundFolder":
                    result = @"OutPost";
                    break;
                case "EmailHost":
                    result = "smtp.office365.com";
                    break;
                case "EmailFrom":
                    result = "broadcast@crossmw.com";
                    break;
                case "EmailUsername":
                    result = "broadcastsmtp@crossmw.com";
                    break;
                case "EmailPassword":
                    result = "7TUCE+HAp3LDexQ6JIvaEA==";
                    break;
                case "EmailWhiteList":
                    result = "mhohenshilt@crossmw.com";
                    break;
                case "WWTV_NotificationEmail":
                    result = "mhohenshilt@crossmw.com";
                    break;
                case "EmailNotificationsEnabled":
                    result = "True";
                    break;
                case "WWTV_LocalFtpErrorFolder":
                    result = ".\\Files";
                    break;
                case "WWTV_SharedFolder_UserName":
                    result = "username";
                    break;
                case "WWTV_SharedFolder_Password":
                    result = "password";
                    break;
                case "DefaultNtiConversionFactor":
                    result = "0.2";
                    break;
                case "WWTV_SharedFolder":
                    result = "C:\\WWTV\\WWTVData";
                    break;
                case "WWTV_PostLogDropFolder":
                    result = @"E:\temp\wwtv\PostLogPreprocessing";
                    break;
                case "WWTV_PostLogErrorFolder":
                    result = @"E:\temp\wwtv\WWTVErrors\PostLogPreprocessing";
                    break;
                case "WWTV_SpotTrackerDropFolder":
                    result = @"D:\temp\wwtv\SpotTrackerDropFolder";
                    break;
                case "WWTV_PostLogFtpOutboundFolder":
                    result = "InPrePost";
                    break;
                case "MediaMonthCruchCacheSlidingExpirationSeconds":
                    result = "24";
                    break;
                case "WWTV_KeepingTracFtpInboundFolder":
                    result = @"D:\temp\wwtv\WWTV_KeepingTracFtpInboundFolder";
                    break;
                case "DefaultMarketCoverage":
                    result = "0";
                    break;
                case "BroadcastNTIUploadApiUrl":
                    result = "http://devvmqa2.dev.crossmw.com/BroadcastNtiWeb/api/broadcastnti";
                    break;
                case "ImpressionStartEndTimeAdjustment":
                    result = "420";
                    break;
                case "ImpressionStartOfDayForAdjustment":
                    result = @"18000";
                    break;
                case "DataLake_SharedFolder":
                    result = Path.GetTempPath();
                    break;
                case "DataLake_SharedFolder_UserName":
                    result = string.Empty;
                    break;
                case "DataLake_SharedFolder_Password":
                    result = string.Empty;
                    break;
                case "DataLake_NotificationEmail":
                    result = "mhohenshilt@crossmw.com";
                    break;
                case "InventoryUploadErrorsFolder":
                    result = @"\\cadfs11\Inventory Management UI\Continuous Deployment";
                    break;
                case "ScxGenerationFolder":
                    result = Path.GetTempPath();
                    break;
                case "ScxGenerationIntervalSeconds":
                    result = "300";
                    break;
                case "ScxGenerationParallelJobs":
                    result = "3";
                    break;
                case "InventorySummaryCacheAbsoluteExpirationSeconds":
                    result = "600";
                    break;
                case "EnableCampaignsLocking":
                    result = "True";
                    break;
                case "BroadcastExcelTemplatesPath":
                    result = @"\\cadfs11\\Inventory Management UI\Continuous Deployment";
                    break;
                case "EnableBarterInventoryForPricingModel":
                    result = "True";
                    break;
                case "EnableOpenMarketInventoryForPricingModel":
                    result = "True";
                    break;
                case "EnableProprietaryOAndOInventoryForPricingModel":
                    result = "True";
                    break;
                case "ProgramsSearchUrl":
                    result = @"https://pemonp3we9.execute-api.us-east-1.amazonaws.com/staging/v1/programs/search/";
                    break;
                case "ProgramsSearchTokenUrl":
                    result = @"https://dev-cmw.auth.us-east-1.amazoncognito.com/oauth2/token";
                    break;
                case "ProgramsSearchClientId":
                    result = @"5e9kdecif9k6r7ttetgd4e500t";
                    break;
                case "ProgramsSearchEncryptedSecret":
                    result = @"OJE8vVrWiuZrou5oVn/uVdCmMSCRf/7vhlBB9Uz9bG/dQkN8WKjS1gXV01ANViI+UvbDSI8XjCs=";
                    break;
                case "ProgramGuideUrl":
                    result = @"https://pemonp3we9.execute-api.us-east-1.amazonaws.com/staging/v1/programs/guide/";
                    break;
                case "ProgramGuideTokenUrl":
                    result = @"https://dev-cmw.auth.us-east-1.amazoncognito.com/oauth2/token";
                    break;
                case "ProgramGuideTimeoutSeconds":
                    result = "1200";
                    break;
                case "ProgramGuideEncryptedSecret":
                    result = @"OJE8vVrWiuZrou5oVn/uVdCmMSCRf/7vhlBB9Uz9bG/dQkN8WKjS1gXV01ANViI+UvbDSI8XjCs=";
                    break;
                case "ProgramGuideClientId":
                    result = @"5e9kdecif9k6r7ttetgd4e500t";
                    break;
                case "ProgramGuideRequestElementMaxCount":
                    result = "10";
                    break;
                case "PlanPricingFloorPricingUrl":
                    result = @"https://datascience-dev.cadent.tv/broadcast-floor-pricing/v1/optimization";
                    break;
                case "PlanPricingAllocationsUrl":
                    result = @"http://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v1/allocation";
                    break;
                case "InventoryProcessingNotificationEmails":
                    result = "TestEmailsForInventoryProcessingNotificationEmails@cadent.tv";
                    break;
                case "InventoryProcessingGroupTimeoutHours":
                    result = "7";
                    break;
                case "InventoryProcessingGroupMonitorPauseSeconds":
                    result = "300";
                    break;
                case "InventoryProgramsEngineParallelEnabled":
                    result = "false";
                    break;
                case "InventoryProgramsEngineMaxDop":
                    result = "-1";
                    break;
                case "InventoryProgramsEngineParallelBatchSize":
                    result = "10";
                    break;
                case "InventoryProgramsEngineSaveBatchSize":
                    result = "1000";
                    break;
                case "InventoryProgramsEngineDeleteBatchSize":
                    result = "1000";
                    break;
                case "DisplayCampaignLink":
                    result = "True";
                    break;
                case "DisplayBuyingLink":
                    result = "True";
                    break;
                case "AllowMultipleCreativeLengths":
                    result = "False";
                    break;
                case "BroadcastAppFolder":
                    result = @"\\cadfs11\Broadcast\IntegrationTests\";
                    break;
                default:
                    throw new Exception("Unknown SystemComponentParameter: " + parameterId);
            }
            return result;
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
