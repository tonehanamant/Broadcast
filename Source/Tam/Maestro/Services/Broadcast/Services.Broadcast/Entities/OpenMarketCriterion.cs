using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class OpenMarketCriterion
    {
        public OpenMarketCriterion()
        {
            ProgramNameSearchCriteria = new List<ProgramCriteria>();
            GenreSearchCriteria = new List<GenreCriteria>();
            CpmCriteria = new List<CpmCriteria>();
        }

        public List<CpmCriteria> CpmCriteria { get; set; }
        public List<ProgramCriteria> ProgramNameSearchCriteria { get; set; }
        public List<GenreCriteria> GenreSearchCriteria { get; set; }
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

    public enum ContainTypeEnum
    {
        Include = 1,
        Exclude = 2
    }

    public enum MinMaxEnum
    {
        Min = 1,
        Max = 2
    }
}
