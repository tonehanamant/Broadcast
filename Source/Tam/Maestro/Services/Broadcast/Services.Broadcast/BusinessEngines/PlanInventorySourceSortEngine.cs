using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public static class PlanInventorySourceSortEngine
    {
        private class OrderedString<T>
        {
            public int OrderBy { get; set; }
            public T OrderedValue { get; set; }
        }

        // ids are different between environments so must go off the name
        private static readonly OrderedString<string>[] _OrderedSources =
        {
            new OrderedString<string> { OrderBy = 1, OrderedValue = "ABC O&O"},
            new OrderedString<string> { OrderBy = 2, OrderedValue = "CNN"},
            new OrderedString<string> { OrderBy = 3, OrderedValue = "KATZ"},
            new OrderedString<string> { OrderBy = 4, OrderedValue = "LilaMax"},
            new OrderedString<string> { OrderBy = 5, OrderedValue = "NBC O&O"},
            new OrderedString<string> { OrderBy = 6, OrderedValue = "Sinclair"},
            new OrderedString<string> { OrderBy = 7, OrderedValue = "TVB"}
        };

        private static readonly OrderedString<InventorySourceTypeEnum>[] _OrderedSourceTypes =
        {
            new OrderedString<InventorySourceTypeEnum> { OrderBy = 1, OrderedValue = InventorySourceTypeEnum.Syndication},
            new OrderedString<InventorySourceTypeEnum> { OrderBy = 2, OrderedValue = InventorySourceTypeEnum.Diginet},
        };

        public static List<PlanInventorySourceDto> GetSortedInventorySourcePercents(int defaultPercent, List<InventorySource> allSources)
        {
            return PlanInventorySourceSortEngine._OrderedSources.OrderBy(sn => sn.OrderBy).Select(sn =>
                new PlanInventorySourceDto
                {
                    Id = allSources.Single(s => s.Name.Equals(sn.OrderedValue)).Id,
                    Name = sn.OrderedValue,
                    Percentage = defaultPercent
                }).ToList();
        }

        public static List<PlanInventorySourceTypeDto> GetSortedInventorySourceTypePercents(int defaultPercent)
        {
            return PlanInventorySourceSortEngine._OrderedSourceTypes.OrderBy(st => st.OrderBy).Select(st =>
                new PlanInventorySourceTypeDto
                {
                    Id = (int)st.OrderedValue,
                    Name = st.OrderedValue.GetDescriptionAttribute(),
                    Percentage = defaultPercent
                }).ToList();
        }

        public static List<PlanInventorySourceDto> SortInventorySourcePercents(List<PlanInventorySourceDto> toSort)
        {
            var sorted = new List<PlanInventorySourceDto>();

            foreach (var source in PlanInventorySourceSortEngine._OrderedSources.OrderBy(s => s.OrderBy))
            {
                var toAdd = toSort.SingleOrDefault(s => s.Name.Equals(source.OrderedValue));

                if (toAdd != null)
                {
                    sorted.Add(toAdd);
                }
            }

            // add unknown items in the end of the list
            var unknownItems = toSort.Except(sorted);
            sorted.AddRange(unknownItems);

            return sorted;
        }

        public static List<PlanInventorySourceTypeDto> SortInventorySourceTypePercents(List<PlanInventorySourceTypeDto> toSort)
        {
            var sorted = new List<PlanInventorySourceTypeDto>();

            foreach (var sourceType in PlanInventorySourceSortEngine._OrderedSourceTypes.OrderBy(s => s.OrderBy))
            {
                var sourceTypeDescription = sourceType.OrderedValue.GetDescriptionAttribute();
                var toAdd = toSort.SingleOrDefault(s => s.Name.Equals(sourceTypeDescription));

                if (toAdd != null)
                {
                    sorted.Add(toAdd);
                }
            }

            // add unknown items in the end of the list
            var unknownItems = toSort.Except(sorted);
            sorted.AddRange(unknownItems);

            return sorted;
        }
    }
}
