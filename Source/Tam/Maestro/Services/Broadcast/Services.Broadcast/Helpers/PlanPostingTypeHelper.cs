using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Helpers
{
    public static class PlanPostingTypeHelper
    {
        public static IList<PlanPricingParametersDto> GetNtiAndNsiPricingParameters(PlanPricingParametersDto planPricingParameters, double ntiToNsiConversionRate)
        {
            var result = new List<PlanPricingParametersDto>();

            if (planPricingParameters != null)
            {
                var convertedParameters = planPricingParameters.DeepCloneUsingSerialization();

                //Add original parameters
                planPricingParameters.IsSelected = true;
                result.Add(planPricingParameters);

                //Convert parameters
                if (convertedParameters.PostingType == PostingTypeEnum.NSI)
                {
                    convertedParameters.DeliveryImpressions = Math.Floor(convertedParameters.DeliveryImpressions * ntiToNsiConversionRate);
                    convertedParameters.PostingType = PostingTypeEnum.NTI;
                }
                else if (convertedParameters.PostingType == PostingTypeEnum.NTI)
                {
                    convertedParameters.DeliveryImpressions = Math.Floor(convertedParameters.DeliveryImpressions / ntiToNsiConversionRate);
                    convertedParameters.PostingType = PostingTypeEnum.NSI;
                }

                if (convertedParameters.DeliveryImpressions != 0)
                {
                    convertedParameters.CPM = (decimal)((double)convertedParameters.Budget / convertedParameters.DeliveryImpressions);
                }
                else
                {
                    convertedParameters.CPM = 0;
                }

                result.Add(convertedParameters);
            }

            return result;
        }
    }
}
