-- usp_TCS_GetTrafficBudgetByNetwork 34420
CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficBudgetByNetwork]
	@traffic_id INT
AS
BEGIN
	select 
		traffic_details.network_id, 
		networks.code, 
		sum(proposal_details.proposal_rate), 
		sum(traffic_details_proposal_details_map.proposal_spots), 
		traffic_details.daypart_id, 
		traffic_details.id 
	from traffic_details  (NOLOCK)
		join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
		join uvw_network_universe (NOLOCK) networks 
			on networks.network_id = traffic_details.network_id 
			AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
		join traffic_details_proposal_details_map (NOLOCK) on traffic_details.id = traffic_details_proposal_details_map.traffic_detail_id
		join proposal_details (NOLOCK) on proposal_details.id = traffic_details_proposal_details_map.proposal_detail_id
	where 
		traffic_details.traffic_id = @traffic_id
	group by 
		traffic_details.id, 
		traffic_details.network_id, 
		networks.code, 
		traffic_details.daypart_id
	order by 
		traffic_details.network_id, 
		traffic_details.daypart_id 
END
