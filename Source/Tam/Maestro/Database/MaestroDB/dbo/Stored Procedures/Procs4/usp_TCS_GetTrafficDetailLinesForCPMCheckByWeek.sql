CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailLinesForCPMCheckByWeek]
	@tid as int
AS
BEGIN
	select distinct 
      traffic_details.id, 
      traffic_details.network_id, 
      networks.code, 
      traffic_detail_topographies.daypart_id, 
      spot_lengths.length,
      traffic_detail_audiences.traffic_rating,
      traffic_details_proposal_details_map.proposal_rate ,
      traffic_details_proposal_details_map.proposal_spots ,
      proposals.id,
      proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor,
      proposal_detail_audiences.rating,
      traffic_detail_weeks.start_date,
      traffic_detail_weeks.end_date,
      (select tda2.us_universe from traffic_detail_audiences tda2 WITH (NOLOCK) join traffic_details td2 WITH (NOLOCK) ON td2.id = tda2.traffic_detail_id WHERE tda2.audience_id = 31 and td2.id = traffic_details.id),
      traffic_detail_audiences.us_universe [DemoUniverse],
      (select sum(proposal_details.proposal_rate) from proposal_details (NOLOCK) where proposal_details.id = traffic_details_proposal_details_map.proposal_detail_id),
      traffic.status_id,
	  ts.start_time
	from 
      traffic (NOLOCK) join traffic_details (NOLOCK) 
            on traffic.id = traffic_details.traffic_id
      join traffic_orders WITH (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
      join uvw_network_universe (NOLOCK) networks 
            on networks.network_id = traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
      join spot_lengths (NOLOCK) 
            on spot_lengths.id = traffic_details.spot_length_id
      join traffic_detail_weeks (NOLOCK)
            on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
      join traffic_detail_topographies (NOLOCK) 
            on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
      join traffic_detail_audiences (NOLOCK) 
            on traffic_detail_audiences.traffic_detail_id = traffic_details.id
      join traffic_audiences (NOLOCK) 
            on traffic_audiences.traffic_id = traffic.id
      join traffic_proposals (NOLOCK) 
            on traffic_proposals.traffic_id = traffic.id and traffic_proposals.primary_proposal = 1
      join proposals (NOLOCK) 
            on proposals.id = traffic_proposals.proposal_id 
      join proposal_details (NOLOCK) 
            on proposal_details.proposal_id = proposals.id
      join proposal_audiences (NOLOCK) 
            on proposal_audiences.proposal_id = proposals.id and traffic.audience_id = proposal_audiences.audience_id
      join proposal_detail_audiences (NOLOCK) 
            on proposal_detail_audiences.proposal_detail_id = proposal_details.id and proposal_audiences.audience_id = proposal_detail_audiences.audience_id 
            and traffic_audiences.audience_id = proposal_detail_audiences.audience_id and traffic_detail_audiences.audience_id = proposal_detail_audiences.audience_id
      join dayparts dp (NOLOCK)  on dp.id = traffic_detail_topographies.daypart_id 
      join timespans ts (NOLOCK)  on ts.id = dp.timespan_id
	  right join traffic_details_proposal_details_map (NOLOCK) 
      on traffic_details.id = traffic_details_proposal_details_map.traffic_detail_id and proposal_details.id = traffic_details_proposal_details_map.proposal_detail_id
	where
      traffic_details.traffic_id = @tid
      and traffic_detail_weeks.suspended = 0 
      and traffic_detail_topographies.spots > 0 and traffic_orders.ordered_spots > 0
      and traffic_orders.on_financial_reports = 1
        and traffic_orders.active = 1
	order by 
      ts.start_time desc,
	  networks.code, 
      traffic_detail_weeks.start_date, 
      traffic_detail_topographies.daypart_id
END
