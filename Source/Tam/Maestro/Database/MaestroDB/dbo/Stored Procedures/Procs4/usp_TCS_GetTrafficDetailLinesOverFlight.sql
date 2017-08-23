CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailLinesOverFlight]
	@tid INT
AS
BEGIN
	SELECT DISTINCT 
        traffic_details.id, 
        traffic_details.network_id, 
        networks.code, 
        traffic_details.daypart_id, 
        spot_lengths.length,
        traffic_detail_audiences.traffic_rating,
        (SELECT sum(traffic_details_proposal_details_map.proposal_rate) FROM traffic_details_proposal_details_map (NOLOCK) WHERE traffic_details_proposal_details_map.traffic_detail_id = traffic_details.id), 
        (SELECT sum(traffic_details_proposal_details_map.proposal_spots) FROM traffic_details_proposal_details_map (NOLOCK) WHERE traffic_details_proposal_details_map.traffic_detail_id = traffic_details.id), 
        proposals.id,
        proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor,
        proposal_detail_audiences.rating,
        MIN(traffic_detail_weeks.start_date),
        MAX(traffic_detail_weeks.end_date),
        (SELECT tda2.us_universe FROM traffic_detail_audiences (NOLOCK) as tda2, traffic_details (NOLOCK) as td2 WHERE td2.id = tda2.traffic_detail_id AND tda2.audience_id = 31 AND td2.id = traffic_details.id),
        traffic_detail_audiences.us_universe [DemoUniverse],
        (SELECT sum(proposal_details.proposal_rate) FROM proposal_details (NOLOCK) WHERE proposal_details.id = traffic_details_proposal_details_map.proposal_detail_id),
        traffic_details.daypart_id,
        traffic.status_id,
        ts.start_time,
        ts.end_time
	FROM 
        traffic (NOLOCK) 
        JOIN traffic_details (NOLOCK) ON traffic.id = traffic_details.traffic_id
        JOIN uvw_network_universe (NOLOCK) networks ON networks.network_id = traffic_details.network_id 
			AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
        JOIN spot_lengths (NOLOCK) ON spot_lengths.id = traffic_details.spot_length_id
        JOIN traffic_detail_weeks (NOLOCK) ON traffic_detail_weeks.traffic_detail_id = traffic_details.id
        JOIN traffic_detail_topographies (NOLOCK) ON traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
        JOIN traffic_detail_audiences (NOLOCK) ON traffic_detail_audiences.traffic_detail_id = traffic_details.id
        JOIN traffic_audiences (NOLOCK) ON traffic_audiences.traffic_id = traffic.id
        JOIN traffic_proposals (NOLOCK) ON traffic_proposals.traffic_id = traffic.id AND traffic_proposals.primary_proposal = 1
        JOIN proposals (NOLOCK) ON proposals.id = traffic_proposals.proposal_id 
        JOIN proposal_details (NOLOCK) ON proposal_details.proposal_id = proposals.id
        JOIN proposal_audiences (NOLOCK) ON proposal_audiences.proposal_id = proposals.id AND traffic.audience_id = proposal_audiences.audience_id
        JOIN proposal_detail_audiences (NOLOCK) ON proposal_detail_audiences.proposal_detail_id = proposal_details.id AND proposal_audiences.audience_id = proposal_detail_audiences.audience_id 
			AND traffic_audiences.audience_id = proposal_detail_audiences.audience_id AND traffic_detail_audiences.audience_id = proposal_detail_audiences.audience_id
        JOIN dayparts dp (NOLOCK) ON dp.id = traffic_detail_topographies.daypart_id
        JOIN timespans ts (NOLOCK) ON ts.id = dp.timespan_id
        RIGHT JOIN traffic_details_proposal_details_map (NOLOCK) ON traffic_details.id = traffic_details_proposal_details_map.traffic_detail_id 
			AND proposal_details.id = traffic_details_proposal_details_map.proposal_detail_id
	WHERE
		traffic_details.traffic_id = @tid
	GROUP BY
		traffic_details.id, 
		traffic_details.network_id, 
		networks.code, 
		traffic_details.daypart_id, 
		spot_lengths.length,
		traffic_detail_audiences.traffic_rating,
		proposals.id,
		proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor,
		proposal_detail_audiences.rating,
		traffic_detail_audiences.us_universe,
		traffic_details.daypart_id ,
		traffic_details_proposal_details_map.proposal_detail_id,
		traffic.status_id,
		ts.start_time,
		ts.end_time
	ORDER BY 
		networks.code, 
		MIN(traffic_detail_weeks.start_date)
END
