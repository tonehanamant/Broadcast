
CREATE procedure usp_BS_GetProposalDetailMaxVersions
(
	@proposal_id int
)
as
select 
	bpd.original_broadcast_proposal_detail_id,
	max(bpd.revision)
from
	broadcast_proposal_details bpd with (NOLOCK) 
where
	bpd.broadcast_proposal_id = @proposal_id
GROUP BY
	bpd.original_broadcast_proposal_detail_id

