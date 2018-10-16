using Services.Broadcast.Entities.Enums;
using System.ComponentModel;
using System.Reflection;

namespace Services.Broadcast.Helpers
{
    public static class EnumHelper
    {
        public static string GetFileDetailProblemDescription(FileDetailProblemTypeEnum problemType)
        {
            switch (problemType)
            {
                case FileDetailProblemTypeEnum.UnlinkedIsci:
                    return "Not in system";
                case FileDetailProblemTypeEnum.UnmarriedOnMultipleContracts:
                    return "Multiple Proposals";
                case FileDetailProblemTypeEnum.MarriedAndUnmarried:
                    return "Married and Unmarried";
                case FileDetailProblemTypeEnum.UnmatchedSpotLength:
                    return "Unmatched Spot length";
                default:
                    return null;
            }
        }

        public static string GetDescriptionAttribute<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

    }
}
