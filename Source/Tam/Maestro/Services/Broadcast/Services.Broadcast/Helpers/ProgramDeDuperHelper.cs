using Services.Broadcast.Entities.DTO.Program;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Helpers
{
    public static class ProgramDeDuperHelper
    {
        public static List<ProgramDto> RemoveDuplicateProgramsByName(List<ProgramDto> candidates)
        {
            var results = candidates
                .Distinct(new ProgramEqualityComparer())
                .ToList();

            return results;
        }

        private sealed class ProgramEqualityComparer : IEqualityComparer<ProgramDto>
        {
            /***
             * BP-5366 
             *  The current use case is only for viewing in program restrictions.
             *  This bug changes that uniqueness to be by program name only, not case sensitive.
             ***/

            public bool Equals(ProgramDto x, ProgramDto y)
            {
                if (x is null || y is null)
                {
                    return false;
                }

                if (!x.Name.Trim().Equals(y.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                return true;
            }

            public int GetHashCode(ProgramDto obj)
            {
                return $"{obj?.Name.ToUpper().Trim()}".GetHashCode();
            }
        }
    }    
}