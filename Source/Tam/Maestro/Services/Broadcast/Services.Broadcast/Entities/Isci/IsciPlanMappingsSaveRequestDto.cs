using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanMappingsSaveRequestDto
    {
        public List<IsciPlanMappingDto> IsciPlanMappings { get; set; } = new List<IsciPlanMappingDto>();
        public List<int> IsciPlanMappingsDeleted { get; set; } = new List<int>();
        public List<IsciPlanModifiedMappingDto> IsciPlanMappingsModified { get; set; } = new List<IsciPlanModifiedMappingDto>();
        public List<IsciProductMappingDto> IsciProductMappings { get; set; } = new List<IsciProductMappingDto>();
    }

    public class IsciPlanMappingModifiedCountsDto
    {
        public int TotalChangedCount { get; set; }
        public int NoDuplicateCount { get; set; }
        public int DuplicateCount { get; set; }
    }
}
