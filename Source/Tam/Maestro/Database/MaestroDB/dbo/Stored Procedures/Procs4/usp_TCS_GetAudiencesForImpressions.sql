CREATE PROCEDURE [dbo].[usp_TCS_GetAudiencesForImpressions]
	@traffic_id as int
AS
BEGIN
	select 
		traffic_audiences.audience_id, 
		traffic_audiences.ordinal, 
		a.code,
		a.name,
		traffic_audiences.universe, 
		traffic.audience_id,
		spot_lengths.length
	from 
		traffic_audiences (NOLOCK)
		join traffic (NOLOCK) on traffic.id = traffic_audiences.traffic_id
		join traffic_proposals (NOLOCK) on traffic_proposals.traffic_id = traffic.id and traffic_proposals.primary_proposal = 1
		join proposals (NOLOCK) on proposals.id = traffic_proposals.proposal_id 
		join spot_lengths (NOLOCK) on spot_lengths.id = proposals.default_spot_length_id
		JOIN audiences a (NOLOCK) ON a.id=traffic_audiences.audience_id
	where 
		traffic_audiences.traffic_id = @traffic_id 
	order by 
		traffic_audiences.ordinal
END
