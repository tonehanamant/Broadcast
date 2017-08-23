	
CREATE Procedure [dbo].[usp_REL_RemoveDetailLineFromSystem]
(
	@system_id int,
	@daypart_id int,
	@network_id int,
	@traffic_detail_id int,
	@start_date datetime,
	@end_date datetime
)
AS

delete from traffic_orders 
	where traffic_orders.system_id = @system_id 
	and traffic_orders.traffic_detail_id = @traffic_detail_id 
	and traffic_orders.start_date >= @start_date 
	and traffic_orders.end_date <= @end_date
	and traffic_orders.daypart_id = @daypart_id
	and traffic_orders.display_network_id = @network_id
