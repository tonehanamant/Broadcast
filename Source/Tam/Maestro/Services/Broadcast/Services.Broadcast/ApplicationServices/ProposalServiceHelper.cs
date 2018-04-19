
using System;
using Services.Broadcast.Entities;

namespace Services.Broadcast.ApplicationServices
{
    public static class PropoeralsServiceHelper
    {
        public static int GetBookId(IHaveSingleSharedPostingBooks proposalDetail)
        {
            if (proposalDetail.SharePostingBookId.HasValue)
                return proposalDetail.SharePostingBookId.Value;

            if (proposalDetail.SinglePostingBookId.HasValue)
                return proposalDetail.SinglePostingBookId.Value;

            throw new Exception("Proposal detail is missing a valid ratings book");
        }
    }
}