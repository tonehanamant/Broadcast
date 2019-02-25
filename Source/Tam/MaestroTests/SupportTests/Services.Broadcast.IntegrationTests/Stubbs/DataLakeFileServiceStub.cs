using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;

namespace Services.Broadcast.IntegrationTests.Stubbs
{
    public class DataLakeFileServiceStub : IDataLakeFileService
    {
        public void Save(FileRequest fileRequest)
        {
            // Do nothing.
        }
    }
}
