using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.BusinessEngines
{
    public interface ICreativeLengthEngine : IApplicationService
    {
        /// <summary>
        /// Distributes weightToDistribute to the creative length that have no weight set
        /// </summary>
        /// <param name="input">Creative lengths w/o weight.</param>
        /// <returns>List of creative lengths with distributed weight</returns>
        List<CreativeLength> DistributeWeight(IEnumerable<CreativeLength> input);

        /// <summary>
        /// Validates the creative lengths.
        /// </summary>
        /// <param name="creativeLengths">The creative lengths.</param>
        void ValidateCreativeLengths(List<CreativeLength> creativeLengths);

        /// <summary>
        /// Validates the creative lengths for plan save.
        /// </summary>
        /// <param name="creativeLengths">The creative lengths.</param>
        void ValidateCreativeLengthsForPlanSave(List<CreativeLength> creativeLengths);

        /// <summary>
        /// Sums the delivery multipliers.
        /// </summary>
        /// <param name="creativeLengths">The creative lengths.</param>
        /// <returns></returns>
        double CalculateDeliveryMultipliers(List<CreativeLength> creativeLengths);
    }

    public class CreativeLengthEngine : ICreativeLengthEngine
    {
        private readonly ISpotLengthEngine _SpotLengthEngine;

        const string INVALID_NUMBER_OF_CREATIVE_LENGTHS = "There should be at least 1 creative length selected on the plan";
        const string INVALID_CREATIVE_LENGTH_WEIGHT = "Creative length weight must be between 1 and 100";
        const string INVALID_CREATIVE_LENGTH_WEIGHT_TOTAL = "Sum Weight of all Creative Lengths must equal 100%";
        const string INVALID_SPOT_LENGTH = "Invalid spot length id {0}";

        public CreativeLengthEngine(ISpotLengthEngine spotLengthEngine)
        {
            _SpotLengthEngine = spotLengthEngine;
        }

        /// <inheritdoc/>
        public List<CreativeLength> DistributeWeight(IEnumerable<CreativeLength> input)
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

            var initialCreativeLengthsOrder = input
                .Select((item, index) => new { item, order = index + 1 })
                .ToDictionary(x => x.item.SpotLengthId, x => x.order);

            // to keep the original order
            return result
                .OrderBy(x => initialCreativeLengthsOrder[x.SpotLengthId])
                .ToList();
        }

        /// <inheritdoc/>
        public void ValidateCreativeLengths(List<CreativeLength> creativeLengths)
        {
            if (creativeLengths.Count < 1)
            {
                throw new ApplicationException(INVALID_NUMBER_OF_CREATIVE_LENGTHS);
            }
            creativeLengths.ForEach(creativeLength =>
            {
                if (!_SpotLengthEngine.SpotLengthIdExists(creativeLength.SpotLengthId))
                {
                    throw new ApplicationException(string.Format(INVALID_SPOT_LENGTH, creativeLength.SpotLengthId));
                }
            });
            //each creative length must be between 1 and 100
            creativeLengths.Where(x => x.Weight.HasValue).Select(x => x.Weight)
                .ToList()
                .ForEach(weight =>
                {
                    if (weight > 100 || weight < 1)
                    {
                        throw new ApplicationException(INVALID_CREATIVE_LENGTH_WEIGHT);
                    }
                });
        }

        /// <inheritdoc/>
        public void ValidateCreativeLengthsForPlanSave(List<CreativeLength> creativeLengths)
        {
            ValidateCreativeLengths(creativeLengths);
            if (creativeLengths.All(x => x.Weight.HasValue))
            {   //the sum of all creative lengths has to be 100 if all are set by the user
                if (creativeLengths.Sum(x => x.Weight.Value) != 100)
                {
                    throw new ApplicationException(INVALID_CREATIVE_LENGTH_WEIGHT_TOTAL);
                }
            }
            else
            {
                throw new ApplicationException(INVALID_CREATIVE_LENGTH_WEIGHT);
            }
        }

        /// <inheritdoc/>
        public double CalculateDeliveryMultipliers(List<CreativeLength> creativeLengths)
        {
            var spotLengthsMultiplier = _SpotLengthEngine.GetSpotLengthMultipliers();
            creativeLengths = DistributeWeight(creativeLengths);
            return creativeLengths.Sum(p => GeneralMath.ConvertPercentageToFraction(p.Weight.Value) * spotLengthsMultiplier[p.SpotLengthId]);

        }
    }
}
