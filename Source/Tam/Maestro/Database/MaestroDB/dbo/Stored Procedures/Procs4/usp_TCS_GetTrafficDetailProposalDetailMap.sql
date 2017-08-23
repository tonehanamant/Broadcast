

CREATE Procedure [dbo].[usp_TCS_GetTrafficDetailProposalDetailMap]
      (
            @id Int
      )
AS

SELECT 
	id, 
	traffic_detail_id, 
	proposal_detail_id, 
	proposal_rate, 
	proposal_spots 
from 
	traffic_details_proposal_details_map (NOLOCK) 
where 
	traffic_detail_id = @id

 



