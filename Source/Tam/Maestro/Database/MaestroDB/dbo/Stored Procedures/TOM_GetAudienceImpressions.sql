


CREATE PROCEDURE [dbo].[TOM_GetAudienceImpressions]
(
            @traffic_id as int,
            @audience_id as int
)

AS

select distinct traffic_details.network_id, traffic_detail_audiences.audience_id, traffic_detail_audiences.traffic_rating, proposal_detail_audiences.rating, 
traffic_details_proposal_details_map.proposal_spots, 
traffic_details.id, proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor, traffic_detail_topographies.start_date, 
(select tda2.us_universe from traffic_detail_audiences (NOLOCK) as tda2, traffic_details as td2 where td2.id = tda2.traffic_detail_id and tda2.audience_id = 31 and td2.id = traffic_details.id),
traffic_detail_audiences.us_universe, traffic_details.daypart_id, traffic_details.id, traffic_details_proposal_details_map.proposal_rate,
proposal_details.proposal_rate
 from traffic_detail_audiences (NOLOCK) join traffic_details (NOLOCK) on traffic_detail_audiences.traffic_detail_id = traffic_details.id 
join traffic_detail_topographies (NOLOCK) on traffic_detail_topographies.traffic_detail_id = traffic_details.id 
join traffic_audiences (NOLOCK) on traffic_audiences.audience_id = traffic_detail_audiences.audience_id and traffic_audiences.traffic_id = traffic_details.traffic_id
left join traffic_details_proposal_details_map (NOLOCK) on traffic_details_proposal_details_map.traffic_detail_id = traffic_details.id 
LEFT JOIN proposal_detail_audiences (NOLOCK) ON 
proposal_detail_audiences.audience_id=traffic_detail_audiences.audience_id AND proposal_detail_audiences.proposal_detail_id=traffic_details_proposal_details_map.proposal_detail_id 
LEFT JOIN proposal_details (NOLOCK) ON proposal_details.id=proposal_detail_audiences.proposal_detail_id 
where 
traffic_details.traffic_id = @traffic_id and traffic_detail_audiences.audience_id = @audience_id
GROUP BY traffic_details.network_id, proposal_details.proposal_id, traffic_detail_audiences.audience_id, traffic_detail_audiences.traffic_rating, proposal_detail_audiences.rating, 
traffic_details_proposal_details_map.proposal_spots, traffic_details.id, proposal_detail_audiences.us_universe, proposal_details.universal_scaling_factor, traffic_detail_topographies.start_date, 
traffic_audiences.universe, traffic_detail_audiences.us_universe, traffic_details.daypart_id, traffic_details_proposal_details_map.proposal_rate, proposal_details.proposal_rate