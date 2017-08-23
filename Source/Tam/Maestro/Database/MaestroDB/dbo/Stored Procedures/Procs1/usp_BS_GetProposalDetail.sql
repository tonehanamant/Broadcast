
CREATE procedure usp_BS_GetProposalDetail
(
	@proposal_detail_id int,
	@version int
)
as
select 
	bpd.*
from
	broadcast_proposal_details bpd with (NOLOCK) 
where
	bpd.original_broadcast_proposal_detail_id = @proposal_detail_id
	and
	bpd.revision = @version

