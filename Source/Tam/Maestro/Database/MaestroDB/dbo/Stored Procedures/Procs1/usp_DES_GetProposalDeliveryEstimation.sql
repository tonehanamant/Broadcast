
CREATE PROCEDURE [dbo].[usp_DES_GetProposalDeliveryEstimation]
(
            @proposal_id as int,
            @audience_id as int
)
 
AS
 
select distinct 
	proposal_details.network_id, proposal_detail_audiences.audience_id, proposal_detail_audiences.rating, 
	proposal_details.num_spots, proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor
from 
	proposal_details (NOLOCK)
	JOIN proposal_detail_audiences (NOLOCK) ON 
		proposal_details.id=proposal_detail_audiences.proposal_detail_id
where 
	proposal_details.proposal_id = @proposal_id 
	and 
	proposal_detail_audiences.audience_id = @audience_id
order by 
	proposal_details.network_id, proposal_detail_audiences.audience_id;
