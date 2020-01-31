using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Nti;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Services.Broadcast.ApplicationServices
{
    public interface INtiUniverseService : IApplicationService
    {
        /// <summary>
        /// Loads NTI universes from an xlsx file to the broadcast database and aggregates them by audience
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="userName">The name of the current user</param>
        /// <param name="createdDate">Created date</param>
        void LoadUniverses(Stream fileStream, string userName, DateTime createdDate);
    }

    public class NtiUniverseService : INtiUniverseService
    {
        private readonly INtiUniverseRepository _NtiUniverseRepository;
        private readonly IBroadcastAudiencesCache _BroadcastAudiencesCache;
        private readonly IUniversesFileImporter _UniversesFileImporter;

        public NtiUniverseService(
            IDataRepositoryFactory dataRepositoryFactory,
            IBroadcastAudiencesCache broadcastAudiencesCache,
            IUniversesFileImporter universesFileImporter)
        {
            _NtiUniverseRepository = dataRepositoryFactory.GetDataRepository<INtiUniverseRepository>();
            _BroadcastAudiencesCache = broadcastAudiencesCache;
            _UniversesFileImporter = universesFileImporter;
        }

        public void LoadUniverses(Stream fileStream, string userName, DateTime createdDate)
        {
            var universes = _UniversesFileImporter.ReadUniverses(fileStream);
            var ntiUniverseAudienceMappings = _NtiUniverseRepository.GetNtiUniverseAudienceMappings();

            _ValidateUniverses(universes);
            _CheckForMissingAudiences(universes, ntiUniverseAudienceMappings);

            var header = _MapToNtiUniverseHeader(universes, userName, createdDate);

            _AggregateUniversesByBroadcastAudience(header, ntiUniverseAudienceMappings);

            _NtiUniverseRepository.SaveNtiUniverses(header);
        }

        private void _AggregateUniversesByBroadcastAudience(
            NtiUniverseHeader header, 
            List<NtiUniverseAudienceMapping> ntiUniverseAudienceMappings)
        {
            var broadcastAudiences = _BroadcastAudiencesCache.GetAllEntities();
            var result = new List<NtiUniverse>();

            foreach (var audience in broadcastAudiences)
            {
                // a broadcast audience can consist of several nti audiences
                var ntiAudienceCodes = ntiUniverseAudienceMappings.Where(x => x.Audience.Id == audience.Id).Select(x => x.NtiAudienceCode);
                var universe = header.NtiUniverseDetails
                    .Where(x => ntiAudienceCodes.Contains(x.NtiAudienceCode, StringComparer.OrdinalIgnoreCase))
                    .Sum(x => x.Universe);
                
                result.Add(new NtiUniverse
                {
                    Audience = audience,
                    Universe = universe
                });
            }

            header.NtiUniverses = result;
        }

        private NtiUniverseHeader _MapToNtiUniverseHeader(List<NtiUniverseExcelRecord> universes, string userName, DateTime createdDate)
        {
            var first = universes.First();
            return new NtiUniverseHeader
            {
                CreatedDate = createdDate,
                CreatedBy = userName,
                Year = first.Year,
                Month = first.Month,
                WeekNumber = first.WeekNumber,
                NtiUniverseDetails = universes.Select(_MapToNtiUniverseDetail).ToList()
            };
        }

        private NtiUniverseDetail _MapToNtiUniverseDetail(NtiUniverseExcelRecord ntiUniverse)
        {
            return new NtiUniverseDetail
            {
                NtiAudienceId = ntiUniverse.NtiAudienceId,
                NtiAudienceCode = ntiUniverse.NtiAudienceCode,
                Universe = ntiUniverse.Universe
            };
        }

        private void _ValidateUniverses(List<NtiUniverseExcelRecord> universes)
        {
            if (universes.GroupBy(x => x.Year).Count() > 1)
                throw new ApplicationException("All records must belong to the same year");

            if (universes.GroupBy(x => x.Month).Count() > 1)
                throw new ApplicationException("All records must belong to the same month");

            if (universes.GroupBy(x => x.WeekNumber).Count() > 1)
                throw new ApplicationException("All records must belong to the same week");

            if (universes.Any(x => string.IsNullOrWhiteSpace(x.NtiAudienceCode)))
                throw new ApplicationException("Audience code can not be empty");
        }

        private void _CheckForMissingAudiences(
            List<NtiUniverseExcelRecord> universes, 
            List<NtiUniverseAudienceMapping> ntiUniverseAudienceMappings)
        {
            var ntiAudienceCodesFromFile = universes.Select(x => x.NtiAudienceCode);
            var ntiAudienceCodesFromDB = ntiUniverseAudienceMappings.Select(x => x.NtiAudienceCode).Distinct();
            var missingAudiences = ntiAudienceCodesFromDB.Except(ntiAudienceCodesFromFile, StringComparer.OrdinalIgnoreCase).ToList();

            if (missingAudiences.Any())
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Please provide universes for next audiences: ");

                for (var i = 0; i < missingAudiences.Count() - 1; i++)
                {
                    stringBuilder.Append($"{missingAudiences[i]}, ");
                }

                stringBuilder.Append($"{missingAudiences[missingAudiences.Count() - 1]}");

                throw new ApplicationException(stringBuilder.ToString());
            }
        }
    }
}
