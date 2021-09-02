using System.Threading.Tasks;

namespace PricingModelEndpointTester
{
    public interface ITest
    {
        Task<bool> Run();
    }
}