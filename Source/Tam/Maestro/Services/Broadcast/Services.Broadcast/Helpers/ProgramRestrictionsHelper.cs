using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Extensions;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Helpers
{
    public static class ProgramRestrictionsHelper
    {
        /// <summary>
        /// This works only for OpenMarket. 
        /// When we start using other sources, the PlanBuyingInventoryProgram model structure should be reviewed
        /// Other sources may have more than 1 daypart that this logic does not assume
        /// </summary>
        public static List<T> FilterProgramsByDaypartAndSetStandardDaypart<T>(
            List<PlanDaypartDto> dayparts,
            List<T> programs,
            DisplayDaypart planDays,
            List<Day> cadentDayDefinitions,
            Dictionary<int, List<int>> daypartDefaultDayIds,
            int thresholdInSecondsForProgramIntersect
            ) where T : BasePlanInventoryProgram
        {
            var result = new List<T>();

            foreach (var program in programs)
            {
                var planDaypartsMatchedByTimeAndDays = _GetPlanDaypartsThatMatchProgramByTimeAndDays(dayparts, planDays, program, cadentDayDefinitions, daypartDefaultDayIds);
                if (planDaypartsMatchedByTimeAndDays.Count == 0)
                {
                    continue;
                }
                var planDaypartsMatchByRestrictions = _GetPlanDaypartsThatMatchProgramByRestrictions(planDaypartsMatchedByTimeAndDays, program);
                if (planDaypartsMatchByRestrictions.Count == 0)
                {
                    continue;
                }
                var planDaypartWithMostIntersectingTime = _FindPlanDaypartWithMostIntersectingTime(planDaypartsMatchByRestrictions, planDays, daypartDefaultDayIds, thresholdInSecondsForProgramIntersect);

                if (planDaypartWithMostIntersectingTime != null)
                {
                    program.StandardDaypartId = planDaypartWithMostIntersectingTime.PlanDaypart.DaypartCodeId;

                    result.Add(program);
                }
            }

            return result;
        }

        private static ProgramInventoryDaypart _FindPlanDaypartWithMostIntersectingTime<T>(List<T> programInventoryDayparts, DisplayDaypart planDays, Dictionary<int, 
            List<int>> daypartDefaultDayIds, int thresholdInSecondsForProgramIntersect)
            where T : ProgramInventoryDaypart
        {
            var planDaypartsWithIntersectingTime = programInventoryDayparts
                .Select(x => _CalculateProgramIntersectInfo(x, daypartDefaultDayIds, planDays))
                .ToList();

            var planDaypartWithmostIntersectingTime = planDaypartsWithIntersectingTime
                .Where(x => x.SingleIntersectionTime >= thresholdInSecondsForProgramIntersect)
                .OrderByDescending(x => x.TotalIntersectingTime)
                .Select(x => x.ProgramInventoryDaypart)
                .FirstOrDefault();

            return planDaypartWithmostIntersectingTime;
        }

        private static ProgramFlightDaypartIntersectInfo _CalculateProgramIntersectInfo(ProgramInventoryDaypart x, Dictionary<int, List<int>> daypartDefaultDayIds, DisplayDaypart planDays)
        {
            var planDaypartTimeRange = new TimeRange
            {
                StartTime = x.PlanDaypart.StartTimeSeconds,
                EndTime = x.PlanDaypart.EndTimeSeconds
            };

            var inventoryDaypartTimeRange = new TimeRange
            {
                StartTime = x.ManifestDaypart.Daypart.StartTime,
                EndTime = x.ManifestDaypart.Daypart.EndTime
            };

            var singleIntersectionTime = DaypartTimeHelper.GetIntersectingTotalTime(planDaypartTimeRange, inventoryDaypartTimeRange);

            var daypartDayIds = daypartDefaultDayIds[x.PlanDaypart.DaypartCodeId].Select(_ConvertCadentDayIdToSystemDayId).ToList();
            var flightDayIds = planDays.Days.Select(d => (int)d).ToList();
            var programDayIds = x.ManifestDaypart.Daypart.Days.Select(d => (int)d).ToList();
            var intersectingDayIds = programDayIds.Intersect(daypartDayIds).Intersect(flightDayIds).ToList();

            var totalIntersectingTime = singleIntersectionTime * intersectingDayIds.Count();

            var result = new ProgramFlightDaypartIntersectInfo
            {
                ProgramInventoryDaypart = x,
                SingleIntersectionTime = singleIntersectionTime,
                TotalIntersectingTime = totalIntersectingTime
            };

            return result;
        }

        private static int _ConvertCadentDayIdToSystemDayId(int systemDayId)
        {
            // Cadent day ids are 1-indexed; Mon=1;Sun=7
            if (systemDayId == 7)
            {
                return 0;
            }
            return systemDayId;
        }

        private static List<ProgramInventoryDaypart> _GetPlanDaypartsThatMatchProgramByTimeAndDays(
            List<PlanDaypartDto> planDayparts,
            DisplayDaypart planDisplayDaypart,
            BasePlanInventoryProgram program,
            List<Day> cadentDayDefinitions,
            Dictionary<int, List<int>> daypartDefaultDayIds)
        {
            var result = new List<ProgramInventoryDaypart>();

            foreach (var planDaypart in planDayparts)
            {
                var blendedFlightDaypart = _GetDisplayDaypartForPlanDaypart(planDaypart, planDisplayDaypart, cadentDayDefinitions, daypartDefaultDayIds);

                var programInventoryDayparts = program.ManifestDayparts
                    .Where(x => x.Daypart.Intersects(blendedFlightDaypart))
                    .Select(x => new ProgramInventoryDaypart
                    {
                        PlanDaypart = planDaypart,
                        ManifestDaypart = x
                    }).ToList();

                result.AddRange(programInventoryDayparts);
            }

            return result;
        }

        private static List<ProgramInventoryDaypart> _GetPlanDaypartsThatMatchProgramByRestrictions(List<ProgramInventoryDaypart> programInventoryDayparts, BasePlanInventoryProgram program)
        {
            var result = new List<ProgramInventoryDaypart>();

            foreach (var inventoryDaypart in programInventoryDayparts)
            {
                
                    var passGenreOrProgramRestrictions = _IsProgramAllowedByGenreOrProgramRestrictions(inventoryDaypart);
                    if (!passGenreOrProgramRestrictions)
                    {
                        continue;
                    }

                if (!_IsProgramAllowedByShowTypeRestrictions(inventoryDaypart))
                    continue;

                if (!_IsProgramAllowedByAffiliateRestrictions(inventoryDaypart, program))
                    continue;

                result.Add(inventoryDaypart);
            }

            return result;
        }

        private static bool _IsProgramAllowedByAffiliateRestrictions(ProgramInventoryDaypart programInventoryDaypart, BasePlanInventoryProgram program)
        {
            var affiliateRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.AffiliateRestrictions;

            if (affiliateRestrictions == null || affiliateRestrictions.Affiliates.IsEmpty())
                return true;

            var restrictedAffiliates = affiliateRestrictions.Affiliates.Select(x => x.Display);
            bool hasIntersections;
            if (restrictedAffiliates.Any(x => x.Equals("IND")) && program.Station.Affiliation.Equals("IND"))
            {
                hasIntersections = program.Station.IsTrueInd == true;
            }
            else
            {
                hasIntersections = restrictedAffiliates.Contains(program.Station.Affiliation);
            }

            return affiliateRestrictions.ContainType == ContainTypeEnum.Include ?
                hasIntersections :
                !hasIntersections;
        }

        private static bool _IsProgramAllowedByShowTypeRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var showTypeRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.ShowTypeRestrictions;

            if (showTypeRestrictions == null || showTypeRestrictions.ShowTypes.IsEmpty())
            {
                return true;
            }

            var restrictedShowTypes = showTypeRestrictions.ShowTypes.Select(x => x.Display);
            var primaryProgramShowType = programInventoryDaypart.ManifestDaypart.PrimaryProgram.ShowType;
            var isInTheList = restrictedShowTypes.Contains(primaryProgramShowType);

            if (showTypeRestrictions.ContainType == ContainTypeEnum.Include)
            {
                return isInTheList;
            }

            return isInTheList == false;
        }

        private static bool _IsProgramAllowedByGenreOrProgramRestrictions(ProgramInventoryDaypart inventoryDaypart)
        {
            /*** if we don't have one of them then just resolve the other ***/
            var programInspectionResult = _InspectProgramRestriction(inventoryDaypart);
            if (!programInspectionResult.RestrictionExists)
            {
                var result = _IsProgramAllowedByGenreRestrictions(inventoryDaypart);
                return result;
            }

            var genreInspectionResult = _InspectGenreRestriction(inventoryDaypart);
            if (!genreInspectionResult.RestrictionExists)
            {
                var result = _IsProgramAllowedByProgramRestrictions(inventoryDaypart);
                return result;
            }

            /*** Use cases are from BP-3250 ***/
            
            // use case 1
            if (programInspectionResult.ContainType == ContainTypeEnum.Include
                && genreInspectionResult.ContainType == ContainTypeEnum.Exclude)
            {
                return programInspectionResult.IsInList;
            }

            // use case 2
            if (programInspectionResult.ContainType == ContainTypeEnum.Include
                && genreInspectionResult.ContainType == ContainTypeEnum.Include)
            {
                if (programInspectionResult.IsInList)
                {
                    return true;
                }

                return genreInspectionResult.IsInList;
            }

            // use case 3
            if (programInspectionResult.ContainType == ContainTypeEnum.Exclude
                && genreInspectionResult.ContainType == ContainTypeEnum.Include)
            {
                if (programInspectionResult.IsInList)
                {
                    return false;
                }

                return genreInspectionResult.IsInList;
            }

            // use case 4
            if (programInspectionResult.ContainType == ContainTypeEnum.Exclude
                && genreInspectionResult.ContainType == ContainTypeEnum.Exclude)
            {
                if (programInspectionResult.IsInList)
                {
                    return false;
                }

                if (genreInspectionResult.IsInList)
                {
                    return false;
                }
            }

            return true;
        }

        private class RestrictionInspectionResult
        {
            public bool RestrictionExists { get; set; }
            public bool IsInList { get; set; }
            public ContainTypeEnum ContainType { get; set; }
        }

        private static RestrictionInspectionResult _InspectProgramRestriction(ProgramInventoryDaypart inventoryDaypart)
        {
            var restrictions = inventoryDaypart.PlanDaypart.Restrictions.ProgramRestrictions;
            if (restrictions == null || restrictions.Programs.IsEmpty())
            {
                return new RestrictionInspectionResult { RestrictionExists = false };
            }

            var restrictedProgramNames = restrictions.Programs.Select(x => x.Name.ToLowerInvariant()).ToList();
            var primaryProgramName = inventoryDaypart.ManifestDaypart.PrimaryProgram.Name.ToLowerInvariant();
            var isInTheList = restrictedProgramNames.Contains(primaryProgramName);

            var result = new RestrictionInspectionResult
            {
                RestrictionExists = true,
                IsInList = isInTheList,
                ContainType = restrictions.ContainType
            };
            return result;
        }

        private static RestrictionInspectionResult _InspectGenreRestriction(ProgramInventoryDaypart inventoryDaypart)
        {
            var restrictions = inventoryDaypart.PlanDaypart.Restrictions.GenreRestrictions;
            if (restrictions == null || restrictions.Genres.IsEmpty())
            {
                return new RestrictionInspectionResult { RestrictionExists = false };
            }

            var restrictedGenres = restrictions.Genres.Select(x => x.Display);
            var primaryProgramGenre = inventoryDaypart.ManifestDaypart.PrimaryProgram.Genre;
            var isInTheList = restrictedGenres.Contains(primaryProgramGenre);

            var result = new RestrictionInspectionResult
            {
                RestrictionExists = true,
                IsInList = isInTheList,
                ContainType = restrictions.ContainType
            };
            return result;
        }

        private static bool _IsProgramAllowedByProgramRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var programRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.ProgramRestrictions;

            if (programRestrictions == null || programRestrictions.Programs.IsEmpty())
            {
                return true;
            }

            var restrictedProgramNames = programRestrictions.Programs.Select(x => x.Name.ToLowerInvariant()).ToList();
            var primaryProgramName = programInventoryDaypart.ManifestDaypart.PrimaryProgram.Name.ToLowerInvariant();
            var isInTheList = restrictedProgramNames.Contains(primaryProgramName);

            if (programRestrictions.ContainType == ContainTypeEnum.Include)
            {
                return isInTheList;
            }

            return isInTheList == false;
        }

        private static bool _IsProgramAllowedByGenreRestrictions(ProgramInventoryDaypart programInventoryDaypart)
        {
            var genreRestrictions = programInventoryDaypart.PlanDaypart.Restrictions.GenreRestrictions;

            if (genreRestrictions == null || genreRestrictions.Genres.IsEmpty())
            {
                return true;
            }

            var restrictedGenres = genreRestrictions.Genres.Select(x => x.Display);
            var primaryProgramGenre = programInventoryDaypart.ManifestDaypart.PrimaryProgram.Genre;
            var isInTheList = restrictedGenres.Contains(primaryProgramGenre);

            if (genreRestrictions.ContainType == ContainTypeEnum.Include)
            {
                return isInTheList;
            }

            return isInTheList == false;
        }

        private static DisplayDaypart _GetDisplayDaypartForPlanDaypart(PlanDaypartDto planDaypart, DisplayDaypart planFlightDays, 
            List<Day> cadentDayDefinitions, Dictionary<int, List<int>> daypartDefaultDayIds)
        {
            var planDaypartDayIds = daypartDefaultDayIds[planDaypart.DaypartCodeId];
            var coveredDayNamesSet = _GetCoveredDayNamesHashSet(planDaypartDayIds, cadentDayDefinitions);

            var blendedFlightDaypart = new DisplayDaypart
            {
                Monday = planFlightDays.Monday && coveredDayNamesSet.Contains("Monday"),
                Tuesday = planFlightDays.Tuesday && coveredDayNamesSet.Contains("Tuesday"),
                Wednesday = planFlightDays.Wednesday && coveredDayNamesSet.Contains("Wednesday"),
                Thursday = planFlightDays.Thursday && coveredDayNamesSet.Contains("Thursday"),
                Friday = planFlightDays.Friday && coveredDayNamesSet.Contains("Friday"),
                Saturday = planFlightDays.Saturday && coveredDayNamesSet.Contains("Saturday"),
                Sunday = planFlightDays.Sunday && coveredDayNamesSet.Contains("Sunday"),
                StartTime = planDaypart.StartTimeSeconds,
                EndTime = planDaypart.EndTimeSeconds
            };

            return blendedFlightDaypart;
        }

        private static HashSet<string> _GetCoveredDayNamesHashSet(List<int> planDaypartDayIds, List<Day> cadentDayDefinitions)
        {
            var coveredDayNames = cadentDayDefinitions
                .Where(x => planDaypartDayIds.Contains(x.Id))
                .Select(x => x.Name);
            var coveredDayNamesHashSet = new HashSet<string>(coveredDayNames);
            return coveredDayNamesHashSet;
        }

        /// <summary>
        /// The method is used to filter PricingInventoryProgram list based on genres
        /// </summary>
        /// <param name="allPrograms"></param>
        /// <returns>The method will return filtered PricingInventoryProgram list</returns>
        public static List<PlanPricingInventoryProgram> ApplyGeneralFilterForPricingPrograms(List<PlanPricingInventoryProgram> allPrograms)
        {
            List<PlanPricingInventoryProgram> filteredPrograms = new List<PlanPricingInventoryProgram>();
            foreach (var program in allPrograms)
            {
                if (program.ManifestDayparts.SelectMany(d => d.Programs).Any(prgrm => prgrm.Genre != "Unmatched"))
                {
                    filteredPrograms.Add(program);
                }
            }
            return filteredPrograms;
        }

        /// <summary>
        /// The method is used to filter BuyingInventoryProgram list based on genres
        /// </summary>
        /// <param name="allPrograms"></param>
        /// <returns>The method will return filtered BuyingInventoryProgram list</returns>
        public static List<PlanBuyingInventoryProgram> ApplyGeneralFilterForBuyingPrograms(List<PlanBuyingInventoryProgram> allPrograms)
        {
            List<PlanBuyingInventoryProgram> filteredPrograms = new List<PlanBuyingInventoryProgram>();
            foreach (var program in allPrograms)
            {
                if (program.ManifestDayparts.SelectMany(d => d.Programs).Any(prgrm => prgrm.Genre != "Unmatched"))
                {
                    filteredPrograms.Add(program);
                }
            }
            return filteredPrograms;
        }
    }
}