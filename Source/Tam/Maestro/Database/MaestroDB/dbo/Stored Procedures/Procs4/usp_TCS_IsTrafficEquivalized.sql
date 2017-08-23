
CREATE PROCEDURE [dbo].[usp_TCS_IsTrafficEquivalized]
      @traffic_id as int
AS

select proposals.is_equivalized from proposals (NOLOCK) 
join traffic_proposals (NOLOCK) on proposals.id = traffic_proposals.proposal_id and traffic_proposals.primary_proposal = 1
where traffic_proposals.traffic_id = @traffic_id

