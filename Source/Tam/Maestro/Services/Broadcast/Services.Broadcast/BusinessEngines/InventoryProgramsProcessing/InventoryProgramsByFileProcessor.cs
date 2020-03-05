﻿using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsByFileProcessor : InventoryProgramsProcessingEngineBase
    {
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IInventoryProgramsByFileJobsRepository _InventoryProgramsByFileJobsRepository;

        public InventoryProgramsByFileProcessor(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService,
            IGenreCache genreCache)
            : base(broadcastDataRepositoryFactory,
                programGuideApiClient,
                stationMappingService,
                genreCache)
        {
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _InventoryProgramsByFileJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsByFileJobsRepository>();
        }

        protected override IInventoryProgramsJobsRepository _GetJobsRepository()
        {
            return _InventoryProgramsByFileJobsRepository;
        }


        protected override InventoryProgramsProcessingJobDiagnostics _GetNewDiagnostics()
        {
            return new InventoryProgramsProcessingJobByFileDiagnostics(OnDiagnosticMessageUpdate);
        }

        protected override InventorySource _GetInventorySource(int jobId)
        {
            var fileId = _InventoryProgramsByFileJobsRepository.GetJob(jobId).InventoryFileId;
            var inventorySource = _InventoryFileRepository.GetInventoryFileById(fileId).InventorySource;
            return inventorySource;
        }

        protected override List<StationInventoryManifest> _GatherInventory(int jobId, InventoryProgramsProcessingJobDiagnostics processDiagnostics)
        {
            var fileId = _InventoryProgramsByFileJobsRepository.GetJob(jobId).InventoryFileId;
            ((InventoryProgramsProcessingJobByFileDiagnostics)processDiagnostics).RecordRequestParameters(fileId);
            
            var manifests = _InventoryRepository.GetInventoryByFileIdForProgramsProcessing(fileId);
            return manifests;
        }

        protected override InventoryProgramsRequestPackage _BuildRequestPackage(List<StationInventoryManifest> inventoryManifests,
            InventorySource inventorySource, InventoryProgramsProcessingJobDiagnostics processDiagnostics, int jobId)
        {
            var requestElementNumber = 0;

            var rangeStartDate = inventoryManifests.SelectMany(s => s.ManifestWeeks.Select(w => w.StartDate)).Min();
            var rangeEndDate = inventoryManifests.SelectMany(s => s.ManifestWeeks.Select(w => w.EndDate)).Max();

            processDiagnostics.RecordTransformToInputStart(inventoryManifests.Count, rangeStartDate, rangeEndDate);

            var requestMappings = new List<GuideRequestResponseMapping>();
            var requestElements = new List<GuideRequestElementDto>();

            foreach (var manifest in inventoryManifests)
            {
                if (string.IsNullOrWhiteSpace(manifest.Station.Affiliation))
                {
                    // no affiliate indicates unrated so don't query for.
                    continue;
                }

                // a manifest should ALWAYS have a daypart.
                if (manifest.ManifestDayparts.Any() == false)
                {
                    _InventoryProgramsByFileJobsRepository.UpdateJobNotes(jobId, $"WARNING : Inventory Manifest '{manifest.Id}' has no dayparts.");
                    continue;
                }

                var firstDaypart = manifest.ManifestDayparts.OrderBy(d => d.Daypart.StartTime).First();

                var entryStartDate = _GetEntryStartDate(firstDaypart, rangeStartDate);
                var entryEndDate = _GetEntryEndDate(firstDaypart, entryStartDate);

                var entryDaypartStartTimeAsSeconds = firstDaypart.Daypart.StartTime;
                var entryDaypartEndTimeAsSeconds = _GetEndTimeInSeconds(entryDaypartStartTimeAsSeconds);
                var stationCallLetters = _GetManifestStationCallLetters(manifest, inventorySource);

                var requestElementMapping = new GuideRequestResponseMapping
                {
                    RequestElementNumber = ++requestElementNumber,
                    ManifestId = manifest.Id ?? 0,
                    ManifestDaypartId = firstDaypart.Id ?? 0,
                    StartDate = entryStartDate,
                    EndDate = entryEndDate,
                    DaypartText = firstDaypart.Daypart.Preview, // leave this as-is
                    StationCallLetters = stationCallLetters,
                    NetworkAffiliate = manifest.Station.Affiliation
                };
                requestMappings.Add(requestElementMapping);
                requestElements.Add(
                    new GuideRequestElementDto
                    {
                        Id = requestElementMapping.RequestEntryId,
                        StartDate = entryStartDate,
                        EndDate = entryEndDate,
                        StationCallLetters = stationCallLetters,
                        NetworkAffiliate = manifest.Station.Affiliation,
                        Daypart = new GuideRequestDaypartDto
                        {
                            Id = requestElementMapping.RequestEntryId,
                            Name = firstDaypart.Daypart.Preview,
                            Sunday = entryStartDate.DayOfWeek == DayOfWeek.Sunday || entryEndDate.DayOfWeek == DayOfWeek.Sunday,
                            Monday = entryStartDate.DayOfWeek == DayOfWeek.Monday || entryEndDate.DayOfWeek == DayOfWeek.Monday,
                            Tuesday = entryStartDate.DayOfWeek == DayOfWeek.Tuesday || entryEndDate.DayOfWeek == DayOfWeek.Tuesday,
                            Wednesday = entryStartDate.DayOfWeek == DayOfWeek.Wednesday || entryEndDate.DayOfWeek == DayOfWeek.Wednesday,
                            Thursday = entryStartDate.DayOfWeek == DayOfWeek.Thursday || entryEndDate.DayOfWeek == DayOfWeek.Thursday,
                            Friday = entryStartDate.DayOfWeek == DayOfWeek.Friday || entryEndDate.DayOfWeek == DayOfWeek.Friday,
                            Saturday = entryStartDate.DayOfWeek == DayOfWeek.Saturday || entryEndDate.DayOfWeek == DayOfWeek.Saturday,
                            StartTime = entryDaypartStartTimeAsSeconds,
                            EndTime = entryDaypartEndTimeAsSeconds
                        }
                    });
            }

            processDiagnostics.RecordTransformToInputStop(requestElements.Count);

            var requestPackage = new InventoryProgramsRequestPackage
            {
                RequestMappings = requestMappings,
                RequestElements = requestElements,
                InventoryManifests = inventoryManifests,
                StartDateRange = rangeStartDate,
                EndDateRange = rangeEndDate
            };
            return requestPackage;
        }

        protected override List<StationInventoryManifestDaypartProgram> _GetProgramsFromResponse(
            GuideResponseElementDto currentResponse,
            GuideRequestResponseMapping currentMapping,
            InventoryProgramsRequestPackage requestPackage
            )
        {
            var programs = new List<StationInventoryManifestDaypartProgram>();
            requestPackage.InventoryManifests
                .Where(m => m.Id == currentMapping.ManifestId)
                .SelectMany(m => m.ManifestDayparts)
                .Select(d => d.Id.Value)
                .ForEach(daypartId => 
                    currentResponse.Programs.Select(p => 
                        _MapProgramDto(p, daypartId, requestPackage))
                        .ForEach(a => programs.Add(a)));
            return programs;
        }

        private DateTime _GetEntryStartDate(StationInventoryManifestDaypart daypart, DateTime rangeStartDate)
        {
            const int oneDay = 1;
            var result = rangeStartDate;
            while (daypart.Daypart.Days.Contains(result.DayOfWeek) == false)
            {
                result = result.AddDays(oneDay);
            }
            return result;
        }

        private const int oneDay = 1;
        private const int fiveMinutesAsSeconds = 300;
        private const int twentyFourHoursInSeconds = 86400;
        private const int fiveMinutesToMidnightAsSeconds = twentyFourHoursInSeconds - fiveMinutesAsSeconds;

        private DateTime _GetEntryEndDate(StationInventoryManifestDaypart daypart, DateTime rangeStartDate)
        {
            var result = rangeStartDate;
            if (daypart.Daypart.StartTime >= fiveMinutesToMidnightAsSeconds)
            {
                result = result.AddDays(oneDay);
            }
            return result;
        }

        private int _GetEndTimeInSeconds(int entryDaypartStartTimeAsSeconds)
        {
            var result = entryDaypartStartTimeAsSeconds + fiveMinutesAsSeconds;
            if (result >= twentyFourHoursInSeconds)
            {
                result = result - twentyFourHoursInSeconds;
            }
            return result;
        }
    }
}