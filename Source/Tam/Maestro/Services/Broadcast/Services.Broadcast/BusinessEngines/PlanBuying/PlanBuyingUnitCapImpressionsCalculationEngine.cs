﻿using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanBuyingUnitCapImpressionsCalculationEngine
    {
        double CalculateTotalImpressionsForUnitCaps(List<PlanBuyingInventoryProgram> inventory, PlanBuyingParametersDto parametersDto);
        double CalculateTotalImpressionsForUnitCaps(List<PlanBuyingBandStationDetailDto> inventory, PlanBuyingParametersDto parametersDto);
    }

    public class PlanBuyingUnitCapImpressionsCalculationEngine : IPlanBuyingUnitCapImpressionsCalculationEngine
    {
        public double CalculateTotalImpressionsForUnitCaps(List<PlanBuyingInventoryProgram> inventory, PlanBuyingParametersDto parametersDto)
        {
            var totalInventoryImpressions = 0d;

            var groupedByStation = inventory.GroupBy(x => x.Station.Id);

            foreach (var stationGroup in groupedByStation)
            {
                var programWithHighestImpressions = stationGroup.OrderByDescending(x => x.Impressions).FirstOrDefault();

                if (programWithHighestImpressions == null)
                    continue;

                var numberOfWeeks = programWithHighestImpressions.ManifestWeeks.Count;
                var numberOfMonths = numberOfWeeks / 4;
                numberOfMonths = numberOfMonths == 0 ? 1 : numberOfMonths;

                switch (parametersDto.UnitCapsType)
                {
                    case UnitCapEnum.PerMonth:
                        totalInventoryImpressions += numberOfMonths * 
                                parametersDto.UnitCaps * 
                                programWithHighestImpressions.Impressions;
                        break;
                    case UnitCapEnum.PerWeek:
                        totalInventoryImpressions += numberOfWeeks * 
                                parametersDto.UnitCaps * 
                                programWithHighestImpressions.Impressions;
                        break;
                    case UnitCapEnum.PerDay:
                        foreach (var manifestDaypart in programWithHighestImpressions.ManifestDayparts)
                        {
                            totalInventoryImpressions += numberOfWeeks * 
                                    manifestDaypart.Daypart.ActiveDays * 
                                    parametersDto.UnitCaps * 
                                    programWithHighestImpressions.Impressions;
                        }
                        break;
                    case UnitCapEnum.PerHour:
                        foreach (var manifestDaypart in programWithHighestImpressions.ManifestDayparts)
                        {
                            var hours = manifestDaypart.Daypart.Hours;
                            hours = hours == 0 ? 1 : hours;

                            totalInventoryImpressions += numberOfWeeks * 
                                    manifestDaypart.Daypart.ActiveDays * 
                                    hours * 
                                    parametersDto.UnitCaps *
                                    programWithHighestImpressions.Impressions;
                        }
                        break;
                    case UnitCapEnum.Per30Min:
                        foreach (var manifestDaypart in programWithHighestImpressions.ManifestDayparts)
                        {
                            const int per30MinMultiplier = 2;
                            var hours = manifestDaypart.Daypart.Hours;
                            hours = hours == 0 ? 1 : hours;

                            totalInventoryImpressions += per30MinMultiplier * 
                                numberOfWeeks *
                                manifestDaypart.Daypart.ActiveDays * 
                                hours * 
                                parametersDto.UnitCaps * 
                                programWithHighestImpressions.Impressions;
                        }
                        break;
                }
            }

            return totalInventoryImpressions;
        }

        public double CalculateTotalImpressionsForUnitCaps(List<PlanBuyingBandStationDetailDto> inventory, PlanBuyingParametersDto parametersDto)
        {
            var totalInventoryImpressions = 0d;

            var groupedByStation = inventory.GroupBy(x => x.StationId);

            foreach (var stationGroup in groupedByStation)
            {
                var programWithHighestImpressions = stationGroup.OrderByDescending(x => x.Impressions).FirstOrDefault();

                if (programWithHighestImpressions == null)
                    continue;

                var numberOfWeeks = programWithHighestImpressions.ManifestWeeksCount;
                var numberOfMonths = numberOfWeeks / 4;
                numberOfMonths = numberOfMonths == 0 ? 1 : numberOfMonths;

                switch (parametersDto.UnitCapsType)
                {
                    case UnitCapEnum.PerMonth:
                        totalInventoryImpressions += numberOfMonths *
                                parametersDto.UnitCaps *
                                programWithHighestImpressions.Impressions;
                        break;
                    case UnitCapEnum.PerWeek:
                        totalInventoryImpressions += numberOfWeeks *
                                parametersDto.UnitCaps *
                                programWithHighestImpressions.Impressions;
                        break;
                    case UnitCapEnum.PerDay:
                        foreach (var planBuyingBandInventoryStationDaypart in programWithHighestImpressions.PlanBuyingBandInventoryStationDayparts)
                        {
                            totalInventoryImpressions += numberOfWeeks *
                                    planBuyingBandInventoryStationDaypart.ActiveDays *
                                    parametersDto.UnitCaps *
                                    programWithHighestImpressions.Impressions;
                        }
                        break;
                    case UnitCapEnum.PerHour:
                        foreach (var planBuyingBandInventoryStationDaypart in programWithHighestImpressions.PlanBuyingBandInventoryStationDayparts)
                        {
                            var hours = planBuyingBandInventoryStationDaypart.Hours;
                            hours = hours == 0 ? 1 : hours;

                            totalInventoryImpressions += numberOfWeeks *
                                    planBuyingBandInventoryStationDaypart.ActiveDays *
                                    hours *
                                    parametersDto.UnitCaps *
                                    programWithHighestImpressions.Impressions;
                        }
                        break;
                    case UnitCapEnum.Per30Min:
                        foreach (var planBuyingBandInventoryStationDaypart in programWithHighestImpressions.PlanBuyingBandInventoryStationDayparts)
                        {
                            const int per30MinMultiplier = 2;
                            var hours = planBuyingBandInventoryStationDaypart.Hours;
                            hours = hours == 0 ? 1 : hours;

                            totalInventoryImpressions += per30MinMultiplier *
                                numberOfWeeks *
                                planBuyingBandInventoryStationDaypart.ActiveDays *
                                hours *
                                parametersDto.UnitCaps *
                                programWithHighestImpressions.Impressions;
                        }
                        break;
                }
            }

            return totalInventoryImpressions;
        }
    }
}
