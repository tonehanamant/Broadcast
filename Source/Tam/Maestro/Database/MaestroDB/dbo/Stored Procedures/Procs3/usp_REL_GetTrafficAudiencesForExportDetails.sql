



CREATE Procedure [dbo].[usp_REL_GetTrafficAudiencesForExportDetails]
      (
            @id Int
      )
AS

select 
	traffic_audiences.audience_id, 
	traffic_audiences.universe, 
	traffic_detail_audiences.traffic_rating, 
	traffic_detail_audiences.us_universe, 
	proposal_detail_audiences.rating, 
	proposal_detail_audiences.us_universe, 
	proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor 
from 
traffic_audiences (NOLOCK) join 
traffic_detail_audiences (NOLOCK) on traffic_audiences.audience_id = traffic_detail_audiences.audience_id 
join traffic_details (NOLOCK) on traffic_details.id = traffic_detail_audiences.traffic_detail_id and traffic_details.traffic_id = traffic_audiences.traffic_id 
join traffic_details_proposal_details_map (NOLOCK) on traffic_details_proposal_details_map.traffic_detail_id = traffic_details.id 
join proposal_details (NOLOCK) on proposal_details.id = traffic_details_proposal_details_map.proposal_detail_id
join traffic_proposals (NOLOCK) on traffic_proposals.proposal_id = proposal_details.proposal_id and traffic_proposals.traffic_id = traffic_details.traffic_id and traffic_proposals.primary_proposal = 1 
join proposal_audiences (NOLOCK) on proposal_audiences.audience_id = traffic_audiences.audience_id and proposal_audiences.proposal_id = proposal_details.proposal_id 
join proposal_detail_audiences (NOLOCK) on proposal_audiences.audience_id = proposal_detail_audiences.audience_id and proposal_detail_audiences.proposal_detail_id = proposal_details.id 
where traffic_detail_audiences.traffic_detail_id = @id
order by traffic_audiences.ordinal ASC





