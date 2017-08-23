CREATE PROCEDURE [dbo].[usp_REL_GetTrafficOrdersByDetailIDByDaypart]
	@traffic_detail_id int,
	@daypart_id int,
	@start_date datetime,
	@end_date datetime
AS
BEGIN
	select
		traffic_orders.*
	from 
		traffic_orders (NOLOCK) 
		join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	where 
		traffic_orders.traffic_detail_id = @traffic_detail_id  
		and traffic_orders.daypart_id = @daypart_id
		and traffic_orders.start_date >= @start_date
		and traffic_orders.end_date <= @end_date
END
