using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsRequestPackage
    {
        public GenreSourceEnum GenreSource => GenreSourceEnum.Dativa;
        public DateTime StartDateRange { get; set; }
        public DateTime EndDateRange { get; set; }
        public List<GuideRequestResponseMapping> RequestMappings { get; set; } = new List<GuideRequestResponseMapping>();
        public List<GuideRequestElementDto> RequestElements { get; set; } = new List<GuideRequestElementDto>();
        public List<StationInventoryManifest> InventoryManifests { get; set; } = new List<StationInventoryManifest>();
    }
}