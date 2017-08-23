using IntegrationTests.Common;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests
{
    public class AttributeIgnorer : IJsonIgnorignator
    {
        public void SetupIgnoreFields(IgnorableSerializerContractResolver jsonResolver)
        {
            jsonResolver.Ignore(typeof(NewStationProgramDto), "Id");
            jsonResolver.Ignore(typeof(StationProgramAudienceRateDto), "Id");

            jsonResolver.Ignore(typeof(PostFile), "Id");
            jsonResolver.Ignore(typeof(PostFile), "UploadDate");
            jsonResolver.Ignore(typeof(PostFile), "ModifiedDate");

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