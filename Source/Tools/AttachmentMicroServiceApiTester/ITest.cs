using System.Threading.Tasks;

namespace AttachmentMicroServiceApiTester
{
    public interface ITest
    {
        Task<bool> Run();
    }
}