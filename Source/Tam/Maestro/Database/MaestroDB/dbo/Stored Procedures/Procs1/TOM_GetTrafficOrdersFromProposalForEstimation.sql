

 

 

 

 

CREATE PROCEDURE [dbo].[TOM_GetTrafficOrdersFromProposalForEstimation]

(

@pid as int

)

 

AS

 

select traffic.id from traffic (NOLOCK), traffic_proposals (NOLOCK) where traffic.status_id in (6, 5, 14) and traffic.id = traffic_proposals.traffic_id

and traffic_proposals.proposal_id = @pid


