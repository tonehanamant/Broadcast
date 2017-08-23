
CREATE PROCEDURE [dbo].[usp_REL_GetRollupTrafficDetails]
      @traffic_id as int
AS
 
SELECT 
	traffic_details.id, 
	max(network_maps.map_value), 
	spot_lengths.length, 
	traffic_details.daypart_id, 
	min(traffic_detail_weeks.start_date),
	max(traffic_detail_weeks.end_date),
	traffic_details_proposal_details_map.proposal_rate,
	proposal_details.num_spots,
	proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor,
	proposal_detail_audiences.rating,
	0,
	traffic_detail_audiences.us_universe [DemoUniverse],
	proposal_details.universal_scaling_factor,
	spot_lengths.delivery_multiplier
from 
	traffic (NOLOCK)
	join traffic_details (NOLOCK) on traffic.id = traffic_details.traffic_id
	join uvw_network_universe (NOLOCK) networks on networks.network_id = traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
	join uvw_networkmap_universe (NOLOCK) network_maps on traffic_details.network_id = network_maps.network_id AND (network_maps.start_date<=traffic.start_date AND (network_maps.end_date>=traffic.start_date OR network_maps.end_date IS NULL))
	join spot_lengths (NOLOCK) on spot_lengths.id = traffic_details.spot_length_id
	join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id
	join traffic_detail_topographies (NOLOCK)  on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
	join traffic_detail_audiences (NOLOCK) on traffic_detail_audiences.traffic_detail_id = traffic_details.id
	join traffic_audiences (NOLOCK) on traffic_audiences.traffic_id = traffic.id
	join traffic_proposals (NOLOCK) on traffic_proposals.traffic_id = traffic.id 
		and traffic_proposals.primary_proposal = 1
	join proposals (NOLOCK) on proposals.id = traffic_proposals.proposal_id 
	join proposal_details (NOLOCK) on proposal_details.proposal_id = proposals.id
	join proposal_audiences (NOLOCK) on proposal_audiences.proposal_id = proposals.id 
		and traffic.audience_id = proposal_audiences.audience_id
	join proposal_detail_audiences (NOLOCK) on proposal_detail_audiences.proposal_detail_id = proposal_details.id 
		and proposal_audiences.audience_id = proposal_detail_audiences.audience_id 
		and traffic_audiences.audience_id = proposal_detail_audiences.audience_id 
		and traffic_detail_audiences.audience_id = proposal_detail_audiences.audience_id
	right join traffic_details_proposal_details_map (NOLOCK) on traffic_details.id = traffic_details_proposal_details_map.traffic_detail_id 
		and proposal_details.id = traffic_details_proposal_details_map.proposal_detail_id
where
	traffic_details.traffic_id = @traffic_id 
	and network_maps.map_set = 'conimport'
GROUP BY 
	traffic_details.id, spot_lengths.length, 
	traffic_details.daypart_id,
	traffic_details_proposal_details_map.proposal_rate,
	proposal_details.num_spots,
	proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor,
	proposal_detail_audiences.rating, 
	traffic_detail_audiences.us_universe, 
	proposal_details.universal_scaling_factor, 
	traffic_details.network_id, 
	spot_lengths.delivery_multiplier
ORDER BY 
	traffic_details.network_id
	
