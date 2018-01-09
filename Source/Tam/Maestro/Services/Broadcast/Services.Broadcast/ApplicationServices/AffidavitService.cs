using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common;

namespace Services.Broadcast.ApplicationServices
{
    public enum AffidaviteFileSource
    {
        Strata = 1
    };

    public interface IAffidavitService : IApplicationService
    {
        int SaveAffidavit(AffidavitSaveRequest saveRequest);
    }

    public class AffidavitService : IAffidavitService
    {
        protected readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;

        public AffidavitService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
        }
        public int SaveAffidavit(AffidavitSaveRequest saveRequest)
        {
            Dictionary<int, int> spotLengthDict = null;

            if (saveRequest == null)
            {
                throw new Exception("No affidavit data received.");
            }

            var affidavit_file = new affidavit_files();
            affidavit_file.created_date = DateTime.Now;
            affidavit_file.file_hash = saveRequest.FileHash;
            affidavit_file.file_name = saveRequest.FileName;
            affidavit_file.source_id = saveRequest.Source;

            foreach (var detail in saveRequest.Details)
            {
                var det = new affidavit_file_details();
                det.air_time = Convert.ToInt32(detail.AirTime.TimeOfDay.TotalSeconds);
                det.original_air_date = detail.AirTime;
                det.isci = detail.Isci;
                det.program_name = detail.ProgramName;
                det.spot_length_id = GetSpotlength(detail.SpotLength,ref spotLengthDict);
                det.station = detail.Station;

                affidavit_file.affidavit_file_details.Add(det);
            }
            var repo = _BroadcastDataRepositoryFactory.GetDataRepository<IAffidavitRepository>();
            int id = repo.SaveAffidavitFile(affidavit_file);

            return id;
        }

        private int GetSpotlength(int spotLength, ref Dictionary<int, int> spotLengthDict)
        {
            if (spotLengthDict == null)
                spotLengthDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

            if (!spotLengthDict.ContainsKey(spotLength))
                throw new Exception(string.Format("Invalid spot length '{0}' found.", spotLength));

            return spotLengthDict[spotLength];
        }

    }
}