
using System;
using Services.Broadcast.Entities;

namespace Services.Broadcast.ApplicationServices
{
    public static class PropoeralsServiceHelper
    {
        public static int GetBookId(IHaveSingleSharedPostingBooks proposalDetail)
        {
            if (proposalDetail.ShareProjectionBookId.HasValue)
                return proposalDetail.ShareProjectionBookId.Value;

            if (proposalDetail.SingleProjectionBookId.HasValue)
                return proposalDetail.SingleProjectionBookId.Value;

            throw new Exception("Proposal detail is missing a valid ratings book");
        }
    }
}