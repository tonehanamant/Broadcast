
CREATE procedure usp_BS_GetProposalDetailMarkets
(
	@proposal_detail_id int
)
as
select 
	d.name,
	bpdm.*
from
	broadcast_detail_markets bpdm with (NOLOCK) 
	left join dmas d with (NOLOCK) on (cast(d.code as int) - 400) = bpdm.market_code
where
	bpdm.broadcast_proposal_detail_id = @proposal_detail_id
order by
	bpdm.id

