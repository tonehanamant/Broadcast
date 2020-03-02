using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.BusinessEngines
{
    public static class PlanPricingInventorySourceSortEngine
    {
        private class OrderedString<T>
        {
            public int OrderBy { get; set; }
            public T OrderedValue { get; set; }
        }

        // ids are different between environments so must go off the name
        private static readonly OrderedString<string>[] _OrderedPricingSources =
        {
            new OrderedString<string> { OrderBy = 1, OrderedValue = "ABC O&O"},
            new OrderedString<string> { OrderBy = 2, OrderedValue = "CNN"},
            new OrderedString<string> { OrderBy = 3, OrderedValue = "KATZ"},
            new OrderedString<string> { OrderBy = 4, OrderedValue = "LilaMax"},
            new OrderedString<string> { OrderBy = 5, OrderedValue = "NBC O&O"},
            new OrderedString<string> { OrderBy = 6, OrderedValue = "Sinclair"},
            new OrderedString<string> { OrderBy = 7, OrderedValue = "TVB"}
        };

        private static readonly OrderedString<InventorySourceTypeEnum>[] _OrderedPricingSourceTypes =
        {
            new OrderedString<InventorySourceTypeEnum> { OrderBy = 1, OrderedValue = InventorySourceTypeEnum.Syndication},
            new OrderedString<InventorySourceTypeEnum> { OrderBy = 2, OrderedValue = InventorySourceTypeEnum.Diginet},
        };

        public static List<PlanPricingInventorySourceDto> GetSortedInventorySourcePercents(int defaultPercent, List<InventorySource> allSources)
        {
            return _OrderedPricingSources.OrderBy(sn => sn.OrderBy).Select(sn =>
                new PlanPricingInventorySourceDto
                {
                    Id = allSources.Single(s => s.Name.Equals(sn.OrderedValue)).Id,
                    Name = sn.OrderedValue,
                    Percentage = defaultPercent
                }).ToList();
        }

        public static List<PlanPricingInventorySourceTypeDto> GetSortedInventorySourceTypePercents(int defaultPercent)
        {
            return _OrderedPricingSourceTypes.OrderBy(st => st.OrderBy).Select(st =>
                new PlanPricingInventorySourceTypeDto
                {
                    Id = (int)st.OrderedValue,
                    Name = st.OrderedValue.GetDescriptionAttribute(),
                    Percentage = defaultPercent
                }).ToList();
        }

        public static List<PlanPricingInventorySourceDto> SortInventorySourcePercents(List<PlanPricingInventorySourceDto> toSort)
        {
            var sorted = new List<PlanPricingInventorySourceDto>();

            foreach (var source in _OrderedPricingSources.OrderBy(s => s.OrderBy))
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

        public static List<PlanPricingInventorySourceTypeDto> SortInventorySourceTypePercents(List<PlanPricingInventorySourceTypeDto> toSort)
        {
            var sorted = new List<PlanPricingInventorySourceTypeDto>();

            foreach (var sourceType in _OrderedPricingSourceTypes.OrderBy(s => s.OrderBy))
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