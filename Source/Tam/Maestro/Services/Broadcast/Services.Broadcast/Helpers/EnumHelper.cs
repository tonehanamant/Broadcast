using Services.Broadcast.Entities.Enums;

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

    }
}
