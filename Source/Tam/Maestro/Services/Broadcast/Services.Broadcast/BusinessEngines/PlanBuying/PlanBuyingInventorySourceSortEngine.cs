using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.BusinessEngines
{
    public static class PlanBuyingInventorySourceSortEngine
    {
        private class OrderedString<T>
        {
            public int OrderBy { get; set; }
            public T OrderedValue { get; set; }
        }

        // ids are different between environments so must go off the name
        private static readonly OrderedString<string>[] _OrderedBuyingSources =
        {
            new OrderedString<string> { OrderBy = 1, OrderedValue = "ABC O&O"},
            new OrderedString<string> { OrderBy = 2, OrderedValue = "CNN"},
            new OrderedString<string> { OrderBy = 3, OrderedValue = "KATZ"},
            new OrderedString<string> { OrderBy = 4, OrderedValue = "LilaMax"},
            new OrderedString<string> { OrderBy = 5, OrderedValue = "NBC O&O"},
            new OrderedString<string> { OrderBy = 6, OrderedValue = "Sinclair"},
            new OrderedString<string> { OrderBy = 7, OrderedValue = "TVB"}
        };

        private static readonly OrderedString<InventorySourceTypeEnum>[] _OrderedBuyingSourceTypes =
        {
            new OrderedString<InventorySourceTypeEnum> { OrderBy = 1, OrderedValue = InventorySourceTypeEnum.Syndication},
            new OrderedString<InventorySourceTypeEnum> { OrderBy = 2, OrderedValue = InventorySourceTypeEnum.Diginet},
        };

        public static List<PlanBuyingInventorySourceDto> GetSortedInventorySourcePercents(int defaultPercent, List<InventorySource> allSources)
        {
            return _OrderedBuyingSources.OrderBy(sn => sn.OrderBy).Select(sn =>
                new PlanBuyingInventorySourceDto
                {
                    Id = allSources.Single(s => s.Name.Equals(sn.OrderedValue)).Id,
                    Name = sn.OrderedValue,
                    Percentage = defaultPercent
                }).ToList();
        }

        public static List<PlanBuyingInventorySourceTypeDto> GetSortedInventorySourceTypePercents(int defaultPercent)
        {
            return _OrderedBuyingSourceTypes.OrderBy(st => st.OrderBy).Select(st =>
                new PlanBuyingInventorySourceTypeDto
                {
                    Id = (int)st.OrderedValue,
                    Name = st.OrderedValue.GetDescriptionAttribute(),
                    Percentage = defaultPercent
                }).ToList();
        }

        public static List<PlanBuyingInventorySourceDto> SortInventorySourcePercents(List<PlanBuyingInventorySourceDto> toSort)
        {
            var sorted = new List<PlanBuyingInventorySourceDto>();

            foreach (var source in _OrderedBuyingSources.OrderBy(s => s.OrderBy))
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

        public static List<PlanBuyingInventorySourceDto> SortInventorySourceTypePercents(List<PlanBuyingInventorySourceDto> toSort)
        {
            var sorted = new List<PlanBuyingInventorySourceDto>();

            foreach (var sourceType in _OrderedBuyingSourceTypes.OrderBy(s => s.OrderBy))
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