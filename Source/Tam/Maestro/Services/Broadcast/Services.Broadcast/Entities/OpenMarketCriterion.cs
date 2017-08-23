using System.Collections.Generic;

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
        public string ProgramName { get; set; }
        public int? Id { get; set; }
    }

    public class GenreCriteria
    {
        public ContainTypeEnum Contain { get; set; }
        public int GenreId { get; set; }
        public int? Id { get; set; }
    }

    public class CpmCriteria
    {
        public MinMaxEnum MinMax { get; set; }
        public decimal Value { get; set; }
        public int? Id { get; set; }
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
