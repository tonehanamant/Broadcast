using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.DTO.Program
{
    public class ProgramDto
    {
        public string Name { get; set; }

        public LookupDto Genre { get; set; }

        public string ContentRating { get; set; }
    }

    internal class ProgramEqualityComparer : IEqualityComparer<ProgramDto>
    {
        public bool Equals(ProgramDto x, ProgramDto y)
        {
            if (x is null || y is null)
            {
                return false;
            }

            if (x.Name.Equals(y.Name) == false)
            {
                return false;
            }

            if (x.Genre?.Display?.Equals(y.Genre?.Display) == false)
            {
                return false;
            }

            if (x.ContentRating?.Equals(y.ContentRating) == false)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(ProgramDto obj)
        {
            return $"{obj?.Name}|{obj?.Genre?.Display}|{obj?.ContentRating}".GetHashCode();
        }
    }
}
