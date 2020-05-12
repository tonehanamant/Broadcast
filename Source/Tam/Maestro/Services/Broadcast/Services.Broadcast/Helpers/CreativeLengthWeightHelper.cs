using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Helpers
{
    public static class CreativeLengthWeightHelper
    {
        /// <summary>
        /// Distributes weightToDistribute to the creative length that have no weight set
        /// </summary>
        /// <param name="input">Creative lengths w/o weight.</param>
        /// <returns>List of creative lengths with distributed weight</returns>
        public static List<CreativeLength> DistributeWeight(IEnumerable<CreativeLength> input)
        {
            List<CreativeLength> result = new List<CreativeLength>();

            int weightToDistribute = 100;
            int withoutWeightCount = input.Count(x => !x.Weight.HasValue);
            int sumOfSetWeigh = input.Where(x => x.Weight.HasValue).Sum(x => x.Weight.Value);
            int undistributedWeight = weightToDistribute - sumOfSetWeigh;
            foreach (var creativeLength in input.Where(x => !x.Weight.HasValue))
            {
                result.Add(new CreativeLength
                {
                    SpotLengthId = creativeLength.SpotLengthId,
                    Weight = undistributedWeight / withoutWeightCount
                });
            }

            //if there is remaining weight, we allocate it to the first creative length
            int sumOfAllocatedWeight = result.Sum(x => x.Weight.Value) + sumOfSetWeigh;
            if (sumOfAllocatedWeight < weightToDistribute)
            {
                result.First().Weight += weightToDistribute - sumOfAllocatedWeight;
            }
            result.AddRange(input.Where(x => x.Weight.HasValue));

            return result;
        }
    }
}
