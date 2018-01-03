using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using System;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffidavitService : IApplicationService
    {

        bool SaveAffidavit(AffadavitSaveRequest saveRequest);
    }

    public class AffidavitService : IAffidavitService
    {

        public bool SaveAffidavit(AffadavitSaveRequest saveRequest)
        {
            if (saveRequest == null)
            {
                throw new Exception("No affadavit data received.");
            }

            return true;
        }
    }
}