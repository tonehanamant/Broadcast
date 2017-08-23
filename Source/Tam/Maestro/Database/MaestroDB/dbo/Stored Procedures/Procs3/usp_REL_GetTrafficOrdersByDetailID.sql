CREATE PROCEDURE [dbo].[usp_REL_GetTrafficOrdersByDetailID]
	@traffic_detail_id int
AS
BEGIN
	select 
		traffic_orders.*
	from 
		traffic_orders (NOLOCK) 
		join traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	where 
		traffic_orders.traffic_detail_id = @traffic_detail_id
		and traffic_orders.system_id not in (668, 667)
		and traffic_detail_weeks.suspended = 0
		and traffic_orders.active = 1
END
