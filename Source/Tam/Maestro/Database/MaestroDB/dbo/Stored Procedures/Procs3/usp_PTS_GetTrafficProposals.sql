
CREATE PROCEDURE [dbo].[usp_PTS_GetTrafficProposals]
(
@tid as int
)
AS

SELECT 
	traffic_proposals.traffic_id, 
	traffic_proposals.proposal_id, 
	traffic_proposals.primary_proposal
from 
	traffic_proposals (NOLOCK)
where 
	traffic_proposals.traffic_id = @tid

