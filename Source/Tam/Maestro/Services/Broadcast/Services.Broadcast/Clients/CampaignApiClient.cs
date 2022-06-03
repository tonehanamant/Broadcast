using Services.Broadcast.Helpers;
using System.Net.Http;

namespace Services.Broadcast.Clients
{
    public interface ICampaignApiClient
    {
        HttpClient GetServiceHttpClient(string svcBaseUrl, string applicationId, string bearerToken = "");
    }

    public class CampaignApiClient : ServiceClientBase, ICampaignApiClient
    {
        public CampaignApiClient(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
                : base(featureToggleHelper, configurationSettingsHelper)
        {
        }

        public HttpClient GetServiceHttpClient(string svcBaseUrl, string applicationId, string bearerToken = "")
        {
            return base.GetServiceHttpClient(svcBaseUrl, applicationId, bearerToken);
        }
    }
}
