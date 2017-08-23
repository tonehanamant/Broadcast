
CREATE VIEW uvw_analysis_mediaplan_traffic_ratings
(traffic_id, traffic_detail_id, product_name, network, spot_length, media_month_id, proposal_daypart_id, traffic_daypart_id, mp_hh_rating,
traffic_hh_rating, demo_audience_id, mp_demo_rating, traffic_demo_rating)
AS

select traffic.id, traffic_details.id, products.name, networks.code [Network], spot_lengths.length, media_months.id, proposal_details.daypart_id [Proposal_Daypart], 
traffic_details.daypart_id [Traffic_Daypart], pda.rating [MP_HH_Rating], 
tda.traffic_rating [TR_HH_Rating], pda2.audience_id [Demo_Audience_id] , pda2.rating [MP_DEMO_Rating], 
tda2.traffic_rating [TR_DEMO_Rating]
from 
	traffic (NOLOCK) 
	join traffic_details (NOLOCK) on traffic.id = traffic_details.traffic_id 
	join traffic_details_proposal_details_map tdpdm (NOLOCK) on tdpdm.traffic_detail_id = traffic_details.id 
	join proposal_details on proposal_details.id = tdpdm.proposal_detail_id 
	join proposal_detail_audiences (NOLOCK) pda on pda.proposal_detail_id = proposal_details.id 
	join traffic_detail_audiences (NOLOCK) tda on tda.traffic_detail_id = traffic_details.id and tda.audience_id = pda.audience_id and tda.audience_id = 31 
	join proposal_detail_audiences (NOLOCK) pda2 on pda2.proposal_detail_id = proposal_details.id 
	join traffic_detail_audiences (NOLOCK) tda2 on tda2.traffic_detail_id = traffic_details.id and tda2.audience_id = pda2.audience_id and tda2.audience_id <> 31
	join networks (NOLOCK) on networks.id = traffic_details.network_id 
	join spot_lengths (NOLOCK) on proposal_details.spot_length_id = spot_lengths.id
	join proposals (NOLOCK) on proposals.id = proposal_details.proposal_id 
	join media_months (NOLOCK) on 1=1 
	left join products (NOLOCK) on proposals.product_id = products.id 
where traffic.release_id is not null and 
(media_months.start_date between proposal_details.start_date and proposal_details.end_date OR 
media_months.end_date between proposal_details.start_date and proposal_details.end_date)
