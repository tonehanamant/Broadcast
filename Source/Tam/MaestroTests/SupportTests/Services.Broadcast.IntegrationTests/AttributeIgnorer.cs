using IntegrationTests.Common;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests
{
    public class AttributeIgnorer : IJsonIgnorignator
    {
        public void SetupIgnoreFields(IgnorableSerializerContractResolver jsonResolver)
        {
            jsonResolver.Ignore(typeof(NewStationProgramDto), "Id");

            jsonResolver.Ignore(typeof(PostPrePostingFile), "Id");
            jsonResolver.Ignore(typeof(PostPrePostingFile), "UploadDate");
            jsonResolver.Ignore(typeof(PostPrePostingFile), "ModifiedDate");

            jsonResolver.Ignore(typeof(PostFileDetail), "Id");
            jsonResolver.Ignore(typeof(PostFileDetail), "PostFileId");

            jsonResolver.Ignore(typeof(PostFileDetailImpression), "FileDetailId");

            jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailDaypartId");
            jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailSpotLengthId");
            jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "DetailId");
            jsonResolver.Ignore(typeof(ProposalDetailProprietaryInventoryDto), "ProposalId");
        }
    }
}