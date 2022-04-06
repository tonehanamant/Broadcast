using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Helpers
{
    /// <summary>
    /// Contains the calculations for converting impressions through Posting Types.
    /// </summary>
    public static class PostingTypeConversionHelper
    {
        /// <summary>
        /// Converts the given impressions from nsi to nti.
        /// </summary>
        /// <param name="impressions">The impressions.</param>
        /// <param name="conversionRate">The conversion rate.</param>
        /// <returns></returns>
        public static double ConvertImpressionsFromNsiToNti(double impressions, double conversionRate)
        {
            var result = impressions * conversionRate;
            return result;
        }

        /// <summary>
        /// Converts the given impressions from nti to nsi.
        /// </summary>
        /// <param name="impressions">The impressions.</param>
        /// <param name="conversionRate">The conversion rate.</param>
        /// <returns></returns>
        public static double ConvertImpressionsFromNtiToNsi(double impressions, double conversionRate)
        {
            var result = impressions / conversionRate;
            return result;
        }

        /// <summary>
        /// Converts the given impressions to 'the other' posting type.
        /// </summary>
        /// <param name="sourceImpressions">The source impressions.</param>
        /// <param name="sourcePostingType">Type of the source posting.</param>
        /// <param name="conversionRate">The conversion rate.</param>
        /// <returns></returns>
        public static double ConvertImpressions(double sourceImpressions, PostingTypeEnum sourcePostingType, double conversionRate)
        {
            double result;
            if (sourcePostingType == PostingTypeEnum.NSI)
            {
                result = ConvertImpressionsFromNsiToNti(sourceImpressions, conversionRate);
            }
            else
            {
                result = ConvertImpressionsFromNtiToNsi(sourceImpressions, conversionRate);
            }
            return result;
        }
    }
}
