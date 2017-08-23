
CREATE procedure usp_BS_GetProposalBusinessObject
(
	@proposal_id int
)
as
select 
	bp.advertiser_company_id,
	bp.agency_company_id,
	p.name ,
	bp.*
from broadcast_proposals bp with (NOLOCK)
left join products p with (NOLOCK) on p.id = bp.product_id
where
	bp.id = @proposal_id
