using System.Net.Http;
using System.Threading.Tasks;

namespace PricingModelEndpointTester
{
    public interface ITest
    {
        HttpClient CreateHttpClient();
        Task<bool> RunPricingFalse(HttpClient httpClient);
        Task<bool> RunPricingTrue(HttpClient httpClient);

        Task<bool> RunBuyingFalse(HttpClient httpClient);
        Task<bool> RunBuyingTrue(HttpClient httpClient);
    }
}