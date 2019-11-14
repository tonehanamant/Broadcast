using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.ProgramGuide;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProgramGuideService : IApplicationService
    {
        List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements, string queuedBy, bool simulate);
    }

    public class ProgramGuideService : IProgramGuideService
    {
        private IProgramGuideApiClient _ProgramGuideApiClient;
        private IProgramGuideApiClientSimulator _ProgramGuideApiClientSimulator;

        public ProgramGuideService(
            IProgramGuideApiClient programGuideApiClient,
            IProgramGuideApiClientSimulator programGuideApiClientSimulator)
        {
            _ProgramGuideApiClient = programGuideApiClient;
            _ProgramGuideApiClientSimulator = programGuideApiClientSimulator;
        }

        public List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements, string queuedBy, bool simulate = true)
        {
            return simulate ? _ProgramGuideApiClientSimulator.GetProgramsForGuide(requestElements) : _ProgramGuideApiClient.GetProgramsForGuide(requestElements);
        }
    }
}
