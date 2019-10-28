using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class OpenMarketCriterion
    {
        public List<CpmCriteria> CpmCriteria { get; set; } = new List<CpmCriteria>();
        public List<ProgramCriteria> ProgramNameSearchCriteria { get; set; } = new List<ProgramCriteria>();
        public List<GenreCriteria> GenreSearchCriteria { get; set; } = new List<GenreCriteria>();
    }

    public class ProgramCriteria
    {
        public ContainTypeEnum Contain { get; set; }
        public int? Id { get; set; }
        public LookupDto Program { get; set; }
    }

    public class GenreCriteria
    {
        public ContainTypeEnum Contain { get; set; }
        public int? Id { get; set; }
        public LookupDto Genre { get; set; }
    }
    
    public class CpmCriteria
    {
        public MinMaxEnum MinMax { get; set; }
        public decimal Value { get; set; }
        public int? Id { get; set; }
    }

    public class ShowTypeCriteria
    {
        public ContainTypeEnum Contain { get; set; }
        public int? Id { get; set; }
        public LookupDto ShowType { get; set; }
    }

    public enum MinMaxEnum
    {
        Min = 1,
        Max = 2
    }
}
