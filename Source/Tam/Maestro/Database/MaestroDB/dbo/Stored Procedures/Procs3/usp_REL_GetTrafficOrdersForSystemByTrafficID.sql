
CREATE Procedure [dbo].[usp_REL_GetTrafficOrdersForSystemByTrafficID]
      (
            @traffic_id int,
			@system_id int
	  )

AS

select traffic_orders.id, 
		traffic_orders.system_id, 
		traffic_orders.zone_id, 
		traffic_orders.traffic_detail_id, 
		traffic_orders.daypart_id, 
		traffic_orders.ordered_spots,
		traffic_orders.ordered_spot_rate, 
		traffic_orders.start_date, 
		traffic_orders.end_date,
		traffic_orders.release_id, 
		traffic_orders.subscribers, 
		traffic_orders.display_network_id,
		traffic_orders.on_financial_reports,
		traffic_orders.active
from traffic_orders (NOLOCK) 
	join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
where 
	traffic_details.traffic_id = @traffic_id and 
	traffic_orders.system_id = @system_id 
