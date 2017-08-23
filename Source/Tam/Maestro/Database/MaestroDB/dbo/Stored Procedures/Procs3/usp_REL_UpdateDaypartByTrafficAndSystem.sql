
CREATE PROCEDURE [dbo].[usp_REL_UpdateDaypartByTrafficAndSystem]
	@system_id int,
	@daypart_id int,
	@network_id int,
	@traffic_detail_id int,
	@new_daypart_id int,
	@start_date datetime,
	@end_date datetime
AS
 
update 
	traffic_orders 
	set traffic_orders.daypart_id = @new_daypart_id 
from traffic_orders WITH (NOLOCK)
	join traffic_details WITH (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id 
where 
	traffic_orders.system_id = @system_id 
	and traffic_orders.daypart_id = @daypart_id 
	and traffic_orders.display_network_id = @network_id
	and	
	(
		traffic_orders.start_date >= @start_date and traffic_orders.end_date <= @end_date
	) 
	and
	traffic_details.id = @traffic_detail_id
