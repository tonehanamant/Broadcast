

CREATE PROCEDURE [dbo].[usp_REL_GetProposalProduct]
	@proposal_id int
AS
 
select 
	proposals.id, 
	proposals.original_proposal_id, 
	products.name 
from proposals (NOLOCK)
	left join products (NOLOCK) on products.id = proposals.product_id 
where 
	proposals.id = @proposal_id
