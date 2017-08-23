
CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficOrdersFromProposal]
(
	@pid as int
)

AS

with uvw_traffic(id, original_traffic_id, revision)
 as 
(
select 
	id, 
	isnull(original_traffic_id, id) original_traffic_id,
	revision 
from 
	traffic (NOLOCK) 
where 
	status_id not in (24, 25)
)
SELECT DISTINCT 
	max(traffic.id), 
	max(traffic.revision),	
	traffic.original_traffic_id, 
	traffic_proposals.proposal_id
FROM uvw_traffic traffic join
	 traffic_proposals (NOLOCK) on traffic.id = traffic_proposals.traffic_id
WHERE
	 traffic_proposals.proposal_id = @pid
group by 
	traffic.original_traffic_id, 
	traffic_proposals.proposal_id
order by 
	traffic.original_traffic_id
