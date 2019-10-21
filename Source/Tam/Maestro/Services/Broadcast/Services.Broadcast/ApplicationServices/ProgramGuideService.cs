﻿using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    /// <summary>
    /// Temporary service to allow testing of this Api.
    /// </summary>
    /// <remarks>
    /// TODO: Remove this during PRI-17014.
    /// </remarks>
    /// <seealso cref="Common.Services.ApplicationServices.IApplicationService" />
    public interface IProgramGuideService : IApplicationService
    {
        List<SearchResponseProgramDto> GetPrograms(bool simulate);

        List<GuideResponseElementDto> GetProgramsForGuide(int fileId, List<GuideRequestElementDto> requestElements, string queuedBy, bool simulate);
    }

    public class ProgramGuideService : IProgramGuideService
    {
        private IProgramGuideApiClient _ProgramGuideApiClient;
        private IProgramGuideApiClientSimulator _ProgramGuideApiClientSimulator;
        private IInventoryFileProgramNameJobsRepository _InventoryFileProgramNameJobsRepository;

        public ProgramGuideService(IProgramGuideApiClient programGuideApiClient, IDataRepositoryFactory dataRepositoryFactory
            , IProgramGuideApiClientSimulator programGuideApiClientSimulator)
        {
            _ProgramGuideApiClient = programGuideApiClient;
            _InventoryFileProgramNameJobsRepository = dataRepositoryFactory.GetDataRepository<IInventoryFileProgramNameJobsRepository>();

            _ProgramGuideApiClientSimulator = programGuideApiClientSimulator;
        }

        
        public List<SearchResponseProgramDto> GetPrograms(bool simulate)
        {
            return simulate ? _ProgramGuideApiClientSimulator.GetPrograms() : _ProgramGuideApiClient.GetPrograms();
        }

        public List<GuideResponseElementDto> GetProgramsForGuide(int fileId, List<GuideRequestElementDto> requestElements, string queuedBy, bool simulate = true)
        {
            _InventoryFileProgramNameJobsRepository.AddJob(new InventoryFileProgramNameJob
            {
                InventoryFileId = fileId,
                Status = InventoryFileProgramNameJobStatus.CallApi,
                QueuedAt = DateTime.Now,
                QueuedBy = queuedBy
            });

            return simulate ? _ProgramGuideApiClientSimulator.GetProgramsForGuide(requestElements) : _ProgramGuideApiClient.GetProgramsForGuide(requestElements);
        }
    }
}