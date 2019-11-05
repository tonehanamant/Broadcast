using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class DataLakeFileServiceStub : IDataLakeFileService
    {
        public void Save(FileRequest fileRequest)
        {
            // Do nothing.
        }

        public void Save(string filePath)
        {
            // Do nothing.
        }
    }
}
