

CREATE Procedure [dbo].[usp_REL_GetTrafficSpotsBySystem]
      (
            @traffic_id int,
			@system_id int
      )

AS

select 
	networks.code, 
	traffic_orders.display_network_id, 
	traffic_orders.daypart_id, 
	traffic_orders.traffic_detail_id,
	avg(traffic_orders.ordered_spots), 
	traffic_orders.start_date, 
	traffic_orders.end_date,
	sum(traffic_orders.ordered_spot_rate) 
from
	traffic_orders (NOLOCK) 
	join traffic_details  (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
	join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
	join uvw_network_universe (NOLOCK) networks on networks.network_id = traffic_orders.display_network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
where 
	traffic_details.traffic_id = @traffic_id and traffic_orders.system_id = @system_id 
	and traffic_detail_weeks.suspended = 0
	and traffic_orders.active = 1
group by
	networks.code, 
	traffic_orders.display_network_id, 
	traffic_orders.daypart_id, 
	traffic_orders.traffic_detail_id,
	traffic_orders.start_date, 
	traffic_orders.end_date
order by 
	networks.code, 
	traffic_orders.start_date, 
	traffic_orders.daypart_id

